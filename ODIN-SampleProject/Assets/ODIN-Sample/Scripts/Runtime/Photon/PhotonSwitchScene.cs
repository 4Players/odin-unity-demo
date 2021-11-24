using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ODIN_Sample.Scripts.Runtime.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Room = OdinNative.Odin.Room.Room;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    public class PhotonSwitchScene : MonoBehaviour, IMatchmakingCallbacks
    {
        [SerializeField] private KeyCode loadKeyCode = KeyCode.L;
        [SerializeField] private StringVariable sceneToLoad;

        private bool _wasSceneLoadRequested = false;

        void Awake()
        {
            Assert.IsNotNull(sceneToLoad);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(loadKeyCode))
            {
                if (PhotonNetwork.IsConnectedAndReady)
                {
                    PhotonNetwork.LeaveRoom();
                }

                if (OdinHandler.Instance && OdinHandler.Instance.HasConnections)
                {
                    foreach (Room room in OdinHandler.Instance.Rooms)
                    {
                        OdinHandler.Instance.LeaveRoom(room.Config.Name);
                    }
                }

                _wasSceneLoadRequested = true;
            }
        }


        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinedRoom()
        {
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnLeftRoom()
        {
            if (_wasSceneLoadRequested)
                SceneManager.LoadScene(sceneToLoad.Value);
        }
    }
}