namespace Engine
module Entry =

    open Browser.Dom
    open Fable.Core.JsInterop
    open Engine.Sprite 
    open Engine.GameLoop
    open Main 
    open Engine.Runtime
    open Engine.Input
    open Engine.GetSpriteData
    open Engine.Scene

    let mutable alreadyStarted = false

    let init() =
        if not alreadyStarted then
            alreadyStarted <- true
            
            Input.Init() 
            
            Main.main() 
            
            GameLoop.onUpdate Main.update 
            GameLoop.onDraw (fun dt -> Scene.draw Main.mainCam dt) 
            GameLoop.onAnimate (fun dt -> Scene.updateAnimations (dt / 1000.0))
            
            GameLoop.start()

    Async.StartWithContinuations(
        Engine.GetSpriteData.loadSpriteAtlas(),
        (fun _ -> init()),
        (fun ex ->
            Browser.Dom.console.error("Failed to load sprite atlas in entry.fs (GetSpriteData):", ex)
            ()), 
        (fun _ -> ()) 
    )