using System;
using ODIN_Sample.Scripts.Runtime.Data;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    public class OdinAutoJoin : MonoBehaviour
    {
        [SerializeField] private StringVariable refRoomName;
        [SerializeField] private StringVariable refPlayerName;

        private void Awake()
        {
            Assert.IsNotNull(refRoomName);
            Assert.IsNotNull(refPlayerName);
        }

        private void Start()
        {
            string userId = refPlayerName.Value + DateTime.Now.Ticks;

            if (!PhotonNetwork.IsConnected)
                return;

            if (OdinHandler.Instance && !OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
            {
                Debug.Log($"Odin - joining room {refRoomName.Value}");
                OdinHandler.Instance.JoinRoom(refRoomName.Value, userId);

            }
        }

        private void OnDestroy()
        {
            if (OdinHandler.Instance && OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
            {
                Debug.Log($"Odin - leaving room {refRoomName.Value}");
                OdinHandler.Instance.LeaveRoom(refRoomName.Value);
            }
        }
    }
}