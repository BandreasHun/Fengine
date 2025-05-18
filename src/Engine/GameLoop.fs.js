import { iterate } from "../../fable_modules/fable-library-js.4.24.0/Seq.js";
import { toArray } from "../../fable_modules/fable-library-js.4.24.0/Option.js";

let lastFrameTime = 0;

let updateCallback = undefined;

let drawCallback = undefined;

let animCallback = undefined;

let frameRequestId = 0;

export function onUpdate(f) {
    updateCallback = f;
}

export function onDraw(f) {
    drawCallback = f;
}

export function onAnimate(f) {
    animCallback = f;
}

export function start() {
    const loop = (timestamp) => {
        const deltaTime = timestamp - lastFrameTime;
        lastFrameTime = timestamp;
        iterate((f) => {
            f(deltaTime);
        }, toArray(updateCallback));
        iterate((f_1) => {
            f_1(deltaTime);
        }, toArray(animCallback));
        iterate((f_2) => {
            f_2(deltaTime);
        }, toArray(drawCallback));
        frameRequestId = window.requestAnimationFrame(loop);
    };
    frameRequestId = window.requestAnimationFrame(loop);
}

export function stop() {
    window.cancelAnimationFrame(frameRequestId);
    frameRequestId = 0;
}

