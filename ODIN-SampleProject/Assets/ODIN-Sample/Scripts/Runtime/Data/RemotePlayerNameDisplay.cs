using System;
using ODIN_Sample.Scripts.Runtime.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Data
{
    public class RemotePlayerNameDisplay : MonoBehaviourPunCallbacks
    {
        [SerializeField] private StringVariable playerName;
        [SerializeField] private TextMesh nameDisplay;

        [SerializeField] private int maxDisplayCharacters = 8;

        private string _remoteName;

        private void Awake()
        {
            Assert.IsNotNull(playerName);
            Assert.IsNotNull(nameDisplay);
            nameDisplay.text = "";
        }

        private void Start()
        {
            string displayedName = "";
            if (photonView.IsMine)
            {
                displayedName = TruncateName(playerName.Value);
                nameDisplay.text = displayedName;

                photonView.RPC("UpdateName", RpcTarget.OthersBuffered, playerName.Value);
            }
        }

        [PunRPC]
        private void UpdateName(string remotePlayerName)
        {
            if (!photonView.IsMine)
            {
                _remoteName = remotePlayerName;
                nameDisplay.text = TruncateName(remotePlayerName);
            }
        }

        private string TruncateName(string displayedName)
        {
            if (displayedName.Length > maxDisplayCharacters)
            {
                displayedName = displayedName.Substring(0, maxDisplayCharacters) + "...";
            }

            return displayedName;
        }
    }
}