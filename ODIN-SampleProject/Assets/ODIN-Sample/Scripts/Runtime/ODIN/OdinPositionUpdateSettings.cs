using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [CreateAssetMenu(fileName = "OdinPositionUpdateSettings", menuName = "Odin-Demo/PositionUpdateSettings", order = 0)]
    public class OdinPositionUpdateSettings : ScriptableObject
    {
        public float updateInterval = 0.5f;
        public List<OdinRoomProximityStatus> roomSettings;

        public OdinRoomProximityStatus GetRoomProximityStatus(string roomName)
        {
            foreach (OdinRoomProximityStatus roomSetting in roomSettings)
            {
                if (roomName == roomSetting.roomName)
                    return roomSetting;
            }
            return null;
        }
    }
    [Serializable]
    public class OdinRoomProximityStatus
    {
        public OdinStringVariable roomName;
        public bool isActive = true;
        public float proximityRadius = 20.0f;
    }
}