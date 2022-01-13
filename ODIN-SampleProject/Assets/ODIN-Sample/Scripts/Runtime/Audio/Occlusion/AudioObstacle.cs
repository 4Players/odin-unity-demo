using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    /// <summary>
    /// Data component, containing settings for overwriting the default occlusion behavior of objects. 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AudioObstacle : MonoBehaviour
    {
        /// <summary>
        /// Reference to a scriptable object, containing the Effect Settings Data.
        /// </summary>
        [FormerlySerializedAs("data")] [FormerlySerializedAs("audioObstacleData")] public AudioObstacleSettings settings;

        private void Awake()
        {
            Assert.IsNotNull(settings);
        }
    }
}