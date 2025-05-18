module Engine.Physics

open Engine.Sprite

let gravityAcceleration = 980.0
let jumpImpulseStrength = -450.0

let applyImpulse (sprite: Sprite) (impulseX: float) (impulseY: float) =
    sprite.velocityX <- sprite.velocityX + impulseX
    sprite.velocityY <- sprite.velocityY + impulseY

let applyGravity (sprite: Sprite) (deltaTimeInSeconds: float) =
    if sprite.affectedByGravity then
        sprite.velocityY <- sprite.velocityY + gravityAcceleration * deltaTimeInSeconds

let updatePosition (sprite: Sprite) (deltaTimeInSeconds: float) =
    sprite.x <- sprite.x + sprite.velocityX * deltaTimeInSeconds
    sprite.y <- sprite.y + sprite.velocityY * deltaTimeInSeconds

let checkCollision (sprite1: Sprite) (sprite2: Sprite) =
    let rect1 = {| X = sprite1.x; Y = sprite1.y; Width = sprite1.width * sprite1.scale; Height = sprite1.height * sprite1.scale |}
    let rect2 = {| X = sprite2.x; Y = sprite2.y; Width = sprite2.width * sprite2.scale; Height = sprite2.height * sprite2.scale |}

    rect1.X < rect2.X + rect2.Width &&
    rect1.X + rect1.Width > rect2.X &&
    rect1.Y < rect2.Y + rect2.Height &&
    rect1.Y + rect1.Height > rect2.Y

let handlePlatformLanding (sprite: Sprite) (platform: Sprite) (deltaTimeInSeconds: float) : bool =
    if checkCollision sprite platform then
        let spriteBottom = sprite.y + sprite.height * sprite.scale
        let platformTop = platform.y
        
        let movingDown = sprite.velocityY >= 0.0 
        
        let prevSpriteBottomApprox = sprite.y - sprite.velocityY * deltaTimeInSeconds + sprite.height * sprite.scale
        
        let tolerance = 5.0

        if movingDown && prevSpriteBottomApprox <= platformTop + tolerance && spriteBottom >= platformTop && sprite.y < platformTop then
            sprite.y <- platform.y - sprite.height * sprite.scale
            sprite.velocityY <- 0.0
            sprite.isGrounded <- true
            true
        else
            false
    else
        false
