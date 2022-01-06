using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Used to automatically register an Audio Listener to the given <see cref="AudioListenerReference"/>.
    /// </summary>
    [RequireComponent(typeof(AudioListener))]
    public class AudioListenerRegister : MonoBehaviour
    {
        /// <summary>
        /// The reference, to which the listener is registered to.
        /// </summary>
        [SerializeField] private AudioListenerReference listenerReference;

        private AudioListener _listener;

        private void Awake()
        {
            Assert.IsNotNull(listenerReference);
            _listener = GetComponent<AudioListener>();
            Assert.IsNotNull(_listener);
            listenerReference.Register(_listener);
        }

        private void OnDestroy()
        {
            if(_listener)
                listenerReference.Unregister(_listener);
        }
    }
}