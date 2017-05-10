module rec Iris.Web.Widgets.CuePlayer

open Iris.Core
open Iris.Web.Core
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Helpers

importAll "../../../css/cuePlayer.css"

module private Helpers =
  type RCom = React.ComponentClass<obj>
  let Clock: RCom = importDefault "../../../src/widgets/Clock"
  let SpreadView: RCom = importMember "../../../src/widgets/Spread"

  let inline Class x = ClassName x
  let inline (~%) x = createObj x


type Layout =
  {
    x: int; y: int;
    w: int; h: int;
    minW: int; maxW: int;
    minH: int; maxH: int;
  }

type ISpread =
  abstract member pin: Pin
  abstract member rows: (string*obj)[]

let updateSlices(pin: Pin, rowIndex, newValue: obj) =
  let updateArray (i: int) (v: obj) (ar: 'T[]) =
    let newArray = Array.copy ar
    newArray.[i] <- unbox v
    newArray
  match pin with
  | StringPin pin ->
    StringSlices(pin.Id, updateArray rowIndex newValue pin.Values)
  | NumberPin pin ->
    let newValue =
      match newValue with
      | :? string as v -> box(double v)
      | v -> v
    NumberSlices(pin.Id, updateArray rowIndex newValue pin.Values)
  | BoolPin pin ->
    let newValue =
      match newValue with
      | :? string as v -> box(v.ToLower() = "true")
      | v -> v
    BoolSlices(pin.Id, updateArray rowIndex newValue pin.Values)
  | BytePin   _pin -> failwith "TO BE IMPLEMENTED"
  | EnumPin   _pin -> failwith "TO BE IMPLEMENTED"
  | ColorPin  _pin -> failwith "TO BE IMPLEMENTED"
  |> UpdateSlices |> ClientContext.Singleton.Post

type Cue(spread: ISpread) =
  member val Pin = spread.pin
  member val Rows = spread.rows
  member val Open = false with get, set
  member val UpdateView = true

  member this.Update(rowIndex, newValue) =
    let oldValue = this.Rows.[rowIndex]
    this.Rows.[rowIndex] <- fst oldValue, newValue

  member this.UpdateSource() =
    for i = 0 to this.Rows.Length do
      updateSlices(this.Pin, i, snd this.Rows.[i])

type CuePlayer() =
  member val view = typeof<CuePlayerView>
  member val name = "Cue List"
  member val cues = ResizeArray()
  member val layout =
    {
      x = 0; y = 0;
      w = 8; h = 5;
      minW = 2; maxW = 10;
      minH = 1; maxH = 10;
    }

[<Pojo>]
type CuePlayerProps =
  { id: int;
    model: CuePlayer
    globalModel: GlobalModel }

type CuePlayerView(props) =
    inherit React.Component<CuePlayerProps, obj>(props)
    let mutable el = Unchecked.defaultof<_>

    // member this.render() =
    //   let header =
    //     div [Class "level"]
    //       [div [Class "level-left"]
    //         [button [Class "button level-item"
    //                  Style [Margin 5]
    //                  OnClick(fun ev -> this.UpdateSource())]
    //           [str "Fire!"]
    //         ]
    //       ;div [Class "level-right"]
    //         [div [Class "level-item"]
    //           [from Clock %["global"==>this.props.globalModel] []]
    //         ]
    //     ]
    //   let rows =
    //     this.props.model.cues
    //     |> Seq.mapi (fun i cue ->
    //       let foo = from SpreadView %["model"==>cue; "global"==>this.props.globalModel] []
    //       div [Key (string i)] [from SpreadView %["model"==>cue; "global"==>this.props.globalModel] []])
    //     |> Seq.toList
    //   // Return value
    //   div [Class "iris-cuelist"; Ref(fun el' -> el <- el')] (header::rows)

    member this.renderCue() =
      div [Class "cueplayer-list-header cueplayer-cue"] [
        ul [Class "cueplayer-ul"] [
          li [] [span [Class "iris-icon iris-icon-caret-down-two"] []]
          li [] [
            div [Class "cueplayer-button iris-icon cueplayer-player"] [
              span [Class "iris-icon iris-icon-play"] []
            ]
          ]
          li [] [
            form [] [
              input [
                Class "cueplayer-cueDesc"
                Type "text"
                Value !^"0000"
                Name "firstname"
              ]
              br []
            ]
          ]
          li [Id "cueName"] [
            form [] [
              input [
                Class "cueplayer-cueDesc"
                Type "text"
                Value !^"Untitled"
                Name "firstname"
              ]
              br []
            ]
          ]
          li [] []
          form [] [
            input [
              Class "cueplayer-cueDesc"
              // !!("style", "width:60px; margin-right:5px;")
              Type "text"
              Value !^"00:00:00"
              Name "firstname"
            ]
            br []
          ]
          li [] [
            div [Class "cueplayer-button iris-icon"] [
              span [Class "iris-icon iris-icon-duplicate"] []
            ]
            div [Class "cueplayer-button iris-icon cueplayer-close"] [
              span [Class "iris-icon iris-icon-close"] []
            ]
          ]
        ]
      ]

    member this.render() =
      div [] [
        // HEADER
        div [Class "cueplayer-list-header"] [
          div [Class "cueplayer-button cueplayer-go"] [
            span [Class "iris-icon"; !!("data-icon", "c")] [str "GO"]
          ]
          div [Class "cueplayer-button iris-icon"] [
            span [Class "iris-icon iris-icon-fast-backward"] []
          ]
          div [Class "cueplayer-button iris-icon"] [
            span [Class "iris-icon iris-icon-fast-forward"] []
          ]
          div [Class "cueplayer-button"] [str "Add Cue"]
          div [Class "cueplayer-button"] [str "Add Group"]
          div [
            // !!("style", "clear:both;")
              ] []
        ]
        // CUE
        this.renderCue()
      ]
