using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.AudioOcclusion
{
    [RequireComponent(typeof(AudioListener))]
    public class AudioListenerRegister : MonoBehaviour
    {
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