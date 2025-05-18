module SoundManager

open Browser.Types
open Browser.Dom
open System.Collections.Generic
open Fable.Core.JS

let private effectsCache = Dictionary<string, HTMLAudioElement>()
let private musicTracks = Dictionary<string, HTMLAudioElement>()

let mutable masterVolume = 1.0
let mutable effectsSubVolume = 1.0
let mutable musicSubVolume = 1.0

let private calculateActualVolume (isMusic: bool) =
    let subVolume = if isMusic then musicSubVolume else effectsSubVolume
    masterVolume * subVolume

let loadSound (id: string) (path: string) (isMusicTrack: bool) =
    try
        let audioElem = document.createElement "audio"
        let audio = audioElem :?> HTMLAudioElement
        audio.src <- path

        audio.volume <- calculateActualVolume isMusicTrack
        if isMusicTrack then
            if musicTracks.ContainsKey(id) then
                musicTracks.[id] <- audio
        else
            if effectsCache.ContainsKey(id) then
                effectsCache.[id] <- audio
    with
    | ex -> ()

let playSound (id: string) (loop: bool option) =
    let findAndPlay (cache: Dictionary<string, HTMLAudioElement>) (isMusic: bool) =
        match cache.TryGetValue(id) with
        | true, audio ->
            audio.volume <- calculateActualVolume isMusic
            if isMusic then
                audio.loop <- defaultArg loop false
                audio.play() |> ignore
            else
                let effectAudioElem = document.createElement "audio"
                let effectInstance = effectAudioElem :?> HTMLAudioElement
                effectInstance.src <- audio.src
                effectInstance.volume <- audio.volume
                effectInstance.play() |> ignore
            true
        | false, _ -> false

    if not (findAndPlay musicTracks true) then
        if not (findAndPlay effectsCache false) then
            ()

let pauseMusic (id: string) =
    match musicTracks.TryGetValue(id) with
    | true, audio -> audio.pause()
    | false, _ -> ()

let stopMusic (id: string) =
    match musicTracks.TryGetValue(id) with
    | true, audio ->
        audio.pause()
        audio.currentTime <- 0.0
    | false, _ -> ()

let private updateAllVolumes () =
    for kvp in effectsCache do kvp.Value.volume <- calculateActualVolume false
    for kvp in musicTracks do kvp.Value.volume <- calculateActualVolume true

let setMasterVolume (level: float) =
    masterVolume <- max 0.0 (min 1.0 level)
    updateAllVolumes()

let setEffectsVolume (level: float) =
    effectsSubVolume <- max 0.0 (min 1.0 level)
    updateAllVolumes()

let setMusicVolume (level: float) =
    musicSubVolume <- max 0.0 (min 1.0 level)
    updateAllVolumes()