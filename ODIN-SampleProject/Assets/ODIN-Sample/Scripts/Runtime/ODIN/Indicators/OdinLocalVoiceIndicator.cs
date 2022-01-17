using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    /// Behaviour for showing visual feedback, when the local player's voice is transmitting to an ODIN room with the name
    /// <see cref="roomName"/>. 
    /// </summary>
    public class OdinLocalVoiceIndicator : MonoBehaviour
    {
        /// <summary>
        /// This renderers color will be switched to <see cref="voiceOnColor"/>, if the local player is transmitting. The
        /// color will return back to the original color of the renderers material.
        /// </summary>
        [SerializeField] private Renderer indicationTarget;
        /// <summary>
        /// The name of the ODIN room on which the indicator should be listening for transmissions.
        /// </summary>
        [SerializeField] private OdinStringVariable roomName;
        /// <summary>
        /// The color the <see cref="indicationTarget"/> should display when the local player is transmitting.
        /// </summary>
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