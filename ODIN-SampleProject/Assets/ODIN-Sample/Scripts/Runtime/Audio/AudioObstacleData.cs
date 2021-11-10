using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioObstacle", menuName = "Odin-Sample/AudioObstacle", order = 0)]
    public class AudioObstacleData : ScriptableObject, IComparable<AudioObstacleData>
    {
        public float cutoffFrequency = 500.0f;
        public float lowpassResonanceQ = 1.0f;
        public int CompareTo(AudioObstacleData other)
        {
            return cutoffFrequency.CompareTo(other.cutoffFrequency);
        }
    }
}