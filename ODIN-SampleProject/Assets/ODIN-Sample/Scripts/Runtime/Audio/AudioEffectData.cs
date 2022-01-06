using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Data class, containing values for adjusting audio effects.
    /// </summary>
    [Serializable]
    public class AudioEffectData : IComparable<AudioEffectData>
    {
        /// <summary>
        /// The volume in range [0,1].
        /// </summary>
        [FormerlySerializedAs("volumeMultiplier")]
        [Range(0f,1f)]
        public float volume;

        /// <summary>
        /// The cutoff frequency for a Low Pass Filter effect in range [0,22000].
        /// </summary>
        [Range(0f,22000f)]
        public float cutoffFrequency;
        /// <summary>
        /// The low pass Resonance Q value for a Low Pass Filter effect. 1 is default.
        /// </summary>
        public float lowpassResonanceQ;


        public AudioEffectData(AudioEffectData other)
        {
            this.volume = other.volume;
            this.cutoffFrequency = other.cutoffFrequency;
            this.lowpassResonanceQ = other.lowpassResonanceQ;
        }
        
        public AudioEffectData(float cutoffFrequency = 22000.0f, float volume = 1.0f, float lowpassResonanceQ = 1.0f)
        {
            this.volume = volume;
            this.cutoffFrequency = cutoffFrequency;
            this.lowpassResonanceQ = lowpassResonanceQ;
        }

        /// <summary>
        /// Compares the <see cref="cutoffFrequency"/> to the value of the other object.
        /// </summary>
        /// <param name="other">The effect to compare to.</param>
        /// <returns>Less than zero, if the cutoffFrequency is lower than the other effects, greater than zero if the
        /// cutoffFrequency is higher and zero if equal.</returns>
        public int CompareTo(AudioEffectData other)
        {
            if (null == other)
                return -1;
            return cutoffFrequency.CompareTo(other.cutoffFrequency);
        }

        /// <summary>
        /// Checks whether this effect is audible when applied.
        /// </summary>
        /// <returns>True, if the effect's parameter hit certain thresholds, false otherwise.</returns>
        public bool HasAudibleEffect()
        {
            return volume < 0.99f || volume > 1.01f || cutoffFrequency < 22000 || lowpassResonanceQ < 0.99f || lowpassResonanceQ > 1.01f;
        }

        /// <summary>
        /// Returns a new <see cref="AudioEffectData"/> object, containing the combined effect of the given two effects.
        /// </summary>
        /// <param name="first">First effect</param>
        /// <param name="second">Second effect</param>
        /// <returns>Combined effect.</returns>
        public static AudioEffectData GetCombinedEffect(AudioEffectData first, AudioEffectData second)
        {
            if (null == first)
                return second;
            if (null == second)
                return first;

            var combined = new AudioEffectData();

            combined.volume = Mathf.Min(first.volume, second.volume);
            combined.volume = Mathf.Min(1.0f, combined.volume);
            combined.cutoffFrequency = Mathf.Min(first.cutoffFrequency, second.cutoffFrequency);
            combined.lowpassResonanceQ = Mathf.Min(first.lowpassResonanceQ, second.lowpassResonanceQ);

            return combined;
        }

        public override string ToString()
        {
            var asString =
                $"Volume: {volume}, CutoffFrequency: {cutoffFrequency}, lowpassResonanceQ: {lowpassResonanceQ}";

            return asString;
        }
    }
}