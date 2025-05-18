module Engine.Sprite

open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Engine.GetSpriteData

let mutable private nextSpriteId = 0

type Sprite = {
    id: int
    image: HTMLImageElement
    texture: WebGLTexture
    texturePath: string
    mutable x: float
    mutable y: float
    mutable velocityX: float
    mutable velocityY: float
    mutable affectedByGravity: bool
    mutable isGrounded: bool
    width: float
    height: float
    mutable scale: float
    mutable rotation: float
    mutable vertices : ResizeArray<float[]>
    animation: bool
    mutable isAnimating: bool
    mutable flipX: bool
    mutable no: int 
}
let public items = ResizeArray<Sprite>()
type Renderer = {
    program: WebGLProgram
    posBuf: WebGLBuffer
    texBuf: WebGLBuffer
    posLoc: int
    texLoc: int
    resLoc: WebGLUniformLocation
    imgLoc: WebGLUniformLocation
    camTranslationLoc: WebGLUniformLocation
    camZoomLoc: WebGLUniformLocation
}

let getCellTexCoords (image: HTMLImageElement) (pcsdata: Engine.GetSpriteData.ImageData) =
    let atlasW = float image.width
    let atlasH = float image.height
    let col = pcsdata.no % pcsdata.col
    let row = pcsdata.no / pcsdata.col
    let x0 = pcsdata.startx + float col * pcsdata.width
    let y0 = pcsdata.starty + float row * pcsdata.height
    let u0, v0 = x0 / atlasW,         y0 / atlasH
    let u1, v1 = (x0 + pcsdata.width) / atlasW, (y0 + pcsdata.height) / atlasH
    [| u0; v0; u1; v0
       u0; v1; u0; v1
       u1; v0; u1; v1 |]

let createTextureForImage (image: HTMLImageElement) : WebGLTexture =
    let canvas = Browser.Dom.document.querySelector("canvas") :?> HTMLCanvasElement
    let ctx = canvas.getContext("webgl") :?> WebGLRenderingContext
    let tex = ctx.createTexture()
    ctx.bindTexture(ctx.TEXTURE_2D, tex)
    let placeholder = Fable.Core.JS.Constructors.Uint8Array.Create([|255uy; 0uy; 255uy; 255uy|]) :> obj :?> ArrayBufferView
    ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, 1, 1, 0, ctx.RGBA, ctx.UNSIGNED_BYTE, placeholder)
    let upload () =
        ctx.bindTexture(ctx.TEXTURE_2D, tex)
        ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, ctx.RGBA, ctx.UNSIGNED_BYTE, image)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_S, ctx.CLAMP_TO_EDGE)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_T, ctx.CLAMP_TO_EDGE)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MIN_FILTER, ctx.NEAREST)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MAG_FILTER, ctx.NEAREST)
    image.onload <- fun _ -> upload()
    if image.complete && image.naturalWidth > 0 then upload()
    tex

let init (ctx: WebGLRenderingContext) (spr: Sprite) : Renderer =
    let vsSrc = """
        attribute vec2 a_position;
        attribute vec2 a_texCoord;
        
        uniform vec2 u_resolution;      // Canvas dimensions
        uniform vec2 u_cameraTranslation; // Camera's top-left world coordinate
        uniform float u_cameraZoom;       // Camera's zoom factor
        
        varying vec2 v_texCoord;
        
        void main() {
          // Apply camera transformation
          vec2 worldPos = a_position; // a_position is already scaled and rotated world position
          vec2 viewPos = (worldPos - u_cameraTranslation) * u_cameraZoom;
          
          // Convert from view space to 0.0 to 1.0
          vec2 zeroToOne = viewPos / u_resolution;
          // Convert from 0->1 to 0->2
          vec2 zeroToTwo = zeroToOne * 2.0;
          // Convert from 0->2 to -1->+1 (clip space)
          vec2 clipSpace = zeroToTwo - 1.0;
          
          gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1); // Y is flipped for screen coordinates
          v_texCoord = a_texCoord;
        }
    """
    let fsSrc = """
        precision mediump float;
        uniform sampler2D u_image;
        varying vec2 v_texCoord;
        void main() {
          gl_FragColor = texture2D(u_image, v_texCoord);
        }
    """
    let vs = 
        let s = ctx.createShader(ctx.VERTEX_SHADER)
        ctx.shaderSource(s, vsSrc)
        ctx.compileShader(s)
        let ok = ctx.getShaderParameter(s, ctx.COMPILE_STATUS) |> unbox<bool>
        if not ok then
            let info = ctx.getShaderInfoLog(s) |> Option.ofObj |> Option.defaultValue "<no log>"
            failwithf "Vertex shader compilation failed: %s" info
        s
    let fs = 
        let s = ctx.createShader(ctx.FRAGMENT_SHADER)
        ctx.shaderSource(s, fsSrc)
        ctx.compileShader(s)
        let ok = ctx.getShaderParameter(s, ctx.COMPILE_STATUS) |> unbox<bool>
        if not ok then
            let info = ctx.getShaderInfoLog(s) |> Option.ofObj |> Option.defaultValue "<no log>"
            failwithf "Fragment shader compilation failed: %s" info
        s
    let prog =
        let p = ctx.createProgram()
        ctx.attachShader(p, vs)
        ctx.attachShader(p, fs)
        ctx.linkProgram(p)
        let ok = ctx.getProgramParameter(p, ctx.LINK_STATUS) |> unbox<bool>
        if not ok then
            let info = ctx.getProgramInfoLog(p) |> Option.ofObj |> Option.defaultValue "<no log>"
            failwithf "Program linking failed: %s" info
        p
    ctx.useProgram(prog)
    ctx.enable(ctx.BLEND)
    ctx.blendFunc(ctx.SRC_ALPHA, ctx.ONE_MINUS_SRC_ALPHA)
    let posBuf = ctx.createBuffer()
    let texBuf = ctx.createBuffer()
    let posLoc = int (ctx.getAttribLocation(prog, "a_position"))
    let texLoc = int (ctx.getAttribLocation(prog, "a_texCoord"))
    let resLoc = ctx.getUniformLocation(prog, "u_resolution")
    let imgLoc = ctx.getUniformLocation(prog, "u_image")
    
    let camTranslationLoc = ctx.getUniformLocation(prog, "u_cameraTranslation")
    let camZoomLoc = ctx.getUniformLocation(prog, "u_cameraZoom")

    { program = prog
      posBuf   = posBuf
      texBuf   = texBuf
      posLoc   = posLoc
      texLoc   = texLoc
      resLoc   = resLoc
      imgLoc   = imgLoc
      camTranslationLoc = camTranslationLoc
      camZoomLoc = camZoomLoc }

let public draw (ctx: WebGLRenderingContext) (r: Renderer) (spr: Sprite) (cameraX: float) (cameraY: float) (cameraZoom: float) =
    let actualWidth = spr.width * spr.scale
    let actualHeight = spr.height * spr.scale
    
    let x1 = spr.x
    let y1 = spr.y
    let x2 = spr.x + actualWidth
    let y2 = spr.y + actualHeight

    let cx = spr.x + actualWidth / 2.0
    let cy = spr.y + actualHeight / 2.0
    
    let cosAngle = cos spr.rotation
    let sinAngle = sin spr.rotation
    
    let transform (px, py) =
        let translatedX = px - cx
        let translatedY = py - cy
        let rotatedX = translatedX * cosAngle - translatedY * sinAngle
        let rotatedY = translatedX * sinAngle + translatedY * cosAngle
        (rotatedX + cx, rotatedY + cy)

    let (tlx, tly) = transform (x1, y1)       // Top-left
    let (trx, tr_y) = transform (x2, y1)      // Top-right
    let (blx, bly) = transform (x1, y2)       // Bottom-left
    let (brx, bry) = transform (x2, y2)      // Bottom-right

    let positions =
      [| tlx; tly;  trx; tr_y;  blx; bly;
         blx; bly;  trx; tr_y;  brx; bry |]
      |> List.ofArray
      |> List.toArray

    ctx.useProgram(r.program)
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.posBuf)
    let posArr = Constructors.Float32Array.Create(positions :> obj)
    ctx.bufferData(ctx.ARRAY_BUFFER, U3.Case2(unbox<ArrayBufferView> posArr), ctx.STATIC_DRAW)
    ctx.enableVertexAttribArray(r.posLoc)
    ctx.vertexAttribPointer(r.posLoc, 2, ctx.FLOAT, false, 0, 0)
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.texBuf)
    
    let baseTexCoords = spr.vertices.[spr.no]
    let finalTexCoords =
        if spr.flipX then
            [|
                baseTexCoords.[2]; baseTexCoords.[3];  // Geo TL -> Tex TR (u1,v0)
                baseTexCoords.[0]; baseTexCoords.[1];  // Geo TR -> Tex TL (u0,v0)
                baseTexCoords.[10]; baseTexCoords.[11]; // Geo BL (Tri1) -> Tex BR (u1,v1)

                baseTexCoords.[10]; baseTexCoords.[11]; // Geo BL (Tri2) -> Tex BR (u1,v1)
                baseTexCoords.[0]; baseTexCoords.[1];  // Geo TR (Tri2) -> Tex TL (u0,v0)
                baseTexCoords.[4]; baseTexCoords.[5]   // Geo BR (Tri2) -> Tex BL (u0,v1)
            |]
        else
            baseTexCoords

    let texArr = Constructors.Float32Array.Create(finalTexCoords :> obj)
    ctx.bufferData(ctx.ARRAY_BUFFER, U3.Case2(unbox<ArrayBufferView> texArr), ctx.STATIC_DRAW)
    ctx.vertexAttribPointer(r.texLoc, 2, ctx.FLOAT, false, 0, 0)
    ctx.enableVertexAttribArray(r.texLoc)
    
    ctx.uniform2f(r.camTranslationLoc, cameraX, cameraY)
    ctx.uniform1f(r.camZoomLoc, cameraZoom)
    
    ctx.uniform2f(r.resLoc, float ctx.canvas.width, float ctx.canvas.height)
    ctx.activeTexture(ctx.TEXTURE0)
    ctx.bindTexture(ctx.TEXTURE_2D, spr.texture)
    ctx.uniform1i(r.imgLoc, 0)
    ctx.drawArrays(ctx.TRIANGLES, 0, 6)

let public create (url: string) (position: float * float) (size: float * float) : Sprite =
    let x, y = position
    let width, height = size
    let frame = Engine.GetSpriteData.getdataa url
    let image = frame.image
    let verticesArr = ResizeArray<float[]>()
    if frame.animation then
        for i in 0 .. frame.col * frame.row - 1 do
            frame.no <- i
            verticesArr.Add (getCellTexCoords image frame)
    else
        verticesArr.Add (getCellTexCoords image frame)
    let tex = createTextureForImage image
    
    nextSpriteId <- nextSpriteId + 1
    let sprite = { 
      id = nextSpriteId
      image = image
      texture = tex
      texturePath = url
      x = x; y = y
      velocityX = 0.0
      velocityY = 0.0
      affectedByGravity = false
      isGrounded = false
      width = width; height = height
      scale = 1.0; rotation = 0.0
      vertices = verticesArr
      animation = frame.animation
      isAnimating = frame.animation
      flipX = false
      no = frame.no
    }

    items.Add sprite
    
    sprite

let startSpriteAnimation (sprite: Sprite) =
    if sprite.animation then
        sprite.isAnimating <- true

let stopSpriteAnimation (sprite: Sprite) =
    sprite.isAnimating <- false



