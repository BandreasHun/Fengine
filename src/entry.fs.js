import { createAtom } from "../fable_modules/fable-library-js.4.24.0/Util.js";
import { Init } from "./Engine/Input.fs.js";
import { mainCam, update, main } from "./main.fs.js";
import { start, onAnimate, onDraw, onUpdate } from "./Engine/GameLoop.fs.js";
import { updateAnimations, draw } from "./Engine/Scene.fs.js";
import { startWithContinuations } from "../fable_modules/fable-library-js.4.24.0/Async.js";
import { loadSpriteAtlas } from "./Engine/GetSpriteData.fs.js";
import { some } from "../fable_modules/fable-library-js.4.24.0/Option.js";

export let alreadyStarted = createAtom(false);

export function init() {
    if (!alreadyStarted()) {
        alreadyStarted(true);
        Init();
        main();
        onUpdate((deltaTimeMillis) => {
            update(deltaTimeMillis);
        });
        onDraw((dt) => {
            draw(mainCam(), dt);
        });
        onAnimate((dt_1) => {
            updateAnimations(dt_1 / 1000);
        });
        start();
    }
}

startWithContinuations(loadSpriteAtlas(), () => {
    init();
}, (ex) => {
    console.error(some("Failed to load sprite atlas in entry.fs (GetSpriteData):"), ex);
}, (_arg_1) => {
});

