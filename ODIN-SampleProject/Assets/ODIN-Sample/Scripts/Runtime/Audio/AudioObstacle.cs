using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    ///     Data component, containing settings for overwriting the default occlusion behavior of objects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AudioObstacle : MonoBehaviour
    {
        /// <summary>
        ///     Reference to a scriptable object, containing the Effect Settings Data.
        /// </summary>
        [FormerlySerializedAs("settings")] public AudioEffectDefinition effect;

        private void Awake()
        {
            Assert.IsNotNull(effect);
        }
    }
}