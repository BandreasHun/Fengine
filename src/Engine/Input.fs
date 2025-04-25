// src/Engine/Input.fs
namespace Engine

module Input =
    open Browser.Dom
    open Browser.Types

    // internal mutable state: which keys are currently held down
    let mutable private pressed = Set.empty<string>

    /// Call once at startup to wire up key listeners
    let Init () =
        window.addEventListener("keydown", fun (e: Event) ->
            let ke = e :?> KeyboardEvent
            pressed <- pressed.Add ke.key
        )
        window.addEventListener("keyup", fun (e: Event) ->
            let ke = e :?> KeyboardEvent
            pressed <- pressed.Remove ke.key
        )

    /// Returns true if the given key (e.g. "ArrowUp", "a", " ") is currently down
    let IsDown (key:string) : bool =
        pressed.Contains key
