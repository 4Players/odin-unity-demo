using Photon.Pun;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonInitialisePlayer : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private GameObject localPlayerOnly;
        
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
