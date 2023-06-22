using System.Collections;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    ///     Applies the push to talk settings for ODIN rooms.
    /// </summary>
    public class OdinPushToTalk : MonoBehaviour
    {
        public static OdinPushToTalk Instance { get; private set; }
        
        /// <summary>
        ///     The list of settings for different rooms. Allows definition of different push-to-talk buttons for different
        ///     ODIN rooms.
        /// </summary>
        [SerializeField] protected OdinPushToTalkSettings pushToTalkSettings;

        private MicrophoneReader _microphoneReader;
        
        protected virtual void Awake()
        {
            if(Instance)
                Destroy(this);
            else
                Instance = this;
            
            Assert.IsNotNull(pushToTalkSettings);
            foreach (OdinPushToTalkSettings.OdinPushToTalkData data in pushToTalkSettings.settings)
            {
                Assert.IsNotNull(data.connectedRoom, $"Missing push to talk setting on object {gameObject.name}");
                Assert.IsNotNull(data.pushToTalkButton, $"Missing push to talk setting on object {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
            pushToTalkSettings.Load();
            
            foreach (var odinPushToTalkData in pushToTalkSettings.settings)
            {
                odinPushToTalkData.pushToTalkButton.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (OdinHandler.Instance && _microphoneReader)
                _microphoneReader.OnMicrophoneData -= OnMicrophoneData;
        }

        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance || !OdinHandler.Instance.Microphone)
                yield return null;

            _microphoneReader = OdinHandler.Instance.Microphone;
            _microphoneReader.RedirectCapturedAudio = false;
            _microphoneReader.OnMicrophoneData += OnMicrophoneData;
        }

        private void OnMicrophoneData(float[] buffer, int position)
        {
            SetCustomVolume(buffer);
            
            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                if (IsMicrophoneMuted(room.Config.Name))
                    continue;
                
                if (room.MicrophoneMedia != null)
                    room.MicrophoneMedia.AudioPushData(buffer);
                else if (room.IsJoined && OdinHandler.Config.VerboseDebug)
                    Debug.LogWarning($"Room {room.Config.Name} is missing a microphone stream. See Room.CreateMicrophoneMedia");
            }
           
        }

        private void SetCustomVolume(float[] buffer)
        {
            if (_microphoneReader.CustomMicVolumeScale)
            {
                float bufferScale = GetVolumeScale(_microphoneReader.MicVolumeScale);
                SetVolume(ref buffer, bufferScale);
            }
        }

        private float GetVolumeScale(float value)
        {
            return Mathf.Pow(value, 3);
        }
        
        private void SetVolume(ref float[] buffer, float scale)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] *= scale;
        }

        /// <summary>
        /// Checks whether the microphone is muted for the given room.
        /// </summary>
        /// <param name="roomName">The room name.</param>
        /// <returns>True, if push to talk is active and push to talk button is pressed or if push to talk is inactive. False otherwise.</returns>
        protected virtual bool IsMicrophoneMuted(string roomName)
        {
            if (OdinHandler.Instance.Rooms.Contains(roomName))
            {
                Room room = OdinHandler.Instance.Rooms[roomName];
                foreach (OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData in pushToTalkSettings.settings)
                    if (pushToTalkData.connectedRoom == room.Config.Name)
                    {
                        if (!pushToTalkData.pushToTalkIsActive) return false;
                        return !IsPushToTalkButtonPressed(pushToTalkData);
                    }
            }
            return false;
        }

        /// <summary>
        /// Checks if the push to talk button is pressed.
        /// </summary>
        /// <param name="pushToTalkData"></param>
        /// <returns></returns>
        protected virtual bool IsPushToTalkButtonPressed(OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData)
        {
            bool isPushToTalkPressed = pushToTalkData.pushToTalkButton.action.IsPressed();
            return isPushToTalkPressed;
        }
        
    }
}