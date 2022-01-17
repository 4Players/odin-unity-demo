using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    ///     Utility Behaviour: Leave the photon room on keypress and return to scene given by <see cref="sceneToLoad" />.
    /// </summary>
    public class PhotonLeaveRoom : MonoBehaviour, IMatchmakingCallbacks
    {
        /// <summary>
        /// Leave the Photon Room and load scene <see cref="sceneToLoad"/> when pressing this Unity button.
        /// </summary>
        [SerializeField] private OdinStringVariable leaveRoomsButton;
        /// <summary>
        /// Reference to the name of the Unity scene we should load.
        /// </summary>
        [SerializeField] private OdinStringVariable sceneToLoad;

        private bool _wasSceneLoadRequested;

        private void Awake()
        {
            Assert.IsNotNull(sceneToLoad);
            Assert.IsNotNull(leaveRoomsButton);
        }

        private void Update()
        {
            if (Input.GetButtonDown(leaveRoomsButton) && !_wasSceneLoadRequested)
            {
                // Leave the photon room
                if (PhotonNetwork.IsConnectedAndReady) PhotonNetwork.LeaveRoom();

                _wasSceneLoadRequested = true;
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        
        /// <summary>
        /// Load lobby scene after successfully leaving the photon room.
        /// </summary>
        public void OnLeftRoom()
        {
            if (_wasSceneLoadRequested)
                SceneManager.LoadScene(sceneToLoad.Value);
        }

        #region Unused Photon Callbacks

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

        

        #endregion
    }
}