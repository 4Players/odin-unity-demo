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
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>())
            {
                _audioSources.Remove(audioSource);
            }
        }

        private void Update()
        {
            // iterate backwards in case we remove an audio source from the list
            for (int i = _audioSources.Count - 1; i >= 0; i--)
            {
                AudioSource audioSource = _audioSources[i];
                // Remove already destroyed audiosources from our list
                if (!audioSource)
                {
                    _audioSources.RemoveAt(i);
                    continue;
                }

                Vector3[] rayOrigins = { audioListener.transform.position, audioSource.transform.position };
                Vector3 toAudioSource = rayOrigins[1] - rayOrigins[0];

                var forwardHits = GetOccluderHits(rayOrigins[0], toAudioSource);
                RemoveOriginCollisions(ref forwardHits, rayOrigins);


                AudioObstacleEffect applicableEffect = null;
                int numHitAudioObstacles = 0;
                
                foreach (RaycastHit hit in forwardHits)
                {
                    AudioObstacle audioObstacle = hit.collider.GetComponent<AudioObstacle>();
                    if (audioObstacle)
                    {
                        AudioObstacleEffect currentEffect = audioObstacle.settings.effect;
                        applicableEffect = GetLargerEffect(applicableEffect, currentEffect);
                        numHitAudioObstacles++;
                    }
                }

                // Only check occlusion thickness, if we found occlusions that don't have an AudioObstacle component
                if (numHitAudioObstacles != forwardHits.Count)
                {
                    // Debug.Log($"Num Hit Audio Obstacles: {numHitAudioObstacles} vs ForwardHits: {forwardHits.Count}");
                    
                    var backwardsHits = GetOccluderHits(rayOrigins[1], -toAudioSource);
                    RemoveOriginCollisions(ref backwardsHits, rayOrigins);

                    if (forwardHits.Count == backwardsHits.Count)
                    {
                        float occlusionThicknessSum = GetOccluderThickness(forwardHits, backwardsHits, toAudioSource);
                        if (occlusionThicknessSum > 0.0f)
                        {
                            float thicknessCutoffFrequency =
                                occlusionSettings.occlusionCurve.Evaluate(occlusionThicknessSum);
                            AudioObstacleEffect thicknessEffect = new AudioObstacleEffect(thicknessCutoffFrequency);
                            
                            applicableEffect = GetLargerEffect(applicableEffect, thicknessEffect);
                        }
                    }
                }

                if (null != applicableEffect)
                {
                    // Debug.Log($"Audiosource: {audioSource.gameObject.name} Cutoff Frequency: {applicableEffect.cutoffFrequency}");
                    ApplyOcclusionEffect(applicableEffect, audioSource);
                }
                else
                {
                    DeactivateOcclusionEffect(audioSource);
                }
            }
        }

        private AudioObstacleEffect GetLargerEffect(AudioObstacleEffect first, AudioObstacleEffect second)
        {
            if (null == first)
                return second;
            if (null == second)
                return first;
            if (first.CompareTo(second) < 0)
                return first;
            
            return second;
        }

        private void ApplyOcclusionEffect(AudioObstacleEffect obstacleEffect, AudioSource source)
        {
            AudioLowPassFilter filter = source.GetComponent<AudioLowPassFilter>();
            if (!filter)
                filter = source.gameObject.AddComponent<AudioLowPassFilter>();
            Assert.IsNotNull(filter);
            
            filter.enabled = true;
            filter.cutoffFrequency = obstacleEffect.cutoffFrequency;
            filter.lowpassResonanceQ = obstacleEffect.lowpassResonanceQ;
        }
        
        private static bool IsValidCutoffFrequency(float cutoffFrequency)
        {
            return cutoffFrequency < float.MaxValue && cutoffFrequency > 0.0f;
        }

        private static void DeactivateOcclusionEffect(AudioSource audioSource)
        {
            AudioLowPassFilter filter = audioSource.GetComponent<AudioLowPassFilter>();
            if (filter)
            {
                filter.enabled = false;
            }
        }

        private static float GetOccluderThickness(List<RaycastHit> forwardHits, List<RaycastHit> backwardsHits,
            Vector3 toAudioSource)
        {
            float occlusionThicknessSum = 0.0f;

            if (forwardHits.Count == backwardsHits.Count)
            {
                for (int forwardsIndex = 0; forwardsIndex < forwardHits.Count; ++forwardsIndex)
                {
                    int backwardsIndex = backwardsHits.Count - 1 - forwardsIndex;
                    var forwardHit = forwardHits[forwardsIndex];
                    var backwardHit = backwardsHits[backwardsIndex];
                    if (forwardHit.collider == backwardHit.collider)
                    {
                        float occlusionThickness = toAudioSource.magnitude - forwardHit.distance -
                                                   backwardHit.distance;
                        //make every occluding object at least a millimeter thick.
                        occlusionThickness = Mathf.Max(occlusionThickness, 0.001f);

                        occlusionThicknessSum += occlusionThickness;
                    }
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

            // TODO: Improve performance by using Non-Alloc Raycast
            // Using two arrays with a fixed max size would only be problematic if numFoundHits > max size
            // but in that case we can just assume, that the max occlusion effect can be applied.
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
            for (int i = hits.Count - 1; i >= 0; i--)
            {
                RaycastHit occludingHit = hits[i];
                foreach (Vector3 origin in origins)
                {
                    if (occludingHit.collider.bounds.Contains(origin))
                    {
                        hits.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}