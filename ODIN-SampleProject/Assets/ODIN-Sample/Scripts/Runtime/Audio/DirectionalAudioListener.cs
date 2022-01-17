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
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class DirectionalAudioListener : MonoBehaviour
    {
        /// <summary>
        /// The detection range for audio sources.
        /// </summary>
        [SerializeField] private float audioSourceDetectionRange = 100.0f;

        /// <summary>
        /// The settings file, containing data which manipulates audio sources in range based on distance and
        /// angle between source and listener.
        /// </summary>
        [FormerlySerializedAs("directionAudioSettings")] [FormerlySerializedAs("directionSettings")] [SerializeField]
        private DirectionalAudioEffectSettings directionalSettings;

        /// <summary>
        /// Reference to the audio listener. If null, will try to get the audio listener on this gameobject.
        /// </summary>
        [SerializeField] private AudioListener audioListener;

        /// <summary>
        /// Whether to search for inactive audio sources on objects in the detection range.
        /// </summary>
        [SerializeField] private bool includeInactiveAudioSourcesInSearch = true;

        private readonly Dictionary<AudioSource, DirectionalAudioSourceData> _audioSources =
            new Dictionary<AudioSource, DirectionalAudioSourceData>();

        private void Awake()
        {
            var audioSourceDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(audioSourceDetector);
            audioSourceDetector.isTrigger = true;

            var detectorRigidBody = GetComponent<Rigidbody>();
            Assert.IsNotNull(detectorRigidBody);
            detectorRigidBody.isKinematic = true;
            detectorRigidBody.useGravity = false;

            Assert.IsNotNull(directionalSettings);
            audioSourceDetector.radius = audioSourceDetectionRange;

            if (null == audioListener)
                audioListener = GetComponent<AudioListener>();
            Assert.IsNotNull(audioListener);
        }

        private void Update()
        {
            var listenerTransform = audioListener.transform;
            var listenerPosition = listenerTransform.position;
            var listenerForwards = listenerTransform.forward;

            // store audio sources that should be removed and remove all at the end of the update.
            var dataToRemove = new List<AudioSource>();
            foreach (var audioSource in _audioSources.Keys)
            {
                var sourceData = _audioSources[audioSource];
                if (null == sourceData || !sourceData.ConnectedSource)
                {
                    dataToRemove.Add(audioSource);
                    continue;
                }

                var sourcePosition = sourceData.ConnectedSource.transform.position;
                var toSource = (sourcePosition - listenerPosition).normalized;

                // determine angle between audio listener and audio source
                var signedAngle = Vector3.SignedAngle(listenerForwards, toSource, Vector3.up);

                var applicator = sourceData.GetApplicator();
                if (!applicator) applicator = audioSource.gameObject.AddComponent<AudioEffectApplicator>();

                applicator.Apply(directionalSettings.GetAudioEffectData(signedAngle));
            }

            // Remove destroyed audio sources from the audio sources container.
            foreach (var source in dataToRemove) _audioSources.Remove(source);
        }

        /// <summary>
        /// Checks, whether we detected a new Audio source in range. Will ignore audio sources, that don't have
        /// Spatial Blending enabled (e.g. 2D sounds)
        /// </summary>
        /// <param name="other">The collider of the object we detected.</param>
        private void OnTriggerEnter(Collider other)
        {
            foreach (var audioSource in other.GetComponentsInChildren<AudioSource>(
                includeInactiveAudioSourcesInSearch))
            {
                // only use audio source, if it's a 3d sound
                if (!(audioSource.spatialBlend > 0.0f))
                    return;

                if (_audioSources.ContainsKey(audioSource))
                {
                    var sourceData = _audioSources[audioSource];
                    sourceData.Increment();
                }
                else
                {
                    _audioSources[audioSource] = new DirectionalAudioSourceData(1, audioSource);
                }
                //
                // Debug.Log(
                //     $"Entered audio source Trigger: {audioSource.gameObject} with count: {_audioSources[audioSource].NumTriggersReceived}");
            }
        }

        /// <summary>
        /// Will remove audio sources on the <see cref="other"/> object, if all colliders connected to that audio source
        /// have left the detection range. 
        /// </summary>
        /// <param name="other">The collider of the object we detected leaving the detection range.</param>
        private void OnTriggerExit(Collider other)
        {
            foreach (var audioSource in other.GetComponentsInChildren<AudioSource>(
                includeInactiveAudioSourcesInSearch))
                if (_audioSources.ContainsKey(audioSource))
                {
                    var sourceData = _audioSources[audioSource];
                    sourceData.Decrement();

                    // Debug.Log(
                    //     $"Exited audio source Trigger: {audioSource.gameObject} with count: {sourceData.NumTriggersReceived}");
                    if (sourceData.NumTriggersEntered <= 0)
                        _audioSources.Remove(audioSource);
                }
        }

        /// <summary>
        /// Used to store data on the audio sources that were detected by the script
        /// </summary>
        [Serializable]
        private class DirectionalAudioSourceData
        {
            /// <summary>
            /// The number of triggers, that entered the detection range and are connected to the <see cref="ConnectedSource"/>.
            /// </summary>
            public int NumTriggersEntered { get; private set; }

            /// <summary>
            /// The original volume of the audio source, before being affected by the directional audio listener
            /// </summary>
            public float OriginalVolume { get; private set; }

            /// <summary>
            /// The Audio Source being affected.
            /// </summary>
            public AudioSource ConnectedSource { get; private set; }

            private AudioEffectApplicator _cachedApplicator;

            public DirectionalAudioSourceData(int numTriggersEntered, AudioSource connectedSource)
            {
                NumTriggersEntered = numTriggersEntered;
                OriginalVolume = connectedSource.volume;
                ConnectedSource = connectedSource;
            }

            /// <summary>
            /// Retrieve the effect applicator, used to apply effects to the <see cref="ConnectedSource"/>.
            /// </summary>
            /// <returns>The effect applicator.</returns>
            public AudioEffectApplicator GetApplicator()
            {
                if (!_cachedApplicator && ConnectedSource)
                    _cachedApplicator = ConnectedSource.GetComponent<AudioEffectApplicator>();
                return _cachedApplicator;
            }

            /// <summary>
            /// Increments the amount of triggers, that were found to be containing this audio source.
            ///
            /// Because an object can have multiple colliders in the hierarchy above the audio source, multiple
            /// colliders could start the registration process of the audio source. 
            /// </summary>
            /// <returns>The amount of triggers, that are connected to this audio source data, after incrementing.</returns>
            public int Increment()
            {
                return ++NumTriggersEntered;
            }

            /// <summary>
            /// Decrements the amount of triggers, that were found to be containing this audio source.
            ///
            /// Because an object can have multiple colliders in the hierarchy above the audio source, multiple
            /// colliders could start the registration process of the audio source. 
            /// </summary>
            /// <returns>The amount of triggers, that are connected to this audio source data, after decrementing.</returns>
            public int Decrement()
            {
                return --NumTriggersEntered;
            }
        }
    }
}