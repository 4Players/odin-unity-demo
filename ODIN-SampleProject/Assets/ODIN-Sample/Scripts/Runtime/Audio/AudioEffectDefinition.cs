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
        /// <summary>
        ///  The curve defining the cutoff frequency values for a low pass filter effect.
        /// If the object is x meters thick, the cutoff frequency will have a y value.
        /// </summary>
        [SerializeField] private AnimationCurve cutoffFrequencyCurve;

        /// <summary>
        ///    The curve defining the low pass Resonance Q value for a Low Pass Filter effect. 1 is default.
        /// If the object is x meters thick, the low pass resonance q will have a y value.
        /// </summary>
        [SerializeField] private AnimationCurve lowpassResonanceQCurve;

        /// <summary>
        /// The curve defining the applied volume based on the thickness of objects that this effect definition is applied to.
        /// --> If the object is x meters thick, the volume will have y value.
        /// </summary>
        [SerializeField] private AnimationCurve volumeCurve;

        /// <summary>
        /// Resets the AnimationCurves to default values.
        /// </summary>
        public void Reset()
        {
            lowpassResonanceQCurve = new AnimationCurve(new Keyframe(0, 1.0f), new Keyframe(1, 1.0f));
            volumeCurve = new AnimationCurve(new Keyframe(0, 1.0f), new Keyframe(1, 1.0f));
            cutoffFrequencyCurve = new AnimationCurve(new Keyframe(0, 22000.0f), new Keyframe(1, 22000.0f));
        }

        /// <summary>
        /// Returns a volume given the object thickness. Based on the <see cref="volumeCurve"/>.
        /// </summary>
        /// <param name="thickness">Thickness of the object between audio listener and  source in meters.</param>
        /// <returns>Returns resulting volume value.</returns>
        public float GetVolume(float thickness)
        {
            return volumeCurve.Evaluate(thickness);
        }

        /// <summary>
        /// Returns a cutoff frequency given the object thickness. Based on the <see cref="cutoffFrequencyCurve"/>.
        /// </summary>
        /// <param name="thickness">Thickness of the object between audio listener and  source in meters.</param>
        /// <returns>Returns resulting cutoff frequency value.</returns>
        public float GetCutoffFrequency(float thickness)
        {
            return cutoffFrequencyCurve.Evaluate(thickness);
        }

        /// <summary>
        /// Returns a low pass resonance q value given the object thickness. Based on the <see cref="lowpassResonanceQCurve"/>.
        /// </summary>
        /// <param name="thickness">Thickness of the object between audio listener and  source in meters.</param>
        /// <returns>Returns resulting low pass resonance q value.</returns>
        public float GetLowpassResonanceQ(float thickness)
        {
            return lowpassResonanceQCurve.Evaluate(thickness);
        }

        /// <summary>
        /// Returns the values for all effect curves based on the object thickness. 
        /// </summary>
        /// <param name="thickness">Thickness of the object between audio listener and  source in meters.</param>
        /// <returns>Returns resulting effect data.</returns>
        public AudioEffectData GetEffect(float thickness)
        {
            return new AudioEffectData
            {
                Volume = GetVolume(thickness),
                CutoffFrequency = GetCutoffFrequency(thickness),
                LowpassResonanceQ = GetLowpassResonanceQ(thickness)
            };
        }

        /// <summary>
        /// Checks whether the effect is audible for a given thickness. 
        /// </summary>
        /// <param name="thickness">Thickness of the object between audio listener and  source in meters.</param>
        /// <returns>Returns false, if the effect can't be perceived by the human ear, true otherwise.</returns>
        public bool IsEffectAudible(float thickness)
        {
            return GetEffect(thickness).IsEffectAudible;
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
            if (!first.IsEffectAudible)
                return second;
            if (!second.IsEffectAudible)
                return first;

            var combined = new AudioEffectData();

            combined.Volume = Mathf.Min(first.Volume, second.Volume, 1.0f);
            combined.CutoffFrequency = Mathf.Min(first.CutoffFrequency, second.CutoffFrequency);
            combined.LowpassResonanceQ = Mathf.Max(first.LowpassResonanceQ, second.LowpassResonanceQ);

            return combined;
        }
    }
}