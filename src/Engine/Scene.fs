module Engine.Scene

open Browser.Dom
open Browser.Types
open Engine.Sprite
open Engine.Runtime 
open System.Collections.Generic
open Engine.Camera 

let gl = Engine.Runtime.gl 

let mutable private renderer : Renderer option = None
let private animElapsed = Dictionary<int, float>()

// Default animation frame duration
let mutable private defaultAnimationFrameDuration = 0.2 // másodperc

// Background color
let mutable private backgroundColor = (0.1, 0.1, 0.1, 1.0) // Defined backgroundColor

// Placeholder sprite for renderer initialization
let private placeholderSprite = 
    { image = document.createElement "img" :?> HTMLImageElement
      texture = Sprite.createTextureForImage (document.createElement "img" :?> HTMLImageElement)
      texturePath = "" 
      x = 0.0; y = 0.0; velocityX = 0.0; velocityY = 0.0; affectedByGravity = false; width = 1.0; height = 1.0
      scale = 1.0; rotation = 0.0
      no = 0; vertices = ResizeArray<float array>(); animation = false; id = 0; isGrounded = false; isAnimating = false; flipX = false }

let setBackgroundColor (r: float) (g: float) (b: float) (a: float) =
    backgroundColor <- (r, g, b, a)

let setDefaultAnimationFrameDuration (duration: float) =
    defaultAnimationFrameDuration <- duration

let ensureRenderer () =
    if renderer.IsNone then
        renderer <- Some (init gl (items |> Seq.tryHead |> Option.defaultValue placeholderSprite))

let updateAnimations (dt: float) = // dt is in seconds
    for spr in Sprite.items do
        if spr.animation && spr.isAnimating then // HOZZÁADVA: spr.isAnimating ellenőrzés
            // Pepe animációját (képkocka léptetését) a main.fs/update kezeli
            // ezért itt kihagyjuk, ha a sprite neve "pepe.png"
            // Ez egy egyszerű megoldás, feltételezve, hogy a texturePath egyedi azonosítóként használható itt.
            if spr.texturePath <> "pepe.png" then
                let frameCount = spr.vertices.Count
                if frameCount > 0 then
                    let frameDuration = defaultAnimationFrameDuration
                    // GetValueOrDefault helyett TryGetValue használata
                    let mutable elapsed = 0.0
                    let success = animElapsed.TryGetValue(spr.id, &elapsed)
                    // Ha a kulcs nem létezett, az elapsed alapértelmezésben 0.0 marad (a mutable definíció miatt),
                    // vagy a TryGetValue false-t ad vissza, és az elapsed értéke nem változik (de itt már 0.0-ra inicializáltuk).
                    // A Fable fordítási sajátosságok miatt expliciten kezeljük, ha nem volt sikeres a lekérdezés,
                    // bár a mutable elapsed = 0.0 már ezt biztosítja.
                    if not success then
                        elapsed <- 0.0 // Biztosítjuk, hogy 0.0 legyen, ha a kulcs nem volt a dictionary-ben

                    elapsed <- elapsed + dt
                    if elapsed >= frameDuration then
                        let framesToAdvance = floor (elapsed / frameDuration)
                        spr.no <- (spr.no + int framesToAdvance) % frameCount
                        animElapsed.[spr.id] <- elapsed - (framesToAdvance * frameDuration)
                    else
                        animElapsed.[spr.id] <- elapsed

let public draw (camera: Camera.Camera) (_dt: float) =
    ensureRenderer()
    let r, g, b, a = backgroundColor
    gl.clearColor(r, g, b, a) 
    gl.clear gl.COLOR_BUFFER_BIT
    match renderer with
    | Some currentRenderer -> 
        let camViewXStart = camera.x
        let camViewYStart = camera.y
        let camViewXEnd = camera.x + camera.viewportWidth / camera.zoom
        let camViewYEnd = camera.y + camera.viewportHeight / camera.zoom

        for spr in Sprite.items do
            let spriteXStart = spr.x
            let spriteYStart = spr.y
            let spriteActualWidth = spr.width * spr.scale 
            let spriteActualHeight = spr.height * spr.scale
            let spriteXEnd = spr.x + spriteActualWidth
            let spriteYEnd = spr.y + spriteActualHeight

            let isVisible = // Defined isVisible
                spriteXEnd >= camViewXStart &&
                spriteXStart <= camViewXEnd &&
                spriteYEnd >= camViewYStart &&
                spriteYStart <= camViewYEnd
            
            if isVisible then
                Sprite.draw gl currentRenderer spr camera.x camera.y camera.zoom 
    | None -> ()




