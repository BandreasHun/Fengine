import { Record } from "../../fable_modules/fable-library-js.4.24.0/Types.js";
import { record_type, array_type, bool_type, float64_type, string_type, class_type, int32_type } from "../../fable_modules/fable-library-js.4.24.0/Reflection.js";
import { ofNullable, defaultArg } from "../../fable_modules/fable-library-js.4.24.0/Option.js";
import { printf, toFail } from "../../fable_modules/fable-library-js.4.24.0/String.js";
import { ofArray, toArray } from "../../fable_modules/fable-library-js.4.24.0/List.js";
import { item } from "../../fable_modules/fable-library-js.4.24.0/Array.js";
import { getdataa } from "./GetSpriteData.fs.js";

let nextSpriteId = 0;

export class Sprite extends Record {
    constructor(id, image, texture, texturePath, x, y, velocityX, velocityY, affectedByGravity, isGrounded, width, height, scale, rotation, vertices, animation, isAnimating, flipX, no) {
        super();
        this.id = (id | 0);
        this.image = image;
        this.texture = texture;
        this.texturePath = texturePath;
        this.x = x;
        this.y = y;
        this.velocityX = velocityX;
        this.velocityY = velocityY;
        this.affectedByGravity = affectedByGravity;
        this.isGrounded = isGrounded;
        this.width = width;
        this.height = height;
        this.scale = scale;
        this.rotation = rotation;
        this.vertices = vertices;
        this.animation = animation;
        this.isAnimating = isAnimating;
        this.flipX = flipX;
        this.no = (no | 0);
    }
}

export function Sprite_$reflection() {
    return record_type("Engine.Sprite.Sprite", [], Sprite, () => [["id", int32_type], ["image", class_type("Browser.Types.HTMLImageElement", undefined)], ["texture", class_type("Browser.Types.WebGLTexture", undefined)], ["texturePath", string_type], ["x", float64_type], ["y", float64_type], ["velocityX", float64_type], ["velocityY", float64_type], ["affectedByGravity", bool_type], ["isGrounded", bool_type], ["width", float64_type], ["height", float64_type], ["scale", float64_type], ["rotation", float64_type], ["vertices", array_type(array_type(float64_type))], ["animation", bool_type], ["isAnimating", bool_type], ["flipX", bool_type], ["no", int32_type]]);
}

export const items = [];

export class Renderer extends Record {
    constructor(program, posBuf, texBuf, posLoc, texLoc, resLoc, imgLoc, camTranslationLoc, camZoomLoc) {
        super();
        this.program = program;
        this.posBuf = posBuf;
        this.texBuf = texBuf;
        this.posLoc = (posLoc | 0);
        this.texLoc = (texLoc | 0);
        this.resLoc = resLoc;
        this.imgLoc = imgLoc;
        this.camTranslationLoc = camTranslationLoc;
        this.camZoomLoc = camZoomLoc;
    }
}

export function Renderer_$reflection() {
    return record_type("Engine.Sprite.Renderer", [], Renderer, () => [["program", class_type("Browser.Types.WebGLProgram", undefined)], ["posBuf", class_type("Browser.Types.WebGLBuffer", undefined)], ["texBuf", class_type("Browser.Types.WebGLBuffer", undefined)], ["posLoc", int32_type], ["texLoc", int32_type], ["resLoc", class_type("Browser.Types.WebGLUniformLocation", undefined)], ["imgLoc", class_type("Browser.Types.WebGLUniformLocation", undefined)], ["camTranslationLoc", class_type("Browser.Types.WebGLUniformLocation", undefined)], ["camZoomLoc", class_type("Browser.Types.WebGLUniformLocation", undefined)]]);
}

export function getCellTexCoords(image, pcsdata) {
    const atlasW = image.width;
    const atlasH = image.height;
    const x0 = pcsdata.startx + ((pcsdata.no % pcsdata.col) * pcsdata.width);
    const y0 = pcsdata.starty + (~~(pcsdata.no / pcsdata.col) * pcsdata.height);
    const v0 = y0 / atlasH;
    const u0 = x0 / atlasW;
    const v1 = (y0 + pcsdata.height) / atlasH;
    const u1 = (x0 + pcsdata.width) / atlasW;
    return new Float64Array([u0, v0, u1, v0, u0, v1, u0, v1, u1, v0, u1, v1]);
}

export function createTextureForImage(image) {
    const canvas = document.querySelector("canvas");
    const ctx = canvas.getContext("webgl");
    const tex = ctx.createTexture();
    ctx.bindTexture(ctx.TEXTURE_2D, tex);
    const placeholder = new Uint8Array(new Uint8Array([255, 0, 255, 255]));
    ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, 1, 1, 0, ctx.RGBA, ctx.UNSIGNED_BYTE, placeholder);
    const upload = () => {
        ctx.bindTexture(ctx.TEXTURE_2D, tex);
        ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, ctx.RGBA, ctx.UNSIGNED_BYTE, image);
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_S, ctx.CLAMP_TO_EDGE);
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_T, ctx.CLAMP_TO_EDGE);
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MIN_FILTER, ctx.NEAREST);
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MAG_FILTER, ctx.NEAREST);
    };
    image.onload = ((_arg) => {
        upload();
    });
    if (image.complete && (image.naturalWidth > 0)) {
        upload();
    }
    return tex;
}

export function init(ctx, spr) {
    const vsSrc = "\r\n        attribute vec2 a_position;\r\n        attribute vec2 a_texCoord;\r\n        \r\n        uniform vec2 u_resolution;      // Canvas dimensions\r\n        uniform vec2 u_cameraTranslation; // Camera\'s top-left world coordinate\r\n        uniform float u_cameraZoom;       // Camera\'s zoom factor\r\n        \r\n        varying vec2 v_texCoord;\r\n        \r\n        void main() {\r\n          // Apply camera transformation\r\n          vec2 worldPos = a_position; // a_position is already scaled and rotated world position\r\n          vec2 viewPos = (worldPos - u_cameraTranslation) * u_cameraZoom;\r\n          \r\n          // Convert from view space to 0.0 to 1.0\r\n          vec2 zeroToOne = viewPos / u_resolution;\r\n          // Convert from 0->1 to 0->2\r\n          vec2 zeroToTwo = zeroToOne * 2.0;\r\n          // Convert from 0->2 to -1->+1 (clip space)\r\n          vec2 clipSpace = zeroToTwo - 1.0;\r\n          \r\n          gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1); // Y is flipped for screen coordinates\r\n          v_texCoord = a_texCoord;\r\n        }\r\n    ";
    const fsSrc = "\r\n        precision mediump float;\r\n        uniform sampler2D u_image;\r\n        varying vec2 v_texCoord;\r\n        void main() {\r\n          gl_FragColor = texture2D(u_image, v_texCoord);\r\n        }\r\n    ";
    let vs;
    const s = ctx.createShader(ctx.VERTEX_SHADER);
    ctx.shaderSource(s, vsSrc);
    ctx.compileShader(s);
    if (!ctx.getShaderParameter(s, ctx.COMPILE_STATUS)) {
        const info = defaultArg(ofNullable(ctx.getShaderInfoLog(s)), "<no log>");
        toFail(printf("Vertex shader compilation failed: %s"))(info);
    }
    vs = s;
    let fs;
    const s_1 = ctx.createShader(ctx.FRAGMENT_SHADER);
    ctx.shaderSource(s_1, fsSrc);
    ctx.compileShader(s_1);
    if (!ctx.getShaderParameter(s_1, ctx.COMPILE_STATUS)) {
        const info_1 = defaultArg(ofNullable(ctx.getShaderInfoLog(s_1)), "<no log>");
        toFail(printf("Fragment shader compilation failed: %s"))(info_1);
    }
    fs = s_1;
    let prog;
    const p = ctx.createProgram();
    ctx.attachShader(p, vs);
    ctx.attachShader(p, fs);
    ctx.linkProgram(p);
    if (!ctx.getProgramParameter(p, ctx.LINK_STATUS)) {
        const info_2 = defaultArg(ofNullable(ctx.getProgramInfoLog(p)), "<no log>");
        toFail(printf("Program linking failed: %s"))(info_2);
    }
    prog = p;
    ctx.useProgram(prog);
    ctx.enable(ctx.BLEND);
    ctx.blendFunc(ctx.SRC_ALPHA, ctx.ONE_MINUS_SRC_ALPHA);
    return new Renderer(prog, ctx.createBuffer(), ctx.createBuffer(), ~~ctx.getAttribLocation(prog, "a_position"), ~~ctx.getAttribLocation(prog, "a_texCoord"), ctx.getUniformLocation(prog, "u_resolution"), ctx.getUniformLocation(prog, "u_image"), ctx.getUniformLocation(prog, "u_cameraTranslation"), ctx.getUniformLocation(prog, "u_cameraZoom"));
}

export function draw(ctx, r, spr, cameraX, cameraY, cameraZoom) {
    const actualWidth = spr.width * spr.scale;
    const actualHeight = spr.height * spr.scale;
    const x1 = spr.x;
    const y1 = spr.y;
    const x2 = spr.x + actualWidth;
    const y2 = spr.y + actualHeight;
    const cx = spr.x + (actualWidth / 2);
    const cy = spr.y + (actualHeight / 2);
    const cosAngle = Math.cos(spr.rotation);
    const sinAngle = Math.sin(spr.rotation);
    const transform = (tupledArg) => {
        const translatedX = tupledArg[0] - cx;
        const translatedY = tupledArg[1] - cy;
        return [((translatedX * cosAngle) - (translatedY * sinAngle)) + cx, ((translatedX * sinAngle) + (translatedY * cosAngle)) + cy];
    };
    const patternInput = transform([x1, y1]);
    const patternInput_1 = transform([x2, y1]);
    const trx = patternInput_1[0];
    const tr_y = patternInput_1[1];
    const patternInput_2 = transform([x1, y2]);
    const bly = patternInput_2[1];
    const blx = patternInput_2[0];
    const patternInput_3 = transform([x2, y2]);
    const positions = toArray(ofArray(new Float64Array([patternInput[0], patternInput[1], trx, tr_y, blx, bly, blx, bly, trx, tr_y, patternInput_3[0], patternInput_3[1]])));
    ctx.useProgram(r.program);
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.posBuf);
    const posArr = new Float32Array(positions);
    ctx.bufferData(ctx.ARRAY_BUFFER, posArr, ctx.STATIC_DRAW);
    ctx.enableVertexAttribArray(r.posLoc);
    ctx.vertexAttribPointer(r.posLoc, 2, ctx.FLOAT, false, 0, 0);
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.texBuf);
    const baseTexCoords = spr.vertices[spr.no];
    const finalTexCoords = spr.flipX ? (new Float64Array([item(2, baseTexCoords), item(3, baseTexCoords), item(0, baseTexCoords), item(1, baseTexCoords), item(10, baseTexCoords), item(11, baseTexCoords), item(10, baseTexCoords), item(11, baseTexCoords), item(0, baseTexCoords), item(1, baseTexCoords), item(4, baseTexCoords), item(5, baseTexCoords)])) : baseTexCoords;
    const texArr = new Float32Array(finalTexCoords);
    ctx.bufferData(ctx.ARRAY_BUFFER, texArr, ctx.STATIC_DRAW);
    ctx.vertexAttribPointer(r.texLoc, 2, ctx.FLOAT, false, 0, 0);
    ctx.enableVertexAttribArray(r.texLoc);
    ctx.uniform2f(r.camTranslationLoc, cameraX, cameraY);
    ctx.uniform1f(r.camZoomLoc, cameraZoom);
    ctx.uniform2f(r.resLoc, ctx.canvas.width, ctx.canvas.height);
    ctx.activeTexture(ctx.TEXTURE0);
    ctx.bindTexture(ctx.TEXTURE_2D, spr.texture);
    ctx.uniform1i(r.imgLoc, 0);
    ctx.drawArrays(ctx.TRIANGLES, 0, 6);
}

export function create(url, position_, position__1, size_, size__1) {
    const position = [position_, position__1];
    const size = [size_, size__1];
    const frame = getdataa(url);
    const image = frame.image;
    const verticesArr = [];
    if (frame.animation) {
        for (let i = 0; i <= ((frame.col * frame.row) - 1); i++) {
            frame.no = (i | 0);
            void (verticesArr.push(getCellTexCoords(image, frame)));
        }
    }
    else {
        void (verticesArr.push(getCellTexCoords(image, frame)));
    }
    const tex = createTextureForImage(image);
    nextSpriteId = ((nextSpriteId + 1) | 0);
    const sprite = new Sprite(nextSpriteId, image, tex, url, position[0], position[1], 0, 0, false, false, size[0], size[1], 1, 0, verticesArr, frame.animation, frame.animation, false, frame.no);
    void (items.push(sprite));
    return sprite;
}

export function startSpriteAnimation(sprite) {
    if (sprite.animation) {
        sprite.isAnimating = true;
    }
}

export function stopSpriteAnimation(sprite) {
    sprite.isAnimating = false;
}

