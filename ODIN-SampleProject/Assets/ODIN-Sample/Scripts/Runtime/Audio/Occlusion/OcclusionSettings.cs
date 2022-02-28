﻿using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    /// <summary>
    ///     Settings used by the <see cref="OcclusionAudioListener" /> when applying occlusion effects to audio sources
    ///     in range. Mainly used for the automatic audio occlusion system, which bases occlusion effects on the thickness
    ///     of colliders between listener and source, when no <see cref="AudioObstacle" /> script is attached to an Audio
    ///     Source.
    /// </summary>
    [CreateAssetMenu(fileName = "OcclusionSettings", menuName = "Odin-Sample/OcclusionSettings", order = 0)]
    public class OcclusionSettings : ScriptableObject
    {
        /// <summary>
        ///     The range in which Audio Sources are detected by the <see cref="OcclusionAudioListener" />.
        /// </summary>
        [Header("Automatic Occlusion")] public float audioSourceDetectionRange = 100.0f;

        /// <summary>
        ///     The Layers on which audio occluding Colliders are detected by the <see cref="OcclusionAudioListener" />.
        /// </summary>
        public LayerMask audioSourceDetectionLayer = ~0;

        /// <summary>
        ///     Maps the thickness of colliders between the Audio listener and a Source to the Cutoff Frequency used
        ///     in the lowpass filter.
        /// </summary>
        [FormerlySerializedAs("DefaultEffectDefinition")]
        public AudioEffectDefinition defaultEffectDefinition;
    }
}