module Engine.Sprite

open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Engine.GetSpriteData



type Sprite = {
    image: HTMLImageElement
    mutable x: float
    mutable y: float
    width: float
    height: float
    mutable scale: float
    mutable rotation: float
    mutable vertices : float[][]
    animation: bool
    no: int 
}


type Renderer = {
    program: WebGLProgram  // A shader program (vertex + fragment shader összeállítva), a WebGL inicializáció során jön létre.
    posBuf: WebGLBuffer    // A vertex buffer, ami a sprite pozícióit tárolja, a WebGL buffer inicializáció során jön létre.
    texBuf: WebGLBuffer    // A texture buffer, ami a sprite textúra koordinátait tárolja, a WebGL buffer inicializáció során jön létre.
    posLoc: int            // A vertex shaderben a pozíció attribútum helye, a shader programból lekérdezve.
    texLoc: int            // A vertex shaderben a textúra koordináta attribútum helye, a shader programból lekérdezve.
    resLoc: WebGLUniformLocation  // A fragment shaderben a képernyő felbontás uniform helye, a shader programból lekérdezve.
    imgLoc: WebGLUniformLocation  // A fragment shaderben a textúra uniform helye, a shader programból lekérdezve.
    texture: WebGLTexture         // A sprite textúrája, a WebGL textúra inicializáció során jön létre.
}




       

        
let getCellTexCoords (image: HTMLImageElement) (pcsdata: Frame) 
                      =
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


let init (ctx: WebGLRenderingContext) (spr: Sprite) : Renderer =
    
    let vsSrc = """
        attribute vec2 a_position;
        attribute vec2 a_texCoord;
        uniform vec2 u_resolution;
        varying vec2 v_texCoord;
        void main() {
          vec2 zeroToOne = a_position / u_resolution;
          vec2 zeroToTwo = zeroToOne * 2.0;
          vec2 clipSpace = zeroToTwo - 1.0;
          gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);
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
            console.error("=== GLSL SOURCE ===\n" + vsSrc)
            console.error("=== COMPILER LOG ===\n" + info)
            failwithf "Vertex shader compilation failed: %s" info
        s

    let fs = 
        let s = ctx.createShader(ctx.FRAGMENT_SHADER)
        ctx.shaderSource(s, fsSrc)
        ctx.compileShader(s)
        let ok = ctx.getShaderParameter(s, ctx.COMPILE_STATUS) |> unbox<bool>
        if not ok then
            let info = ctx.getShaderInfoLog(s) |> Option.ofObj |> Option.defaultValue "<no log>"
            console.error("=== GLSL SOURCE ===\n" + fsSrc)
            console.error("=== COMPILER LOG ===\n" + info)
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
    if isNull resLoc then console.warn("Uniform 'u_resolution' not found in shader")
    let imgLoc = ctx.getUniformLocation(prog, "u_image")
    if isNull imgLoc then console.warn("Uniform 'u_image' not found in shader")

    
    let tex = ctx.createTexture()
    ctx.bindTexture(ctx.TEXTURE_2D, tex)
    let placeholder = Constructors.Uint8Array.Create([|255uy; 0uy; 255uy; 255uy|]) :> obj :?> ArrayBufferView
    ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, 1, 1, 0, ctx.RGBA, ctx.UNSIGNED_BYTE, placeholder)
    spr.image.onload <- fun _ ->
        if spr.image.width = 0 || spr.image.height = 0 then
            console.error("Loaded image has invalid dimensions: ", spr.image.width, spr.image.height)
        ctx.bindTexture(ctx.TEXTURE_2D, tex)
        ctx.texImage2D(ctx.TEXTURE_2D, 0, ctx.RGBA, ctx.RGBA, ctx.UNSIGNED_BYTE, spr.image)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_S, ctx.CLAMP_TO_EDGE)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_WRAP_T, ctx.CLAMP_TO_EDGE)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MIN_FILTER, ctx.NEAREST)
        ctx.texParameteri(ctx.TEXTURE_2D, ctx.TEXTURE_MAG_FILTER, ctx.NEAREST)

    
    if posLoc < 0 then console.error("Attribute 'a_position' not found in shader program")
    if texLoc < 0 then console.error("Attribute 'a_texCoord' not found in shader program")

    { program = prog
      posBuf   = posBuf // Vertex buffer
      texBuf   = texBuf // Texture buffer
      posLoc   = posLoc // Vertex attribútum helye
      texLoc   = texLoc // Texture attribútum helye
      resLoc   = resLoc // Felbontás uniform helye
      imgLoc   = imgLoc // Textúra uniform helye
      texture  = tex } // Textúra objektum


let draw (ctx: WebGLRenderingContext) (r: Renderer) (spr: Sprite) =
    ctx.useProgram(r.program) 
    
    
    let x, y = spr.x, spr.y 
    let w, h = spr.width * spr.scale, spr.height * spr.scale 
    let cx, cy = x + w/2.0, y + h/2.0 
    let ca, sa = cos spr.rotation, sin spr.rotation 
    let transform (px, py) = 
        let dx, dy = px - cx, py - cy 
        let rx = dx*ca - dy*sa + cx  
        let ry = dx*sa + dy*ca + cy 
        rx, ry 

    let positions =
      [ x,y; x+w,y; x,y+h; x,y+h; x+w,y; x+w,y+h ] 
      |> List.map transform 
      |> List.collect (fun (u,v) -> [u;v]) 
      |> List.toArray 

    
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.posBuf)
    let posArr = Constructors.Float32Array.Create(positions :> obj)
    ctx.bufferData(ctx.ARRAY_BUFFER, U3.Case2(unbox<ArrayBufferView> posArr), ctx.STATIC_DRAW)
    ctx.enableVertexAttribArray(r.posLoc)
    ctx.vertexAttribPointer(r.posLoc, 2, ctx.FLOAT, false, 0, 0)

    
    ctx.bindBuffer(ctx.ARRAY_BUFFER, r.texBuf)
    
    let texCoords = spr.vertices.[spr.no]
    let texArr = Constructors.Float32Array.Create(texCoords :> obj)
    ctx.bufferData(ctx.ARRAY_BUFFER, U3.Case2(unbox<ArrayBufferView> texArr), ctx.STATIC_DRAW)
    ctx.enableVertexAttribArray(r.texLoc)
    ctx.vertexAttribPointer(r.texLoc, 2, ctx.FLOAT, false, 0, 0)

   
    ctx.uniform2f(r.resLoc, float ctx.canvas.width, float ctx.canvas.height)

    
    ctx.activeTexture(ctx.TEXTURE0)
    ctx.bindTexture(ctx.TEXTURE_2D, r.texture)
    ctx.uniform1i(r.imgLoc, 0)

    
    ctx.drawArrays(ctx.TRIANGLES, 0, 6)


let create (url: string) (position: float * float) (size: float * float) : Sprite =
    let x, y = position
    let width, height = size
    let image = document.createElement "img" :?> HTMLImageElement
    image.src <- url
    // Initialize default frame for texture coords
    let defaultFrame = { startx = 0.0; starty = 0.0; width = width; height = height; col = 1; row = 1; animation = false; no = 0 }
    let cellCoords = getCellTexCoords image defaultFrame
    let verticesArr = [| cellCoords |]
    { image = image
      x = x; y = y
      width = width; height = height
      scale = 1.0; rotation = 0.0
      vertices = verticesArr
      animation = defaultFrame.animation
      no = defaultFrame.no }









