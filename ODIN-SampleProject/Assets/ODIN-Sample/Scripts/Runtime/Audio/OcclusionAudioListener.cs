using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class OcclusionAudioListener : MonoBehaviour
    {
        [SerializeField] private OcclusionSettings occlusionSettings;
        [SerializeField] private AudioListener audioListener;

        private List<AudioSource> _audioSources = new List<AudioSource>();
        private SphereCollider _audioSourceDetector;

        private void Awake()
        {
            _audioSourceDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(_audioSourceDetector);
            _audioSourceDetector.isTrigger = true;

            Rigidbody detectorRigidBody = GetComponent<Rigidbody>();
            Assert.IsNotNull(detectorRigidBody);
            detectorRigidBody.isKinematic = true;
            detectorRigidBody.useGravity = false;

            Assert.IsNotNull(occlusionSettings);
            _audioSourceDetector.radius = occlusionSettings.audioSourceDetectionRange;

            if (null == audioListener)
                audioListener = GetComponent<AudioListener>();
            Assert.IsNotNull(audioListener);
        }

        private void OnTriggerEnter(Collider other)
        {
            _audioSources.AddRange(other.GetComponentsInChildren<AudioSource>());
            // Debug.Log($"Tried Adding from {other.name} - AudioSource Count: {_audioSources.Count}");
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>())
            {
                _audioSources.Remove(audioSource);
            }
            // Debug.Log($"Tried Removing from {other.name} - AudioSource Count: {_audioSources.Count}");
        }

        private void Update()
        {
            foreach (AudioSource audioSource in _audioSources)
            {
                Vector3[] rayOrigins = { audioListener.transform.position, audioSource.transform.position };
                Vector3 toAudioSource = rayOrigins[1] - rayOrigins[0];
                var forwardHits = GetOccluderHits(rayOrigins[0], toAudioSource);
                RemoveOriginCollisions(ref forwardHits, rayOrigins);
                var backwardsHits = GetOccluderHits(rayOrigins[1], -toAudioSource);
                RemoveOriginCollisions(ref backwardsHits, rayOrigins);

                Assert.IsTrue(forwardHits.Count == backwardsHits.Count);
                
                float occlusionThicknessSum = GetOcclusionThickness(forwardHits, backwardsHits, toAudioSource);
                if (occlusionThicknessSum > 0.0f)
                {
                    float cutoffFrequency = occlusionSettings.occlusionCurve.Evaluate(occlusionThicknessSum);
                    AudioLowPassFilter filter = audioSource.GetComponent<AudioLowPassFilter>();
                    if (!filter)
                        filter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
                    Assert.IsNotNull(filter);
                    filter.enabled = true;
                    filter.cutoffFrequency = cutoffFrequency;
                }
                else
                {
                    AudioLowPassFilter filter = audioSource.GetComponent<AudioLowPassFilter>();
                    if (filter)
                    {
                        filter.enabled = false;
                    }
                }
            }
        }

        private static float GetOcclusionThickness(List<RaycastHit> forwardHits, List<RaycastHit> backwardsHits, Vector3 toAudioSource)
        {
            float occlusionThicknessSum = 0.0f;
            for (int forwardsIndex = 0; forwardsIndex < forwardHits.Count; ++forwardsIndex)
            {
                int backwardsIndex = backwardsHits.Count - 1 - forwardsIndex;
                var forwardHit = forwardHits[forwardsIndex];
                var backwardHit = backwardsHits[backwardsIndex];
                if (forwardHit.collider == backwardHit.collider)
                {
                    float occlusionThickness = toAudioSource.magnitude - forwardHit.distance -
                                               backwardHit.distance;

                    occlusionThicknessSum += occlusionThickness;
                }
            }
            return occlusionThicknessSum;
        }

        /// <summary>
        /// Get all hits
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <returns></returns>
        private List<RaycastHit> GetOccluderHits(Vector3 rayOrigin, Vector3 rayDirection)
        {
            Ray occluderRay = new Ray(rayOrigin, rayDirection);
            List<RaycastHit> occludingHits = new List<RaycastHit>(
                Physics.RaycastAll(
                    occluderRay,
                    rayDirection.magnitude + Single.Epsilon,
                    occlusionSettings.audioSourceDetectionLayer,
                    QueryTriggerInteraction.Ignore));
            // Sort by distance
            occludingHits.Sort(delegate(RaycastHit hit1, RaycastHit hit2)
            {
                return hit1.distance.CompareTo(hit2.distance);
            });
            return occludingHits;
        }

        /// <summary>
        /// Remove all hits with colliders that contain the ray origins --> e.g. if the audio listener or the audio source
        /// has a collider on its gameobject.
        /// </summary>
        /// <param name="hits">Reference to the list of raycast hits.</param>
        /// <param name="origins">The ray origins.</param>
        private void RemoveOriginCollisions(ref List<RaycastHit> hits, params Vector3[] origins)
        {
            for (int i = hits.Count - 1; i > -1; i--)
            {
                RaycastHit occludingHit = hits[i];
                foreach (Vector3 origin in origins)
                {
                    if (occludingHit.collider.bounds.Contains(origin))
                    {
                        hits.RemoveAt(i);
                    }
                }
            }
        }
    }
}