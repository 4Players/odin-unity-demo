using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [CreateAssetMenu(fileName = "PushToTalkSettings", menuName = "Odin-Demo/PushToTalkSettings", order = 0)]
    public class OdinPushToTalkSettings : ScriptableObject
    {
        public OdinPushToTalkData[] settings;

        [SerializeField] private string playerPrefsKeyPrefix = "Odin.PushToTalkSettings.";

        public void Save()
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
                PlayerPrefs.SetInt(playerPrefsKeyPrefix + pushToTalkData.connectedRoom.Value,
                    pushToTalkData.pushToTalkIsActive ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
            {
                string key = playerPrefsKeyPrefix + pushToTalkData.connectedRoom;
                if (PlayerPrefs.HasKey(key)) pushToTalkData.pushToTalkIsActive = PlayerPrefs.GetInt(key) == 1;
            }
        }

        public OdinPushToTalkData GetData(string room)
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
                if (pushToTalkData.connectedRoom == room)
                    return pushToTalkData;
            return null;
        }

        public void SetPushToTalkActive(string room, bool newActive)
        {
            Debug.Log($"Setting Push To Talk {room} to {newActive}");
            foreach (OdinPushToTalkData pushToTalkData in settings)
                if (room == pushToTalkData.connectedRoom)
                    pushToTalkData.pushToTalkIsActive = newActive;
        }

        /// <summary>
        ///     Data container for storing push to talk settings.
        /// </summary>
        [Serializable]
        public class OdinPushToTalkData
        {
            /// <summary>
            ///     The room for which the push to talk button should work.
            /// </summary>
            public OdinStringVariable connectedRoom;

            /// <summary>
            ///     The push to talk button. If this is pressed, the microphone data
            ///     will be transmitted in the room given by <see cref="connectedRoom" />.
            /// </summary>
            public OdinStringVariable pushToTalkButton;

            /// <summary>
            ///     This can be set to false, if we want to give the option to leave a room voice-activated, but with the option
            ///     to later set it to push-to-talk.
            /// </summary>
            public bool pushToTalkIsActive = true;
        }
    }
}