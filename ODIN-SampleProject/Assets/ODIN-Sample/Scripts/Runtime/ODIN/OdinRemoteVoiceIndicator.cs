using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using ODIN_Sample.Scripts.Runtime.Photon;
using OdinNative.Unity.Audio;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    
    /// <summary>
    /// Indicates whether this remote player is currently talking in the room, given by <see cref="odinRoomName"/>, by changing the
    /// color of a mesh. Requires to be put on the same gameobject as the mesh which should change its color.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class OdinRemoteVoiceIndicator : MonoBehaviour
    {
        /// <summary>
        /// The name of the ODIN room, for which this indicator should signal voice activity.
        /// </summary>
        [SerializeField] private StringVariable odinRoomName;
        [SerializeField] private OdinDistanceVoiceUser voiceUser;
        [SerializeField] private Color voiceOnColor = Color.green;

        private List<PlaybackComponent> _playbackComponents = new List<PlaybackComponent>();
        private int _numActivePlaybacks = 0;

        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(odinRoomName);
            
            Assert.IsNotNull(voiceUser);
            _renderer = GetComponent<Renderer>();
            Assert.IsNotNull(_renderer);
            _originalColor = _renderer.material.color;
        }

        public void OnEnable()
        {
            voiceUser.onPlaybackComponentAdded.AddListener(OnPlaybackAdded);
        }

        public void OnDisable()
        {
            voiceUser.onPlaybackComponentAdded.RemoveListener(OnPlaybackAdded);
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
            if (playback.RoomName == odinRoomName.Value)
            {
                _playbackComponents.Add(playback);
                playback.OnPlaybackPlayingStatusChanged += OnPlaybackPlayingStatusChanged;
            }
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