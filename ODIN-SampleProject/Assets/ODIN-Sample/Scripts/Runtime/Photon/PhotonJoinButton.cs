using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
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

        [SerializeField] private TMP_Text buttonText;
        

        private void Awake()
        {
            Assert.IsNotNull(joinButton);
            Assert.IsNotNull(buttonText);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (!PhotonNetwork.IsConnected)
            {
                buttonText.text = "Connecting...";
                joinButton.interactable = false;
            }
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        
        public void OnConnectedToMaster()
        {
            joinButton.interactable = true;
            buttonText.text = "Join";
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
