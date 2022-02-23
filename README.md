# ODIN Sample: Multiplayer with Photon PUN 2
In this guide we’ll walk you through the basic concepts of integrating ODIN into a game built with PUN 2. If you are unsure why you should use ODIN for that, learn more about our features and what makes us special in our [introduction](https://developers.4players.io/odin/introduction/).

## Multiplayer



## Audio

To better showcase the capabilities of ODIN in apps and games, we've implemented some audio
features that are often used in games, but not included in Unity's Audio System: Audio Occlusion and Directional Audio. Because we want to keep things simple
and performant, we're going to approximate those effects.

### Audio Occlusion

Audio Occlusion should occur when some object is placed between the ears of our player and audio sources in the scene - imagine
hearing the muffled sounds of an enemy approaching from behind a wall.
Unity does not have any kind of built-in audio occlusion, so we need to implement our own system. The 
`OcclusionAudioListener` script contains most of the occlusion logic and is placed with the `AudioListener` script
on our local player object. 

The `OcclusionAudioListener` registers objects with colliders, that enter the script's detection range and have at 
least one `AudioSource` script attached in the collider's transform hierarchy. By default the detection range 
is set to 100 meters - Audio Sources that are farther away than that are usually 
not loud enough to be affected meaningfully by our occlusion system.

Each frame, we then apply the occlusion effects to each of the registered Audio Sources. To determine the type
off occlusion effect, the system has two different modes:

1.  **Default:** The occlusion effect is based on the thickness of objects between our ``AudioListener`` and a ``AudioSource``.
2. Customized occlusion: By adding a ``AudioObstacle`` script to a game object, 


### Directional Audio

