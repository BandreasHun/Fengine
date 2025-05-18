module Engine.Runtime

open Browser.Dom
open Browser.Types
open Engine.Sprite
open Fable.SimpleHttp
open Fable.SimpleJson
open System.Collections.Generic 

let public canvas = document.querySelector("canvas") :?> HTMLCanvasElement
let public gl = canvas.getContext("webgl") :?> WebGLRenderingContext

let public renderer : Sprite.Renderer =
    let placeholderSprite : Sprite = 
        { image = document.createElement "img" :?> HTMLImageElement
          texture = Sprite.createTextureForImage (document.createElement "img" :?> HTMLImageElement)
          texturePath = "" 
          x = 0.0; y = 0.0; velocityX = 0.0; velocityY = 0.0; affectedByGravity = false; width = 1.0; height = 1.0
          scale = 1.0; rotation = 0.0
          isGrounded = false 
          no = 0; vertices = ResizeArray<float array>(); animation = false
          isAnimating = false
          id = 0
          flipX = false }
    Sprite.init gl placeholderSprite
    
type Frame = { 
      startx: float
      starty: float
      width: float
      height: float
      col: int
      row: int
      animation : bool
          }

type public SpriteDatas = Map<string, Frame list>
        
let loadSpriteAtlas () = 
      async {
          let! (status, responseText) = Http.get "/sprites.json"
          if status = 200 then
              try
                  let atlas : SpriteDatas = Json.parseAs responseText
                  return Some atlas
              with ex ->
                  Browser.Dom.console.error("Error parsing sprites.json in Runtime.fs:", ex.Message)
                  return None
          else
              Browser.Dom.console.error("Failed to load sprites.json in Runtime.fs, status:", status)
              return None
        }
