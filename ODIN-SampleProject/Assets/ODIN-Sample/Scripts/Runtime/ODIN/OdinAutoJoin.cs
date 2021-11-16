using System;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin;
using OdinNative.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
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
            if (OdinHandler.Instance && !OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
            {
                Debug.Log($"ODIN - joining room {refRoomName.Value}");
                //
                // OdinUserData odinUserData = new OdinUserData();
                // odinUserData.name = refPlayerName.Value;
                OdinSampleUserData userData = new OdinSampleUserData(refPlayerName.Value);
                OdinHandler.Instance.JoinRoom(refRoomName.Value, userData);
            }
        }
    }
}