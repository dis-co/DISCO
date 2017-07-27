module Iris.Web.PanelLeft

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop
open Elmish.React
open Helpers
open State
open System

module Widgets =
    let [<Literal>] Log = "LOG"

let onClick dispatch name _ =
  match name with
  | Widgets.Log ->
    let id = Guid.NewGuid()
    let widget = Log.createLogWidget(id)
    AddWidget(id, widget) |> dispatch
  | _ -> printfn "Widget %s is not currently supported" name

let card dispatch name letter text =
    div [
        Key name
        Class "iris-panel-left-child"
        OnClick (onClick dispatch name)
    ] [
        div [] [str letter]
        div [] [
            p [] [strong [] [str name]]
            p [] [str text]
        ]
    ]

let render dispatch () =
    div [Class "iris-panel-left"] [
        card dispatch Widgets.Log        "L" "Cluster Settings"
        card dispatch "Graph View"       "G" "Cluster Settings"
        card dispatch "Cue Player"       "C" "Cluster Settings"
        card dispatch "Project View"     "P" "Cluster Settings"
        card dispatch "Test Widget"      "T" "Cluster Settings"
        card dispatch "Cluster"          "R" "Cluster Settings"
        card dispatch "Discovery"        "D" "Cluster Settings"
        card dispatch "Unassigned Hosts" "H" "Cluster Settings"
        card dispatch "Remotter"         "R" "Cluster Settings"
        card dispatch "Project Settings" "S" "Cluster Settings"
        card dispatch "Library"          "L" "Graph View"
        card dispatch "Project Overview (Big)" "P" "Cluster Settings"
    ]

let view dispatch () =
  lazyViewWith
    (fun x y -> obj.ReferenceEquals(x, y))
    (fun () -> render dispatch ())
    ()
