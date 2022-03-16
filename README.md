# ODIN Unity Sample Project

Please check out https://developers.4players.io/odin/ for more info on ODIN.

More info on this sample project can be found here: https://developers.4players.io/odin/guides/unity/pun-sample/.

## Download the Binary

You can download the binaries here: https://github.com/4Players/odin-unity-demo/releases/latest

## Download for Unity

**Please note**: This repository uses LFS. You need to clone this repo with LFS enabled. **Downloading the ZIP file via Githubs Download ZIP functionality does not work!**


# ODIN Sample: Extended Audio System and Multiplayer with Photon PUN 2
In this guide we’ll walk you through the basic concepts of integrating ODIN into a multiplayer game. In this sample we'll be using
the Photon PUN 2 multiplayer framework, but ODIN can be integrated into your game using any multiplayer solution or
even without multiplayer. The ODIN-Sample itself is also built in a way, that allows us to easily switch out Photon.

If you are unsure why you should use ODIN for that, learn more about our features and what makes us special in our [introduction](https://developers.4players.io/odin/introduction/).

## Multiplayer

### Lobby

Photon login and join room --> load scene

on load scene: Join ODIN Rooms (Voice and Radio)

### ODIN

Todo

#### OdinHandler

Todo
#### Rooms, Peers and Media
Todo

#### PlaybackComponent

The ODIN SDK provides the `PlaybackComponent` script to easily play back audio data received from
the ODIN server. Each `PlaybackComponent` represents one media stream and is identified by a media id, a
peer id and a room name.

### ODIN Global Voice Chat - Radio transmissions

In the sample project, users automatically join the ODIN room named _Radio_, in
which players can communicate as if using radio transmitters - when pressing down `V`, the microphone input
can be heard by all players in the room independent of their position.

For this scenario, the sample project provides the `OdinDefaultUser`script, which uses the 
`OdinHandler.Instance.OnMediaAdded` Event to spawn an instance of a prefab with a `PlaybackComponent` for
each media stream in a given room. The event provides the room name, peer id and media id required
for the `PlaybackComponent` to work. 

The `OdinDefaultUser` simply
instantiates the `PlaybackComponent` prefabs as children of a single game object, so 
the `OdinDefaultUser` script should only be used for ODIN rooms which do not use proximity voice chat.


### ODIN Proximity Chat - connecting ODIN to the multiplayer framework

In a multiplayer scenario, we encounter the issue of connecting a user's ODIN media stream to the user's avatar
in the game, e.g. in our sample project we'd want a player's voice in the proximity chat to originate
from the player's capsule.

The abstract `AOdinMultiplayerAdapter` script gives access to the methods `string GetUniqueUserId()` and 
`bool IsLocalUser()`. This adapter is used to connect the player's representation in the multiplayer 
framework (using the framework's equivalent of an unique user id) to an ODIN peer. On ODIN's side we use 
custom user data to keep track of that id. When joining an ODIN room, the `AOdinMultiplayerAdapter` 
automatically sets the `uniqueUserId` of our custom ODIN user data for the current peer and sends an 
update to the other peers in the room. On receiving the update, those clients then use a reference to `AOdinMultiplayerAdapter` 
to compare the remote peer's `uniqueUserId` to the id supplied by the remote adapter's `GetUniqueUserId()`.
If both ids are equal,we know that an ODIN peer is represented by the referenced `AOdinMultiplayerAdapter`.

In the sample project
we use this information to correctly play black the proximity chat audio at a player's location -
specifically using the `Odin3dAudioVoiceUser`, which automatically creates a `PlaybackComponent` for each remote user.

The sample project uses Photon as a multiplayer framework, so we add the
`PhotonToOdinAdapter` to our player. The script uses `PhotonView.ViewID` as a unique user id and
`PhotonView.IsMine`
to determine whether the adapter represents a local or a remote player. To switch out Photon for another
multiplayer framework, simply provide your own implementation of `AOdinMultiplayerAdapter`.

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



## Game Logic

Work in Progress


