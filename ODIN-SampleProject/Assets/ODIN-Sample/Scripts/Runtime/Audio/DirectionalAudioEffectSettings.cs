using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Scriptable Object containing data used by the <see cref="DirectionalAudioListener"/> script to apply directional
    /// audio effects to Audio Sources in the detection range. 
    /// </summary>
    [CreateAssetMenu(fileName = "DirectionalAudioEffectSettings", menuName = "Odin-Sample/DirectionalAudioEffectSettings", order = 0)]
    public class DirectionalAudioEffectSettings : ScriptableObject
    {
        /// <summary>
        /// The curve mapping from the angle between audio listener and audio source to a cutoff frequency, which is
        /// applied to a lowpass filter.
        /// </summary>
        [SerializeField] private AnimationCurve cutoffFrequencyCurve;
        /// <summary>
        /// THe curve mapping from the angle between audio listener and audio source to a volume multiplier, which
        /// is applied to the target audio source.
        /// </summary>
        [SerializeField] private AnimationCurve volumeCurve;

        /// <summary>
        /// The curve mapping from the angle between audio listener and audio source to a cutoff frequency, which is
        /// applied to a lowpass filter.
        /// </summary>
        public AnimationCurve CutoffFrequencyCurve => cutoffFrequencyCurve;
        
        /// <summary>
        /// THe curve mapping from the angle between audio listener and audio source to a volume multiplier, which
        /// is applied to the target audio source.
        /// </summary>
        public AnimationCurve VolumeCurve => volumeCurve;

        /// <summary>
        /// Returns a cutoff frequency given an angle. Based on the mapping of the angle to a cutoff frequency given by <see cref="CutoffFrequencyCurve"/>.
        /// </summary>
        /// <param name="angle">Angle (usually between audio listener and audio source).</param>
        /// <returns>Cutoff Frequency for application with a lowpass filter.</returns>
        public float GetCutoffFrequency(float angle)
        {
            return CutoffFrequencyCurve.Evaluate(angle);
        }

        /// <summary>
        /// Returns a volume multiplier given an angle. Based on the mapping of the angle to a volume Multiplier given by <see cref="VolumeCurve"/>.
        /// </summary>
        /// <param name="angle">Angle (usually between audio listener and audio source).</param>
        /// <returns>Volume Multiplier for application on an Audio Source.</returns>
        public float GetVolume(float angle)
        {
            return VolumeCurve.Evaluate(angle);
        }

        /// <summary>
        /// Returns an Audio Effect object, containing the effect to be applied for a given angle. Based on mappings given by
        /// <see cref="VolumeCurve"/> and <see cref="CutoffFrequencyCurve"/>.
        /// </summary>
        /// <param name="angle">Angle (usually between audio listener and audio source).</param>
        /// <returns>Audio Effect object, e.g. for direct application by an <see cref="AudioEffectApplicator"/>.</returns>
        public AudioEffectData GetAudioEffectData(float angle)
        {
            float currentVolume = GetVolume(angle);
            float currentCutoffFrequency = GetCutoffFrequency(angle);

            return new AudioEffectData() { cutoffFrequency = currentCutoffFrequency, volume = currentVolume };
        }
    }
}