module rec Iris.Web.State

open System
open Iris.Core
open Fable.Core
open Fable.Import
open Elmish

type Msg =
  | AddWidget of Guid * IWidget
  | RemoveWidget of Guid
  // | AddTab
  | AddLog of LogEvent
  | UpdateLogConfig of LogConfig

type Direction =
  | Ascending
  | Descending
  member this.Reverse =
    match this with
    | Ascending -> Descending
    | Descending -> Ascending

type Sorting =
  { column: string
    direction: Direction
  }

type LogConfig =
  { filter: string option
    logLevel: LogLevel option
    setLogLevel: LogLevel
    sorting: Sorting option
    columns: Map<string, bool>
    viewLogs: LogEvent array
  }
  static member Create(logs: LogEvent list) =
    { filter = None
      logLevel = None
      // TODO: This should be read from backend
      setLogLevel = LogLevel.Debug
      sorting = None
      columns =
        Map["LogLevel", true
            "Time", true
            "Tag", true
            "Tier", true]
      viewLogs = Array.ofList logs }

type [<Pojo>] Layout =
  { i: Guid; ``static``: bool
    x: int; y: int
    w: int; h: int
    minW: int; maxW: int
    minH: int; maxH: int }

type IWidget =
  abstract InitialLayout: Layout
  abstract Render: Guid * Dispatch<Msg> * Model -> React.ReactElement

type Model =
  { widgets: Map<Guid,IWidget>
    logs: LogEvent list
    logConfig: LogConfig
  }

let init() =
  let logs = List.init 50 (fun _ -> Core.MockData.genLog())
  let initModel =
    { widgets = Map.empty
      logs = logs
      logConfig = LogConfig.Create(logs)
    }
  initModel, []

let update msg model =
  let newModel =
    match msg with
    | AddWidget(id, widget) ->
      { model with widgets = Map.add id widget model.widgets }
    | RemoveWidget id ->
      { model with widgets = Map.remove id model.widgets }
    // | AddTab -> // Add tab and remove widget
    // | RemoveTab -> // Optional, add widget
    | AddLog log ->
      { model with logs = log::model.logs }
    | UpdateLogConfig cfg ->
      { model with logConfig = cfg }
  newModel, []
