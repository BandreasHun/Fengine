
export const gravityAcceleration = 980;

export const jumpImpulseStrength = -450;

export function applyImpulse(sprite, impulseX, impulseY) {
    sprite.velocityX = (sprite.velocityX + impulseX);
    sprite.velocityY = (sprite.velocityY + impulseY);
}

export function applyGravity(sprite, deltaTimeInSeconds) {
    if (sprite.affectedByGravity) {
        sprite.velocityY = (sprite.velocityY + (gravityAcceleration * deltaTimeInSeconds));
    }
}

export function updatePosition(sprite, deltaTimeInSeconds) {
    sprite.x = (sprite.x + (sprite.velocityX * deltaTimeInSeconds));
    sprite.y = (sprite.y + (sprite.velocityY * deltaTimeInSeconds));
}

export function checkCollision(sprite1, sprite2) {
    let rect1;
    const X = sprite1.x;
    const Y = sprite1.y;
    const Width = sprite1.width * sprite1.scale;
    rect1 = {
        Height: sprite1.height * sprite1.scale,
        Width: Width,
        X: X,
        Y: Y,
    };
    let rect2;
    const X_1 = sprite2.x;
    const Y_1 = sprite2.y;
    const Width_1 = sprite2.width * sprite2.scale;
    rect2 = {
        Height: sprite2.height * sprite2.scale,
        Width: Width_1,
        X: X_1,
        Y: Y_1,
    };
    if (((rect1.X < (rect2.X + rect2.Width)) && ((rect1.X + rect1.Width) > rect2.X)) && (rect1.Y < (rect2.Y + rect2.Height))) {
        return (rect1.Y + rect1.Height) > rect2.Y;
    }
    else {
        return false;
    }
}

export function handlePlatformLanding(sprite, platform, deltaTimeInSeconds) {
    if (checkCollision(sprite, platform)) {
        const spriteBottom = sprite.y + (sprite.height * sprite.scale);
        const platformTop = platform.y;
        if ((((sprite.velocityY >= 0) && (((sprite.y - (sprite.velocityY * deltaTimeInSeconds)) + (sprite.height * sprite.scale)) <= (platformTop + 5))) && (spriteBottom >= platformTop)) && (sprite.y < platformTop)) {
            sprite.y = (platform.y - (sprite.height * sprite.scale));
            sprite.velocityY = 0;
            sprite.isGrounded = true;
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}

