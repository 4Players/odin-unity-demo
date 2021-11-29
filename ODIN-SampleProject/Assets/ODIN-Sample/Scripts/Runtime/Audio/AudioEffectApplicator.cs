using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioEffectApplicator : MonoBehaviour
    {

        private AudioSource _audioSource;
        private AudioLowPassFilter _lowPassFilter;

        private float _originalVolume;
        private float _originalCutoffFrequency;

        private List<AudioEffectData> _effectList = new List<AudioEffectData>();

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Assert.IsNotNull(_audioSource);
            _originalVolume = _audioSource.volume;

            _lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (!_lowPassFilter)
            {
                _lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }
            _originalCutoffFrequency = _lowPassFilter.cutoffFrequency;
        }

        public void Reset()
        {
            _audioSource.volume = _originalVolume;
            _lowPassFilter.cutoffFrequency = _originalCutoffFrequency;
        }

        public void Apply(AudioEffectData effectData)
        {
            _effectList.Add(effectData);
        }

        private void Update()
        {
            //TODO
        }
    }

    
}