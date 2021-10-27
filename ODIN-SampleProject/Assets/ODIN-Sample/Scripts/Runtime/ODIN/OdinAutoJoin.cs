using System;
using ODIN_Sample.Scripts.Runtime.Data;
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
            string userId = refPlayerName.Value + SystemInfo.deviceUniqueIdentifier + DateTime.Now.Ticks;
            OdinHandler.Instance.JoinRoom(refRoomName.Value, userId);
        }
    }
}