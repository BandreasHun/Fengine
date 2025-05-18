\
namespace Engine

module Engine.SpriteAtlas =

    open Browser.Dom
    open Fable.SimpleHttp
    open Fable.SimpleJson
    open Fable.Core.JsInterop
    open System.Collections.Generic
    open Browser.Types
    
    type ImageData = {
        startx: float
        starty: float
        width: float
        height: float
        col: int
        row: int
        animation: bool
        mutable no: int
        image: HTMLImageElement
    }

    type public SpriteDatas = Map<string, ImageData>
    let mutable SpriteDatass: SpriteDatas = Map.empty

    let getdataa (source: string) =
        let frame = SpriteDatass.[source]
        
        frame

    
    let loadImageAsync (img: HTMLImageElement) : Async<unit> =
        Async.FromContinuations(fun (cont, _, _) -> 
            img.onload <- fun _ -> cont ()
            img.onerror <- fun _ -> cont ()
        )

    let loadSpriteAtlas() =
        async {
            let! (status, responseText) = Http.get "/sprites.json"
            if status = 200 then
                try
                    let raw: Map<string, obj list> = Json.parseAs responseText
                    let atlas: SpriteDatas =
                        raw
                        |> Map.map (fun key frames -> 
                            let f = unbox<ImageData> (List.head frames)
                            let img = Browser.Dom.document.createElement("img") :?> HTMLImageElement
                            img.src <- key
                            { f with image = img }
                        )
                    SpriteDatass <- atlas
                    
                    let! _ =
                        atlas
                        |> Map.toSeq
                        |> Seq.map (fun (_, data) -> loadImageAsync data.image)
                        |> Async.Parallel
                    return Some atlas
                with ex ->
                    System.Console.WriteLine(ex.Message)
                    return None
            else
                System.Console.WriteLine("Failed to load sprite atlas, status: " + string status)
                return None
        }
