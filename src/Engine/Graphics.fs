namespace Engine

module Engine.Graphics =

    open Browser.Dom
    open Browser.Types
    open Engine.Sprite
    open System.Collections.Generic 
    

    let public canvas = document.querySelector("canvas") :?> HTMLCanvasElement
    let public gl = canvas.getContext("webgl") :?> WebGLRenderingContext
    
    let public renderer : Sprite.Renderer =
        let placeholderSprite = 
            { image = document.createElement "img" :?> HTMLImageElement
              texture = Sprite.createTextureForImage (document.createElement "img" :?> HTMLImageElement)
              texturePath = ""
              x = 0.0; y = 0.0; velocityX = 0.0; velocityY = 0.0; affectedByGravity = false; width = 1.0; height = 1.0
              scale = 1.0; rotation = 0.0
              isGrounded = false
              no = 0; vertices = ResizeArray<float array>(); animation = false
              id = -1 }
        Sprite.init gl placeholderSprite
