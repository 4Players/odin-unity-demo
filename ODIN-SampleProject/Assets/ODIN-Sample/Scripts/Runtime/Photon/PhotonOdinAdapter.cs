using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonOdinAdapter : AOdinMultiplayerAdapter
    {
        [SerializeField] private PhotonView photonView;

        private void Awake()
        {
            Assert.IsNotNull(photonView);
        }

        public override string GetUniqueUserId()
        {
            return photonView.ViewID.ToString();
        }

        public override bool IsLocalUser()
        {
            return photonView.IsMine;
        }

        public override void OnUpdateUniqueUserId(Room newRoom)
        {
            OdinSampleUserData userData = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
            userData.playerId = photonView.ViewID.ToString();
            newRoom.UpdateUserData(userData.ToUserData());
        }
    }
}