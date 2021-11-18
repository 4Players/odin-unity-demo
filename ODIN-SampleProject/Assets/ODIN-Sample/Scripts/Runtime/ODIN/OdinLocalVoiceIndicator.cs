using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    [RequireComponent(typeof(Renderer))]
    public class OdinLocalVoiceIndicator : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Color voiceOnColor = Color.green;
        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            Assert.IsNotNull(_renderer);
            _originalColor = _renderer.material.color;
        }

        private void Update()
        {
            if (photonView && photonView.IsMine)
            {
                bool isVoiceOn = false;
                if (OdinHandler.Instance)
                    isVoiceOn = OdinHandler.Instance.Microphone.RedirectCapturedAudio;
                SetFeedbackColor(isVoiceOn);
            }
        }
        
        private void SetFeedbackColor(bool isVoiceOn)
        {
            if (isVoiceOn)
            {
                _renderer.material.color = voiceOnColor;
            }
            else
            {
                _renderer.material.color = _originalColor;
            }
        }
    }
}