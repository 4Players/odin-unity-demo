using System;
using ODIN_Sample.Scripts.Runtime.GameLogic;
using ODIN_Sample.Scripts.Runtime.ThirdPerson;
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
        /// The 
        /// </summary>
        [SerializeField] private ToggleRadioBehaviour toggleBehaviour;
        

        private void Awake()
        {
            Assert.IsNotNull(toggleBehaviour);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (photonView.IsMine)
            {
                SendRadioSynchronizationUpdate(toggleBehaviour.IsRadioActive());
            }
        }

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