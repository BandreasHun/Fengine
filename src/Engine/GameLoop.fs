namespace Engine

module GameLoop =
    open Browser.Dom
    
    open Engine.Sprite
    open Browser.Types
    let mutable private lastFrameTime = 0.0
    let mutable private updateCallback: (float -> unit) option = None
    let mutable private drawCallback: (float -> unit) option = None
    let mutable private animCallback: (float -> unit) option = None
    let mutable private frameRequestId = 0.0

    let onUpdate f =
        updateCallback <- Some f

    let onDraw f =
        drawCallback <- Some f

    let onAnimate f =
        animCallback <- Some f

    let start () =
        
        let rec loop (timestamp: float) =
            let deltaTime = timestamp - lastFrameTime
            lastFrameTime <- timestamp
            updateCallback |> Option.iter (fun f -> f deltaTime)
            animCallback   |> Option.iter (fun f -> f deltaTime)
            drawCallback   |> Option.iter (fun f -> f deltaTime)
            frameRequestId <- window.requestAnimationFrame(loop)
        frameRequestId <- window.requestAnimationFrame(loop)

    let stop () =
        window.cancelAnimationFrame(frameRequestId)
        frameRequestId <- 0.0