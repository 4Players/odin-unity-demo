# ODIN Unity Sample Project

Please check out https://developers.4players.io/odin/ for more info on ODIN.

More info on this sample project can be found here: https://developers.4players.io/odin/guides/unity/pun-sample/.

## Download the Windows Binary

You can download the Windows binary here: https://github.com/4Players/odin-unity-demo/releases/latest

## Download for Unity

**Please note**: This repository uses LFS. You need to clone this repo with LFS enabled. **Downloading the ZIP file via Githubs Download ZIP functionality does not work!**


# ODIN Sample: Extended Audio System and Multiplayer with Photon PUN 2
In this guide we’ll walk you through the basic concepts of integrating ODIN into a multiplayer game. In this sample we'll be using
the Photon PUN 2 multiplayer framework, but ODIN can be integrated into your game using any multiplayer solution or
even without multiplayer. The ODIN-Sample itself is also built in a way, that allows us to easily switch out Photon.

If you are unsure why you should use ODIN for that, learn more about our features and what makes us special in our [introduction](https://developers.4players.io/odin/introduction/).

## Audio

To better showcase the capabilities of ODIN in apps and games, we've implemented some audio
features that are often used in games, but not included in Unity's Audio System: Audio Occlusion and Directional Audio. Because we want to keep things simple
and performant, we're going to approximate those effects, using Unity's ``AudioLowPassFilter`` component
and by adjusting the volume of individual audio sources.

### Audio Occlusion

Audio Occlusion should occur when an object is placed between the audio listener (our player) and audio sources in the scene - e.g.
hearing the muffled sounds of an enemy approaching from behind a wall.
Unity does not have any kind of built-in audio occlusion, so we need to implement our own system. 
The 
`OcclusionAudioListener` script contains most of the occlusion logic and is placed with the `AudioListener` script
on our local player object. The `OcclusionAudioListener` registers objects with colliders, that enter the detection range and have at 
least one `AudioSource` script attached in the transform hierarchy. By default the detection range 
is set to 100 meters - Audio Sources that are farther away than that are usually 
not loud enough to be affected meaningfully by our occlusion system.
We then apply the occlusion effects to each of the registered Audio Sources in every frame. 

Our occlusion effects have the parameters 
`Volume`, `Cutoff Frequency` and `Lowpass Resonance Q`:
- **Volume:** Multiplier for the audio source's volume.
- **Cutoff Frequency:** Removes all frequencies above this value from the output of the Audio Source. This value is probably
the most important for our occlusion effect, as is makes audio sound muffled. The cutoff frequency can range
from 0 to 22.000 Hz.
- **Lowpass Resonance Q:** This value determines how much the filter dampens self-resonance. This basically means, the 
higher the value, the better sound is transmitted through the material the filter is representing. E.g. for imitating an iron
door, the `Lowpass Resonance Q` value should be higher than for imitating a wooden door.

The occlusion effect is based on the thickness of objects between our 
`AudioListener` and the `AudioSource`. For each audio source we check for colliders placed between the listener and the source (using raycasts) and
determine the thickness of the collider. This thickness value is then used to look
up the final effect values from an `AudioEffectDefinition` ScriptableObject. For each of 
the three parameters `Volume`, `Cutoff Frequency` and `Lowpass Resonance Q` the ScriptableObject
contains a curve mapping from the collider's thickness on the x-Axis to the parameter value
on the y-Axis.

The `AudioEffectDefinition` is retrieved using one of two options:
- By placing an `AudioObstacle` script on the collider's gameobject. This can be
used to customize a collider's occlusion effect and give it a certain material's damping
behaviour. The sample uses the `AudioObstacle` to define the occlusion effect of a brick wall,
a wooden door, a glass pane or even a 100% soundproof barrier.
- Using the default `AudioEffectDefinition` - this option is used, if no `AudioObstacle`
is attached to the collider. 

You can create your own `AudioEffectDefinition` by using the `Create > Odin-Sample > AudioEffectDefinition` 
menu in your project hierarchy. 

### Directional Audio

Unity's built in audio system allows us to hear differences between sounds coming from left 
or right, but not whether the object is in front or behind us. The `DirectionalAudioListener` script will take care
of this using basically the same effects as the audio occlusion system. 

Similar to the `OcclusionAudioListener`, we apply an effect to each Audio Source in 
the detection range - but instead of using the thickness of objects between source and listener,
we interpolate the effect based on the angle between the listener's forward vector and a vector
pointing from the listener to the audio source. The 

Note: The implementation won't let us differentiate between sound coming from above or below. To implement this behaviour, 
please take a look at the implementation of [Head Related Transfer Functions (HRTF)](https://en.wikipedia.org/wiki/Head-related_transfer_function).

## Multiplayer

Work in Progress

## Game Logic

Work in Progress


