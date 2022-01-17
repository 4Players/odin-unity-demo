using System;
using ODIN_Sample.Scripts.Runtime.GameLogic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Synchronises the radio on/off state over the PhotonNetwork.
    /// </summary>
    public class PhotonRadioSynchronization : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// The game logic behaviour for toggling the radio.
        /// </summary>
        [SerializeField] private ToggleRadioBehaviour toggleBehaviour;
        
        private void Awake()
        {
            Assert.IsNotNull(toggleBehaviour);
        }

        /// <summary>
        /// Inform joining players about the current state of the radio.
        /// </summary>
        /// <param name="newPlayer"></param>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (photonView.IsMine)
            {
                SendRadioSynchronizationUpdate(toggleBehaviour.IsRadioActive());
            }
        }

        /// <summary>
        /// Send the radio state update using a Photon RPC.
        /// </summary>
        /// <param name="newSourceActive"></param>
        public void SendRadioSynchronizationUpdate(bool newSourceActive)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                photonView.RPC("UpdateRadioAudioSourceState", RpcTarget.Others, toggleBehaviour.IsRadioActive());
            }
        }

        [PunRPC]
        private void UpdateRadioAudioSourceState(bool newActive)
        {
            toggleBehaviour.SetRadio(newActive);
            // Debug.Log("Received Update Radio Audio Source State");
        }
    }
}