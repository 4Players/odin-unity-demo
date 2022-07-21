using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin;
using ODIN_Sample.Scripts.Runtime.ODIN.Utility;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [CreateAssetMenu(fileName = "OdinPositionUpdateSettings", menuName = "Odin-Demo/PositionUpdateSettings", order = 0)]
    public class OdinPositionUpdateSettings : ScriptableObject
    {
        public float updateInterval = 0.5f;
        public List<OdinRoomProximityStatus> roomSettings;

        private static readonly string SAVE_FILE_NAME = "OdinRoomProximitySettings.json";
        
        private void OnEnable()
        {
            string settingsPath = SaveFileUtility.GetDefaultSettingsPath(SAVE_FILE_NAME);
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

        public OdinRoomProximityStatus GetRoomProximityStatus(string roomName)
        {
            foreach (OdinRoomProximityStatus roomSetting in roomSettings)
            {
                if (roomName == roomSetting.roomName)
                    return roomSetting;
            }
            return null;
        }

        [Serializable]
        public class OdinPositionUpdateSettingsSchema
        {
            public float updateInterval;
            public List<OdinRoomProximityStatus> roomSettings;
        }
    }
    [Serializable]
    public class OdinRoomProximityStatus
    {
        public string roomName;
        public bool isActive = true;
        public float proximityRadius = 20.0f;
    }
}