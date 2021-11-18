using System;
using ODIN_Sample.Scripts.Runtime.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonJoinRoom : MonoBehaviourPunCallbacks
    {
        [SerializeField] private bool autoJoin = false;
        
        [SerializeField] private StringVariable roomName;

        private void Awake()
        {
            Assert.IsNotNull(roomName);
        }

        public void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.IsConnected && autoJoin)
            {
                JoinPhotonRoom();
            }
            else if(!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            if(autoJoin)
                JoinPhotonRoom();
        }
        
        public void JoinPhotonRoom()
        {
            PhotonNetwork.JoinOrCreateRoom(roomName.Value, new RoomOptions(), TypedLobby.Default);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Could not join room {roomName.Value}, joining room with null roomName");
            OnFailedToJoinAnyRoom();
        }

        private void OnFailedToJoinAnyRoom()
        {
            Debug.Log($"Failed to join Photon room {roomName.Value}, creating new room.");
            PhotonNetwork.CreateRoom(null, new RoomOptions());
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined Photon {roomName.Value} room.");
        }
    }
}
