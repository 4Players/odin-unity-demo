using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    ///     Leave the photon room and all ODIN rooms on keypress and return to scene given by <see cref="sceneToLoad" />.
    /// </summary>
    public class PhotonLeaveRoom : MonoBehaviour, IMatchmakingCallbacks
    {
        /// <summary>
        /// Load the scene given by <see cref="sceneToLoad"/> when pressing this button.
        /// </summary>
        [SerializeField] private KeyCode loadKeyCode = KeyCode.L;
        /// <summary>
        /// Reference to the name of the Unity scene we should load.
        /// </summary>
        [SerializeField] private OdinStringVariable sceneToLoad;

        private bool _wasSceneLoadRequested;

        private void Awake()
        {
            Assert.IsNotNull(sceneToLoad);
        }

        private void Update()
        {
            if (Input.GetKeyDown(loadKeyCode) && !_wasSceneLoadRequested)
            {
                if (PhotonNetwork.IsConnectedAndReady) PhotonNetwork.LeaveRoom();

                if (OdinHandler.Instance && OdinHandler.Instance.HasConnections)
                    foreach (var room in OdinHandler.Instance.Rooms)
                        OdinHandler.Instance.LeaveRoom(room.Config.Name);

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