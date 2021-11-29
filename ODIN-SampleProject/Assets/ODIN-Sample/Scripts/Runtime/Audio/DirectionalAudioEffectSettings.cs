using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = "DirectionalAudioEffectSettings", menuName = "Odin-Sample/DirectionalAudioEffectSettings", order = 0)]
    public class DirectionalAudioEffectSettings : ScriptableObject
    {
        [SerializeField] private AnimationCurve cutoffFrequencyCurve;
        [SerializeField] private AnimationCurve volumeCurve;

        public AnimationCurve CutoffFrequencyCurve => cutoffFrequencyCurve;
        public AnimationCurve VolumeCurve => volumeCurve;

        public float GetCutoffFrequency(float angle)
        {
            return CutoffFrequencyCurve.Evaluate(angle);
        }

        public float GetVolume(float angle)
        {
            return VolumeCurve.Evaluate(angle);
        }

        public AudioEffectData GetAudioEffectData(float angle)
        {
            float currentVolume = GetVolume(angle);
            float currentCutoffFrequency = GetCutoffFrequency(angle);

            return new AudioEffectData() { cutoffFrequency = currentCutoffFrequency, volume = currentVolume };
        }
    }
}