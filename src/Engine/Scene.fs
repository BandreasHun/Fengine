module Engine.Scene

open Browser.Dom
open Browser.Types
open Engine.Sprite
open Engine.Runtime
open System.Collections.Generic







let public items = ResizeArray<Sprite>()
let gl = Runtime.gl


let mutable private renderer : Renderer option = None


let addSprite (spr: Sprite) =
    
    renderer <- Some (Sprite.init gl spr)
    items.Add spr
 

let draw (_dt: float) =
    gl.clearColor(0.1, 0.1, 0.1, 1.0)
    gl.clear gl.COLOR_BUFFER_BIT
    match renderer with
    | Some r ->
        for spr in items do
            Sprite.draw gl r spr
    | None -> ()




