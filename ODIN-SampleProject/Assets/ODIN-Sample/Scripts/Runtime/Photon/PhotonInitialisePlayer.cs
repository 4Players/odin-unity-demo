using Photon.Pun;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Initialises the player's state: if we belong to a remote player, destroy the local-only behaviours, otherwise
    /// enable the local-only behaviours.
    /// </summary>
    public class PhotonInitialisePlayer : MonoBehaviour, IPunInstantiateMagicCallback
    {
        /// <summary>
        /// Reference to the root object, which contains all the local-only behaviour scripts.
        /// </summary>
        [SerializeField] private GameObject localPlayerOnly;
        
        /// <summary>
        /// Initialises the player's state: if we belong to a remote player, destroy the local-only behaviours, otherwise
        /// enable the local-only behaviours.
        /// </summary>
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (!info.photonView.IsMine)
            {
                Destroy(localPlayerOnly);
            }
            else
            {
                localPlayerOnly.SetActive(true);
            }

            if(null != info.Sender)
                info.Sender.TagObject = this.gameObject;
        }
    }
}
