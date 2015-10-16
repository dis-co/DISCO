[<ReflectedDefinition>]
module Iris.Web.Core.IOBox

open FunScript
open FunScript.TypeScript

(*   ___ ___  ____            
    |_ _/ _ \| __ )  _____  __
     | | | | |  _ \ / _ \ \/ /
     | | |_| | |_) | (_) >  < 
    |___\___/|____/ \___/_/\_\

*)

[<NoEquality; NoComparison>]
type Slice = { idx : int; value : string; }

[<NoEquality; NoComparison>]
type IOBox =
  { id      : string
  ; name    : string
  ; patch   : string
  ; kind    : string
  ; slices  : Slice array
  }