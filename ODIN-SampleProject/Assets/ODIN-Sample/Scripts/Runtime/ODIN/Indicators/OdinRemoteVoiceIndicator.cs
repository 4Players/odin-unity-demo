using System.Collections.Generic;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    
    /// <summary>
    /// Behaviour for displaying feedback on whether the remote player represented by the <see cref="odinUser"/> script
    /// is currently transmitting in the ODIN room with the name <see cref="odinRoomName"/>, by changing the color of a
    /// mesh.
    /// </summary>
    public class OdinRemoteVoiceIndicator : MonoBehaviour
    {
        
        /// <summary>
        /// Reference to the remote player for which the indicator should check for transmissions.
        /// </summary>
        [SerializeField] private AOdinUser odinUser;
        /// <summary>
        /// This renderers color will be switched to <see cref="voiceOnColor"/>, if the remote player is transmitting. The
        /// color will return back to the original color of the main materials' initial color.
        /// </summary>
        [SerializeField] private Renderer indicationTarget;
        /// <summary>
        /// The name of the ODIN room, for which this indicator should signal voice activity.
        /// </summary>
        [SerializeField] private OdinStringVariable odinRoomName;
        /// <summary>
        /// The color the <see cref="indicationTarget"/> should display when the remote player is transmitting.
        /// </summary>
        [SerializeField] private Color voiceOnColor = Color.green;
        

        private List<PlaybackComponent> _playbackComponents = new List<PlaybackComponent>();
        private int _numActivePlaybacks = 0;

        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(odinRoomName);
            
            Assert.IsNotNull(odinUser);
            
            if(null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            Assert.IsNotNull(indicationTarget);
            
            _originalColor = indicationTarget.material.color;
        }

        private void OnEnable()
        {
            odinUser.OnPlaybackComponentAdded += OnPlaybackAdded;
        }

        private void OnDisable()
        {
            odinUser.OnPlaybackComponentAdded -= OnPlaybackAdded;
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
                indicationTarget.material.color = voiceOnColor;
            }
            else
            {
                indicationTarget.material.color = _originalColor;
            }
        }
    }
}