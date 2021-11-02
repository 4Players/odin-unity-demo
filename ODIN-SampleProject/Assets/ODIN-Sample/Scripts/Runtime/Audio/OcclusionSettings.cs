using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = "OcclusionSettings", menuName = "Odin-Sample/OcclusionSettings", order = 0)]
    public class OcclusionSettings : ScriptableObject
    {
        public float audioSourceDetectionRange = 100.0f;
        public LayerMask audioSourceDetectionLayer = ~0;
    }
}