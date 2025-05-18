import { Sprite, createTextureForImage, init } from "./Sprite.fs.js";
import { Record } from "../../fable_modules/fable-library-js.4.24.0/Types.js";
import { class_type, list_type, string_type, record_type, bool_type, int32_type, float64_type } from "../../fable_modules/fable-library-js.4.24.0/Reflection.js";
import { singleton } from "../../fable_modules/fable-library-js.4.24.0/AsyncBuilder.js";
import { Http_get } from "../../fable_modules/Fable.SimpleHttp.3.6.0/Http.fs.js";
import { SimpleJson_tryParse } from "../../fable_modules/Fable.SimpleJson.3.24.0/./SimpleJson.fs.js";
import { Convert_fromJson } from "../../fable_modules/Fable.SimpleJson.3.24.0/./Json.Converter.fs.js";
import { createTypeInfo } from "../../fable_modules/Fable.SimpleJson.3.24.0/./TypeInfo.Converter.fs.js";
import { some } from "../../fable_modules/fable-library-js.4.24.0/Option.js";

export const canvas = document.querySelector("canvas");

export const gl = canvas.getContext("webgl");

export const renderer = init(gl, new Sprite(0, document.createElement("img"), createTextureForImage(document.createElement("img")), "", 0, 0, 0, 0, false, false, 1, 1, 1, 0, [], false, false, false, 0));

export class Frame extends Record {
    constructor(startx, starty, width, height, col, row, animation) {
        super();
        this.startx = startx;
        this.starty = starty;
        this.width = width;
        this.height = height;
        this.col = (col | 0);
        this.row = (row | 0);
        this.animation = animation;
    }
}

export function Frame_$reflection() {
    return record_type("Engine.Runtime.Frame", [], Frame, () => [["startx", float64_type], ["starty", float64_type], ["width", float64_type], ["height", float64_type], ["col", int32_type], ["row", int32_type], ["animation", bool_type]]);
}

export function loadSpriteAtlas() {
    return singleton.Delay(() => singleton.Bind(Http_get("/sprites.json"), (_arg) => {
        const status = _arg[0] | 0;
        if (status === 200) {
            return singleton.TryWith(singleton.Delay(() => {
                let atlas;
                const matchValue = SimpleJson_tryParse(_arg[1]);
                if (matchValue != null) {
                    atlas = Convert_fromJson(matchValue, createTypeInfo(class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, list_type(Frame_$reflection())])));
                }
                else {
                    throw new Error("Couldn\'t parse the input JSON string because it seems to be invalid");
                }
                return singleton.Return(atlas);
            }), (_arg_1) => {
                console.error(some("Error parsing sprites.json in Runtime.fs:"), _arg_1.message);
                return singleton.Return(undefined);
            });
        }
        else {
            console.error(some("Failed to load sprites.json in Runtime.fs, status:"), status);
            return singleton.Return(undefined);
        }
    }));
}

