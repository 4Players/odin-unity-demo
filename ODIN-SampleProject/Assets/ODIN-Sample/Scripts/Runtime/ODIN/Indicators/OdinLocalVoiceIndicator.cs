using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    public class OdinLocalVoiceIndicator : MonoBehaviour
    {
        [SerializeField] private Renderer indicationTarget;
        [SerializeField] private OdinStringVariable roomName;
        [SerializeField] private Color voiceOnColor = Color.green;
        
        private Color _originalColor;

        private void Awake()
        {
            if (null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            Assert.IsNotNull(indicationTarget);
            _originalColor = indicationTarget.material.color;

            Assert.IsNotNull(roomName);
        }

        private void Update()
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