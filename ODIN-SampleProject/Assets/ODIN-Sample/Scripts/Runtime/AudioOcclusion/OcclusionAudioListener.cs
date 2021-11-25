using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.AudioOcclusion
{
    
    /// <summary>
    /// This component automatically detects Audio Sources inside the radius given by the <see cref="occlusionSettings"/>
    /// and applies audio occlusion effects to them, if required.
    /// If Audio Source gameobjects have a <see cref="AudioObstacle"/> script attached, the <see cref="AudioObstacleEffect"/>
    /// associated to the obstacle will be used, otherwise a fallback method based on detecting the thickness of objects
    /// with colliders between the <see cref="AudioListener"/> and <see cref="AudioSource"/> will be used.
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class OcclusionAudioListener : MonoBehaviour
    {
        [SerializeField] private OcclusionSettings occlusionSettings;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private bool includeInactiveSourcesInSearch = true;
        

        private readonly Dictionary<AudioSource, AudioObstacleEffect> _audioSources =
            new Dictionary<AudioSource, AudioObstacleEffect>();

        private Collider[] _collidersOnAudiolistener;

        private void Awake()
        {
            SphereCollider audioSourceDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(audioSourceDetector);
            audioSourceDetector.isTrigger = true;

            Rigidbody detectorRigidBody = GetComponent<Rigidbody>();
            Assert.IsNotNull(detectorRigidBody);
            detectorRigidBody.isKinematic = true;
            detectorRigidBody.useGravity = false;

            Assert.IsNotNull(occlusionSettings);
            audioSourceDetector.radius = occlusionSettings.audioSourceDetectionRange;

            if (null == audioListener)
                audioListener = GetComponent<AudioListener>();
            Assert.IsNotNull(audioListener);
        }

        private void Start()
        {
            _collidersOnAudiolistener = audioListener.GetComponentsInParent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>(includeInactiveSourcesInSearch))
            {
                // only use audio source, if it's a 3d sound
                if (!(audioSource.spatialBlend > 0.0f))
                    return;
                
                if (!_audioSources.ContainsKey(audioSource))
                {
                    AudioObstacleEffect originalEffect = new AudioObstacleEffect();
                    originalEffect.volumeMultiplier = audioSource.volume;

                    AudioLowPassFilter lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
                    if (lowPassFilter)
                    {
                        originalEffect.cutoffFrequency = lowPassFilter.cutoffFrequency;
                        originalEffect.lowpassResonanceQ = lowPassFilter.lowpassResonanceQ;
                    }

                    _audioSources[audioSource] = originalEffect;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (AudioSource audioSource in other.GetComponentsInChildren<AudioSource>(includeInactiveSourcesInSearch))
            {
                _audioSources.Remove(audioSource);
            }
        }

        private void Update()
        {
            List<AudioSource> toRemove = new List<AudioSource>();
            // iterate backwards in case we remove an audio source from the list
            foreach (AudioSource audioSource in _audioSources.Keys)
            {
                // Remove already destroyed audiosources from our list
                if (!audioSource)
                {
                    toRemove.Add(audioSource);
                    continue;
                }

                // Determine ray origins and ray direction
                Vector3[] rayOrigins = { audioListener.transform.position, audioSource.transform.position };
                Vector3 toAudioSource = rayOrigins[1] - rayOrigins[0];
                Collider[] audioSourceColliders = audioSource.GetComponentsInParent<Collider>();

                // Get all hits from audio listener to audio source
                var forwardHits = GetOccluderHits(rayOrigins[0], toAudioSource);
                // Remove colliders, that are inside the audio listener or inside the audio source
                RemoveOriginCollisions(ref forwardHits, _collidersOnAudiolistener);
                RemoveOriginCollisions(ref forwardHits, audioSourceColliders);

                AudioObstacleEffect applicableEffect = null;
                var numHitAudioObstacles = TryRetrieveAudioObstacle(forwardHits, ref applicableEffect);

                // Only check occlusion thickness, if we found occlusions that don't have an AudioObstacle component
                if (numHitAudioObstacles != forwardHits.Count)
                {
                    TryRetrieveDefaultOcclusion(forwardHits, rayOrigins, toAudioSource, audioSourceColliders,
                        ref applicableEffect);
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

            foreach (AudioSource audioSource in toRemove)
            {
                _audioSources.Remove(audioSource);
            }
        }

        /// <summary>
        /// Casts a ray from the ray target (usually the audio source position) to the ray source (usually the audio listener),
        /// to determine the thickness of each occluder between the two. If <see cref="applicableEffect"/> is pre-filled
        /// from data of an occluder with a <see cref="AudioObstacle"/> component, it will compare the default occlusion effect of
        /// the combined thickness to the given effect and use the stronger one. If the <see cref="applicableEffect"/> was
        /// not prefilled, it we will just return the default thickness occlusion effect.
        /// </summary>
        /// <remarks>
        /// The <see cref="targetColliders"/> are necessary to remove colliders that surround the audio source. E.g. you have
        /// a radio playing, the radio has a rough collider on itself and the radio audio source is located inside of that collider.
        /// Without automatically removing the radios colliders from the algorithm, the occlusion effect would dampen the radio audio source.
        /// </remarks>
        /// <param name="forwardHits">The hits determined in the forward step (Audio Listener --> Audio Source)</param>
        /// <param name="rayOrigins">The two origins of the ray (usually Audio Listener and Audio Source). Assumes the Array has two entries, the first
        /// contains the ray origin, the second the ray target.</param>
        /// <param name="toTarget">Vector towards target.</param>
        /// <param name="targetColliders">All colliders on the ray target. Used to remove colliders that surround the audio source from dampening the audio.</param>
        /// <param name="applicableEffect">The effect that should be filled in.</param>
        private void TryRetrieveDefaultOcclusion(List<RaycastHit> forwardHits,
            Vector3[] rayOrigins, Vector3 toTarget, Collider[] targetColliders,
            ref AudioObstacleEffect applicableEffect)
        {
            Assert.IsTrue(rayOrigins.Length == 2);
            
            var backwardsHits = GetOccluderHits(rayOrigins[1], -toTarget);
            RemoveOriginCollisions(ref backwardsHits, _collidersOnAudiolistener);
            RemoveOriginCollisions(ref backwardsHits, targetColliders);

            if (forwardHits.Count == backwardsHits.Count)
            {
                float occlusionThicknessSum = GetOccluderThickness(forwardHits, backwardsHits, toTarget);
                if (occlusionThicknessSum > 0.0f)
                {
                    float thicknessCutoffFrequency =
                        occlusionSettings.occlusionCurve.Evaluate(occlusionThicknessSum);
                    AudioObstacleEffect thicknessEffect = new AudioObstacleEffect(thicknessCutoffFrequency);

                    applicableEffect = GetLargerEffect(applicableEffect, thicknessEffect);
                }
            }
        }

        private int TryRetrieveAudioObstacle(List<RaycastHit> forwardHits, ref AudioObstacleEffect applicableEffect)
        {
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

            return numHitAudioObstacles;
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

            if (obstacleEffect.cutoffFrequency < 0.0f)
            {
                filter.enabled = false;
            }
            else
            {
                filter.enabled = true;
                filter.cutoffFrequency = obstacleEffect.cutoffFrequency;
                filter.lowpassResonanceQ = obstacleEffect.lowpassResonanceQ;
            }

            AudioObstacleEffect originalEffect = _audioSources[source];
            source.volume = originalEffect.volumeMultiplier * obstacleEffect.volumeMultiplier;
        }

        private void DeactivateOcclusionEffect(AudioSource audioSource)
        {
            AudioObstacleEffect originalEffect = _audioSources[audioSource];
            audioSource.volume = originalEffect.volumeMultiplier;

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
            // but in that case we can just assume, that a max occlusion effect can be applied.
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
        /// <param name="collidersToRemove">All colliders which should not be present in the hits list</param>
        private void RemoveOriginCollisions(ref List<RaycastHit> hits, Collider[] collidersToRemove)
        {
            foreach (Collider toRemove in collidersToRemove)
            {
                for (int i = hits.Count - 1; i >= 0; i--)
                {
                    RaycastHit occludingHit = hits[i];
                    if (toRemove == occludingHit.collider)
                    {
                        hits.RemoveAt(i);
                    }
                }
            }
        }
    }
}