import { stringHash, defaultOf, createAtom } from "../fable_modules/fable-library-js.4.24.0/Util.js";
import { nonSeeded } from "../fable_modules/fable-library-js.4.24.0/Random.js";
import { handlePlatformLanding, updatePosition, applyGravity, applyImpulse, gravityAcceleration, jumpImpulseStrength } from "./Engine/Physics.fs.js";
import { IsDown } from "./Engine/Input.fs.js";
import { printf, toText } from "../fable_modules/fable-library-js.4.24.0/String.js";
import { min } from "../fable_modules/fable-library-js.4.24.0/Double.js";
import { filter, iterate } from "../fable_modules/fable-library-js.4.24.0/Seq.js";
import { item, contains } from "../fable_modules/fable-library-js.4.24.0/Array.js";
import { create, items } from "./Engine/Sprite.fs.js";
import { setTarget, create as create_1, update as update_1 } from "./Engine/Camera.fs.js";
import { canvas } from "./Engine/Runtime.fs.js";
import { some } from "../fable_modules/fable-library-js.4.24.0/Option.js";
import { onUpdate } from "./Engine/GameLoop.fs.js";

export let pepe = createAtom(defaultOf());

export let mainCam = createAtom(defaultOf());

export let bgSprite = createAtom(defaultOf());

export let nextPlatformX = createAtom(0);

export const rnd = nonSeeded();

export const horizontalSpeed = 200;

export const jumpVelocity = -jumpImpulseStrength;

export const airtime = (2 * jumpVelocity) / gravityAcceleration;

export const maxJumpDistance = horizontalSpeed * airtime;

export const platformHeight = 50;

export const textures = ["sand.png", "dirt.png"];

export const platformY = 40;

export let nextDecorX = createAtom(0);

export const patternInput$004031 = [384, 216];

export const decoWidth = patternInput$004031[0];

export const decoHeight = patternInput$004031[1];

export const decoY = 0;

export let nextWaterX = createAtom(0);

export const patternInput$004034$002D1 = [100, 100];

export const waterWidth = patternInput$004034$002D1[0];

export const waterHeight = patternInput$004034$002D1[1];

export const waterY = platformY + platformHeight;

export let elapsedTime = createAtom(0);

export let scoreEl = createAtom(defaultOf());

export let gameOver = createAtom(false);

export function update(deltaTimeMillis) {
    let arg;
    if (gameOver()) {
        if (IsDown(" ")) {
            window.location.reload();
        }
    }
    else {
        const dtSeconds = deltaTimeMillis / 1000;
        elapsedTime(elapsedTime() + dtSeconds);
        if (!(scoreEl() == null)) {
            scoreEl().textContent = ((arg = (~~elapsedTime() | 0), toText(printf("%d"))(arg)));
        }
        const effectiveDtSeconds = min(dtSeconds, 1 / 30);
        pepe().velocityX = horizontalSpeed;
        if (IsDown(" ") && pepe().isGrounded) {
            applyImpulse(pepe(), 0, jumpImpulseStrength);
            pepe().isGrounded = false;
        }
        applyGravity(pepe(), effectiveDtSeconds);
        updatePosition(pepe(), effectiveDtSeconds);
        if (pepe().y > (platformY + platformHeight)) {
            gameOver(true);
            scoreEl().textContent = "You died! Press Space to restart";
        }
        else {
            iterate((platform) => {
                handlePlatformLanding(pepe(), platform, effectiveDtSeconds);
            }, filter((s) => {
                if (contains(s.texturePath, textures, {
                    Equals: (x, y) => (x === y),
                    GetHashCode: stringHash,
                })) {
                    return s.id !== pepe().id;
                }
                else {
                    return false;
                }
            }, items));
            update_1(mainCam(), deltaTimeMillis);
            bgSprite().x = mainCam().x;
            bgSprite().y = mainCam().y;
            const viewRight = mainCam().x + (canvas.width * 3);
            while (nextPlatformX() < viewRight) {
                const width = (rnd.NextDouble() * 200) + 200;
                const minGap = pepe().width * pepe().scale;
                const gap = minGap + (rnd.NextDouble() * (maxJumpDistance - minGap));
                const platform_1 = create(item(rnd.Next1(textures.length), textures), nextPlatformX(), platformY, width, platformHeight);
                platform_1.affectedByGravity = false;
                nextPlatformX((nextPlatformX() + width) + gap);
            }
        }
    }
}

export function main() {
    scoreEl(document.createElement("div"));
    scoreEl().style.position = "absolute";
    scoreEl().style.top = "10px";
    scoreEl().style.left = "10px";
    scoreEl().style.fontSize = "24px";
    scoreEl().style.fontWeight = "bold";
    scoreEl().style.color = "white";
    scoreEl().style.zIndex = "1000";
    document.body.appendChild(scoreEl());
    const rnd_1 = nonSeeded();
    console.log(some(toText(printf("Max jump distance: %f"))(maxJumpDistance)));
    mainCam(create_1(0, 0, canvas.width, canvas.height));
    bgSprite(create("bg.png", 0, 0, canvas.width, canvas.height));
    bgSprite().affectedByGravity = false;
    pepe(create("main.png", 0, platformY - 130, 64, 64));
    pepe().scale = 1.5;
    pepe().affectedByGravity = true;
    pepe().no = 0;
    pepe().isAnimating = true;
    setTarget(mainCam(), pepe());
    nextPlatformX(pepe().x);
    nextDecorX(pepe().x);
    const initialViewRight = pepe().x + (canvas.width * 3);
    while (nextPlatformX() < initialViewRight) {
        const width = (rnd_1.NextDouble() * 200) + 200;
        const minGap = pepe().width * pepe().scale;
        const gap = minGap + (rnd_1.NextDouble() * (maxJumpDistance - minGap));
        const platform = create(item(rnd_1.Next1(textures.length), textures), nextPlatformX(), platformY, width, platformHeight);
        platform.affectedByGravity = false;
        nextPlatformX((nextPlatformX() + width) + gap);
    }
    onUpdate((deltaTimeMillis) => {
        update(deltaTimeMillis);
    });
}

