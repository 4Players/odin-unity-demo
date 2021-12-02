using System;
using ODIN_Sample.Scripts.Runtime.ThirdPerson;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonRadioSynchronization : MonoBehaviourPunCallbacks
    {
        [SerializeField] private AudioSource radioAudioSource;
        [SerializeField] private ToggleRadioBehaviour toggleBehaviour;
        

        private void Awake()
        {
            Assert.IsNotNull(radioAudioSource);
            Assert.IsNotNull(toggleBehaviour);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (photonView.IsMine)
            {
                SendRadioSynchronizationUpdate(radioAudioSource.enabled);
            }
        }

        public void SendRadioSynchronizationUpdate(bool newSourceActive)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                photonView.RPC("UpdateRadioAudioSourceState", RpcTarget.Others, radioAudioSource.gameObject.activeSelf);
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