namespace rec Iris.Core

// * Imports

#if FABLE_COMPILER

open Fable.Core
open Fable.Import
open Iris.Core.FlatBuffers
open Iris.Web.Core.FlatBufferTypes

#else

open System.IO
open FlatBuffers
open Iris.Serialization

#endif

// * CueGroupYaml

open System
open Path

#if !FABLE_COMPILER && !IRIS_NODES

open SharpYaml
open SharpYaml.Serialization

type CueGroupYaml() =
  [<DefaultValue>] val mutable Id: string
  [<DefaultValue>] val mutable Name: string
  [<DefaultValue>] val mutable CueRefs: CueReferenceYaml array

  // ** From

  static member From(cueGroup: CueGroup) =
    let yaml = CueGroupYaml()
    yaml.Id <- string cueGroup.Id
    yaml.Name <- cueGroup.Name |> Option.map unwrap |> Option.defaultValue null
    yaml.CueRefs <- Array.map Yaml.toYaml cueGroup.CueRefs
    yaml

  // ** ToCueGroup

  member yaml.ToCueGroup() =
    either {
      let! id = IrisId.TryParse yaml.Id
      let! cues = Either.bindArray Yaml.fromYaml yaml.CueRefs
      let name =
        if System.String.IsNullOrWhiteSpace yaml.Name
        then None
        else Some (name yaml.Name)
      return {
        Id = id
        Name = name
        CueRefs = cues
      }
    }

#endif

// * CueGroup

[<StructuralEquality; StructuralComparison>]
type CueGroup =
  { Id:      CueGroupId
    Name:    Name option
    CueRefs: CueReference array }

  // ** FromFB

  //  ____  _
  // | __ )(_)_ __   __ _ _ __ _   _
  // |  _ \| | '_ \ / _` | '__| | | |
  // | |_) | | | | | (_| | |  | |_| |
  // |____/|_|_| |_|\__,_|_|   \__, |
  //                           |___/

  static member FromFB(fb: CueGroupFB) : Either<IrisError,CueGroup> =
    either {
      let! cues =
        EitherExt.bindGeneratorToArray
          "CueGroup.FromFB"
          fb.CueRefsLength
          fb.CueRefs
          CueReference.FromFB
      let! id = Id.decodeId fb
      let name =
        match fb.Name with
        | null -> None
        | str -> Some (name str)
      return {
        Id = id
        Name = name
        CueRefs = cues
      }
    }

  // ** ToOffset

  member self.ToOffset(builder: FlatBufferBuilder) : Offset<CueGroupFB> =
    let id = CueGroupFB.CreateIdVector(builder,self.Id.ToByteArray())
    let name = self.Name |> Option.map (unwrap >> builder.CreateString)
    let cueoffsets = Array.map (Binary.toOffset builder) self.CueRefs
    let cuesvec = CueGroupFB.CreateCueRefsVector(builder, cueoffsets)
    CueGroupFB.StartCueGroupFB(builder)
    CueGroupFB.AddId(builder, id)
    Option.iter (fun value -> CueGroupFB.AddName(builder,value)) name
    CueGroupFB.AddCueRefs(builder, cuesvec)
    CueGroupFB.EndCueGroupFB(builder)

  // ** FromBytes

  static member FromBytes(bytes: byte[]) : Either<IrisError,CueGroup> =
    bytes
    |> Binary.createBuffer
    |> CueGroupFB.GetRootAsCueGroupFB
    |> CueGroup.FromFB

  // ** ToBytes

  member self.ToBytes() = Binary.buildBuffer self

  // ** ToYaml

  // __   __              _
  // \ \ / /_ _ _ __ ___ | |
  //  \ V / _` | '_ ` _ \| |
  //   | | (_| | | | | | | |
  //   |_|\__,_|_| |_| |_|_|

  #if !FABLE_COMPILER && !IRIS_NODES

  member cue.ToYaml() = CueGroupYaml.From(cue)

  // ** FromYaml

  static member FromYaml(yaml: CueGroupYaml) : Either<IrisError,CueGroup> =
    yaml.ToCueGroup()

  #endif

// * CueGroup module

module CueGroup =

  // ** create

  let create refs =
    { Id = IrisId.Create()
      Name = None
      CueRefs = refs }

  // ** filter

  let filter (f: CueGroup -> bool) (groups: CueGroup array) =
    Array.filter f groups

  // ** contains

  let contains (cueId: CueId) (group: CueGroup) =
    Array.fold
      (fun result ref ->
        if not result
        then ref.CueId = cueId
        else result)
      false
      group.CueRefs

  // ** insertAfter

  let insertAfter (idx:int) (item:CueReference) cueGroup =
    { cueGroup with CueRefs = Array.insertAfter idx item cueGroup.CueRefs }
