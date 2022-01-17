using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Concrete implementation of the Odin Multiplayer Adapter. Provides the ODIN scripts
    /// with information on whether the <see cref="photonView"/> represents a local user
    /// and returns the unique id of the photon view to connect an ODIN user's transmissions
    /// with the Photon-synchronized representation of the user.
    /// </summary>
    public class PhotonToOdinAdapter : AOdinMultiplayerAdapter
    {
        /// <summary>
        /// The photon view connected to the user.
        /// </summary>
        [SerializeField] private PhotonView photonView;

        private void Awake()
        {
            Assert.IsNotNull(photonView);
        }

        /// <summary>
        /// Returns the connected photon view's unique id.
        /// </summary>
        /// <returns>The photon view's id.</returns>
        public override string GetUniqueUserId()
        {
            return photonView.ViewID.ToString();
        }

        /// <summary>
        /// Determines whether the photon view is owned by the local user or a remote user.
        /// </summary>
        /// <returns>True, if the connected photon view is owned by the local user.</returns>
        public override bool IsLocalUser()
        {
            return photonView.IsMine;
        }

    }
}