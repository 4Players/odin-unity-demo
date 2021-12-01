using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Room;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    [RequireComponent(typeof(Renderer))]
    public class OdinLocalVoiceIndicator : MonoBehaviourPunCallbacks
    {
        [SerializeField] private OdinStringVariable roomName;
        [SerializeField] private Color voiceOnColor = Color.green;
        
        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(roomName);
            _renderer = GetComponent<Renderer>();
            Assert.IsNotNull(_renderer);
            _originalColor = _renderer.material.color;
        }

        private void Update()
        {
            if (photonView && photonView.IsMine)
            {
                bool isVoiceOn = false;

                if (OdinHandler.Instance && OdinHandler.Instance.Rooms.Contains(roomName))
                {
                    Room room = OdinHandler.Instance.Rooms[roomName];
                    if(null != room.MicrophoneMedia)
                        isVoiceOn = !room.MicrophoneMedia.IsMuted;
                }
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