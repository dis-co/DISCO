namespace Iris.Core

// * Imports

open System
open Iris.Core

#if FABLE_COMPILER

open Iris.Core.FlatBuffers
open Iris.Web.Core.FlatBufferTypes

#else

open FlatBuffers
open Iris.Serialization

#endif

// * ServiceType

[<RequireQualifiedAccess>]
type ServiceType =
  | Git
  | Raft
  | Http
  | WebSocket
  | Api

  // ** ToString

  override self.ToString() =
    match self with
    | Git       -> "git"
    | Raft      -> "raft"
    | Api       -> "api"
    | Http      -> "http"
    | WebSocket -> "ws"

  // ** Parse

  static member Parse (str: string) =
    match str with
    | "git"  -> Git
    | "raft" -> Raft
    | "http" -> Http
    | "api"  -> Api
    | "ws"   -> WebSocket
    | _ -> failwithf "unknown service type: %s" str

  // ** TryParse

  static member TryParse (str: string) =
    try
      str
      |> ServiceType.Parse
      |> Either.succeed
    with
      | exn ->
        exn.Message
        |> Error.asParseError "ServiceType.TryParse"
        |> Either.fail

  // ** ToOffset

  member tipe.ToOffset(_: FlatBufferBuilder) =
    match tipe with
    |  Git  -> ExposedServiceTypeFB.GitFB
    |  Raft -> ExposedServiceTypeFB.RaftFB
    |  Http -> ExposedServiceTypeFB.HttpFB
    |  Api  -> ExposedServiceTypeFB.ApiFB
    |  WebSocket -> ExposedServiceTypeFB.WebSocketFB

  // ** FromFB

  static member FromFB(fb: ExposedServiceTypeFB) =
    #if FABLE_COMPILER
    match fb with
    | x when x = ExposedServiceTypeFB.GitFB        -> Right Git
    | x when x = ExposedServiceTypeFB.RaftFB       -> Right Raft
    | x when x = ExposedServiceTypeFB.HttpFB       -> Right Http
    | x when x = ExposedServiceTypeFB.ApiFB        -> Right Api
    | x when x = ExposedServiceTypeFB.WebSocketFB  -> Right WebSocket
    | x ->
      sprintf "Unknown ExposedServiceTypeFB value: %d" x
      |> Error.asParseError "ServiceType.FromFB"
      |> Either.fail
    #else
    match fb with
    | ExposedServiceTypeFB.GitFB        -> Right Git
    | ExposedServiceTypeFB.RaftFB       -> Right Raft
    | ExposedServiceTypeFB.HttpFB       -> Right Http
    | ExposedServiceTypeFB.ApiFB        -> Right Api
    | ExposedServiceTypeFB.WebSocketFB  -> Right WebSocket
    | x ->
      sprintf "Unknown ExposedServiceTypeFB value: %O" x
      |> Error.asParseError "ServiceType.FromFB"
      |> Either.fail
    #endif


// * ServiceType module

module ServiceType =

  // ** isServiceType

  let isServiceType (str: string) =
    try
      ServiceType.Parse str |> ignore
      true
    with
      | _ -> false

// * ExposedService

type ExposedService =
  { ServiceType: ServiceType
    Port: Port }

  with

    // ** ToOffset

    member service.ToOffset(builder: FlatBufferBuilder) =
      let tipe = service.ServiceType.ToOffset builder
      let port = service.Port |> unwrap
      ExposedServiceFB.StartExposedServiceFB(builder)
      ExposedServiceFB.AddType(builder,tipe)
      ExposedServiceFB.AddPort(builder,port)
      ExposedServiceFB.EndExposedServiceFB(builder)

    // ** FromFB

    static member FromFB (fb: ExposedServiceFB) =
      either {
        let! tipe = ServiceType.FromFB fb.Type
        return { ServiceType = tipe; Port = port fb.Port }
      }

// * DiscoverableService

type DiscoverableService =
  { Id: ServiceId
    WebPort: Port
    Status: MachineStatus
    Services: ExposedService array
    ExtraMetadata: Property array }

// * DiscoveredService

type DiscoveredService =
  { Id: ServiceId
    Name: string
    FullName: string
    HostName: string
    HostTarget: string
    Status: MachineStatus
    Aliases: string array
    Protocol: IPProtocol
    AddressList: IpAddress array
    Services: ExposedService array
    ExtraMetadata: Property array }

  // ** ToOffset

  member service.ToOffset(builder: FlatBufferBuilder) =
    let id = DiscoveredServiceFB.CreateIdVector(builder,service.Id.ToByteArray())
    let name = Option.mapNull builder.CreateString service.Name
    let fullname = Option.mapNull builder.CreateString service.FullName
    let hostname = Option.mapNull builder.CreateString service.HostName
    let hosttarget = Option.mapNull builder.CreateString service.HostTarget
    let status = Binary.toOffset builder service.Status

    let aliases =
      let serialize = function
        #if FABLE_COMPILER
        | null -> Unchecked.defaultof<Offset<string>>
        #else
        | null -> Unchecked.defaultof<StringOffset>
        #endif
        | other -> builder.CreateString other

      (builder, Array.map serialize service.Aliases)
      |> DiscoveredServiceFB.CreateAliasesVector

    let protocol =
      match service.Protocol with
      | IPProtocol.IPv4 -> "IPv4"
      | IPProtocol.IPv6 -> "IPv6"
      |> builder.CreateString

    let addressList =
      service.AddressList
      |> Array.map (string >> builder.CreateString)
      |> fun addrs -> DiscoveredServiceFB.CreateAddressListVector(builder, addrs)

    let services =
      (builder, Array.map (Binary.toOffset builder) service.Services)
      |> DiscoveredServiceFB.CreateServicesVector

    let metadata =
      (builder, Array.map (Binary.toOffset builder) service.ExtraMetadata)
      |> DiscoveredServiceFB.CreateExtraMetadataVector

    DiscoveredServiceFB.StartDiscoveredServiceFB(builder)
    DiscoveredServiceFB.AddId(builder, id)
    Option.iter (fun value -> DiscoveredServiceFB.AddName(builder,value)) name
    Option.iter (fun value -> DiscoveredServiceFB.AddFullName(builder,value)) fullname
    Option.iter (fun value -> DiscoveredServiceFB.AddHostName(builder,value)) hostname
    Option.iter (fun value -> DiscoveredServiceFB.AddHostTarget(builder,value)) hosttarget
    DiscoveredServiceFB.AddAliases(builder, aliases)
    DiscoveredServiceFB.AddProtocol(builder, protocol)
    DiscoveredServiceFB.AddAddressList(builder, addressList)
    DiscoveredServiceFB.AddStatus(builder, status)
    DiscoveredServiceFB.AddServices(builder, services)
    DiscoveredServiceFB.AddExtraMetadata(builder, metadata)
    DiscoveredServiceFB.EndDiscoveredServiceFB(builder)

  // ** FromFB

  static member FromFB(fb: DiscoveredServiceFB) =
    either {
      let! protocol =
        match fb.Protocol with
        | "IPv4" -> Right IPProtocol.IPv4
        | "IPv6" -> Right IPProtocol.IPv6
        | other ->
          "Unknown protocol: " + other
          |> Error.asParseError "Discovery.FromFB"
          |> Either.fail

      let! metadata =
        let arr = Array.zeroCreate fb.ExtraMetadataLength
        Array.fold
          (fun (m: Either<IrisError, int * Property array>) _ -> either {
            let! (idx, props) = m

            #if FABLE_COMPILER
            let! prop = fb.ExtraMetadata(idx) |> Property.FromFB
            #else
            let! prop =
              let nullable = fb.ExtraMetadata(idx)
              if nullable.HasValue then
                let value = nullable.Value
                Property.FromFB value
              else
                "Unable to parse empty Property value"
                |> Error.asParseError "DiscoveredService.FromFB"
                |> Either.fail
            #endif

            props.[idx] <- prop
            return (idx + 1, props)
          })
          (Right(0, arr))
          arr
        |> Either.map snd

      let! addressList =
        let arr = Array.zeroCreate fb.AddressListLength
        Array.fold
          (fun (m: Either<IrisError, int * IpAddress[]>) _ -> either {
            let! (idx, addresses) = m
            let! ip = fb.AddressList(idx) |> IpAddress.TryParse
            addresses.[idx] <- ip
            return (idx + 1, addresses)
          })
          (Right(0, arr))
          arr
        |> Either.map snd

      let aliases =
        [| for i = 0 to fb.AliasesLength - 1 do
            let value = try fb.Aliases(i) with | _ -> null
            yield value |]

      let! status =
        #if FABLE_COMPILER
        fb.Status |> MachineStatus.FromFB
        #else
        let nullable = fb.Status
        if nullable.HasValue then
          let value = nullable.Value
          MachineStatus.FromFB value
        else
          "Unable to parse empty ServiceStatus"
          |> Error.asParseError "DiscoveredService.FromFB"
          |> Either.fail
        #endif

      let! services =
        let arr = Array.zeroCreate fb.ServicesLength
        Array.fold
          (fun (m: Either<IrisError,int * ExposedService array>) _ -> either {
            let! (idx, services) = m

            #if FABLE_COMPILER
            let! service = fb.Services(idx) |> ExposedService.FromFB
            #else
            let! service =
              let nullable = fb.Services(idx)
              if nullable.HasValue then
                let value = nullable.Value
                ExposedService.FromFB value
              else
                "Unable to parse empty ExposedService key/value"
                |> Error.asParseError "DiscoveryService.FromFB"
                |> Either.fail
            #endif

            services.[idx] <- service
            return (idx + 1, services)
          })
          (Right(0, arr))
          arr
        |> Either.map snd

      let! id = Id.decodeId fb

      return {
        Id            = id
        Name          = fb.Name
        FullName      = fb.FullName
        HostName      = fb.HostName
        HostTarget    = fb.HostTarget
        Aliases       = aliases
        Protocol      = protocol
        Status        = status
        Services      = services
        AddressList   = addressList |> Seq.toArray
        ExtraMetadata = metadata
      }
    }

  // ** ToBytes

  member request.ToBytes() =
    Binary.buildBuffer request

  // ** FromBytes

  static member FromBytes(raw: byte[]) =
    raw
    |> Binary.createBuffer
    |> DiscoveredServiceFB.GetRootAsDiscoveredServiceFB
    |> DiscoveredService.FromFB

// * Discovery module

#if !FABLE_COMPILER && !IRIS_NODES

open Mono.Zeroconf

module Discovery =

  open System
  open System.Text
  open System.Text.RegularExpressions

  // ** metadata constants

  [<Literal>]
  let MACHINE = "machine"

  [<Literal>]
  let STATUS = "status"

  [<Literal>]
  let PROJECT_NAME = "project_name"

  [<Literal>]
  let PROJECT_ID = "project_id"

  // ** tag

  let private tag (str: string) =
    String.Format("Discovery.{0}", str)

  // ** serviceName

  let private serviceName (id: IrisId) =
    String.Format("{0} [{1}]", Constants.ZEROCONF_SERVICE_NAME, string id)

  // ** (|Machine|_|)

  let private (|Machine|_|) (item: TxtRecordItem) =
    match item.Key, item.ValueString with
    | MACHINE, value when not (isNull value) -> Some value
    | _ -> None

  // ** (|Status|_|)

  let private (|Status|_|) (item: TxtRecordItem) =
    match item.Key, item.ValueString with
    | STATUS, value when not (isNull value) -> Some value
    | _ -> None

  // ** (|ProjectId|_|)

  let private (|ProjectId|_|) (item: TxtRecordItem) =
    match item.Key, item.ValueString with
    | PROJECT_ID, value when not (isNull value) -> Some value
    | _ -> None

  // ** (|ProjectName|_|)

  let private (|ProjectName|_|) (item: TxtRecordItem) =
    match item.Key, item.ValueString with
    | PROJECT_NAME, value when not (isNull value) -> Some value
    | _ -> None

  // ** (|Services|_|)

  let private (|Services|_|) (item: TxtRecordItem) =
    #if FABLE_COMPILER
    let prt = try Some(uint16 item.ValueString) with | _ -> None
    match ServiceType.TryParse item.Key, prt with
    | Right st, (true, prt) -> Some { ServiceType = st; Port = port prt }
    | _ -> None
    #else
    match ServiceType.TryParse item.Key, UInt16.TryParse item.ValueString with
    | Right st, (true, prt) -> Some { ServiceType = st; Port = port prt }
    | _ -> None
    #endif

  // ** (|ServiceId|_|)

  let (|ServiceId|_|) (str: string) =
    let m = Regex.Match(str, "^.*\[(.*)\]$")
    if m.Success then
      m.Groups.[1].Value |> IrisId.Parse |> Some
    else None

  // ** parseFieldWith

  let inline private parseFieldWith (f: TxtRecordItem -> 'a option) (record: ITxtRecord) =
    record
    |> Seq.cast<TxtRecordItem>
    |> Seq.fold (fun m txt -> Option.orElse (f txt) m) None

  // ** parseMachine

  let private parseMachine (txt: ITxtRecord) =
    match parseFieldWith (|Machine|_|) txt with
    | Some id -> id |> IrisId.Parse |> Either.succeed
    | _ ->
      "Could not find machine id in metatdata"
      |> Error.asParseError (tag "parseMachine")
      |> Either.fail

  // ** parseProtocol

  let private parseProtocol (proto: AddressProtocol) =
    match proto with
    | AddressProtocol.IPv4 -> Either.succeed IPv4
    | AddressProtocol.IPv6 -> Either.succeed IPv6
    | x ->
      "AddressProtocol could not be parsed: " + string x
      |> Error.asParseError (tag "parseProtocol")
      |> Either.fail

  // ** parseStatus

  let private parseStatus (record: ITxtRecord) =
    let rawid = parseFieldWith (|ProjectId|_|) record
    let rawname = parseFieldWith (|ProjectName|_|) record
    let rawstatus = parseFieldWith (|Status|_|) record

    match rawstatus, rawid, rawname with
    | Some MachineStatus.IDLE, _, _ -> Right Idle
    | Some MachineStatus.BUSY, Some id, Some parsed
      when not (isNull id) && not (isNull parsed) ->
      Busy (IrisId.Parse id, name parsed) |> Either.succeed
    | _, _, _ ->
      "Failed to parse Machine status: field(s) missing or null"
      |> Error.asParseError (tag "parseStatus")
      |> Either.fail

  // ** reservedField

  let private reservedField (item: TxtRecordItem) =
    match item.Key with
    | STATUS | MACHINE | PROJECT_ID | PROJECT_NAME -> true
    | other -> ServiceType.isServiceType other

  // ** parseMetadata

  let private parseMetadata (record: ITxtRecord) =
    record
    |> Seq.cast<TxtRecordItem>
    |> Seq.filter (not << reservedField)
    |> Seq.map (fun i -> { Key = i.Key; Value = i.ValueString })
    |> Seq.filter (fun prop -> not (isNull prop.Key) && not (isNull prop.Value))
    |> Seq.toArray

  // ** parseServices

  let private parseServices (record: ITxtRecord) =
    record
    |> Seq.cast<TxtRecordItem>
    |> Seq.map ((|Services|_|))
    |> Seq.filter Option.isSome
    |> Seq.map Option.get
    |> Seq.toArray

  // ** toDiscoverableService

  let toDiscoverableService (discoverable: DiscoverableService) =
    let service = new RegisterService()

    service.Name <- serviceName discoverable.Id
    service.RegType <- ZEROCONF_TCP_SERVICE
    service.ReplyDomain <- ZEROCONF_DOMAIN
    service.Port <- int16 discoverable.WebPort

    let record = new TxtRecord()

    record.Add(MACHINE, string discoverable.Id)
    record.Add(STATUS, string discoverable.Status)

    match discoverable.Status with
    | Busy (id, name) ->
      record.Add(PROJECT_ID, string id)
      record.Add(PROJECT_NAME, unwrap name)
    | _ -> ()

    for service in discoverable.Services do
      record.Add(string service.ServiceType, string service.Port)

    for meta in discoverable.ExtraMetadata do
      record.Add(meta.Key, meta.Value)

    service.TxtRecord <- record
    service

  // ** toDiscoveredService

  let toDiscoveredService (service: IResolvableService) =
    either {
      let entry = service.HostEntry

      let! proto = parseProtocol service.AddressProtocol

      let addresses =
        if isNull entry then
          [| |]
        else
          Array.map IpAddress.ofIPAddress entry.AddressList

      let! machine = parseMachine service.TxtRecord
      let! status = parseStatus service.TxtRecord
      let services = parseServices service.TxtRecord
      let metadata = parseMetadata service.TxtRecord

      let name =
        if isNull service.Name then
          Constants.EMPTY
        else service.Name

      let fullname =
        if isNull service.FullName then
          Constants.EMPTY
        else service.FullName

      let hostname =
        if isNull entry || isNull entry.HostName then
          ""
        else entry.HostName

      let hosttarget =
        if isNull service.HostTarget then
          ""
        else service.HostTarget

      let aliases =
        // need to check both, if the entry is null
        // *and* the aliases array, since it *can* be null
        // and would still be valid value (i.e. the type checker)
        // cannot catch this problem. ouf.
        if isNull entry || isNull entry.Aliases then
          [| |]
        else
          Array.filter (isNull >> not) entry.Aliases

      return
        { Id = machine
          Protocol = proto
          Name = name
          FullName = fullname
          HostName = hostname
          HostTarget = hosttarget
          Aliases = aliases
          AddressList = addresses
          Status = status
          Services = services
          ExtraMetadata = metadata }
    }

  // ** mergeDiscovered

  let mergeDiscovered (have: DiscoveredService) (got: DiscoveredService) =
    { have with AddressList = Array.append have.AddressList got.AddressList }


#endif
