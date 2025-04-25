namespace Engine
module Entry =

    open Browser.Dom
    open Fable.Core.JsInterop
    open Sprite
    open GameLoop
    open Main
    open Runtime
    open Input
    open GetSpriteData

    let init() =
        Input.Init()
        GameLoop.onUpdate update
        GameLoop.onDraw Scene.draw
        GameLoop.start()
        Main.main()

    
    Async.StartWithContinuations(
        loadSpriteAtlas(),
        (fun _ ->
            console.log "Sprite atlas betöltve."
            init()),
        (fun ex ->
            console.error("Hiba a sprite atlas betöltésekor:", ex)),
        (fun _ -> ())
    )