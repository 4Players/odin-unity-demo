using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    ///     Scriptable object containing data, which defines the effect on the Audio Source when being used as an
    ///     Audio Obstacle.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioEffectDefinition", menuName = "Odin-Demo/AudioEffectDefinition", order = 0)]
    public class AudioEffectDefinition : ScriptableObject
    {
        [SerializeField] private AnimationCurve cutoffFrequencyCurve;

        /// <summary>
        ///     The low pass Resonance Q value for a Low Pass Filter effect. 1 is default.
        /// </summary>
        [SerializeField] private AnimationCurve lowpassResonanceQCurve;

        [SerializeField] private AnimationCurve volumeCurve;

        public void Reset()
        {
            lowpassResonanceQCurve = new AnimationCurve(new Keyframe(0, 1.0f), new Keyframe(1, 1.0f));
            volumeCurve = new AnimationCurve(new Keyframe(0, 1.0f), new Keyframe(1, 1.0f));
            cutoffFrequencyCurve = new AnimationCurve(new Keyframe(0, 22000.0f), new Keyframe(1, 22000.0f));
        }

        public float GetVolume(float thickness)
        {
            return volumeCurve.Evaluate(thickness);
        }

        public float GetCutoffFrequency(float thickness)
        {
            return cutoffFrequencyCurve.Evaluate(thickness);
        }

        public float GetLowpassResonanceQ(float thickness)
        {
            return lowpassResonanceQCurve.Evaluate(thickness);
        }

        public AudioEffectData GetEffect(float thickness)
        {
            return new AudioEffectData
            {
                Volume = GetVolume(thickness),
                CutoffFrequency = GetCutoffFrequency(thickness),
                LowpassResonanceQ = GetLowpassResonanceQ(thickness)
            };
        }

        public bool IsAudible(float thickness)
        {
            return GetEffect(thickness).IsAudible;
        }

        /// <summary>
        ///     Returns a new <see cref="Audio.AudioEffectDefinition" /> object, containing the combined effect of the given two
        ///     effects.
        /// </summary>
        /// <param name="first">First effect</param>
        /// <param name="second">Second effect</param>
        /// <returns>Combined effect.</returns>
        public static AudioEffectData GetCombinedEffect(AudioEffectData first,
            AudioEffectData second)
        {
            if (!first.IsAudible)
                return second;
            if (!second.IsAudible)
                return first;

            var combined = new AudioEffectData();

            combined.Volume = Mathf.Min(first.Volume, second.Volume, 1.0f);
            combined.CutoffFrequency = Mathf.Min(first.CutoffFrequency, second.CutoffFrequency);
            combined.LowpassResonanceQ = Mathf.Max(first.LowpassResonanceQ, second.LowpassResonanceQ);

            return combined;
        }
    }
}