namespace Engine

 module Runtime =

    open Browser.Dom
    open Browser.Types
    open Engine.Sprite
    open Fable.SimpleHttp
    open Fable.SimpleJson
    

    let public canvas = document.querySelector("canvas") :?> HTMLCanvasElement
    let public gl = canvas.getContext("webgl") :?> WebGLRenderingContext
    
    let public renderer : Sprite.Renderer =
        let placeholderSprite = 
            { image = document.createElement("img") :?> HTMLImageElement
              x = 0.0; y = 0.0; width = 1.0; height = 1.0
              scale = 1.0; rotation = 0.0
              ; no = 0 ; vertices = [||] }
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
          console.log("Sprite atlas betöltése...")
          async {
              console.log("async let!")
              let! (status, responseText) = Http.get "/sprites.json"
              if status = 200 then
                  console.log("status: ", status)
                  console.log("responseText: ", responseText)
                  // Parse the JSON response into a Map<string, Frame list>
                  try
                      let atlas : SpriteDatas = Json.parseAs responseText
                      console.log("Atlas betöltve: ", atlas)
                      // Log frames for all sprites in the atlas
                      atlas |> Map.iter (fun file framesList ->
                          printfn "Betöltött %s frame-ek: %A" file framesList
                      )

                      return Some atlas
                  with ex ->
                      printfn "Hibás JSON: %s" ex.Message
                      console.log("Hibás JSON: ", responseText)
                      return None
              else
                  printfn "Hiba a lekérés során: %d" status
                  console.log("Hiba a lekérés során: ", responseText)
                  return None
            }
    
   
  

    /// Memóriába tárolt sprite adatok (filename -> SpriteDatas[])
    let mutable public spriteDatasMap : Map<string, SpriteDatas[]> = Map.empty



