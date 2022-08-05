using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin;
using ODIN_Sample.Scripts.Runtime.ODIN.Utility;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    /// Settings file for the position update behaviour. Will automatically load from a default setting file
    /// in the streaming assets directory. 
    /// </summary>
    [CreateAssetMenu(fileName = "OdinPositionUpdateSettings", menuName = "Odin-Demo/PositionUpdateSettings", order = 0)]
    public class OdinPositionUpdateSettings : ScriptableObject
    {
        /// <summary>
        /// Amount of seconds in between each position update interval.
        /// </summary>
        public float updateInterval = 0.5f;
        /// <summary>
        /// Individual room settings for each room.
        /// </summary>
        public List<OdinRoomProximityStatus> roomSettings;

        /// <summary>
        /// The room settings file name.
        /// </summary>
        private static readonly string SAVE_FILE_NAME = "OdinRoomProximitySettings.json";
        
        private void OnEnable()
        {
            string settingsPath = SaveFileUtility.GetSettingsPath(SAVE_FILE_NAME);
            var saveData = SaveFileUtility.LoadData<OdinPositionUpdateSettingsSchema>(settingsPath);
            if (null != saveData)
            {
                updateInterval = saveData.updateInterval;
                roomSettings = saveData.roomSettings;
            }
            else
            {
                SaveFileUtility.SaveData(settingsPath, this);
            }
        }

        /// <summary>
        /// Retrieves the proximity settings for the given room.
        /// </summary>
        /// <param name="roomName">The room name.</param>
        /// <returns>Proximity settings if settings are available for the room, null otherweise.</returns>
        public OdinRoomProximityStatus GetRoomProximityStatus(string roomName)
        {
            foreach (OdinRoomProximityStatus roomSetting in roomSettings)
            {
                if (roomName == roomSetting.roomName)
                    return roomSetting;
            }
            return null;
        }

        /// <summary>
        /// Schema in which to save the Position Update Settings to file.
        /// </summary>
        [Serializable]
        public class OdinPositionUpdateSettingsSchema
        {
            public float updateInterval;
            public List<OdinRoomProximityStatus> roomSettings;
        }
    }
    /// <summary>
    /// The individual room settings schema.
    /// </summary>
    [Serializable]
    public class OdinRoomProximityStatus
    {
        public string roomName;
        public bool isActive = true;
        public float proximityRadius = 20.0f;
    }
}