using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin.Utility;
using OdinNative.Odin.Room;
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
        

        private List<OdinConnectionIdentifier> _connections = new List<OdinConnectionIdentifier>();
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
            odinUser.OnMediaStreamEstablished += OnConnectionAdded;
            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaStateChanged);
        }

        private void OnDisable()
        {
            odinUser.OnMediaStreamEstablished -= OnConnectionAdded;
            OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaStateChanged);
        }
        

        private void OnConnectionAdded(OdinConnectionIdentifier connectionIdentifier)
        {
            if (connectionIdentifier.RoomName == odinRoomName.Value && !_connections.Contains(connectionIdentifier))
            {
                _connections.Add(connectionIdentifier);
            }
        }


        private void OnMediaStateChanged(object sender, MediaActiveStateChangedEventArgs mediaActiveStateChangedEventArgs)
        {
            if (!enabled)
                return;
            
            if(sender is Room sendingRoom)
            {
                var connection =  new OdinConnectionIdentifier(sendingRoom.Config.Name, mediaActiveStateChangedEventArgs.PeerId,
                    mediaActiveStateChangedEventArgs.MediaId);
                if (_connections.Contains(connection))
                {
                    if (mediaActiveStateChangedEventArgs.Active)
                    {
                        _numActivePlaybacks++;
                    }
                    else
                    {
                        _numActivePlaybacks--;
                    }
                }
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