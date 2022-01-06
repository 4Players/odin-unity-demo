using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Reference Object to an Audio Listener. Used in place of a Singleton Pattern. The audio listener can register
    /// itself to a <see cref="AudioListenerReference"/> using a <see cref="AudioListenerRegister"/> script.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioListenerReference", menuName = "Odin-Sample/AudioListenerReference", order = 0)]
    public class AudioListenerReference : ScriptableObject
    {
        /// <summary>
        /// The currently referenced Audio Listener script.
        /// </summary>
        public AudioListener AudioListener { get; private set; }

        /// <summary>
        /// Called when a new Audio Listener was registered.
        /// </summary>
        public Action<AudioListener> OnRegisteredListener;
        /// <summary>
        /// Called when a Audio Listener was unregistered.
        /// </summary>
        public Action<AudioListener> OnUnregisteredListener;
        

        /// <summary>
        /// Registers the given listener to this reference. Will not work, if another Audio Listener was already registered.
        /// </summary>
        /// <param name="listener">The listener to register.</param>
        public void Register(AudioListener listener)
        {
            if (!AudioListener)
            {
                AudioListener = listener;
                OnRegisteredListener?.Invoke(AudioListener);
            }
            else
                Debug.LogWarning("Could not register new Audio Listener, an existing AudioListener was already registered.");
        }

        /// <summary>
        /// Unregisters the given listener. Will only work, if this object's registered AudioListener is the same as the
        /// given <see cref="listener"/>.
        /// </summary>
        /// <param name="listener">The listener to unregister.</param>
        public void Unregister(AudioListener listener)
        {
            if (listener == AudioListener)
            {
                OnUnregisteredListener?.Invoke(AudioListener);
                AudioListener = null;
            }
            else
                Debug.LogWarning("Could not unregister AudioListener, Listener Reference contains another listener.");
        }
    }
}