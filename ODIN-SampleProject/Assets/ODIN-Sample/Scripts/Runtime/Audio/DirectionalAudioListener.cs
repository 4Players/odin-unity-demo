using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// <para>
    /// This script is required for the directional audio system to work. It will apply directional effects on Audio
    /// Sources in range, according to the position of the AudioListener in the scene. 
    /// </para>
    /// <para>
    /// This script simulates the difference between sources in front and behind an audio listener, by adjusting
    /// the volume and applying a low pass filter effect, based on the angle between audio listener and audio source.
    /// This is just a very simplified version of applying a HRTF to simulate 3D Audio.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Only audio sources with colliders in the parent hierarchy can be detected!
    /// </remarks>
    public class DirectionalAudioListener : AAudioListenerEffect
    {
        /// <summary>
        /// The settings file, containing data which manipulates audio sources in range based on distance and
        /// angle between source and listener.
        /// </summary>
        [FormerlySerializedAs("directionAudioSettings")] [FormerlySerializedAs("directionSettings")] [SerializeField]
        private AudioEffectDefinition directionalSettings;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(directionalSettings);
        }

        protected override void EffectUpdate(AudioSourceData data)
        {
            Transform listenerTransform = audioListener.transform;
            Vector3 listenerPosition = listenerTransform.position;
            Vector3 listenerForwards = listenerTransform.forward;
            
            Vector3 sourcePosition = data.ConnectedSource.transform.position;
            Vector3 toSource = (sourcePosition - listenerPosition).normalized;

            // determine angle between audio listener and audio source
            float signedAngle = Vector3.SignedAngle(listenerForwards, toSource, Vector3.up);

            AudioEffectApplicator applicator = data.GetApplicator();
            if(applicator) 
                applicator.Apply(directionalSettings.GetEffect(signedAngle));
        }
    }
}