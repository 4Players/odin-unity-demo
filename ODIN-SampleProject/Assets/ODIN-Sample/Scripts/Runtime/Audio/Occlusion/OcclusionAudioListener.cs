using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    /// <summary>
    ///     <para>
    ///         This component automatically detects Audio Sources inside the radius given by the
    ///         <see cref="occlusionSettings" />
    ///         and applies audio occlusion effects to them, if required.
    ///     </para>
    ///     <para>
    ///         If Audio Source gameobjects have a <see cref="AudioObstacle" /> script attached, the
    ///         <see cref="Audio.AudioEffectDefinition" />
    ///         associated to the obstacle will be used, otherwise a fallback method based on detecting the thickness of
    ///         objects
    ///         with colliders between the <see cref="AudioListener" /> and <see cref="AudioSource" /> will be used.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Only audio sources with colliders in the parent hierarchy can be detected!
    /// </remarks>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class OcclusionAudioListener : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the audio occlusion settings object.
        /// </summary>
        [SerializeField] private OcclusionSettings occlusionSettings;

        /// <summary>
        ///     Reference to the audio listener. If null, will try to retrieve audio listener from the same game object.
        /// </summary>
        [SerializeField] private AudioListener audioListener;

        /// <summary>
        ///     Whether to search for inactive audio sources on objects in range.
        /// </summary>
        [SerializeField] private bool includeInactiveSourcesInSearch = true;


        private readonly List<AudioSource> _audioSources =
            new List<AudioSource>();

        private Collider[] _collidersOnAudiolistener;

        private void Awake()
        {
            var audioSourceDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(audioSourceDetector);
            audioSourceDetector.isTrigger = true;

            var detectorRigidBody = GetComponent<Rigidbody>();
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

        private void Update()
        {
            List<AudioSource> toRemove = new List<AudioSource>();
            foreach (AudioSource audioSource in _audioSources)
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
                List<RaycastHit> forwardHits = GetOccluderHits(rayOrigins[0], toAudioSource);
                // Remove colliders, that are inside the audio listener or inside the audio source
                RemoveOriginCollisions(ref forwardHits, _collidersOnAudiolistener);
                RemoveOriginCollisions(ref forwardHits, audioSourceColliders);

                List<RaycastHit> backwardsHits = GetOccluderHits(rayOrigins[1], -toAudioSource);
                RemoveOriginCollisions(ref backwardsHits, _collidersOnAudiolistener);
                RemoveOriginCollisions(ref backwardsHits, audioSourceColliders);


                AudioEffectData combinedEffect = AudioEffectData.Default;
                foreach (RaycastHit hit in forwardHits)
                {
                    float objectThickness = RetrieveThickness(hit, backwardsHits);
                    AudioEffectData occlusionEffect;

                    AudioObstacle audioObstacle = hit.collider.GetComponent<AudioObstacle>();
                    if (audioObstacle)
                    {
                        AudioEffectDefinition effectDefinition = audioObstacle.effect;
                        occlusionEffect = effectDefinition.GetEffect(objectThickness);
                    }
                    else
                    {
                        occlusionEffect = occlusionSettings.defaultEffectDefinition.GetEffect(objectThickness);
                    }

                    combinedEffect = AudioEffectDefinition.GetCombinedEffect(combinedEffect, occlusionEffect);
                }

                ApplyOcclusionEffect(combinedEffect, audioSource);
            }

            foreach (var audioSource in toRemove) _audioSources.Remove(audioSource);
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (var audioSource in other.GetComponentsInChildren<AudioSource>(includeInactiveSourcesInSearch))
            {
                // only use audio source, if it's a 3d sound
                if (!(audioSource.spatialBlend > 0.0f))
                    return;

                if (!_audioSources.Contains(audioSource)) _audioSources.Add(audioSource);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            foreach (var audioSource in other.GetComponentsInChildren<AudioSource>(includeInactiveSourcesInSearch))
            {
                AudioEffectApplicator audioEffectApplicator = audioSource.GetComponent<AudioEffectApplicator>();
                if (audioEffectApplicator) audioEffectApplicator.Reset();

                _audioSources.Remove(audioSource);
            }
        }

        private float RetrieveThickness(RaycastHit frontHit, List<RaycastHit> possibleBacksides)
        {
            int candidateIndex = -1;
            float currentCandidateDistance = float.MaxValue;
            for (var i = 0; i < possibleBacksides.Count; i++)
            {
                RaycastHit backHit = possibleBacksides[i];
                if (backHit.collider == frontHit.collider)
                {
                    float dot = Vector3.Dot(backHit.normal, frontHit.normal);
                    float distance = Vector3.Distance(backHit.point, frontHit.point);

                    // the backhit has to be facing in a different direction than the front hit --> dot product < 0
                    // and we want to use the nearest candidate
                    if (dot < 0 && distance < currentCandidateDistance)
                    {
                        currentCandidateDistance = distance;
                        candidateIndex = i;
                    }
                }
            }

            float thickness = 0.0f;
            if (candidateIndex > -1) thickness = currentCandidateDistance;

            return thickness;
        }

        /// <summary>
        ///     Casts a ray from the ray target (usually the audio source position) to the ray source (usually the audio listener),
        ///     to determine the thickness of each occluder between the two. If <see cref="applicableEffect" /> is pre-filled
        ///     from data of an occluder with a <see cref="AudioObstacle" /> component, it will compare the default occlusion
        ///     effect of
        ///     the combined thickness to the given effect and use the stronger one. If the <see cref="applicableEffect" /> was
        ///     not prefilled, it we will just return the default thickness occlusion effect.
        /// </summary>
        /// <remarks>
        ///     The <see cref="targetColliders" /> are necessary to remove colliders that surround the audio source. E.g. you have
        ///     a radio playing, the radio has a rough collider on itself and the radio audio source is located inside of that
        ///     collider.
        ///     Without automatically removing the radios colliders from the algorithm, the occlusion effect would dampen the radio
        ///     audio source.
        /// </remarks>
        /// <param name="forwardHits">The hits determined in the forward step (Audio Listener --> Audio Source)</param>
        /// <param name="rayOrigins">
        ///     The two origins of the ray (usually Audio Listener and Audio Source). Assumes the Array has two entries, the first
        ///     contains the ray origin, the second the ray target.
        /// </param>
        /// <param name="toTarget">Vector towards target.</param>
        /// <param name="targetColliders">
        ///     All colliders on the ray target. Used to remove colliders that surround the audio source
        ///     from dampening the audio.
        /// </param>
        /// <param name="applicableEffect">The effect that should be filled in.</param>
        /// <summary>
        /// </summary>
        /// <param name="obstacleEffect"></param>
        /// <param name="source"></param>
        private void ApplyOcclusionEffect(AudioEffectData obstacleEffect, AudioSource source)
        {
            var applicator = source.GetComponent<AudioEffectApplicator>();
            if (!applicator)
                applicator = source.gameObject.AddComponent<AudioEffectApplicator>();
            Assert.IsNotNull(applicator);

            applicator.Apply(obstacleEffect);
        }


        /// <summary>
        ///     Get all hits. Ignores Triggers.
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <returns></returns>
        private List<RaycastHit> GetOccluderHits(Vector3 rayOrigin, Vector3 rayDirection)
        {
            var occluderRay = new Ray(rayOrigin, rayDirection);

            // TODO: Improve performance by using Non-Alloc Raycast
            // Using two arrays with a fixed max size would only be problematic if numFoundHits > max size
            // but in that case we can just assume, that a max occlusion effect can be applied.
            var occludingHits = new List<RaycastHit>(
                Physics.RaycastAll(
                    occluderRay,
                    rayDirection.magnitude + float.Epsilon,
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
        ///     Remove all hits with colliders that contain the ray origins --> e.g. if the audio listener or the audio source
        ///     has a collider on its gameobject.
        /// </summary>
        /// <param name="hits">Reference to the list of raycast hits.</param>
        /// <param name="collidersToRemove">All colliders which should not be present in the hits list</param>
        private void RemoveOriginCollisions(ref List<RaycastHit> hits, Collider[] collidersToRemove)
        {
            foreach (var toRemove in collidersToRemove)
                for (var i = hits.Count - 1; i >= 0; i--)
                {
                    var occludingHit = hits[i];
                    if (toRemove == occludingHit.collider) hits.RemoveAt(i);
                }
        }
    }
}