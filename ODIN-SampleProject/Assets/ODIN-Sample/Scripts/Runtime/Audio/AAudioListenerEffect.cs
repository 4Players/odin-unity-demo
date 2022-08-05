using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    ///     Base class for effect behaviours that adjust the behaviour of Audio Sources. Examples are the Occlusion Audio
    ///     Listener Effect,
    ///     which adds dampening to Audio Sources based on objects between the Audio Source and the references audio listener.
    /// </summary>
    [RequireComponent(typeof(AudioListenerSetup))]
    public abstract class AAudioListenerEffect : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the audio listener. If null, will try to get the audio listener on this gameobject.
        /// </summary>
        [SerializeField] protected AudioListener audioListener;

        /// <summary>
        ///     Whether to search for inactive audio sources on objects in the detection range.
        /// </summary>
        [SerializeField] protected bool includeInactiveAudioSourcesInSearch = true;


        protected readonly Dictionary<int, AudioSourceData> DetectedAudioSources = new();


        protected virtual void Awake()
        {
            if (null == audioListener)
                audioListener = GetComponent<AudioListener>();
            Assert.IsNotNull(audioListener);
        }

        protected virtual void Update()
        {
            // store audio sources that should be removed and remove all at the end of the update.
            List<int> valuesToRemove = new List<int>();
            foreach (KeyValuePair<int, AudioSourceData> dataPair in DetectedAudioSources)
            {
                AudioSourceData audioSourceData = dataPair.Value;
                if (!audioSourceData.ConnectedSource)
                    valuesToRemove.Add(dataPair.Key);
                else
                    EffectUpdate(audioSourceData);
            }

            foreach (int key in valuesToRemove) DetectedAudioSources.Remove(key);
        }

        /// <summary>
        ///     Checks, whether we detected a new Audio source in range. Will ignore audio sources, that don't have
        ///     Spatial Blending enabled (e.g. 2D sounds)
        /// </summary>
        /// <param name="other">The collider of the object we detected.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>(
                         includeInactiveAudioSourcesInSearch))
            {
                // only use audio source, if it's a 3d sound
                if (!(audioSource.spatialBlend > 0.0f))
                    return;

                int instanceId = audioSource.GetInstanceID();
                if (DetectedAudioSources.ContainsKey(instanceId))
                {
                    AudioSourceData sourceData = DetectedAudioSources[instanceId];
                    sourceData.Increment();
                }
                else
                {
                    DetectedAudioSources[instanceId] = new AudioSourceData(1, audioSource);
                }
                //
                // Debug.Log(
                //     $"Entered audio source Trigger: {audioSource.gameObject} with count: {_audioSources[audioSource].NumTriggersReceived}");
            }
        }

        /// <summary>
        ///     Will remove audio sources on the <see cref="other" /> object, if all colliders connected to that audio source
        ///     have left the detection range.
        /// </summary>
        /// <param name="other">The collider of the object we detected leaving the detection range.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>(
                         includeInactiveAudioSourcesInSearch))
            {
                int instanceId = audioSource.GetInstanceID();
                if (DetectedAudioSources.ContainsKey(instanceId))
                {
                    AudioSourceData sourceData = DetectedAudioSources[instanceId];
                    sourceData.Decrement();

                    // Debug.Log(
                    //     $"Exited audio source Trigger: {audioSource.gameObject} with count: {sourceData.NumTriggersReceived}");
                    if (sourceData.NumTriggersEntered <= 0)
                        DetectedAudioSources.Remove(instanceId);
                }
            }
        }

        /// <summary>
        ///     The main effect Update call. Should be overriden to apply audio effects on the referenced sources.
        ///     Will supply the Audio Source Data for applying effect data to the referenced audio source.
        /// </summary>
        /// <param name="data">Audio Source Data for applying effect data to the referenced audio source</param>
        protected abstract void EffectUpdate(AudioSourceData data);

        /// <summary>
        ///     Used to store data on the audio sources that were detected by the script
        /// </summary>
        [Serializable]
        protected class AudioSourceData
        {
            private AudioEffectApplicator _cachedApplicator;

            public AudioSourceData(int numTriggersEntered, AudioSource connectedSource)
            {
                NumTriggersEntered = numTriggersEntered;
                ConnectedSource = connectedSource;
            }

            /// <summary>
            ///     The number of triggers, that entered the detection range and are connected to the <see cref="ConnectedSource" />.
            /// </summary>
            public int NumTriggersEntered { get; private set; }

            /// <summary>
            ///     The Audio Source being affected.
            /// </summary>
            public AudioSource ConnectedSource { get; private set; }

            /// <summary>
            ///     Retrieve the effect applicator, used to apply effects to the <see cref="ConnectedSource" />.
            /// </summary>
            /// <returns>The effect applicator.</returns>
            public AudioEffectApplicator GetApplicator()
            {
                if (!_cachedApplicator && ConnectedSource)
                {
                    _cachedApplicator = ConnectedSource.GetComponent<AudioEffectApplicator>();
                    if (!_cachedApplicator)
                        _cachedApplicator = ConnectedSource.gameObject.AddComponent<AudioEffectApplicator>();
                }

                return _cachedApplicator;
            }

            /// <summary>
            ///     Increments the amount of triggers, that were found to be containing this audio source.
            ///     Because an object can have multiple colliders in the hierarchy above the audio source, multiple
            ///     colliders could start the registration process of the audio source.
            /// </summary>
            /// <returns>The amount of triggers, that are connected to this audio source data, after incrementing.</returns>
            public int Increment()
            {
                return ++NumTriggersEntered;
            }

            /// <summary>
            ///     Decrements the amount of triggers, that were found to be containing this audio source.
            ///     Because an object can have multiple colliders in the hierarchy above the audio source, multiple
            ///     colliders could start the registration process of the audio source.
            /// </summary>
            /// <returns>The amount of triggers, that are connected to this audio source data, after decrementing.</returns>
            public int Decrement()
            {
                return --NumTriggersEntered;
            }
        }
    }
}