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
        [SerializeField] private OdinPushToTalkSettings pushToTalkSettings;

        private void Awake()
        {
            Assert.IsNotNull(pushToTalkSettings);
            foreach (OdinPushToTalkSettings.OdinPushToTalkData data in pushToTalkSettings.settings)
            {
                Assert.IsNotNull(data.connectedRoom, $"Missing push to talk setting on object {gameObject.name}");
                Assert.IsNotNull(data.pushToTalkButton, $"Missing push to talk setting on object {gameObject.name}");
            }
        }

        private void Update()
        {
            if (!(OdinHandler.Instance && null != OdinHandler.Instance.Rooms))
                return;

            foreach (OdinPushToTalkSettings.OdinPushToTalkData pushToTalkData in pushToTalkSettings.settings)
                HandleRoomMutedStatus(pushToTalkData);
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
        private void HandleRoomMutedStatus(OdinPushToTalkSettings.OdinPushToTalkData data)
        {
            if (OdinHandler.Instance.Rooms.Contains(data.connectedRoom))
            {
                Room roomToCheck = OdinHandler.Instance.Rooms[data.connectedRoom];
                if (null != roomToCheck.MicrophoneMedia)
                {
                    if (!data.pushToTalkIsActive)
                    {
                        roomToCheck.MicrophoneMedia.SetMute(false);
                    }
                    else
                    {
                        bool isPushToTalkPressed = Input.GetKey(data.pushToTalkButton);
                        roomToCheck.MicrophoneMedia.SetMute(!isPushToTalkPressed);
                    }
                }
            }
        }
    }
}