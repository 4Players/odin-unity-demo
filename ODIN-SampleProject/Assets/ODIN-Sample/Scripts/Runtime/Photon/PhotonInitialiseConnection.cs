using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    ///     Initialises Photon Connection. Can also automatically join the Photon room given by
    ///     <see cref="roomName" />.
    /// </summary>
    public class PhotonInitialiseConnection : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// If true, we'll automatically join the room given by <see cref="roomName"/>.
        /// </summary>
        [SerializeField] private bool autoJoin;

        /// <summary>
        /// Room to join, if <see cref="autoJoin"/> is set to true or if <see cref="JoinPhotonRoom"/> is called on
        /// this script.
        /// </summary>
        [SerializeField] private OdinStringVariable roomName;

        private void Awake()
        {
            Assert.IsNotNull(roomName);
        }

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.IsConnected && autoJoin)
                JoinPhotonRoom();
            else if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            // Debug.Log("Connected to Master");
            if (autoJoin)
                JoinPhotonRoom();
        }

        /// <summary>
        /// Connect to the photon room given by <see cref="roomName"/>.
        /// </summary>
        public void JoinPhotonRoom()
        {
            PhotonNetwork.JoinOrCreateRoom(roomName.Value, new RoomOptions(), TypedLobby.Default);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Could not join room {roomName.Value} given by reference, joining room with null roomName");
            OnFailedToJoinAnyRoom();
        }

        private void OnFailedToJoinAnyRoom()
        {
            Debug.Log($"Failed to join Photon room {roomName.Value}, creating new room.");
            PhotonNetwork.CreateRoom(null, new RoomOptions());
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined Photon room {roomName.Value}.");
        }
    }
}