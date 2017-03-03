namespace Iris.Core

// * Imports

open System
open FlatBuffers

#if FABLE_COMPILER

open Iris.Web.Core.FlatBufferTypes

#else

open SharpYaml.Serialization
open Iris.Serialization

#endif

// * LogLevel

//  _                _                   _
// | |    ___   __ _| |    _____   _____| |
// | |   / _ \ / _` | |   / _ \ \ / / _ \ |
// | |__| (_) | (_| | |__|  __/\ V /  __/ |
// |_____\___/ \__, |_____\___| \_/ \___|_|
//             |___/
type LogLevel =
  | Debug
  | Info
  | Warn
  | Err

  static member Parse (str: string) =
    match String.toLower str with
    | "debug"         -> Debug
    | "info"          -> Info
    | "warn"          -> Warn
    | "err" | "error" -> Err
    | _               -> failwithf "could not parse %s" str

  static member TryParse (str: string) =
    Either.tryWith (Error.asParseError "LogLevel.TryParse") <| fun _ ->
      str |> LogLevel.Parse

  override self.ToString() =
    match self with
    | Debug -> "debug"
    | Info  -> "info"
    | Warn  -> "warn"
    | Err   -> "err"

// * Tier

/// ## Tier
///
/// Tier models the different types of locations in an Iris cluster.
///
/// - FrontEnd: log events from the frontend
/// - Client:   log events from Iris Service clients such as VVVV
/// - Service:  log events from the Iris Services
///
/// Returns: Tier
[<RequireQualifiedAccess>]
type Tier =
  | FrontEnd
  | Client
  | Service

  override self.ToString() =
    match self with
    | FrontEnd -> "FrontEnd"
    | Client   -> "Client"
    | Service  -> "Service"

  static member Parse (str: string) =
    match str with
    | "FrontEnd"  -> FrontEnd
    | "Client"    -> Client
    | "Service"   -> Service
    | _           -> failwithf "could not parse %s" str

  static member TryParse (str: string) =
    Either.tryWith (Error.asParseError "Tier.TryParse") <| fun _ ->
      str |> Tier.Parse

// * LogEventYaml

#if !FABLE_COMPILER

type LogEventYaml() =
  [<DefaultValue>] val mutable Time      : uint32
  [<DefaultValue>] val mutable Thread    : int
  [<DefaultValue>] val mutable Tier      : string
  [<DefaultValue>] val mutable Id        : string
  [<DefaultValue>] val mutable Tag       : string
  [<DefaultValue>] val mutable LogLevel  : string
  [<DefaultValue>] val mutable Message   : string

#endif

// * LogEvent

/// ## LogEvent
///
/// Structured log format record.
///
/// ## Fields:
///
/// - Time:     int64 unixtime in milliseconds
/// - Thread:   int ID of Thread the log event was collected
/// - Tier:     application tier where log was collected
/// - Id:       Id of cluster particiant where log was collected. Depends on Tier.
/// - Tag:      call site tag describing source code location where log was collected
/// - LogLevel: LogLevel of collected log message
/// - Message:  log message
///
/// Returns: LogEvent
type LogEvent =
  { Time      : uint32
    Thread    : int
    Tier      : Tier
    Id        : Id
    Tag       : string
    LogLevel  : LogLevel
    Message   : string }

  override self.ToString() =
    sprintf "[%s - %s - %s - %d - %d - %s]: %s"
      (System.String.Format("{0,-5}",string self.LogLevel))
      (System.String.Format("{0,-8}",string self.Tier))
      (System.String.Format("{0,-8}",self.Id |> string |> String.subString 0 8))
      self.Time
      self.Thread
      self.Tag
      self.Message

  // ** Binary

  //  ____  _
  // | __ )(_)_ __   __ _ _ __ _   _
  // |  _ \| | '_ \ / _` | '__| | | |
  // | |_) | | | | | (_| | |  | |_| |
  // |____/|_|_| |_|\__,_|_|   \__, |
  //                           |___/

  member self.ToOffset(builder: FlatBufferBuilder) =
    let tier = builder.CreateString (string self.Tier)
    let id = builder.CreateString (string self.Id)
    let tag = builder.CreateString self.Tag
    let level = builder.CreateString (string self.LogLevel)
    let msg = builder.CreateString self.Message

    LogEventFB.StartLogEventFB(builder)
    LogEventFB.AddTime(builder, self.Time)
    LogEventFB.AddThread(builder, self.Thread)
    LogEventFB.AddTier(builder, tier)
    LogEventFB.AddId(builder,id)
    LogEventFB.AddTag(builder,tag)
    LogEventFB.AddLogLevel(builder, level)
    LogEventFB.AddMessage(builder, msg)
    LogEventFB.EndLogEventFB(builder)

  static member FromFB(fb: LogEventFB) = either {
      let id = Id fb.Id
      let! tier = Tier.TryParse fb.Tier
      let! level = LogLevel.TryParse fb.LogLevel

      return { Time     = fb.Time
               Thread   = fb.Thread
               Tier     = tier
               Id       = id
               Tag      = fb.Tag
               LogLevel = level
               Message  = fb.Message }
    }

  // ** Yaml

  // __   __              _
  // \ \ / /_ _ _ __ ___ | |
  //  \ V / _` | '_ ` _ \| |
  //   | | (_| | | | | | | |
  //   |_|\__,_|_| |_| |_|_|

  #if !FABLE_COMPILER

  member self.ToYamlObject() =
    let yaml = new LogEventYaml()
    yaml.Time     <- self.Time
    yaml.Thread   <- self.Thread
    yaml.Tier     <- string self.Tier
    yaml.Id       <- string self.Id
    yaml.Tag      <- self.Tag
    yaml.LogLevel <- string self.LogLevel
    yaml.Message  <- self.Message
    yaml

  member self.ToYaml(serializer: Serializer) =
    self
    |> Yaml.toYaml
    |> serializer.Serialize

  static member FromYamlObject(yaml: LogEventYaml) = either {
      let id = Id yaml.Id
      let! level = LogLevel.TryParse yaml.LogLevel
      let! tier = Tier.TryParse yaml.Tier
      return { Time     = yaml.Time
               Thread   = yaml.Thread
               Tier     = tier
               Id       = id
               Tag      = yaml.Tag
               LogLevel = level
               Message  = yaml.Message }
    }

  static member FromYaml(str: string) =
    let serializer = new Serializer()
    str
    |> serializer.Deserialize
    |> LogEvent.FromYamlObject

  #endif

// * Logger

[<RequireQualifiedAccess>]
module Logger =

  // ** Imports

  open System
  open System.Threading

  open Iris.Core

  // ** stdout

  /// ## stdout
  ///
  /// Simple logging to stdout
  ///
  /// ### Signature:
  /// - log: LogEvent
  ///
  /// Returns: unit
  let stdout (log: LogEvent) =
    log |> string |> printfn "%s"

  // ** stdoutWith

  let stdoutWith (level: LogLevel) (log: LogEvent) =
    match level, log.LogLevel with
    | Debug, _ -> stdout log
    | Info, Info | Info, Warn | Info, Err ->
      stdout log
    | Warn, Warn | Warn, Err ->
      stdout log
    | Err, Err ->
      stdout log
    | _ -> ()

  let private subscriptions = new ResizeArray<IObserver<LogEvent>>()

  let listener =
    { new IObservable<LogEvent> with
        member self.Subscribe(obs) =

          #if FABLE_COMPILER
          subscriptions.Add obs
          #else
          lock subscriptions <| fun _ ->
            subscriptions.Add obs
          #endif

          // Subscribe must return an IDisposable so the observer can be gc'd
          { new IDisposable with
              member self.Dispose() =
                #if FABLE_COMPILER
                subscriptions.Remove obs
                |> ignore
                #else
                lock subscriptions <| fun _ ->
                  subscriptions.Remove obs
                  |> ignore
                #endif
              } }

  // ** agent

  /// ## agent
  ///
  /// Logging agent. Hidden.
  ///
  /// Returns: MailboxProcessor<LogEvent>
  let private agent =
    MailboxProcessor<LogEvent>.Start <| fun inbox -> async {
      while true do
        let! log = inbox.Receive()
        for sub in subscriptions do
          sub.OnNext log
    }

  /// ## subscribe
  ///
  /// Log the given string.
  let subscribe cb =
    { new IObserver<LogEvent> with
        member x.OnCompleted() = ()
        member x.OnError(error) = ()
        member x.OnNext(value) = cb value }
    |> listener.Subscribe

  // let filter (level: LogLevel) (logger: LogEvent -> unit) =
  //     /// ## To `log` or not, that is the question.
  //     match level with
  //     /// In Debug, all messages get logged
  //     | Debug -> onLog log

  //     // In Info mode, all messages except `Debug` ones get logged
  //     | Info  ->
  //       match log.LogLevel with
  //       | Info | Warn | Err -> onLog log
  //       | _ -> ()

  //     // In Warn mode, messages of type `Err` and `Warn` get logged
  //     | Warn  ->
  //       match log.LogLevel with
  //       | Warn | Err -> onLog log
  //       | _ -> ()

  //     // In Err mode, only messages of type `Err` get logged
  //     | Err   ->
  //       match log.LogLevel with
  //       | Err -> onLog log
  //       | _ -> ()

  // ** create

  /// ## create
  ///
  /// Create a new LogEvent, hiding some of the nitty gritty details.
  ///
  /// ### Signature:
  /// - arg: arg
  /// - arg: arg
  /// - arg: arg
  ///
  /// Returns: LogEvent
  let create (level: LogLevel) (id: Id) (callsite: CallSite) (msg: string) =
    let tier =
      #if FABLE_COMPILER
      Tier.FrontEnd
      #else
      Tier.Service
      #endif

    let now  = DateTime.UtcNow |> Time.unixTime

    { Time     = uint32 now
      #if FABLE_COMPILER
      Thread   = 1
      #else
      Thread   = Thread.CurrentThread.ManagedThreadId
      #endif
      Tier     = tier
      Id       = id
      Tag      = callsite
      LogLevel = level
      Message  = msg }

  /// ## log
  ///
  /// Log the given string.
  ///
  /// ### Signature:
  /// - level: LogLevel
  /// - id: Id
  /// - callside: CallSite
  /// - msg: string
  ///
  /// Returns: unit
  let log (level: LogLevel) (id: Id) (callsite: CallSite) (msg: string) =
    create level id callsite msg
    |> agent.Post

  // ** debug

  /// ## debug
  ///
  /// Shorthand for creating a Debug event.
  ///
  /// ### Signature:
  /// - id: Id of session/client/service node
  /// - callsite: location where even was generated
  /// - msg: log message
  ///
  /// Returns: unit
  let debug (id: Id) (callsite: CallSite) (msg: string) =
    create LogLevel.Debug id callsite msg
    |> agent.Post

  // ** info

  /// ## info
  ///
  /// Shorthand for creating a Info event.
  ///
  /// ### Signature:
  /// - id: Id of session/client/service node
  /// - callsite: location where even was generated
  /// - msg: log message
  ///
  /// Returns: LogEvent
  let info (id: Id) (callsite: CallSite) (msg: string) =
    create LogLevel.Info id callsite msg
    |> agent.Post

  // ** warn

  /// ## warn
  ///
  /// Shorthand for creating a Warn event.
  ///
  /// ### Signature:
  /// - id: Id of session/client/service node
  /// - callsite: location where even was generated
  /// - msg: log message
  ///
  /// Returns: LogEvent
  let warn (id: Id) (callsite: CallSite) (msg: string) =
    create LogLevel.Warn id callsite msg
    |> agent.Post

  // ** err

  /// ## err
  ///
  /// Shorthand for creating a Err event.
  ///
  /// ### Signature:
  /// - id: Id of session/client/service node
  /// - callsite: location where even was generated
  /// - msg: log message
  ///
  /// Returns: LogEvent
  let err (id: Id) (callsite: CallSite) (msg: string) =
    create LogLevel.Err id callsite msg
    |> agent.Post
