using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Script containing the behaviour for applying and removing both occlusion or direction effects on the connected
    /// audio source. Multiple effects added during one frame will be accumulated according
    /// to the <see cref="AudioEffectData"/>'s <see cref="AudioEffectData.GetCombinedEffect"/> implementation.
    /// </summary>
    /// <remarks>
    /// This script is usually added automatically by the directional or occlusion system.
    /// </remarks>
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
            _audioSource.spatializePostEffects = true;
            
            _originalVolume = _audioSource.volume;

            _lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (!_lowPassFilter)
            {
                _lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }
            _originalCutoffFrequency = _lowPassFilter.cutoffFrequency;
        }

        /// <summary>
        /// Resets effects on the audio source to values applied at scene start.
        /// </summary>
        public void Reset()
        {
            _audioSource.volume = _originalVolume;
            _lowPassFilter.cutoffFrequency = _originalCutoffFrequency;
        }

        /// <summary>
        /// Applies the effect to the audio source. Multiple effects added during one frame will be occumulated according
        /// to the <see cref="AudioEffectData"/>'s <see cref="AudioEffectData.GetCombinedEffect"/> implementation.
        /// </summary>
        /// <param name="effectData"></param>
        public void Apply(AudioEffectData effectData)
        {
            _effectList.Add(effectData);
        }

        private void Update()
        {
            AudioEffectData toApply = null;
            foreach (AudioEffectData effectData in _effectList)
            {
                toApply = AudioEffectData.GetCombinedEffect(toApply, effectData);
            }

            if (null != toApply)
            {
                _lowPassFilter.enabled = true;
                _lowPassFilter.cutoffFrequency = toApply.cutoffFrequency;
                _lowPassFilter.lowpassResonanceQ = toApply.lowpassResonanceQ;
                _audioSource.volume = toApply.volume;
            }
            else
            {
                _lowPassFilter.enabled = false;
                _audioSource.volume = _originalVolume;
            }
            
            _effectList.Clear();
        }
    }

    
}