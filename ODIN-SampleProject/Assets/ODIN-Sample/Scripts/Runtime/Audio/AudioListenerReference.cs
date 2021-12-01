using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioListenerReference", menuName = "Odin-Sample/AudioListenerReference", order = 0)]
    public class AudioListenerReference : ScriptableObject
    {
        public AudioListener AudioListener { get; private set; }

        public Action<AudioListener> OnRegisteredListener;
        public Action<AudioListener> OnUnregisteredListener;
        

        public void Register(AudioListener listener)
        {
            if (!AudioListener)
            {
                AudioListener = listener;
                OnRegisteredListener?.Invoke(AudioListener);
            }
            else
                Debug.LogWarning("Could not register new Audio Listener, ");
        }

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