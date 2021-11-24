using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.AudioOcclusion
{
    [CreateAssetMenu(fileName = "DirectionalAudioSettings", menuName = "Odin-Sample/DirectionalAudioSettings", order = 0)]
    public class DirectionAudioSettings : ScriptableObject
    {
        [SerializeField] public float audioSourceDetectionRange = 100.0f;
        
        [SerializeField] public AnimationCurve angleToVolumeCurve;
    }
}