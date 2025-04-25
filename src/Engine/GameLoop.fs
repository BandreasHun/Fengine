namespace Engine

module GameLoop =
    open Browser.Dom
    
    open Engine.Sprite
    open Browser.Types
    let mutable private lastFrameTime = 0.0
    let mutable private updateCallback: (float -> unit) option = None
    let mutable private drawCallback: (float -> unit) option = None
    let mutable private frameRequestId = 0.0
    
   
    

    

    /// Register the update function (receives delta time in ms)
    let onUpdate f =
        updateCallback <- Some f

    /// Register the draw function (receives delta time in ms)
    let onDraw f =
        
        drawCallback <- Some f

    /// Start the game loop (calls update then draw each frame)
    let start () =
        
        console.log("Starting game loop")
        // clear the canvas
        let rec loop (timestamp: float) =  // timestamp is in ms since page load
            let deltaTime = timestamp - lastFrameTime // deltaTime in ms since last frame
            lastFrameTime <- timestamp // update the last frame time
            updateCallback |> Option.iter (fun f -> f deltaTime) // call the update function
            drawCallback |> Option.iter (fun f -> f deltaTime) // call the draw function
            frameRequestId <- window.requestAnimationFrame(loop) // record the frame ID for potential cancellation
        frameRequestId <- window.requestAnimationFrame(loop) // start and record first frame

    /// Stop the game loop
    let stop () =
        window.cancelAnimationFrame(frameRequestId)
        frameRequestId <- 0.0