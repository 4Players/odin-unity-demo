using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonAutoLogin : MonoBehaviourPunCallbacks
    {
        public void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            OnFailedToJoinAnyRoom();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            OnFailedToJoinAnyRoom();
        }

        private void OnFailedToJoinAnyRoom()
        {
            Debug.Log("Failed to join room, creating new room.");
            PhotonNetwork.CreateRoom(null, new RoomOptions());

        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined room.");
        }
    }
}
