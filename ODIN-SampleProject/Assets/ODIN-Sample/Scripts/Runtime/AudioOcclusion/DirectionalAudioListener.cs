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
            foreach (var audioSource in _audioSources.Keys)
            {
                Vector3 sourcePosition = audioSource.transform.position;
                Vector3 listenerPosition = audioListener.transform.position;

                
                
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (var audioSource in other.GetComponentsInChildren<AudioSource>(
                includeInactiveAudioSourcesInSearch))
            {
                if (_audioSources.ContainsKey(audioSource))
                {
                    var sourceData = _audioSources[audioSource];
                    sourceData.Increment();
                }
                else
                {
                    _audioSources[audioSource] = new DirectionalAudioSourceData(1, audioSource.volume);
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
            public DirectionalAudioSourceData(int numTriggersReceived, float originalVolume)
            {
                NumTriggersReceived = numTriggersReceived;
                OriginalVolume = originalVolume;
            }

            public int NumTriggersReceived { get; private set; }
            public float OriginalVolume { get; private set; }

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