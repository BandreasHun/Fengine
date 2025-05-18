import { disposeSafe, getEnumerator, defaultOf, createAtom } from "../fable_modules/fable-library-js.4.24.0/Util.js";
import { tryGetValue } from "../fable_modules/fable-library-js.4.24.0/MapUtil.js";
import { FSharpRef } from "../fable_modules/fable-library-js.4.24.0/Types.js";
import { defaultArg } from "../fable_modules/fable-library-js.4.24.0/Option.js";
import { min, max } from "../fable_modules/fable-library-js.4.24.0/Double.js";

const effectsCache = new Map([]);

const musicTracks = new Map([]);

export let masterVolume = createAtom(1);

export let effectsSubVolume = createAtom(1);

export let musicSubVolume = createAtom(1);

function calculateActualVolume(isMusic) {
    const subVolume = isMusic ? musicSubVolume() : effectsSubVolume();
    return masterVolume() * subVolume;
}

export function loadSound(id, path, isMusicTrack) {
    try {
        const audio = document.createElement("audio");
        audio.src = path;
        audio.volume = calculateActualVolume(isMusicTrack);
        if (isMusicTrack) {
            if (musicTracks.has(id)) {
                musicTracks.set(id, audio);
            }
        }
        else if (effectsCache.has(id)) {
            effectsCache.set(id, audio);
        }
    }
    catch (ex) {
    }
}

export function playSound(id, loop) {
    const findAndPlay = (cache, isMusic) => {
        let matchValue;
        let outArg = defaultOf();
        matchValue = [tryGetValue(cache, id, new FSharpRef(() => outArg, (v) => {
            outArg = v;
        })), outArg];
        if (matchValue[0]) {
            const audio = matchValue[1];
            audio.volume = calculateActualVolume(isMusic);
            if (isMusic) {
                audio.loop = defaultArg(loop, false);
                const value = audio.play();
            }
            else {
                const effectInstance = document.createElement("audio");
                effectInstance.src = audio.src;
                effectInstance.volume = audio.volume;
                const value_1 = effectInstance.play();
            }
            return true;
        }
        else {
            return false;
        }
    };
    if (!findAndPlay(musicTracks, true)) {
        if (!findAndPlay(effectsCache, false)) {
        }
    }
}

export function pauseMusic(id) {
    let matchValue;
    let outArg = defaultOf();
    matchValue = [tryGetValue(musicTracks, id, new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        matchValue[1].pause();
    }
}

export function stopMusic(id) {
    let matchValue;
    let outArg = defaultOf();
    matchValue = [tryGetValue(musicTracks, id, new FSharpRef(() => outArg, (v) => {
        outArg = v;
    })), outArg];
    if (matchValue[0]) {
        const audio = matchValue[1];
        audio.pause();
        audio.currentTime = 0;
    }
}

function updateAllVolumes() {
    let enumerator = getEnumerator(effectsCache);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const kvp = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            kvp[1].volume = calculateActualVolume(false);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    let enumerator_1 = getEnumerator(musicTracks);
    try {
        while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
            const kvp_1 = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]();
            kvp_1[1].volume = calculateActualVolume(true);
        }
    }
    finally {
        disposeSafe(enumerator_1);
    }
}

export function setMasterVolume(level) {
    masterVolume(max(0, min(1, level)));
    updateAllVolumes();
}

export function setEffectsVolume(level) {
    effectsSubVolume(max(0, min(1, level)));
    updateAllVolumes();
}

export function setMusicVolume(level) {
    musicSubVolume(max(0, min(1, level)));
    updateAllVolumes();
}

