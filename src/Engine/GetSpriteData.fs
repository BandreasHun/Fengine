namespace Engine

module GetSpriteData =

    open Browser.Dom
    open Fable.SimpleHttp
    open Fable.SimpleJson
    open Fable.Core.JsInterop
    open System.Collections.Generic

    
    type Frame = {
        startx: float
        starty: float
        width: float
        height: float
        col: int
        row: int
        animation: bool
        no: int
    }
    type public SpriteDatas = Map<string, Frame list>

    let getdataa (spriteData: SpriteDatas, surce: string) =
        let frames = spriteData.[surce]
        let first = List.head frames
        printfn "Első frame: %A" first
        { startx = first.startx; starty = first.starty; width = first.width; height = first.height; col = first.col; row = first.row; animation = first.animation; no = first.no }



    let loadSpriteAtlas() =
        console.log("Sprite atlas betöltése...")
        async {
            console.log("async let!")
            let! (status, responseText) = Http.get "/sprites.json"
            if status = 200 then
                console.log("status: ", status)
                console.log("responseText: ", responseText)
                try
                    let atlas: SpriteDatas = Json.parseAs responseText
                    console.log("Atlas betöltve: ", atlas)
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
