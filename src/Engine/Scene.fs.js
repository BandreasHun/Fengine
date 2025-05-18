import { gl as gl_1 } from "./Runtime.fs.js";
import { draw as draw_1, items, init, Sprite, createTextureForImage } from "./Sprite.fs.js";
import { defaultArg } from "../../fable_modules/fable-library-js.4.24.0/Option.js";
import { tryHead } from "../../fable_modules/fable-library-js.4.24.0/Seq.js";
import { disposeSafe, getEnumerator } from "../../fable_modules/fable-library-js.4.24.0/Util.js";
import { tryGetValue } from "../../fable_modules/fable-library-js.4.24.0/MapUtil.js";
import { FSharpRef } from "../../fable_modules/fable-library-js.4.24.0/Types.js";

export const gl = gl_1;

let renderer = undefined;

const animElapsed = new Map([]);

let defaultAnimationFrameDuration = 0.2;

let backgroundColor = [0.1, 0.1, 0.1, 1];

const placeholderSprite = new Sprite(0, document.createElement("img"), createTextureForImage(document.createElement("img")), "", 0, 0, 0, 0, false, false, 1, 1, 1, 0, [], false, false, false, 0);

export function setBackgroundColor(r, g, b, a) {
    backgroundColor = [r, g, b, a];
}

export function setDefaultAnimationFrameDuration(duration) {
    defaultAnimationFrameDuration = duration;
}

export function ensureRenderer() {
    if (renderer == null) {
        renderer = init(gl, defaultArg(tryHead(items), placeholderSprite));
    }
}

export function updateAnimations(dt) {
    let enumerator = getEnumerator(items);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const spr = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            if (spr.animation && spr.isAnimating) {
                if (spr.texturePath !== "pepe.png") {
                    const frameCount = spr.vertices.length | 0;
                    if (frameCount > 0) {
                        const frameDuration = defaultAnimationFrameDuration;
                        let elapsed = 0;
                        if (!tryGetValue(animElapsed, spr.id, new FSharpRef(() => elapsed, (v) => {
                            elapsed = v;
                        }))) {
                            elapsed = 0;
                        }
                        elapsed = (elapsed + dt);
                        if (elapsed >= frameDuration) {
                            const framesToAdvance = Math.floor(elapsed / frameDuration);
                            spr.no = (((spr.no + ~~framesToAdvance) % frameCount) | 0);
                            animElapsed.set(spr.id, elapsed - (framesToAdvance * frameDuration));
                        }
                        else {
                            animElapsed.set(spr.id, elapsed);
                        }
                    }
                }
            }
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}

export function draw(camera, _dt) {
    ensureRenderer();
    const r = backgroundColor[0];
    const g = backgroundColor[1];
    const b = backgroundColor[2];
    const a = backgroundColor[3];
    gl.clearColor(r, g, b, a);
    gl.clear(gl.COLOR_BUFFER_BIT);
    if (renderer == null) {
    }
    else {
        const currentRenderer = renderer;
        const camViewXStart = camera.x;
        const camViewYStart = camera.y;
        const camViewXEnd = camera.x + (camera.viewportWidth / camera.zoom);
        const camViewYEnd = camera.y + (camera.viewportHeight / camera.zoom);
        let enumerator = getEnumerator(items);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const spr = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                const spriteXStart = spr.x;
                const spriteYStart = spr.y;
                const spriteActualWidth = spr.width * spr.scale;
                const spriteActualHeight = spr.height * spr.scale;
                if (((((spr.x + spriteActualWidth) >= camViewXStart) && (spriteXStart <= camViewXEnd)) && ((spr.y + spriteActualHeight) >= camViewYStart)) && (spriteYStart <= camViewYEnd)) {
                    draw_1(gl, currentRenderer, spr, camera.x, camera.y, camera.zoom);
                }
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    }
}

