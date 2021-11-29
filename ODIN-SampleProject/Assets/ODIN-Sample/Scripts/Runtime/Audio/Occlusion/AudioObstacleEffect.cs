using System;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    [Serializable]
    public class AudioObstacleEffect : IComparable<AudioObstacleEffect>
    {
        public float volumeMultiplier = 1.0f;
        
        public float cutoffFrequency = 500.0f;
        public float lowpassResonanceQ = 1.0f;
        public int CompareTo(AudioObstacleEffect other)
        {
            if (null == other)
                return -1;
            return cutoffFrequency.CompareTo(other.cutoffFrequency);
        }

        public AudioObstacleEffect()
        {
            
        }

        public AudioObstacleEffect(float cutoffFrequency)
        {
            this.cutoffFrequency = cutoffFrequency;
        }
    }
}