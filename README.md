# ODIN Unity Sample Project

Please check out https://developers.4players.io/odin/ for more info on ODIN.

More info on this sample project can be found here: https://developers.4players.io/odin/guides/unity/pun-sample/.

## Download the Windows Binary

You can download the Windows binary here: https://github.com/4Players/odin-unity-demo/releases/latest

## Download for Unity

**Please note**: This repository uses LFS. You need to clone this repo with LFS enabled. **Downloading the ZIP file via Githubs Download ZIP functionality does not work!**


# ODIN Sample: Multiplayer with Photon PUN 2
In this guide we’ll walk you through the basic concepts of integrating ODIN into a game built with PUN 2. If you are unsure why you should use ODIN for that, learn more about our features and what makes us special in our [introduction](https://developers.4players.io/odin/introduction/).

## Multiplayer

Work in Progress

## Audio

To better showcase the capabilities of ODIN in apps and games, we've implemented some audio
features that are often used in games, but not included in Unity's Audio System: Audio Occlusion and Directional Audio. Because we want to keep things simple
and performant, we're going to approximate those effects, using Unity's ``AudioLowPassFilter`` component
and by adjusting the volume of individual audio sources.

### Audio Occlusion

Audio Occlusion should occur when some object is placed between the ears of our player and audio sources in the scene - imagine
hearing the muffled sounds of an enemy approaching from behind a wall.
Unity does not have any kind of built-in audio occlusion, so we need to implement our own system. 
The 
`OcclusionAudioListener` script contains most of the occlusion logic and is placed with the `AudioListener` script
on our local player object. The script registers objects with colliders, that enter the script's detection range and have at 
least one `AudioSource` script attached in the collider's transform hierarchy. By default the detection range 
is set to 100 meters - Audio Sources that are farther away than that are usually 
not loud enough to be affected meaningfully by our occlusion system.
Each frame, we then apply the occlusion effects to each of the registered Audio Sources. Our occlusion effects have the parameters 
`Volume`, `Cutoff Frequency` and `Lowpass Resonance Q`.
- **Volume:** Multiplier for the audio source's volume.
- **Cutoff Frequency:** Removes all frequencies above this value from the output of the Audio Source. This value is probably
the most important for our occlusion effect, as is makes audio sound muffled. The cutoff frequency can range
from 0 to 22.000 Hz.
- **Lowpass Resonance Q:** This value determines how much the filter dampens self-resonance. This basically means, the 
higher the value, the better sound is transmitted through the material the filter is representing. E.g. for imitating an iron
door, the `Lowpass Resonance Q` value should be higher than for imitating a wooden door.


he occlusion effect is based on the thickness of objects between our 
``AudioListener`` and a ``AudioSource``. For each audio source we check for colliders placed between the listener and the source and
determine the thickness of the collider. The combined thickness of all colliders we find is then used to look up a cutoff 
frequency from settings. 

Work in Progress

### Directional Audio

Work in Progress

### Game Logic

Work in Progress


