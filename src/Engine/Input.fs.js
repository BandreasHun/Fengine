import { FSharpSet__Contains, FSharpSet__Remove, FSharpSet__Add, empty } from "../../fable_modules/fable-library-js.4.24.0/Set.js";
import { comparePrimitives } from "../../fable_modules/fable-library-js.4.24.0/Util.js";

let pressed = empty({
    Compare: comparePrimitives,
});

let mouseJustClicked = undefined;

export function Init() {
    window.addEventListener("keydown", (e) => {
        const ke = e;
        pressed = FSharpSet__Add(pressed, ke.key);
    });
    window.addEventListener("keyup", (e_1) => {
        const ke_1 = e_1;
        pressed = FSharpSet__Remove(pressed, ke_1.key);
    });
    const canvasElement = document.querySelector("canvas");
    canvasElement.addEventListener("mousedown", (e_2) => {
        const me = e_2;
        const rect = canvasElement.getBoundingClientRect();
        const canvasX = me.clientX - rect.left;
        const canvasY = me.clientY - rect.top;
        mouseJustClicked = [canvasX, canvasY];
    });
}

export function IsDown(key) {
    return FSharpSet__Contains(pressed, key);
}

export function ConsumeClick() {
    const click = mouseJustClicked;
    mouseJustClicked = undefined;
    return click;
}

