namespace Iris.Core

#if JAVASCRIPT

open Iris.Core.FlatBuffers
open Iris.Web.Core.FlatBufferTypes

#else

open FlatBuffers
open Iris.Serialization.Raft

#endif

type IrisError =
  | OK

  // GIT
  | BranchNotFound         of string
  | BranchDetailsNotFound  of string
  | RepositoryNotFound     of string
  | RepositoryInitFailed   of string
  | CommitError            of string
  | GitError               of string

  // PROJECT
  | ProjectNotFound        of string
  | ProjectParseError      of string
  | ProjectPathError
  | ProjectSaveError       of string

  // LITEDB
  | DatabaseCreateError    of string
  | DatabaseNotFound       of string
  | MetaDataNotFound

  // CLI
  | MissingStartupDir
  | CliParseError

  // Node
  | MissingNodeId
  | MissingNode            of string

  | AssetSaveError         of string
  | AssetDeleteError       of string

  | Other                  of string

  // RAFT
  | AlreadyVoted
  | AppendEntryFailed
  | CandidateUnknown
  | EntryInvalidated
  | InvalidCurrentIndex
  | InvalidLastLog
  | InvalidLastLogTerm
  | InvalidTerm
  | LogFormatError
  | LogIncomplete
  | NoError
  | NoNode
  | NotCandidate
  | NotLeader
  | NotVotingState
  | ResponseTimeout
  | SnapshotFormatError
  | StaleResponse
  | UnexpectedVotingChange
  | VoteTermMismatch

  with
    static member FromFB (fb: ErrorFB) =
      match fb.Type with
#if JAVASCRIPT
      | x when x = ErrorTypeFB.OKFB                     -> Some OK
      | x when x = ErrorTypeFB.BranchNotFoundFB         -> Some (BranchNotFound fb.Message)
      | x when x = ErrorTypeFB.BranchDetailsNotFoundFB  -> Some (BranchDetailsNotFound fb.Message)
      | x when x = ErrorTypeFB.RepositoryNotFoundFB     -> Some (RepositoryNotFound fb.Message)
      | x when x = ErrorTypeFB.RepositoryInitFailedFB   -> Some (RepositoryInitFailed fb.Message)
      | x when x = ErrorTypeFB.CommitErrorFB            -> Some (CommitError fb.Message)
      | x when x = ErrorTypeFB.GitErrorFB               -> Some (GitError fb.Message)
      | x when x = ErrorTypeFB.ProjectNotFoundFB        -> Some (ProjectNotFound fb.Message)
      | x when x = ErrorTypeFB.ProjectParseErrorFB      -> Some (ProjectParseError fb.Message)
      | x when x = ErrorTypeFB.ProjectPathErrorFB       -> Some ProjectPathError
      | x when x = ErrorTypeFB.ProjectSaveErrorFB       -> Some (ProjectSaveError fb.Message)
      | x when x = ErrorTypeFB.DatabaseCreateErrorFB    -> Some (DatabaseCreateError fb.Message)
      | x when x = ErrorTypeFB.DatabaseNotFoundFB       -> Some (DatabaseNotFound fb.Message)
      | x when x = ErrorTypeFB.MetaDataNotFoundFB       -> Some MetaDataNotFound
      | x when x = ErrorTypeFB.MissingStartupDirFB      -> Some MissingStartupDir
      | x when x = ErrorTypeFB.CliParseErrorFB          -> Some CliParseError
      | x when x = ErrorTypeFB.MissingNodeIdFB          -> Some MissingNodeId
      | x when x = ErrorTypeFB.MissingNodeFB            -> Some (MissingNode fb.Message)
      | x when x = ErrorTypeFB.AssetSaveErrorFB         -> Some (AssetSaveError fb.Message)
      | x when x = ErrorTypeFB.AssetDeleteErrorFB       -> Some (AssetDeleteError fb.Message)
      | x when x = ErrorTypeFB.OtherFB                  -> Some (Other fb.Message)
      | x when x = ErrorTypeFB.AlreadyVotedFB           -> Some AlreadyVoted
      | x when x = ErrorTypeFB.AppendEntryFailedFB      -> Some AppendEntryFailed
      | x when x = ErrorTypeFB.CandidateUnknownFB       -> Some CandidateUnknown
      | x when x = ErrorTypeFB.EntryInvalidatedFB       -> Some EntryInvalidated
      | x when x = ErrorTypeFB.InvalidCurrentIndexFB    -> Some InvalidCurrentIndex
      | x when x = ErrorTypeFB.InvalidLastLogFB         -> Some InvalidLastLog
      | x when x = ErrorTypeFB.InvalidLastLogTermFB     -> Some InvalidLastLogTerm
      | x when x = ErrorTypeFB.InvalidTermFB            -> Some InvalidTerm
      | x when x = ErrorTypeFB.LogFormatErrorFB         -> Some LogFormatError
      | x when x = ErrorTypeFB.LogIncompleteFB          -> Some LogIncomplete
      | x when x = ErrorTypeFB.NoErrorFB                -> Some NoError
      | x when x = ErrorTypeFB.NoNodeFB                 -> Some NoNode
      | x when x = ErrorTypeFB.NotCandidateFB           -> Some NotCandidate
      | x when x = ErrorTypeFB.NotLeaderFB              -> Some NotLeader
      | x when x = ErrorTypeFB.NotVotingStateFB         -> Some NotVotingState
      | x when x = ErrorTypeFB.ResponseTimeoutFB        -> Some ResponseTimeout
      | x when x = ErrorTypeFB.SnapshotFormatErrorFB    -> Some SnapshotFormatError
      | x when x = ErrorTypeFB.StaleResponseFB          -> Some StaleResponse
      | x when x = ErrorTypeFB.UnexpectedVotingChangeFB -> Some UnexpectedVotingChange
      | x when x = ErrorTypeFB.VoteTermMismatchFB       -> Some VoteTermMismatch
      | _                                    -> None
#else
      | ErrorTypeFB.OKFB                     -> Some OK
      | ErrorTypeFB.BranchNotFoundFB         -> Some (BranchNotFound fb.Message)
      | ErrorTypeFB.BranchDetailsNotFoundFB  -> Some (BranchDetailsNotFound fb.Message)
      | ErrorTypeFB.RepositoryNotFoundFB     -> Some (RepositoryNotFound fb.Message)
      | ErrorTypeFB.RepositoryInitFailedFB   -> Some (RepositoryInitFailed fb.Message)
      | ErrorTypeFB.CommitErrorFB            -> Some (CommitError fb.Message)
      | ErrorTypeFB.GitErrorFB               -> Some (GitError fb.Message)
      | ErrorTypeFB.ProjectNotFoundFB        -> Some (ProjectNotFound fb.Message)
      | ErrorTypeFB.ProjectParseErrorFB      -> Some (ProjectParseError fb.Message)
      | ErrorTypeFB.ProjectPathErrorFB       -> Some ProjectPathError
      | ErrorTypeFB.ProjectSaveErrorFB       -> Some (ProjectSaveError fb.Message)
      | ErrorTypeFB.DatabaseCreateErrorFB    -> Some (DatabaseCreateError fb.Message)
      | ErrorTypeFB.DatabaseNotFoundFB       -> Some (DatabaseNotFound fb.Message)
      | ErrorTypeFB.MetaDataNotFoundFB       -> Some MetaDataNotFound
      | ErrorTypeFB.MissingStartupDirFB      -> Some MissingStartupDir
      | ErrorTypeFB.CliParseErrorFB          -> Some CliParseError
      | ErrorTypeFB.MissingNodeIdFB          -> Some MissingNodeId
      | ErrorTypeFB.MissingNodeFB            -> Some (MissingNode fb.Message)
      | ErrorTypeFB.AssetSaveErrorFB         -> Some (AssetSaveError fb.Message)
      | ErrorTypeFB.AssetDeleteErrorFB       -> Some (AssetDeleteError fb.Message)
      | ErrorTypeFB.OtherFB                  -> Some (Other fb.Message)
      | ErrorTypeFB.AlreadyVotedFB           -> Some AlreadyVoted
      | ErrorTypeFB.AppendEntryFailedFB      -> Some AppendEntryFailed
      | ErrorTypeFB.CandidateUnknownFB       -> Some CandidateUnknown
      | ErrorTypeFB.EntryInvalidatedFB       -> Some EntryInvalidated
      | ErrorTypeFB.InvalidCurrentIndexFB    -> Some InvalidCurrentIndex
      | ErrorTypeFB.InvalidLastLogFB         -> Some InvalidLastLog
      | ErrorTypeFB.InvalidLastLogTermFB     -> Some InvalidLastLogTerm
      | ErrorTypeFB.InvalidTermFB            -> Some InvalidTerm
      | ErrorTypeFB.LogFormatErrorFB         -> Some LogFormatError
      | ErrorTypeFB.LogIncompleteFB          -> Some LogIncomplete
      | ErrorTypeFB.NoErrorFB                -> Some NoError
      | ErrorTypeFB.NoNodeFB                 -> Some NoNode
      | ErrorTypeFB.NotCandidateFB           -> Some NotCandidate
      | ErrorTypeFB.NotLeaderFB              -> Some NotLeader
      | ErrorTypeFB.NotVotingStateFB         -> Some NotVotingState
      | ErrorTypeFB.ResponseTimeoutFB        -> Some ResponseTimeout
      | ErrorTypeFB.SnapshotFormatErrorFB    -> Some SnapshotFormatError
      | ErrorTypeFB.StaleResponseFB          -> Some StaleResponse
      | ErrorTypeFB.UnexpectedVotingChangeFB -> Some UnexpectedVotingChange
      | ErrorTypeFB.VoteTermMismatchFB       -> Some VoteTermMismatch
      | _                                    -> None
#endif

    member error.ToOffset (builder: FlatBufferBuilder) =
      let tipe =
        match error with
        | OK                       -> ErrorTypeFB.OKFB
        | BranchNotFound         _ -> ErrorTypeFB.BranchNotFoundFB
        | BranchDetailsNotFound  _ -> ErrorTypeFB.BranchDetailsNotFoundFB
        | RepositoryNotFound     _ -> ErrorTypeFB.RepositoryNotFoundFB
        | RepositoryInitFailed   _ -> ErrorTypeFB.RepositoryInitFailedFB
        | CommitError            _ -> ErrorTypeFB.CommitErrorFB
        | GitError               _ -> ErrorTypeFB.GitErrorFB
        | ProjectNotFound        _ -> ErrorTypeFB.ProjectNotFoundFB
        | ProjectParseError      _ -> ErrorTypeFB.ProjectParseErrorFB
        | ProjectPathError         -> ErrorTypeFB.ProjectPathErrorFB
        | ProjectSaveError       _ -> ErrorTypeFB.ProjectSaveErrorFB
        | DatabaseCreateError    _ -> ErrorTypeFB.DatabaseCreateErrorFB
        | DatabaseNotFound       _ -> ErrorTypeFB.DatabaseNotFoundFB
        | MetaDataNotFound         -> ErrorTypeFB.MetaDataNotFoundFB
        | MissingStartupDir        -> ErrorTypeFB.MissingStartupDirFB
        | CliParseError            -> ErrorTypeFB.CliParseErrorFB
        | MissingNodeId            -> ErrorTypeFB.MissingNodeIdFB
        | MissingNode            _ -> ErrorTypeFB.MissingNodeFB
        | AssetSaveError         _ -> ErrorTypeFB.AssetSaveErrorFB
        | AssetDeleteError       _ -> ErrorTypeFB.AssetDeleteErrorFB
        | Other                  _ -> ErrorTypeFB.OtherFB

        | AlreadyVoted             -> ErrorTypeFB.AlreadyVotedFB
        | AppendEntryFailed        -> ErrorTypeFB.AppendEntryFailedFB
        | CandidateUnknown         -> ErrorTypeFB.CandidateUnknownFB
        | EntryInvalidated         -> ErrorTypeFB.EntryInvalidatedFB
        | InvalidCurrentIndex      -> ErrorTypeFB.InvalidCurrentIndexFB
        | InvalidLastLog           -> ErrorTypeFB.InvalidLastLogFB
        | InvalidLastLogTerm       -> ErrorTypeFB.InvalidLastLogTermFB
        | InvalidTerm              -> ErrorTypeFB.InvalidTermFB
        | LogFormatError           -> ErrorTypeFB.LogFormatErrorFB
        | LogIncomplete            -> ErrorTypeFB.LogIncompleteFB
        | NoError                  -> ErrorTypeFB.NoErrorFB
        | NoNode                   -> ErrorTypeFB.NoNodeFB
        | NotCandidate             -> ErrorTypeFB.NotCandidateFB
        | NotLeader                -> ErrorTypeFB.NotLeaderFB
        | NotVotingState           -> ErrorTypeFB.NotVotingStateFB
        | ResponseTimeout          -> ErrorTypeFB.ResponseTimeoutFB
        | SnapshotFormatError      -> ErrorTypeFB.SnapshotFormatErrorFB
        | StaleResponse            -> ErrorTypeFB.StaleResponseFB
        | UnexpectedVotingChange   -> ErrorTypeFB.UnexpectedVotingChangeFB
        | VoteTermMismatch         -> ErrorTypeFB.VoteTermMismatchFB

      let str =
        match error with
        | BranchNotFound         msg -> builder.CreateString msg |> Some
        | BranchDetailsNotFound  msg -> builder.CreateString msg |> Some
        | RepositoryNotFound     msg -> builder.CreateString msg |> Some
        | RepositoryInitFailed   msg -> builder.CreateString msg |> Some
        | CommitError            msg -> builder.CreateString msg |> Some
        | GitError               msg -> builder.CreateString msg |> Some
        | ProjectNotFound        msg -> builder.CreateString msg |> Some
        | ProjectParseError      msg -> builder.CreateString msg |> Some
        | ProjectSaveError       msg -> builder.CreateString msg |> Some
        | DatabaseCreateError    msg -> builder.CreateString msg |> Some
        | DatabaseNotFound       msg -> builder.CreateString msg |> Some
        | MissingNode            msg -> builder.CreateString msg |> Some
        | AssetSaveError         msg -> builder.CreateString msg |> Some
        | AssetDeleteError       msg -> builder.CreateString msg |> Some
        | Other                  msg -> builder.CreateString msg |> Some
        | _                          -> None

      ErrorFB.StartErrorFB(builder)
      ErrorFB.AddType(builder, tipe)
      match str with
      | Some payload -> ErrorFB.AddMessage(builder, payload)
      | _            -> ()
      ErrorFB.EndErrorFB(builder)

    member self.ToBytes() = Binary.buildBuffer self

    static member FromBytes(bytes: Binary.Buffer) =
      Binary.createBuffer bytes
      |> ErrorFB.GetRootAsErrorFB
      |> IrisError.FromFB

[<RequireQualifiedAccess>]
module Error =

  let inline toMessage (error: IrisError) =
    match error with
    | BranchNotFound        e -> sprintf "Branch does not exist: %s" e
    | BranchDetailsNotFound e -> sprintf "Branch details could not be found: %s" e
    | RepositoryNotFound    e -> sprintf "Repository was not found: %s" e
    | RepositoryInitFailed  e -> sprintf "Repository could not be initialized: %s" e
    | CommitError           e -> sprintf "Could not commit changes: %s" e
    | GitError              e -> sprintf "Git error: %s" e

    | ProjectNotFound       e -> sprintf "Project could not be found: %s" e
    | ProjectPathError        ->         "Project has no path"
    | ProjectSaveError      e -> sprintf "Project could not be saved: %s" e
    | ProjectParseError     e -> sprintf "Project could not be parsed: %s" e

    // LITEDB
    | DatabaseCreateError   e -> sprintf "Database could not be created: %s" e
    | DatabaseNotFound      e -> sprintf "Database could not be found: %s" e
    | MetaDataNotFound        -> sprintf "Metadata could not be loaded from db"

    // CLI
    | MissingStartupDir       ->         "Startup directory missing"
    | CliParseError           ->         "Command line parse error"

    | MissingNodeId           ->         "Node Id missing in environment"
    | MissingNode           e -> sprintf "Node with Id %s missing in Project configuration" e

    | AssetSaveError        e -> sprintf "Could not save asset to disk: %s" e
    | AssetDeleteError      e -> sprintf "Could not delete asset from disl: %s" e

    | Other                 e -> sprintf "Other error occurred: %s" (string e)

    // RAFT
    | AlreadyVoted            -> "Already voted"
    | AppendEntryFailed       -> "AppendEntry request has failed"
    | CandidateUnknown        -> "Election candidate not known to Raft"
    | EntryInvalidated        -> "Entry was invalidated"
    | InvalidCurrentIndex     -> "Invalid CurrentIndex"
    | InvalidLastLog          -> "Invalid last log"
    | InvalidLastLogTerm      -> "Invalid last log term"
    | InvalidTerm             -> "Invalid term"
    | LogFormatError          -> "Log format error"
    | LogIncomplete           -> "Log is incomplete"
    | NoError                 -> "No error"
    | NoNode                  -> "No node"
    | NotCandidate            -> "Not currently candidate"
    | NotLeader               -> "Not currently leader"
    | NotVotingState          -> "Not in voting state"
    | ResponseTimeout         -> "Response timeout"
    | SnapshotFormatError     -> "Snapshot was malformed"
    | StaleResponse           -> "Unsolicited response"
    | UnexpectedVotingChange  -> "Unexpected voting change"
    | VoteTermMismatch        -> "Vote term mismatch"

    | OK                      -> "All good."

  let inline toExitCode (error: IrisError) =
    match error with
    | OK                      -> 0
    | BranchNotFound        _ -> 1
    | BranchDetailsNotFound _ -> 2
    | RepositoryNotFound    _ -> 3
    | RepositoryInitFailed  _ -> 4
    | CommitError           _ -> 5
    | GitError              _ -> 6

    | ProjectNotFound       _ -> 7
    | ProjectPathError        -> 8
    | ProjectSaveError      _ -> 9
    | ProjectParseError     _ -> 10

    | MissingNodeId         _ -> 11
    | MissingNode           _ -> 12

    // LITEDB
    | DatabaseCreateError   _ -> 13
    | DatabaseNotFound      _ -> 14
    | MetaDataNotFound        -> 15

    // CLI
    | MissingStartupDir       -> 16
    | CliParseError           -> 17

    | AssetSaveError        _ -> 18
    | AssetDeleteError      _ -> 19

    | Other                 _ -> 20

    // RAFT
    | AlreadyVoted            -> 21
    | AppendEntryFailed       -> 22
    | CandidateUnknown        -> 23
    | EntryInvalidated        -> 24
    | InvalidCurrentIndex     -> 25
    | InvalidLastLog          -> 26
    | InvalidLastLogTerm      -> 27
    | InvalidTerm             -> 28
    | LogFormatError          -> 29
    | LogIncomplete           -> 30
    | NoError                 -> 31
    | NoNode                  -> 32
    | NotCandidate            -> 33
    | NotLeader               -> 34
    | NotVotingState          -> 35
    | ResponseTimeout         -> 36
    | SnapshotFormatError     -> 37
    | StaleResponse           -> 38
    | UnexpectedVotingChange  -> 39
    | VoteTermMismatch        -> 40


  let inline isOk (error: IrisError) =
    match error with
    | OK -> true
    | _  -> false

  let inline exitWith (error: IrisError) =
    if not (isOk error) then
      toMessage error
      |> printfn "Fatal: %s"
    error |> toExitCode |> exit