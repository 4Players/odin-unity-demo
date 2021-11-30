using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [Serializable]
    public class AudioEffectData : IComparable<AudioEffectData>
    {
        [FormerlySerializedAs("volumeMultiplier")]
        public float volume;

        public float cutoffFrequency;
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

        public int CompareTo(AudioEffectData other)
        {
            if (null == other)
                return -1;
            return cutoffFrequency.CompareTo(other.cutoffFrequency);
        }

        public bool HasAudibleEffect()
        {
            return volume < 1.0f || cutoffFrequency < 22000 || lowpassResonanceQ < 1.0f;
        }

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