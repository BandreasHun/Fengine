module Main

open Browser.Dom
open Browser.Types
open Engine.GameLoop
open Engine.Input
open Engine.Sprite
open Engine.Scene
open Engine.Camera
open Engine.Runtime
open Engine.GetSpriteData
open Engine.Physics
open System
open Fable.Core.JsInterop

let mutable pepe : Sprite = Unchecked.defaultof<Sprite>
let mutable mainCam: Camera = Unchecked.defaultof<Camera>
let mutable bgSprite: Sprite = Unchecked.defaultof<Sprite>

let mutable nextPlatformX = 0.0
let rnd = System.Random()

let horizontalSpeed = 200.0
let jumpVelocity = - jumpImpulseStrength
let airtime = 2.0 * jumpVelocity / gravityAcceleration
let maxJumpDistance = horizontalSpeed * airtime
let platformHeight = 50.0
let textures = [| "sand.png"; "dirt.png" |]
let platformY = 40.0
let mutable nextDecorX = 0.0
let decoWidth, decoHeight = 384.0, 216.0
let decoY = 0.0
let mutable nextWaterX = 0.0
let waterWidth, waterHeight = 100.0, 100.0
let waterY = platformY + platformHeight

let mutable elapsedTime = 0.0
let mutable scoreEl: HTMLElement = Unchecked.defaultof<_>
let mutable gameOver = false

let update (deltaTimeMillis: float) =
    if gameOver then
        if IsDown " " then Browser.Dom.window.location.reload()
    else
        let dtSeconds = deltaTimeMillis / 1000.0
        elapsedTime <- elapsedTime + dtSeconds
        if not (isNull scoreEl) then
            scoreEl.textContent <- sprintf "%d" (int elapsedTime)

        let effectiveDtSeconds = min dtSeconds (1.0/30.0)

        pepe.velocityX <- horizontalSpeed

        if IsDown " " && pepe.isGrounded then
            applyImpulse pepe 0.0 jumpImpulseStrength
            pepe.isGrounded <- false

        applyGravity pepe effectiveDtSeconds
        updatePosition pepe effectiveDtSeconds

        if pepe.y > platformY + platformHeight then
            gameOver <- true
            scoreEl.textContent <- "You died! Press Space to restart"
        else
            items
            |> Seq.filter (fun s -> Array.contains s.texturePath textures && s.id <> pepe.id)
            |> Seq.iter (fun platform -> handlePlatformLanding pepe platform effectiveDtSeconds |> ignore)

            update mainCam deltaTimeMillis
            bgSprite.x <- mainCam.x
            bgSprite.y <- mainCam.y

            let viewRight = mainCam.x + float canvas.width * 3.0
            while nextPlatformX < viewRight do
                let width = rnd.NextDouble() * 200.0 + 200.0
                let minGap = pepe.width * pepe.scale
                let gap = minGap + rnd.NextDouble() * (maxJumpDistance - minGap)
                let texture = textures.[rnd.Next(textures.Length)]
                let platform = Engine.Sprite.create texture (nextPlatformX, platformY) (width, platformHeight)
                platform.affectedByGravity <- false
                nextPlatformX <- nextPlatformX + width + gap

let main () =
    scoreEl <- document.createElement("div") :?> HTMLElement
    scoreEl?style?position <- "absolute"
    scoreEl?style?top <- "10px"
    scoreEl?style?left <- "10px"
    scoreEl?style?fontSize <- "24px"
    scoreEl?style?fontWeight <- "bold"
    scoreEl?style?color <- "white"
    scoreEl?style?zIndex <- "1000"
    document.body.appendChild(scoreEl) |> ignore

    let rnd = System.Random()
    Browser.Dom.console.log(sprintf "Max jump distance: %f" maxJumpDistance)

    mainCam <- Engine.Camera.create 0.0 0.0 (float canvas.width) (float canvas.height)
    
    bgSprite <- Engine.Sprite.create "bg.png" (0.0, 0.0) (float canvas.width, float canvas.height)
    bgSprite.affectedByGravity <- false

    pepe <- Engine.Sprite.create "main.png" (0.0, platformY-130.0) (64.0, 64.0)
    pepe.scale <- 1.5
    pepe.affectedByGravity <- true 
    pepe.no <- 0
    pepe.isAnimating <- true
    
    setTarget mainCam pepe
    nextPlatformX <- pepe.x
    nextDecorX <- pepe.x
    
    let initialViewRight = pepe.x + float canvas.width * 3.0
    while nextPlatformX < initialViewRight do
        let width = rnd.NextDouble() * 200.0 + 200.0
        let minGap = pepe.width * pepe.scale
        let gap = minGap + rnd.NextDouble() * (maxJumpDistance - minGap)
        let texture = textures.[rnd.Next(textures.Length)]
        let platform = Engine.Sprite.create texture (nextPlatformX, platformY) (width, platformHeight)
        platform.affectedByGravity <- false
        nextPlatformX <- nextPlatformX + width + gap  

    onUpdate update













