using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    /// Scriptable object containing push to talk settings.
    ///
    /// TODO: For consistency and readability, should be saved to file instead of using player prefs.
    /// </summary>
    [CreateAssetMenu(fileName = "PushToTalkSettings", menuName = "Odin-Demo/PushToTalkSettings", order = 0)]
    public class OdinPushToTalkSettings : ScriptableObject
    {
        /// <summary>
        /// The settings for each room.
        /// </summary>
        public OdinPushToTalkData[] settings;

        /// <summary>
        /// The key prefix for push to talk settings saved in player prefs.
        /// </summary>
        [SerializeField] private string playerPrefsKeyPrefix = "Odin.PushToTalkSettings.";

        /// <summary>
        /// Saves the current settings to player prefs.
        /// </summary>
        public void Save()
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
                PlayerPrefs.SetInt(playerPrefsKeyPrefix + pushToTalkData.connectedRoom.Value,
                    pushToTalkData.pushToTalkIsActive ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads current settings from player prefs
        /// </summary>
        public void Load()
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
            {
                string key = playerPrefsKeyPrefix + pushToTalkData.connectedRoom;
                if (PlayerPrefs.HasKey(key)) pushToTalkData.pushToTalkIsActive = PlayerPrefs.GetInt(key) == 1;
            }
        }

        /// <summary>
        /// Get push to talk data for a specific room
        /// </summary>
        /// <param name="room">Room name.</param>
        /// <returns>Returns push to talk data, if available for the given room, null otherwise.</returns>
        public OdinPushToTalkData GetData(string room)
        {
            foreach (OdinPushToTalkData pushToTalkData in settings)
                if (pushToTalkData.connectedRoom == room)
                    return pushToTalkData;
            return null;
        }

        /// <summary>
        /// Set the new push to talk active status for the given room.
        /// </summary>
        /// <param name="room">The room name</param>
        /// <param name="newActive">Whether push to talk is active now for this room.</param>
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
            public InputActionReference pushToTalkButton;

            /// <summary>
            ///     This can be set to false, if we want to give the option to leave a room voice-activated, but with the option
            ///     to later set it to push-to-talk.
            /// </summary>
            public bool pushToTalkIsActive = true;
        }
    }
}