using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio.Directional
{
    [CreateAssetMenu(fileName = "DirectionalAudioSettings", menuName = "Odin-Sample/DirectionalAudioSettings", order = 0)]
    public class DirectionalAudioSettings : ScriptableObject
    {
        [SerializeField] public float audioSourceDetectionRange = 100.0f;
        
        [FormerlySerializedAs("angleToVolumeCurve")] [SerializeField] public AnimationCurve angleToCutOffFrequencyCurve;
    }
}