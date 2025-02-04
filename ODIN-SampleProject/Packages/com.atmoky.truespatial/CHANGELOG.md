## v1.4.1

- fixes compilation errors when using Unity 2021.3

## v1.4.0

- moved atmoky scripts into its own namespace (breaking change)
- substantial performance improvements, especially on Windows & Linux/Android
- added visionos support
- improved occlusion script: optionally use non-allocating raycast for better performance, occlusion layer mask setting
- Spatializer: added option to directly output binaural audio
- Renderer: added optional input passthrough feature
- Externalizer: moved out of renderer to separate plug-in

## v1.3.0

- added atmoky Receiver plug-in: allows for pre-spatialization sends e.g. to reverb or effect buses
- added atmokyOcclusionProbeGroup component and atmokyOccluder for automatic ray-based occlusion computation
- added Occlusion Sample Scene to demonstrate atmokyOcclusionProbeGroup feature
- fixes potential initial burst of noisy audio samples when switching from no output to an output bus and prelistening in edit-mode

## v1.2.0

- added Directivity Presets to atmoky Source script
- added Sample Scenes to package
- fixed potential audio silence caused by invalid rotation matrices sent from Unity

## v1.1.0

- Near-field Effects: When the listener is within the near-field of a source, nearfield rendering can be used to create ASMR-like effects.
- Parallax Effect: Sources near to the listener will have a parallax effect applied to them, making them more realistic. This also results in smoother transitions when sounds go through the listener.
- Stereo Rendering: The spatial spread parameter of an audio source will affect stereo signals, enabling preservation of the stero image without changing the sound location.
- Gizmos and Tools: Orientation of Ambisonic sources will be displayed, as well as directivity patterns of regular audio sources. There is also an editor tool which allows for quick adjustment of the source's directivity pattern.
- UI improvements: All plug-ins and scripts come with revised UIs, making the workflow easier.
- Next available renderer functionality: The renderer UI now has a button which automatically selects the next available renderer.

## v1.0.0

First Release of atmoky trueSpatial.
