using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    /// <summary>
    /// Scriptable object containing data, which defines the effect on the Audio Source when being used as an
    /// Audio Obstacle.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioObstacle", menuName = "Odin-Sample/AudioObstacle", order = 0)]
    public class AudioObstacleSettings : ScriptableObject
    {
        /// <summary>
        /// The Audio Effect Data.
        /// </summary>
        [FormerlySerializedAs("data")] [FormerlySerializedAs("lowpass")] public AudioEffectData effect;
    }
}