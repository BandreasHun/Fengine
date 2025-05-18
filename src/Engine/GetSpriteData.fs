module Engine.GetSpriteData

open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom
open System.Collections.Generic
open Fable.SimpleHttp
open Fable.SimpleJson

type FrameJsonData = { 
    startx: float
    starty: float
    width: float
    height: float
    col: int
    row: int
    animation: bool
    no: int
}

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

let private spriteDataStore = ref Map.empty<string, ImageData>

let getdataa (source: string) : ImageData =
    (spriteDataStore.Value).[source]

let private loadImageAsync (img: HTMLImageElement) : Async<unit> =
    Async.FromContinuations(fun (success, error, _) ->
        if img.complete && img.naturalWidth > 0 then
            success ()
        else
            img.onload <- fun (_:Event) -> success ()
            img.onerror <- fun (ev:Event) ->
                Browser.Dom.console.error("Failed to load image:", img.src, ev)
                success ()
    )

let loadSpriteAtlas () : Async<unit> = 
    async {
        let! (status, responseText) = Http.get "/sprites.json"
        if status = 200 then
            try
                let framesFileMap = Json.parseAs<Map<string, FrameJsonData list>> responseText 
                let imageDataMapEntries =
                    framesFileMap
                    |> Map.toList
                    |> List.map (fun (key, frameDataList) ->
                        let fData = List.head frameDataList
                        let img = document.createElement "img" :?> HTMLImageElement
                        img.src <- key
                        let imgData = {
                            startx = fData.startx
                            starty = fData.starty
                            width = fData.width
                            height = fData.height
                            col = fData.col
                            row = fData.row
                            animation = fData.animation
                            no = fData.no
                            image = img
                        }
                        key, imgData
                    )
                let finalImageDataMap = Map.ofList imageDataMapEntries
                spriteDataStore.Value <- finalImageDataMap

                let imageLoadTasks =
                    finalImageDataMap
                    |> Map.values
                    |> Seq.map (fun data -> loadImageAsync data.image)
                    |> List.ofSeq 

                if not (List.isEmpty imageLoadTasks) then
                    let! _imageLoads = Async.Parallel imageLoadTasks |> Async.Ignore
                    Browser.Dom.console.log("All sprite images initiated loading via GetSpriteData.") 
                else
                    Browser.Dom.console.log("Sprite atlas loaded (GetSpriteData), but no images to load.")
                return ()
            with ex ->
                Browser.Dom.console.error("Error processing sprite atlas JSON (GetSpriteData):", ex.Message, ex) 
                return ()
        else
            Browser.Dom.console.error("Failed to load sprites.json (GetSpriteData). Status:", status, "Content:", responseText)
            return ()
    }
