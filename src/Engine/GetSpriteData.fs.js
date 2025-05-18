import { FSharpRef, Record } from "../../fable_modules/fable-library-js.4.24.0/Types.js";
import { list_type, string_type, class_type, record_type, bool_type, int32_type, float64_type } from "../../fable_modules/fable-library-js.4.24.0/Reflection.js";
import { values, toList, ofList, FSharpMap__get_Item, empty } from "../../fable_modules/fable-library-js.4.24.0/Map.js";
import { comparePrimitives } from "../../fable_modules/fable-library-js.4.24.0/Util.js";
import { parallel, ignore, fromContinuations } from "../../fable_modules/fable-library-js.4.24.0/Async.js";
import { some } from "../../fable_modules/fable-library-js.4.24.0/Option.js";
import { singleton } from "../../fable_modules/fable-library-js.4.24.0/AsyncBuilder.js";
import { Http_get } from "../../fable_modules/Fable.SimpleHttp.3.6.0/Http.fs.js";
import { isEmpty, ofSeq, head, map } from "../../fable_modules/fable-library-js.4.24.0/List.js";
import { SimpleJson_tryParse } from "../../fable_modules/Fable.SimpleJson.3.24.0/./SimpleJson.fs.js";
import { Convert_fromJson } from "../../fable_modules/Fable.SimpleJson.3.24.0/./Json.Converter.fs.js";
import { createTypeInfo } from "../../fable_modules/Fable.SimpleJson.3.24.0/./TypeInfo.Converter.fs.js";
import { map as map_1 } from "../../fable_modules/fable-library-js.4.24.0/Seq.js";

export class FrameJsonData extends Record {
    constructor(startx, starty, width, height, col, row, animation, no) {
        super();
        this.startx = startx;
        this.starty = starty;
        this.width = width;
        this.height = height;
        this.col = (col | 0);
        this.row = (row | 0);
        this.animation = animation;
        this.no = (no | 0);
    }
}

export function FrameJsonData_$reflection() {
    return record_type("Engine.GetSpriteData.FrameJsonData", [], FrameJsonData, () => [["startx", float64_type], ["starty", float64_type], ["width", float64_type], ["height", float64_type], ["col", int32_type], ["row", int32_type], ["animation", bool_type], ["no", int32_type]]);
}

export class ImageData extends Record {
    constructor(startx, starty, width, height, col, row, animation, no, image) {
        super();
        this.startx = startx;
        this.starty = starty;
        this.width = width;
        this.height = height;
        this.col = (col | 0);
        this.row = (row | 0);
        this.animation = animation;
        this.no = (no | 0);
        this.image = image;
    }
}

export function ImageData_$reflection() {
    return record_type("Engine.GetSpriteData.ImageData", [], ImageData, () => [["startx", float64_type], ["starty", float64_type], ["width", float64_type], ["height", float64_type], ["col", int32_type], ["row", int32_type], ["animation", bool_type], ["no", int32_type], ["image", class_type("Browser.Types.HTMLImageElement", undefined)]]);
}

const spriteDataStore = new FSharpRef(empty({
    Compare: comparePrimitives,
}));

export function getdataa(source) {
    return FSharpMap__get_Item(spriteDataStore.contents, source);
}

function loadImageAsync(img) {
    return fromContinuations((tupledArg) => {
        const success = tupledArg[0];
        if (img.complete && (img.naturalWidth > 0)) {
            success();
        }
        else {
            img.onload = ((_arg_1) => {
                success();
            });
            img.onerror = ((ev) => {
                console.error(some("Failed to load image:"), img.src, ev);
                success();
            });
        }
    });
}

export function loadSpriteAtlas() {
    return singleton.Delay(() => singleton.Bind(Http_get("/sprites.json"), (_arg) => {
        const status = _arg[0] | 0;
        const responseText = _arg[1];
        if (status === 200) {
            return singleton.TryWith(singleton.Delay(() => {
                let matchValue;
                const finalImageDataMap = ofList(map((tupledArg) => {
                    const key = tupledArg[0];
                    const fData = head(tupledArg[1]);
                    const img = document.createElement("img");
                    img.src = key;
                    return [key, new ImageData(fData.startx, fData.starty, fData.width, fData.height, fData.col, fData.row, fData.animation, fData.no, img)];
                }, toList((matchValue = SimpleJson_tryParse(responseText), (matchValue != null) ? Convert_fromJson(matchValue, createTypeInfo(class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, list_type(FrameJsonData_$reflection())]))) : (() => {
                    throw new Error("Couldn\'t parse the input JSON string because it seems to be invalid");
                })()))), {
                    Compare: comparePrimitives,
                });
                spriteDataStore.contents = finalImageDataMap;
                const imageLoadTasks = ofSeq(map_1((data) => loadImageAsync(data.image), values(finalImageDataMap)));
                return singleton.Combine(!isEmpty(imageLoadTasks) ? singleton.Bind(ignore(parallel(imageLoadTasks)), () => {
                    console.log(some("All sprite images initiated loading via GetSpriteData."));
                    return singleton.Zero();
                }) : ((console.log(some("Sprite atlas loaded (GetSpriteData), but no images to load.")), singleton.Zero())), singleton.Delay(() => singleton.Return(undefined)));
            }), (_arg_2) => {
                const ex = _arg_2;
                console.error(some("Error processing sprite atlas JSON (GetSpriteData):"), ex.message, ex);
                return singleton.Return(undefined);
            });
        }
        else {
            console.error(some("Failed to load sprites.json (GetSpriteData). Status:"), status, "Content:", responseText);
            return singleton.Return(undefined);
        }
    }));
}

