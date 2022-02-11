using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    /// Applies the push to talk settings for ODIN rooms. 
    /// </summary>
    public class OdinPushToTalk : MonoBehaviour
    {
        /// <summary>
        /// The list of settings for different rooms. Allows definition of different push-to-talk buttons for different
        /// ODIN rooms.
        /// </summary>
        [SerializeField] private OdinPushToTalkData[] pushToTalkSettings;

        private void Awake()
        {
            foreach (OdinPushToTalkData data in pushToTalkSettings)
            {
                Assert.IsNotNull(data.connectedRoom, $"Missing push to talk setting on object {gameObject.name}");
                Assert.IsNotNull(data.pushToTalkButton, $"Missing push to talk setting on object {gameObject.name}");
            }
        }


        private void OnEnable()
        {
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        private void OnDisable()
        {
            OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }

        
        /// <summary>
        /// Mute the local microphone when first being added. This avoids e.g. the odin local voice indicator to
        /// show or for some data to be sent on accident.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="mediaAddedEventArgs">Event arguments.</param>
        private void OnMediaAdded(object arg0, MediaAddedEventArgs mediaAddedEventArgs)
        {
            // check if the added media is one of the rooms for which the user has provided push to talk data
            foreach (OdinPushToTalkData pushToTalkData in pushToTalkSettings)
            {
                if (pushToTalkData.connectedRoom == mediaAddedEventArgs.Peer.RoomName)
                {
                    Room pushToTalkRoom = OdinHandler.Instance.Rooms[pushToTalkData.connectedRoom];
                    // if the local microphone is the one, for which OnMediaAdded was called
                    if (null != pushToTalkRoom.MicrophoneMedia && pushToTalkRoom.MicrophoneMedia.Id == mediaAddedEventArgs.Media.Id)
                    {
                        // mute the microphone initially
                        pushToTalkRoom.MicrophoneMedia.SetMute(true);
                    }
                }
            }
        }

        private void Update()
        {
            if (!(OdinHandler.Instance && null != OdinHandler.Instance.Rooms))
                return;
            
            foreach (OdinPushToTalkData pushToTalkData in pushToTalkSettings)
            {
                HandleRoomMutedStatus(pushToTalkData.connectedRoom,pushToTalkData.pushToTalkButton);
            }
        }

        /// <summary>
        /// Mutes / unmutes local microphone in the room given by <see cref="roomName"/> based on whether the button given
        /// by <see cref="pushToTalkButton"/> is pressed.
        /// </summary>
        /// <param name="roomName">Room to check</param>
        /// <param name="pushToTalkButton">Push to talk button for that room</param>
        private void HandleRoomMutedStatus(string roomName, string pushToTalkButton)
        {
            if (OdinHandler.Instance.Rooms.Contains(roomName))
            {
                Room roomToCheck = OdinHandler.Instance.Rooms[roomName];

                if (null != roomToCheck.MicrophoneMedia)
                {
                    bool isPushToTalkPressed = Input.GetKey(pushToTalkButton);
                    roomToCheck.MicrophoneMedia.SetMute(!isPushToTalkPressed);
                }
            }
        }
    }

    /// <summary>
    /// Data container for storing push to talk settings.
    /// </summary>
    [Serializable]
    public class OdinPushToTalkData
    {
        /// <summary>
        /// The room for which the push to talk button should work.
        /// </summary>
        public OdinStringVariable connectedRoom;
        /// <summary>
        /// The push to talk button. If this is pressed, the microphone data
        /// will be transmitted in the room given by <see cref="connectedRoom"/>.
        /// </summary>
        public OdinStringVariable pushToTalkButton;
    }
}
