using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Sets the button interactable state of the <see cref="joinButton"/>. The button should only be interactable, if
    /// we're connected to the PhotonNetwork.
    /// </summary>
    public class PhotonJoinButton : MonoBehaviour, IConnectionCallbacks
    {
        /// <summary>
        /// Reference to a button, that will only be interactable, if we're connected to the PhotonNetwork.
        /// </summary>
        [SerializeField] private Button joinButton;

        private void Awake()
        {
            Assert.IsNotNull(joinButton);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (!PhotonNetwork.IsConnected)
                joinButton.interactable = false;
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        
        public void OnConnectedToMaster()
        {
            joinButton.interactable = true;
        }

        #region Unused Photon Callbacks
        public void OnConnected()
        {
        }
        
        public void OnDisconnected(DisconnectCause cause)
        {
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }
        
        #endregion
    }
}
