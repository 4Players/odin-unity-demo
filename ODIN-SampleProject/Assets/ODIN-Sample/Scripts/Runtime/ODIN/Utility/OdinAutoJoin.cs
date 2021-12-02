using System.Collections;
using ODIN_Sample.Scripts.Runtime.Data;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    public class OdinAutoJoin : MonoBehaviour
    {
        [SerializeField] private OdinStringVariable[] refRoomNames;
        [SerializeField] private OdinStringVariable refPlayerName;

        private void Awake()
        {
            Assert.IsTrue(refRoomNames.Length > 0);
            Assert.IsNotNull(refPlayerName);
        }

        IEnumerator Start()
        {
            // Bug: We have to disperse the Join Room Calls over multiple frames. If called in the same frame, build will crash.
            foreach (OdinStringVariable refRoomName in refRoomNames)
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