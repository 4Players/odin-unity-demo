using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        ///     Reference to the name of the Unity scene we should load.
        /// </summary>
        [SerializeField] private OdinStringVariable sceneToLoad;

        private bool _wasSceneLoadRequested;

        private void Awake()
        {
            Assert.IsNotNull(sceneToLoad);
        }

        private void OnEnable()
        {
            _wasSceneLoadRequested = false;
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private IEnumerator OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                int numTests = 5;
                while (numTests > 0)
                {
                    numTests--;
                    yield return new WaitForSeconds(0.1f);
                    bool bPhotonConnected = PhotonNetwork.IsConnected;
                    bool bOdinConnected = !OdinHandler.Instance.Rooms.Any(r => r.ConnectionRetry > 0);
                
                    Debug.Log($"Photon Connected: {bPhotonConnected}, Odin Connected: {bOdinConnected}");
                    if(!bPhotonConnected || !bOdinConnected)
                        LeaveRoom();
                }
            }
        }

        /// <summary>
        ///     Load lobby scene after successfully leaving the photon room.
        /// </summary>
        public void OnLeftRoom()
        {
            Debug.Log("On Left room");
            if (_wasSceneLoadRequested)
            {
                Debug.Log("Scene Load was requested, leaving odin rooms");
                StartCoroutine(LeaveOdinRooms());
            }
        }

        public void LeaveRoom()
        {
            Debug.Log("Leave Room requested");
            if (!_wasSceneLoadRequested)
            {
                Debug.Log("No leave request active, starting Leave Photon Room");
                LeavePhotonRoom();
            }
        }

        public void LeavePhotonRoom()
        {
            _wasSceneLoadRequested = true;
            // Leave the photon room
            if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Leaving)
            {
                Debug.Log("Photon is still connected, trying to leave photon room");
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                Debug.Log("Photon Network is not connected, directly starting Leave Odin Rooms");
                StartCoroutine(LeaveOdinRooms());

            }
        }

        private IEnumerator LeaveOdinRooms()
        {
            Debug.Log("Entered Leave Odin Rooms");

            if (OdinHandler.Instance && OdinHandler.Instance.HasConnections)
            {
                foreach (var room in OdinHandler.Instance.Rooms)
                {
                    OdinHandler.Instance.LeaveRoom(room.Config.Name);
                    yield return null;
                }
            }

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