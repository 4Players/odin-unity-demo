using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    ///     <para>
    ///         This component automatically detects Audio Sources inside the radius given by the
    ///         <see cref="defaultOcclusionEffect" />
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
    public class OcclusionAudioListener : AAudioListenerEffect
    {
        /// <summary>
        ///     Reference to the audio occlusion settings object.
        /// </summary>
        [FormerlySerializedAs("occlusionSettings")] [SerializeField]
        private AudioEffectDefinition defaultOcclusionEffect;

        /// <summary>
        ///     The Layers on which audio occluding Colliders are detected by the <see cref="OcclusionAudioListener" />.
        /// </summary>
        [SerializeField] private LayerMask audioSourceDetectionLayer = ~0;

        private Collider[] _collidersOnAudiolistener;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(defaultOcclusionEffect);
        }

        private void Start()
        {
            _collidersOnAudiolistener = audioListener.GetComponentsInParent<Collider>();
        }

        protected override void EffectUpdate(AudioSourceData data)
        {
            AudioSource audioSource = data.ConnectedSource;

            // Determine ray origins and ray direction

            Vector3[] rayOrigins = { audioListener.transform.position, audioSource.transform.position };
            Vector3 toAudioSource = rayOrigins[1] - rayOrigins[0];
            // place the origins a centimeter away from their position in the opposite direction of the other ray origin 
            rayOrigins[1] = rayOrigins[0] + toAudioSource + toAudioSource.normalized * 0.001f;
            rayOrigins[0] = rayOrigins[1] - toAudioSource - toAudioSource.normalized * 0.002f;
            toAudioSource = rayOrigins[1] - rayOrigins[0];

            // Retrieve all colliders on the parent object
            Collider[] audioSourceColliders = audioSource.GetComponentsInParent<Collider>();

            bool removeParentColliders = data.GetApplicator().RemoveParentCollidersForOcclusion;
            // Get all hits from audio listener to audio source and from source to listener
            List<RaycastHit> forwardHits = GetOccludingHits(rayOrigins[0], toAudioSource, audioSourceColliders, removeParentColliders);
            List<RaycastHit> backwardsHits = GetOccludingHits(rayOrigins[1], -toAudioSource, audioSourceColliders, removeParentColliders);

            // foreach (RaycastHit forwardHit in forwardHits)
            //     Debug.Log($"Found forwardhit collider on: {forwardHit.collider.gameObject}");
            
            // Initialise with default, non-audible effect
            AudioEffectData combinedEffect = AudioEffectData.Default;
            foreach (RaycastHit forwardHit in forwardHits)
                combinedEffect = AddOcclusionEffect(forwardHit, backwardsHits, combinedEffect);

            // apply the combined effect
            AudioEffectApplicator audioEffectApplicator = data.GetApplicator();
            if (audioEffectApplicator)
                audioEffectApplicator.Apply(combinedEffect);
        }

        private List<RaycastHit> GetOccludingHits(Vector3 rayOrigin, Vector3 rayDirection,
            Collider[] audioSourceColliders, bool RemoveParentColliders)
        {
            List<RaycastHit> hits = GetSortedHits(rayOrigin, rayDirection);
            // Remove colliders, that are inside the audio listener or inside the audio source
            if (RemoveParentColliders)
            {
                RemoveOriginCollisions(ref hits, _collidersOnAudiolistener);
                RemoveOriginCollisions(ref hits, audioSourceColliders);
            }
            
            return hits;
        }

        private AudioEffectData AddOcclusionEffect(RaycastHit forwardHit, List<RaycastHit> backwardsHits,
            AudioEffectData combinedEffect)
        {
            // Get the thickness of the hit object
            float objectThickness = RetrieveThickness(forwardHit, backwardsHits);
            AudioEffectData occlusionEffect;
            // Check if the collider has an Audio Obstacle
            AudioObstacle audioObstacle = forwardHit.collider.GetComponent<AudioObstacle>();
            if (audioObstacle)
            {
                // if yes - use the effect given by the audio obstacle effect definition based on the object thickness
                AudioEffectDefinition effectDefinition = audioObstacle.effect;
                occlusionEffect = effectDefinition.GetEffect(objectThickness);
            }
            else
            {
                // else: use default effect
                occlusionEffect = defaultOcclusionEffect.GetEffect(objectThickness);
            }

            // Combine the effect so far with the newly retrieved effect
            combinedEffect = AudioEffectDefinition.GetCombinedEffect(combinedEffect, occlusionEffect);
            return combinedEffect;
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
                    float distance = Vector3.Distance(backHit.point, frontHit.point);
                    // we want to use the nearest candidate
                    if (distance < currentCandidateDistance)
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
        ///     Get all hits. Ignores Triggers.
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <returns></returns>
        private List<RaycastHit> GetSortedHits(Vector3 rayOrigin, Vector3 rayDirection)
        {
            var occluderRay = new Ray(rayOrigin, rayDirection);

            // TODO: Improve performance by using Non-Alloc Raycast
            // Using two arrays with a fixed max size would only be problematic if numFoundHits > max size
            // but in that case we can just assume, that a max occlusion effect can be applied.
            var occludingHits = new List<RaycastHit>(
                Physics.RaycastAll(
                    occluderRay,
                    rayDirection.magnitude + 0.001f,
                    audioSourceDetectionLayer,
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