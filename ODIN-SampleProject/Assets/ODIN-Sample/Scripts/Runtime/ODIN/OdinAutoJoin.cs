using System;
using System.Collections;
using System.Threading.Tasks;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin;
using OdinNative.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinAutoJoin : MonoBehaviour
    {
        [SerializeField] private StringVariable[] refRoomNames;
        [SerializeField] private StringVariable refPlayerName;

        private void Awake()
        {
            Assert.IsTrue(refRoomNames.Length > 0);
            Assert.IsNotNull(refPlayerName);
        }

        IEnumerator Start()
        {
            foreach (StringVariable refRoomName in refRoomNames)
            {
                if (OdinHandler.Instance && !OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
                {
                    Debug.Log($"ODIN - joining room {refRoomName.Value}");
                
                    OdinSampleUserData userData = new OdinSampleUserData(refPlayerName.Value);
                    OdinHandler.Instance.JoinRoom(refRoomName.Value, userData);
                    
                    // await Task.Delay(TimeSpan.FromSeconds(1));
                    yield return null;
                }
            }
        }
    }
}