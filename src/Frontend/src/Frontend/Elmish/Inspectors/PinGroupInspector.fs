namespace Disco.Web.Inspectors

open System
open System.Collections.Generic
open Fable.Import
open Fable.Import.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.PowerPack
open Elmish.React
open Disco.Core
open Disco.Web.PinView
open Disco.Web.Core
open Disco.Web.Helpers
open Disco.Web.Types
open State

///  ____        _     _ _
/// |  _ \ _   _| |__ | (_) ___
/// | |_) | | | | '_ \| | |/ __|
/// |  __/| |_| | |_) | | | (__
/// |_|    \__,_|_.__/|_|_|\___|

module PinGroupInspector =
  let private buildPin dispatch (model: Model) (pinId, pin) =
    li [] [
      com<PinView,_,_> {
        key = string pinId
        pin = pin
        output = false
        selected = false
        slices = None
        model = model
        updater = None
        onSelect = fun _ -> Select.pin dispatch pin
        onDragStart = None
        dispatch = dispatch
      } []
    ]

  let private renderPins (tag: string) dispatch (model: Model) (group: PinGroup) =
    let pins =
      group.Pins
      |> Map.toList
      |> List.map (buildPin dispatch model)

    Common.row tag [
      ul [ Class "disco-graphview" ] pins
    ]

  let private renderClients tag dispatch model (group: PinGroup) =
    match model.state with
    | None -> Common.row tag []
    | Some state ->
      state.PinGroups
      |> PinGroupMap.findGroupBy (fun g -> g.Id = group.Id)
      |> Map.toList
      |> List.map
        (fun (clientId,group) ->
          match Map.tryFind clientId state.Clients with
          | Some client ->
            Common.link
              (string client.Name)
              (fun _ -> Select.client dispatch client)
          | None ->
            match ClientConfig.tryFind clientId state.Project.Config.Clients with
            | Some exe -> str (string exe.Id)
            | None -> str (string clientId + " (orphaned)"))
      |> Common.row tag

  let private renderRefersTo tag dispatch (model: Model) (group: PinGroup) =
    div [] []

  let render dispatch (model: Model) (client: ClientId) (group: PinGroupId) =
    match model.state with
    | None ->
      Common.render dispatch model "Pin Group" [
        str (string group + " (orphaned)")
      ]
    | Some state ->
      match PinGroupMap.tryFindGroup client group state.PinGroups with
      | None ->
        Common.render dispatch model "Pin Group" [
          str (string group + " (orphaned)")
        ]
      | Some group ->
        Common.render dispatch model "Pin Group" [
          Common.stringRow "Id"         (string group.Id)
          Common.stringRow "Name"       (string group.Name)
          Common.stringRow "Path"       (string group.Path)
          Common.stringRow "Asset Path" (string group.AssetPath)
          renderRefersTo   "Belongs To"  dispatch model group
          renderClients    "Clients"     dispatch model group
          renderPins       "Pins"        dispatch model group
        ]
