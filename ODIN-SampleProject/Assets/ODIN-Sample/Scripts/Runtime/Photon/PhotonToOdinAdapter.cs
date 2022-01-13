using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonToOdinAdapter : AOdinMultiplayerAdapter
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

        protected override void OnUpdateUniqueUserId(Room newRoom)
        {
            OdinSampleUserData userData = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
            userData.uniqueUserId = photonView.ViewID.ToString();
            newRoom.UpdateUserData(userData.ToUserData());
        }
    }
}