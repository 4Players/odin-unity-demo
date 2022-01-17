using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    /// Utility Behaviour: Leave all ODIN rooms on keypress.
    /// </summary>
    public class OdinLeaveRooms : MonoBehaviour
    {
        /// <summary>
        /// Leave all ODIN rooms when pressing this Unity button.
        /// </summary>
        [SerializeField] private OdinStringVariable leaveRoomsButton;

        private void Awake()
        {
            Assert.IsNotNull(leaveRoomsButton);
        }

        private void Update()
        {
            if (Input.GetButtonDown(leaveRoomsButton))
            {
                if (OdinHandler.Instance && OdinHandler.Instance.HasConnections)
                    foreach (var room in OdinHandler.Instance.Rooms)
                        OdinHandler.Instance.LeaveRoom(room.Config.Name);
            }
        }
    }
}