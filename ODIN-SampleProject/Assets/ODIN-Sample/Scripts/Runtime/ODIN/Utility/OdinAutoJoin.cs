﻿using System.Collections;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    /// Use this script to automatically join all ODIN rooms given by <see cref="refRoomNames"/> on scene start. The
    /// player name given by <see cref="refPlayerName"/> will be used.
    /// </summary>
    public class OdinAutoJoin : MonoBehaviour
    {
        /// <summary>
        /// ODIN room names, which should be automatically joined.
        /// </summary>
        [SerializeField] private OdinStringVariable[] refRoomNames;
        /// <summary>
        /// Reference to the current player name.
        /// </summary>
        [SerializeField] private OdinStringVariable refPlayerName;

        private void Awake()
        {
            Assert.IsTrue(refRoomNames.Length > 0);
            Assert.IsNotNull(refPlayerName);
        }

        IEnumerator Start()
        {
            while (!OdinHandler.Instance)
                yield return null;

            Debug.Log("Odin Handler is now available.");

            // yield return null;
            yield return null;
            foreach (OdinStringVariable refRoomName in refRoomNames)
            {
                if (OdinHandler.Instance && !OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
                {
                    Debug.Log($"ODIN Auto Join - joining room {refRoomName.Value}");
                
                    OdinSampleUserData userData = new OdinSampleUserData(refPlayerName.Value);
                    OdinHandler.Instance.JoinRoom(refRoomName.Value, userData);
                    yield return null;
                }
            }
        }

    }
}