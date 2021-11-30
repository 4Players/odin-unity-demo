using ODIN_Sample.Scripts.Runtime.AudioOcclusion;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio.Directional
{
    [RequireComponent(typeof(AudioEffectApplicator))]
    public class DirectionalAudioSource : MonoBehaviour
    {
        [SerializeField] private AudioListenerReference listenerReference;
        [SerializeField] private DirectionalAudioEffectSettings directionalSettings;
        
        private AudioEffectApplicator _effectApplicator;
        private void Awake()
        {
            Assert.IsNotNull(listenerReference);
            _effectApplicator = GetComponent<AudioEffectApplicator>();
            Assert.IsNotNull(_effectApplicator);
        }

        private void Update()
        {
            if (listenerReference.AudioListener)
            {
                Vector3 listenerPosition = listenerReference.AudioListener.transform.position;
                Vector3 currentPosition = transform.position;
                Vector3 toListener = listenerPosition - currentPosition;
                float angle = Vector3.SignedAngle(toListener, transform.forward, Vector3.up);

                AudioEffectData effect = directionalSettings.GetAudioEffectData(angle);
                _effectApplicator.Apply(effect);
            }
        }
    }
}