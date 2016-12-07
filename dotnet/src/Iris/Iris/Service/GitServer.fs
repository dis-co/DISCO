namespace Iris.Service

// * Imports

open Iris.Raft
open Iris.Core
open Iris.Core.Utils
open Iris.Service.Utilities

open System
open System.IO
open System.Threading
open System.Diagnostics
open System.Management
open System.Text.RegularExpressions
open Microsoft.FSharp.Control
open FSharpx.Functional

// * GitServer

//   ____ _ _   ____
//  / ___(_) |_/ ___|  ___ _ ____   _____ _ __
// | |  _| | __\___ \ / _ \ '__\ \ / / _ \ '__|
// | |_| | | |_ ___) |  __/ |   \ V /  __/ |
//  \____|_|\__|____/ \___|_|    \_/ \___|_|

// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
//
// EXAMPLE CLONES
//
// [10422] Ready to rumble
// [11331] Connection from 127.0.0.1:43218
// [11331] Extended attributes (21 bytes) exist <host=localhost:6000>
// [11331] Request upload-pack for '/gittest/.git'
// [10422] [11331] Disconnected
// [11921] Connection from 127.0.0.1:43224
// [11921] Extended attributes (21 bytes) exist <host=localhost:6000>
// [11921] Request upload-pack for '/gittest/.git'
// [10422] [11921] Disconnected
//
// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

module Git =

  [<Literal>]
  let private tag = "GitServer"

  // ** GitEvent

  type GitEvent =
    | Started of pid:int
    | Exited  of code:int
    | Pull    of pid:int * address:string * port:uint16

  // ** Listener

  type private Listener = IObservable<GitEvent>

  // ** IGitServer

  type IGitServer =
    inherit IDisposable

    abstract Status    : Either<IrisError,ServiceStatus>
    abstract Pid       : Either<IrisError,int>
    abstract Subscribe : (GitEvent -> unit) -> IDisposable
    abstract Start     : unit -> Either<IrisError,unit>

  // ** Subscriptions

  type private Subscriptions = ResizeArray<IObserver<GitEvent>>

  // ** GitStateData

  [<NoComparison;NoEquality>]
  type private GitStateData =
    { Status      : ServiceStatus
      Process     : Process
      Pid         : int
      SubPid      : int
      Disposables : IDisposable seq }

    interface IDisposable with
      member self.Dispose() =
        for disposable in self.Disposables do
          dispose disposable

        try
          Process.kill self.Pid
        finally
          dispose self.Process

  // ** GitState

  [<NoComparison;NoEquality>]
  type private GitState =
    | Idle
    | Running of GitStateData

    interface IDisposable with
      member self.Dispose () =
        match self with
        | Running data -> dispose data
        | _ -> ()

  // ** Reply

  [<RequireQualifiedAccess;NoComparison;NoEquality>]
  type private Reply =
    | Ok
    | Pid      of int
    | Status   of ServiceStatus

  // ** ReplyChan

  type private ReplyChan = AsyncReplyChannel<Either<IrisError,Reply>>

  // ** Msg

  [<RequireQualifiedAccess;NoComparison;NoEquality>]
  type private Msg =
    | Start    of path:FilePath * addr:string * port:uint16 * chan:ReplyChan
    | Stop     of chan:ReplyChan
    | Status   of chan:ReplyChan
    | Pid      of chan:ReplyChan
    | Exit     of int                   // Event from Git, needs no reply
    | Log      of string                // Event from Git, needs no reply either

    override self.ToString () =
      match self with
      | Start  (path, addr, port, _) ->
        sprintf "Start path:%s addr:%s port:%d" path addr port
      | Stop   _ -> "Stop"
      | Status _ -> "Status"
      | Pid    _ -> "Pid"
      | Exit   c -> sprintf "Exit: %d" c
      | Log str  -> sprintf "Log: %s" str

  // ** GitAgent

  type private GitAgent = MailboxProcessor<Msg>

  // ** createProcess

  let private createProcess (path: FilePath) (addr: string) (port: uint16) =
    let basedir =
      Path.GetDirectoryName path
      |> String.replace '\\' '/'

    let sanepath = String.replace '\\' '/' path

    let args =
      [| "daemon"
      ; "--verbose"
      ; "--strict-paths"
      ; (sprintf "--base-path=%s" basedir)
      ; (if Platform.isUnix then "--reuseaddr" else "")
      ; (addr |> string |> sprintf "--listen=%s")
      ; (sprintf "--port=%d" port)
      ; (sprintf "%s/.git" sanepath) |]
      |> String.join " "

    let proc = new Process()
    proc.StartInfo.FileName <- "git"
    proc.StartInfo.Arguments <- args
    proc.EnableRaisingEvents <- true
    proc.StartInfo.CreateNoWindow <- true
    proc.StartInfo.UseShellExecute <- false
    proc.StartInfo.RedirectStandardOutput <- true
    proc.StartInfo.RedirectStandardError <- true

    proc

  // ** exitHandler

  /// ## exitHandler
  ///
  /// Handler to monitor the sub-processes. If the `git daemon` process exits for whichever reason
  /// (it might not have been able to start because of erroneous port settings for instance), then
  /// this handler detects this, creates an error from this and ensures control is passed back to
  /// the caller of `Start()`. It also ensures that all resources on the thread will get cleaned up
  /// properly.
  ///
  /// ### Signature:
  /// - proc: Process to monitor (mostly to get ExitCode from)
  ///
  /// Returns: unit
  let private exitHandler (proc: Process) (agent: GitAgent) _ =
    agent.Post(Msg.Exit proc.ExitCode)

  // ** logHandler

  let private logHandler (agent: GitAgent) (data: DataReceivedEventArgs) =
    match data.Data with
    | null -> ()
    | _ -> agent.Post(Msg.Log data.Data)

  let private (|Ready|_|) (input: string) =
    let m = Regex.Match(input, "\[(?<pid>[0-9]*)\] Ready to rumble")
    if m.Success then
      match Int32.TryParse(m.Groups.[1].Value) with
      | (true, pid) -> Some pid
      | _ -> None
    else
      None

  let private (|Connection|_|) (input: string) =
    let pattern = "\[(?<pid>[0-9]*)\] Connection from (?<ip>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})\:(?<port>[0-9]*)"
    let m = Regex.Match(input, pattern)
    if m.Success then
      match Int32.TryParse(m.Groups.[1].Value), UInt16.TryParse(m.Groups.[3].Value) with
      | (true,pid), (true,port) ->
        Some(pid, m.Groups.[2].Value, port)
      | _ -> None
    else
      None

  let private parseLog (line: string) =
    match line with
    | Ready pid ->
      Started pid
      |> Either.succeed

    | Connection (pid, ip, port) ->
      Pull(pid, ip, port)
      |> Either.succeed

    | _ -> Either.fail IrisError.OK      // we don't care about the rest

  // ** createListener

  let private createListener (subscriptions: Subscriptions) =
    { new Listener with
        member self.Subscribe(obs) =
          lock subscriptions <| fun _ ->
            subscriptions.Add obs

          { new IDisposable with
              member self.Dispose () =
                lock subscriptions <| fun _ ->
                  subscriptions.Remove obs
                  |> ignore } }

  // ** handleStart

  let private handleStart (state: GitState)
                          (path: FilePath)
                          (addr: string)
                          (port: uint16)
                          (chan: ReplyChan)
                          (agent: GitAgent) =
    // dispose of previous server
    match state with
    | Running data -> dispose data
    | _ -> ()

    let proc = createProcess path addr port

    let stdoutReader =
      Observable.subscribe (logHandler agent) proc.OutputDataReceived

    let stderrReader =
      Observable.subscribe (logHandler agent) proc.ErrorDataReceived

    let onExitEvent =
      Observable.subscribe (exitHandler proc agent) proc.Exited

    try
      if proc.Start() then
        proc.BeginOutputReadLine()
        proc.BeginErrorReadLine()

        Reply.Ok
        |> Either.succeed
        |> chan.Reply

        Running {
            Status = ServiceStatus.Starting
            Process = proc
            Pid = proc.Id
            SubPid = 0
            Disposables = [ stdoutReader
                            stderrReader
                            onExitEvent ]
          }
      else
        "Could not start git daemon process"
        |> GitError
        |> Either.fail
        |> chan.Reply
        state
    with
      | exn ->
        exn.Message
        |> sprintf "Exception starting git daemon process %s"
        |> GitError
        |> Either.fail
        |> chan.Reply
        state

  // ** handleLog

  let private handleLog (state: GitState) (msg: string) (subscriptions: Subscriptions) =
    match state with
    | Idle -> state
    | Running data ->
      match parseLog msg with
      | Right msg ->
        // notify
        for subscription in subscriptions do
          subscription.OnNext msg

        // handle
        match msg with
        | Started pid ->
          Running { data with
                      Status = ServiceStatus.Running
                      SubPid = pid }
        | _ -> state
      | _ -> state

  // ** handleExit

  let private handleExit (state: GitState) (code: int) (subscriptions: Subscriptions) =
    match state with
    | Idle -> state
    | Running data ->
      // notify
      for subscription in subscriptions do
        subscription.OnNext (Exited code)

      match code with
      | 0 -> Running { data with Status = ServiceStatus.Stopped }
      | _ ->
        let error =
          sprintf "Non-zero exit code: %d" code
          |> GitError
        Running { data with Status = ServiceStatus.Failed error }

  // ** handleStatus

  let private handleStatus (state: GitState) (chan: ReplyChan) =
    match state with
    | Idle ->
      ServiceStatus.Stopped
      |> Reply.Status
      |> Either.succeed
      |> chan.Reply
    | Running data ->
      data.Status
      |> Reply.Status
      |> Either.succeed
      |> chan.Reply
    state

  // ** handlePid

  let private handlePid (state: GitState) (chan: ReplyChan) =
    match state with
    | Idle ->
      "No GitDaemon started"
      |> GitError
      |> Either.fail
      |> chan.Reply
    | Running data ->
      Reply.Pid data.Pid
      |> Either.succeed
      |> chan.Reply
    state

  // ** handleStop

  let private handleStop (state: GitState) (subscriptions: Subscriptions) (chan: ReplyChan) =
    match state with
    | Idle ->
      "No GitDaemon started"
      |> GitError
      |> Either.fail
      |> chan.Reply
      state
    | Running data ->
      for subscription in subscriptions do
        subscription.OnNext (Exited 0)
      dispose data
      Reply.Ok
      |> Either.succeed
      |> chan.Reply
      Idle

  // ** loop

  let private loop (initial: GitState) (subscriptions: Subscriptions) (inbox: GitAgent) =
    let rec act (state: GitState) =
      async {
        let! msg = inbox.Receive()

        let newstate =
          match msg with
          | Msg.Start (path,addr,port,chan) ->
            handleStart state path addr port chan inbox

          | Msg.Pid chan ->
            handlePid state chan

          | Msg.Status chan ->
            handleStatus state chan

          | Msg.Stop chan ->
            handleStop state subscriptions chan

          | Msg.Exit code ->
            handleExit state code subscriptions

          | Msg.Log msg ->
            handleLog state msg subscriptions

        return! act newstate
      }
    act initial

  // ** starting

  let private starting (agent: GitAgent) =
    match agent.PostAndReply(fun chan -> Msg.Status chan) with
    | Right (Reply.Status status) when status = ServiceStatus.Starting -> true
    | _ -> false

  // ** started

  let private running (agent: GitAgent) =
    let result =
      match agent.PostAndReply(fun chan -> Msg.Status chan) with
      | Right (Reply.Status status) when status = ServiceStatus.Running -> true
      | _ -> false
    result

  // ** GitServer

  [<RequireQualifiedAccess>]
  module GitServer =

    let create (node: RaftNode) (path: FilePath) =
      let subscriptions = new Subscriptions()
      let listener = createListener subscriptions
      let agent = new GitAgent(loop Idle subscriptions)
      agent.Start()

      Either.succeed
        { new IGitServer with
            member self.Status
              with get () =
                match agent.PostAndReply(fun chan -> Msg.Status chan) with
                | Right (Reply.Status status) ->
                  Either.succeed status
                | Right other ->
                  other
                  |> sprintf "Unexpected reply from GitAgent: %A"
                  |> GitError
                  |> Either.fail
                | Left error ->
                  error
                  |> Either.fail

            member self.Pid
              with get () =
                match agent.PostAndReply(fun chan -> Msg.Pid chan) with
                | Right (Reply.Pid pid) ->
                  Either.succeed pid
                | Right other ->
                  other
                  |> sprintf "Unexpected reply from GitAgent: %A"
                  |> GitError
                  |> Either.fail
                | Left error ->
                  error
                  |> Either.fail

            member self.Subscribe(callback: GitEvent -> unit) =
              { new IObserver<GitEvent> with
                  member self.OnCompleted() = ()
                  member self.OnError(error) = ()
                  member self.OnNext(value) = callback value }
              |> listener.Subscribe

            member self.Start () =
              let callback (chan: ReplyChan) =
                Msg.Start(path, string node.IpAddr, node.GitPort, chan)

              match agent.PostAndReply(callback) with
              | Right Reply.Ok ->

                // wait for a little while until it forked
                let mutable n = 0
                while starting agent && n < 1000 do
                  n <- n + 10
                  Thread.Sleep 10

                if running agent then
                  Either.succeed ()
                else
                  match agent.PostAndReply(fun chan -> Msg.Status chan) with
                  | Right (Reply.Status status) ->
                    string status
                    |> GitError
                    |> Either.fail
                  | Right other ->
                    "Unexpected reply type from GitAgent"
                    |> GitError
                    |> Either.fail
                  | Left error ->
                    error
                    |> Either.fail

              | Right other ->
                "Unexpected reply type from GitAgent"
                |> GitError
                |> Either.fail
              | Left error ->
                error
                |> Either.fail

            member self.Dispose() =
              agent.PostAndReply(fun chan -> Msg.Stop chan)
              |> ignore
              subscriptions.Clear()
          }