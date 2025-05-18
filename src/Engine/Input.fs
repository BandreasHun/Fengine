namespace Engine

module Input =
    open Browser.Dom
    open Browser.Types

    let mutable private pressed = Set.empty<string>
    let mutable private mouseJustClicked: (float * float) option = None

    let Init () =
        window.addEventListener("keydown", fun (e: Event) ->
            let ke = e :?> KeyboardEvent
            pressed <- pressed.Add ke.key
        )
        window.addEventListener("keyup", fun (e: Event) ->
            let ke = e :?> KeyboardEvent
            pressed <- pressed.Remove ke.key
        )
        let canvasElement = document.querySelector("canvas") :?> HTMLCanvasElement
        canvasElement.addEventListener("mousedown", fun (e: Event) -> 
            let me = e :?> MouseEvent
            let rect = canvasElement.getBoundingClientRect()
            let canvasX = float (me.clientX - rect.left)
            let canvasY = float (me.clientY - rect.top)
            mouseJustClicked <- Some (canvasX, canvasY)
        )

    let IsDown (key:string) : bool =
        pressed.Contains key

    let ConsumeClick() : (float * float) option =
        let click = mouseJustClicked
        mouseJustClicked <- None
        click
