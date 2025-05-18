module Engine.Camera

open Engine.Sprite

type Camera = {
    mutable x: float
    mutable y: float
    mutable viewportWidth: float
    mutable viewportHeight: float
    mutable zoom: float
    mutable target: Sprite option
}

let create (initialX: float) (initialY: float) (vWidth: float) (vHeight: float) =
    { x = initialX; y = initialY; viewportWidth = vWidth; viewportHeight = vHeight; zoom = 1.0; target = None }

let update (camera: Camera) (_dt: float) =
    match camera.target with
    | Some spriteToFollow ->
        let targetCenterX = spriteToFollow.x + spriteToFollow.width * spriteToFollow.scale / 2.0
        let targetCenterY = spriteToFollow.y + spriteToFollow.height * spriteToFollow.scale / 2.0

        camera.x <- targetCenterX - (camera.viewportWidth / camera.zoom / 2.0)
        camera.y <- targetCenterY - (camera.viewportHeight / camera.zoom / 2.0)
    | None -> ()

let setTarget (camera: Camera) (sprite: Sprite) =
    camera.target <- Some sprite

let clearTarget (camera: Camera) =
    camera.target <- None

let setZoom (camera: Camera) (level: float) =
    camera.zoom <- if level > 0.0 then level else 0.01

let setViewportSize (camera: Camera) (vWidth: float) (vHeight: float) =
    camera.viewportWidth <- vWidth
    camera.viewportHeight <- vHeight

