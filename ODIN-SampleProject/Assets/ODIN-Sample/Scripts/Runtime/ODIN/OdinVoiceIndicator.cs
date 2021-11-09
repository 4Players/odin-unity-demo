using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Photon;
using OdinNative.Unity.Audio;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    [RequireComponent(typeof(Renderer))]
    public class OdinVoiceIndicator : MonoBehaviourPunCallbacks
    {
        [SerializeField] private OdinVoiceUser voiceUser;
        [SerializeField] private Color voiceOnColor = Color.green;


        private List<PlaybackComponent> _playbackComponents = new List<PlaybackComponent>();
        private int _numActivePlaybacks = 0;

        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(voiceUser);
            _renderer = GetComponent<Renderer>();
            Assert.IsNotNull(_renderer);
            _originalColor = _renderer.material.color;
        }

        public override void OnEnable()
        {
            base.OnEnable();   
            voiceUser.onPlaybackComponentAdded.AddListener(OnPlaybackAdded);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            voiceUser.onPlaybackComponentAdded.RemoveListener(OnPlaybackAdded);
        }

        private void Update()
        {
            if(photonView.IsMine)
                SetFeedbackColor(OdinHandler.Instance.Microphone.RedirectCapturedAudio);
        }

        private void OnDestroy()
        {
            foreach (PlaybackComponent playbackComponent in _playbackComponents)
            {
                if (playbackComponent)
                {
                    playbackComponent.OnPlaybackPlayingStatusChanged -= OnPlaybackPlayingStatusChanged;
                }
            }
        }

        private void OnPlaybackAdded(PlaybackComponent playback)
        {
            _playbackComponents.Add(playback);
            playback.OnPlaybackPlayingStatusChanged += OnPlaybackPlayingStatusChanged;
        }


        private void OnPlaybackPlayingStatusChanged(PlaybackComponent component, bool isplaying)
        {
            if (!enabled)
                return;

            if (isplaying)
            {
                _numActivePlaybacks++;
            }
            else
            {
                _numActivePlaybacks--;
            }

            SetFeedbackColor(_numActivePlaybacks > 0);
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