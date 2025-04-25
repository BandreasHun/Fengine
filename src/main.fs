module Engine.Main

open Browser.Dom
open Browser.Types
open Engine.GameLoop
open Engine.Input
open Engine.Sprite
open Engine.Scene

let update (dt: float) =
    let sprite = Scene.items.[0] |> Some
    sprite |> Option.iter (fun s ->
        console.log("Sprite position: ", s.x, s.y)
        if Engine.Input.IsDown "ArrowLeft" then s.x <- s.x - 1.0 * dt
        if Engine.Input.IsDown "ArrowRight" then s.x <- s.x + 1.0 * dt
        if Engine.Input.IsDown "ArrowUp"    then s.y <- s.y - 1.0 * dt
        if Engine.Input.IsDown "ArrowDown"  then s.y <- s.y + 1.0 * dt
    )


let main() =
    console.log("Main function called")
    Scene.addSprite (Sprite.create "test.png" (0.0, 0.0) (1024, 1024.0))
    
    










