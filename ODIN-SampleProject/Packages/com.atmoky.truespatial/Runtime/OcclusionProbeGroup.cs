using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Atmoky
{
    [Serializable]
    public class OcclusionProbe
    {
        public Vector3 position;

        private bool isOccluding = false;
        public float occlusion = 0.0f;

        public OcclusionProbe(Vector3 position)
        {
            this.position = position;
        }

        public bool IsOccluding()
        {
            return isOccluding;
        }

        public void SetOccluding(bool isOccluding)
        {
            this.isOccluding = isOccluding;
        }

        public float GetOcclusion()
        {
            return occlusion;
        }

        public void SetOcclusion(float occlusion)
        {
            this.occlusion = occlusion;
        }
    }

    [Icon("Packages/com.atmoky.truespatial/Editor/Icons/AtmokyPentatope.tiff")]
    [AddComponentMenu("Atmoky/Atmoky Occlusion Probe Group")]
    [Serializable]
    [ExecuteInEditMode]
    public class OcclusionProbeGroup : MonoBehaviour
    {
        public Source overrideSource;

        public bool useRaycastNonAlloc = false;

        public int maxNumberOfHits = 100;

        public LayerMask occlusionLayerMask = ~0;

        public List<OcclusionProbe> probes;

        [Range(0.0f, 100.0f)]
        public float occlusionSensitivity = 50.0f;

        private float finalOcclusionValue = 0.0f;

        public float GetFinalOcclusionValue()
        {
            return finalOcclusionValue;
        }

        private Camera referenceCamera;

        public OcclusionProbeGroup()
        {
            if (probes == null)
            {
                probes = new List<OcclusionProbe>();
                probes.Add(new OcclusionProbe(new Vector3(1, 1, 1)));
                probes.Add(new OcclusionProbe(new Vector3(-1, 1, 1)));
                probes.Add(new OcclusionProbe(new Vector3(1, -1, 1)));
                probes.Add(new OcclusionProbe(new Vector3(-1, -1, 1)));
                probes.Add(new OcclusionProbe(new Vector3(1, 1, -1)));
                probes.Add(new OcclusionProbe(new Vector3(-1, 1, -1)));
                probes.Add(new OcclusionProbe(new Vector3(1, -1, -1)));
                probes.Add(new OcclusionProbe(new Vector3(-1, -1, -1)));
            }
        }

        private RaycastHit[] hits;

        public int AddProbe()
        {
            probes.Add(new OcclusionProbe(new Vector3(0, 0, 0)));
            return probes.Count - 1;
        }

        public void RemoveProbe(int index)
        {
            if (index < 0 || index >= probes.Count)
                return;

            probes.RemoveAt(index);
        }

        void Update()
        {
            var source = GetSource();

            if (source == null)
                return;

#if UNITY_EDITOR
            if (Application.isPlaying)
                referenceCamera = Camera.main;
            else
                referenceCamera = SceneView.lastActiveSceneView.camera;
#else
            referenceCamera = Camera.main;
#endif

            var matrix = transform.localToWorldMatrix;

            var minOcclusion = 1.0f;
            var maxOcclusion = 0.0f;
            var meanOcclusion = 0.0f;

            if (useRaycastNonAlloc && (hits == null || hits.Length != maxNumberOfHits))
                hits = new RaycastHit[maxNumberOfHits];
            int numHits;

            var cameraPosition = referenceCamera.transform.position;

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                probe.SetOccluding(false);

                var worldPosition = matrix.MultiplyPoint(probe.position);
                var direction = (worldPosition - cameraPosition).normalized;
                var distance = (worldPosition - cameraPosition).magnitude;

                if (useRaycastNonAlloc)
                    numHits = Physics.RaycastNonAlloc(
                        cameraPosition,
                        direction,
                        hits,
                        distance,
                        occlusionLayerMask
                    );
                else
                {
                    hits = Physics.RaycastAll(
                        cameraPosition,
                        direction,
                        distance,
                        occlusionLayerMask
                    );
                    numHits = hits.Length;
                }

                var occlusionAccumulated = 0.0f;

                for (int j = 0; j < numHits; ++j)
                {
                    var occluder = hits[j].collider.gameObject.GetComponent<Occluder>();
                    if (occluder != null)
                    {
                        probe.SetOccluding(true);
                        occlusionAccumulated += occluder.occlusion;
                    }
                }

                occlusionAccumulated = Math.Clamp(occlusionAccumulated, 0.0f, 1.0f);
                probe.SetOcclusion(occlusionAccumulated);
                minOcclusion = Math.Min(minOcclusion, occlusionAccumulated);
                maxOcclusion = Math.Max(maxOcclusion, occlusionAccumulated);
                meanOcclusion += occlusionAccumulated;
            }

            meanOcclusion = meanOcclusion / probes.Count();

            if (occlusionSensitivity <= 50)
            {
                var normalized = occlusionSensitivity / 50.0f;
                finalOcclusionValue = minOcclusion * (1.0f - normalized) + meanOcclusion * normalized;
            }
            else
            {
                var normalized = (occlusionSensitivity - 50.0f) / 50.0f;
                finalOcclusionValue = meanOcclusion * (1.0f - normalized) + maxOcclusion * normalized;
            }

            source.occlusion = finalOcclusionValue;
        }

        public Source GetSource()
        {
            Source source = overrideSource;

            if (source == null)
                source = GetComponent<Source>();

            return source;
        }

        public Camera GetReferenceCamera()
        {
            return referenceCamera;
        }
    }
}
