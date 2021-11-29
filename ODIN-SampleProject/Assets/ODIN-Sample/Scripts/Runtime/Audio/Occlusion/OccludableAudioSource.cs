using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio.Occlusion
{
    [RequireComponent(typeof(AudioSource), typeof(SphereCollider))]
    public class OccludableAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;
        private SphereCollider _listenerDetector;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Assert.IsNotNull(_audioSource);
            _listenerDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(_listenerDetector);
        }

    }
}
