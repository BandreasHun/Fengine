import { Record } from "../../fable_modules/fable-library-js.4.24.0/Types.js";
import { record_type, option_type, float64_type } from "../../fable_modules/fable-library-js.4.24.0/Reflection.js";
import { Sprite_$reflection } from "./Sprite.fs.js";

export class Camera extends Record {
    constructor(x, y, viewportWidth, viewportHeight, zoom, target) {
        super();
        this.x = x;
        this.y = y;
        this.viewportWidth = viewportWidth;
        this.viewportHeight = viewportHeight;
        this.zoom = zoom;
        this.target = target;
    }
}

export function Camera_$reflection() {
    return record_type("Engine.Camera.Camera", [], Camera, () => [["x", float64_type], ["y", float64_type], ["viewportWidth", float64_type], ["viewportHeight", float64_type], ["zoom", float64_type], ["target", option_type(Sprite_$reflection())]]);
}

export function create(initialX, initialY, vWidth, vHeight) {
    return new Camera(initialX, initialY, vWidth, vHeight, 1, undefined);
}

export function update(camera, _dt) {
    const matchValue = camera.target;
    if (matchValue == null) {
    }
    else {
        const spriteToFollow = matchValue;
        const targetCenterX = spriteToFollow.x + ((spriteToFollow.width * spriteToFollow.scale) / 2);
        const targetCenterY = spriteToFollow.y + ((spriteToFollow.height * spriteToFollow.scale) / 2);
        camera.x = (targetCenterX - ((camera.viewportWidth / camera.zoom) / 2));
        camera.y = (targetCenterY - ((camera.viewportHeight / camera.zoom) / 2));
    }
}

export function setTarget(camera, sprite) {
    camera.target = sprite;
}

export function clearTarget(camera) {
    camera.target = undefined;
}

export function setZoom(camera, level) {
    camera.zoom = ((level > 0) ? level : 0.01);
}

export function setViewportSize(camera, vWidth, vHeight) {
    camera.viewportWidth = vWidth;
    camera.viewportHeight = vHeight;
}

