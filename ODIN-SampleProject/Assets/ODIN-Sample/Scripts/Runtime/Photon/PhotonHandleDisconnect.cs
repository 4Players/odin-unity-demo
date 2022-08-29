using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Handles the reconnect display and leaves the room, if an odin or photon disconnect was detected
    /// </summary>
    [RequireComponent(typeof(PhotonLeaveRoom))]
    public class PhotonHandleDisconnect : MonoBehaviour
    {
        /// <summary>
        /// The display to overlay over the screen, if we're detecting a disconnect
        /// </summary>
        [FormerlySerializedAs("reconnectDisplay")] [SerializeField] private RectTransform disconnectDisplay;
        
        /// <summary>
        /// The amount of connection checks we do, to ensure that we detect any connection issues
        /// </summary>
        [SerializeField] private int connectionChecks = 5;
        /// <summary>
        /// The delay to wait between each connection check
        /// </summary>
        [SerializeField] private float connectionCheckDelays = 0.1f;
        private PhotonLeaveRoom _leaveRoomBehaviour;


        private void Awake()
        {
            _leaveRoomBehaviour = GetComponent<PhotonLeaveRoom>();
            Assert.IsNotNull(_leaveRoomBehaviour);
            Assert.IsNotNull(disconnectDisplay);
        }

        private void OnEnable()
        {
            ShowDisconnectDisplay(false);
        }

        private IEnumerator OnApplicationPause(bool pauseStatus)
        {
            // After we return to the app
            if (!pauseStatus)
            {
                int numTests = connectionChecks;
                while (numTests > 0)
                {
                    // Check the connection a limited amount of time
                    numTests--;
                    yield return new WaitForSeconds(connectionCheckDelays);
                    bool bPhotonConnected = PhotonNetwork.IsConnected;
                    // Iterate through all odin rooms and check, if there are any connection retries. If there are any,
                    // we have lost the connection
                    bool bOdinConnected = !OdinHandler.Instance.Rooms.Any(r => r.ConnectionRetry > 0);

                    Debug.Log($"Photon Connected: {bPhotonConnected}, Odin Connected: {bOdinConnected}");
                    if (!bPhotonConnected || !bOdinConnected)
                    {
                        // Leave the room and show the disconnect display
                        _leaveRoomBehaviour.LeaveRoom();
                        ShowDisconnectDisplay(true);
                    }
                    else
                    {
                        ShowDisconnectDisplay(false);
                    }
                }
            }
        }

        private void ShowDisconnectDisplay(bool newActive)
        {
            disconnectDisplay.gameObject.SetActive(newActive);
        }
    }
}