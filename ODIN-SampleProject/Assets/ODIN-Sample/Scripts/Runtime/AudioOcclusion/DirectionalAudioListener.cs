using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.AudioOcclusion
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody)), DefaultExecutionOrder(100)]
    public class DirectionalAudioListener : MonoBehaviour
    {
        [FormerlySerializedAs("directionAudioSettings")] [FormerlySerializedAs("directionSettings")] [SerializeField]
        private DirectionAudioSettings directionalSettings;

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
            audioSourceDetector.radius = directionalSettings.audioSourceDetectionRange;

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
                    Debug.LogError("Source Data or Connected Source in DirectionalAudioListener is invalid, removing AudioSource from List.");
                }
                
                Vector3 sourcePosition = sourceData.ConnectedSource.transform.position;
                Vector3 toSource = (sourcePosition - listenerPosition).normalized;

                float signedAngle = Vector3.SignedAngle(listenerForwards, toSource, Vector3.up);

                AudioLowPassFilter lowPassFilter = sourceData.GetLowPassFilter();
                if (!lowPassFilter)
                {
                    lowPassFilter = sourceData.ConnectedSource.gameObject.AddComponent<AudioLowPassFilter>();
                }

                // if lowpass filter was disabled, enable and reset it
                if (!lowPassFilter.enabled)
                {
                    lowPassFilter.enabled = true;
                    lowPassFilter.cutoffFrequency = 22000;
                }
                
                float lowPassFrequency = directionalSettings.angleToVolumeCurve.Evaluate(signedAngle);
                lowPassFilter.cutoffFrequency = Mathf.Min(lowPassFilter.cutoffFrequency, lowPassFrequency);
            }

            foreach (var source in dataToRemove)
            {
                _audioSources.Remove(source);
            }
        }

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

                Debug.Log(
                    $"Entered audio source Trigger: {audioSource.gameObject} with count: {_audioSources[audioSource].NumTriggersReceived}");
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

                    Debug.Log(
                        $"Exited audio source Trigger: {audioSource.gameObject} with count: {sourceData.NumTriggersReceived}");
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

            private AudioLowPassFilter _cachedLowPassFilter;

            public DirectionalAudioSourceData(int numTriggersReceived, AudioSource connectedSource)
            {
                NumTriggersReceived = numTriggersReceived;
                OriginalVolume = connectedSource.volume;
                ConnectedSource = connectedSource;
            }

            public AudioLowPassFilter GetLowPassFilter()
            {
                if (!_cachedLowPassFilter && ConnectedSource)
                    _cachedLowPassFilter = ConnectedSource.GetComponent<AudioLowPassFilter>();
                return _cachedLowPassFilter;
            }

            public int Increment()
            {
                return ++NumTriggersReceived;
            }

            public int Decrement()
            {
                return --NumTriggersReceived;
            }
        }
    }
}