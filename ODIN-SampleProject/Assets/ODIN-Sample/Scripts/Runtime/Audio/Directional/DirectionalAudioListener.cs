using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio.Directional
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class DirectionalAudioListener : MonoBehaviour
    {
        [SerializeField] private float audioSourceDetectionRange = 100.0f;
        
        [FormerlySerializedAs("directionAudioSettings")] [FormerlySerializedAs("directionSettings")] [SerializeField]
        private DirectionalAudioEffectSettings directionalSettings;

        [SerializeField] private AudioListener audioListener;
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
            Vector3 listenerPosition = listenerTransform.position;
            Vector3 listenerForwards = listenerTransform.forward;


            List<AudioSource> dataToRemove = new List<AudioSource>();

            foreach (AudioSource audioSource in _audioSources.Keys)
            {
                DirectionalAudioSourceData sourceData = _audioSources[audioSource];
                if (null == sourceData || !sourceData.ConnectedSource)
                {
                    dataToRemove.Add(audioSource);
                    continue;
                }
                
                Vector3 sourcePosition = sourceData.ConnectedSource.transform.position;
                Vector3 toSource = (sourcePosition - listenerPosition).normalized;

                float signedAngle = Vector3.SignedAngle(listenerForwards, toSource, Vector3.up);

                AudioEffectApplicator applicator = sourceData.GetApplicator();
                if (!applicator)
                {
                    applicator = audioSource.gameObject.AddComponent<AudioEffectApplicator>();
                }
                
                applicator.Apply(directionalSettings.GetAudioEffectData(signedAngle));
            }

            // Remove destroyed audio sources from the audio sources container.
            foreach (var source in dataToRemove)
            {
                _audioSources.Remove(source);
            }
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
                    if (sourceData.NumTriggersReceived <= 0)
                        _audioSources.Remove(audioSource);
                }
        }

        [Serializable]
        private class DirectionalAudioSourceData
        {
            public int NumTriggersReceived { get; private set; }
            public float OriginalVolume { get; private set; }
            public AudioSource ConnectedSource { get; private set; }

            private AudioEffectApplicator _cachedApplicator;

            public DirectionalAudioSourceData(int numTriggersReceived, AudioSource connectedSource)
            {
                NumTriggersReceived = numTriggersReceived;
                OriginalVolume = connectedSource.volume;
                ConnectedSource = connectedSource;
            }

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
                return ++NumTriggersReceived;
            }

            /// <summary>
            /// Increments the amount of triggers, that were found to be containing this audio source.
            ///
            /// Because an object can have multiple colliders in the hierarchy above the audio source, multiple
            /// colliders could start the registration process of the audio source. 
            /// </summary>
            /// <returns>The amount of triggers, that are connected to this audio source data, after decrementing.</returns>
            public int Decrement()
            {
                return --NumTriggersReceived;
            }
        }
    }
}