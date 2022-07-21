using System.Collections;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    ///     Applies the push to talk settings for ODIN rooms.
    /// </summary>
    public class OdinPushToTalk : MonoBehaviour
    {
        /// <summary>
        ///     The list of settings for different rooms. Allows definition of different push-to-talk buttons for different
        ///     ODIN rooms.
        /// </summary>
        [SerializeField] protected OdinPushToTalkSettings pushToTalkSettings;

        protected virtual void Awake()
        {
            Assert.IsNotNull(pushToTalkSettings);
            foreach (OdinPushToTalkSettings.OdinPushToTalkData data in pushToTalkSettings.settings)
            {
                Assert.IsNotNull(data.connectedRoom, $"Missing push to talk setting on object {gameObject.name}");
                Assert.IsNotNull(data.pushToTalkButton, $"Missing push to talk setting on object {gameObject.name}");
            }
        }

        protected virtual void Update()
        {
            if (!(OdinHandler.Instance && null != OdinHandler.Instance.Rooms))
                return;

            foreach (OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData in pushToTalkSettings.settings)
                HandleRoomMutedStatus(pushToTalkData.connectedRoom);
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
            pushToTalkSettings.Load();
        }

        private void OnDisable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }

        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

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

        protected virtual bool IsPushToTalkButtonPressed(OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData)
        {
            bool isPushToTalkPressed = Input.GetButton(pushToTalkData.pushToTalkButton);
            return isPushToTalkPressed;
        }


        /// <summary>
        ///     Mute the local microphone when first being added. This avoids e.g. the odin local voice indicator to
        ///     show or for some data to be sent on accident.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="mediaAddedEventArgs">Event arguments.</param>
        private void OnMediaAdded(object arg0, MediaAddedEventArgs mediaAddedEventArgs)
        {
            // check if the added media is one of the rooms for which the user has provided push to talk data
            foreach (OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData in pushToTalkSettings.settings)
                if (pushToTalkData.connectedRoom == mediaAddedEventArgs.Peer.RoomName)
                {
                    Room pushToTalkRoom = OdinHandler.Instance.Rooms[pushToTalkData.connectedRoom];
                    // if the local microphone is the one, for which OnMediaAdded was called
                    if (null != pushToTalkRoom.MicrophoneMedia &&
                        pushToTalkRoom.MicrophoneMedia.Id == mediaAddedEventArgs.Media.Id)
                        // mute the microphone initially
                        pushToTalkRoom.MicrophoneMedia.SetMute(true);
                }
        }

        /// <summary>
        ///     Mutes / unmutes local microphone in the room based on whether the button
        ///     given is pressed.
        /// </summary>
        /// <param name="data">
        ///     Push To Talk data container, containing information on the push to talk button,
        ///     the room to target and whether push to talk is currently activated.
        /// </param>
        protected void HandleRoomMutedStatus(string roomName)
        {
            SetRoomMicrophoneMutedState(roomName, IsMicrophoneMuted(roomName));
        }

        protected void SetRoomMicrophoneMutedState(string roomName, bool newIsMuted)
        {
            if (OdinHandler.Instance.Rooms.Contains(roomName))
            {
                Room roomToCheck = OdinHandler.Instance.Rooms[roomName];
                if (null != roomToCheck.MicrophoneMedia)
                    roomToCheck.MicrophoneMedia.SetMute(newIsMuted);
            }
        }
    }
}