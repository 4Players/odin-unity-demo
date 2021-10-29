using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioObstacle", menuName = "Odin-Sample/AudioObstacle", order = 0)]
    public class AudioObstacleData : ScriptableObject
    {
        public float cutoffFrequency = 500.0f;
        public float lowpassResonanceQ = 1.0f;

        public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
    }
}