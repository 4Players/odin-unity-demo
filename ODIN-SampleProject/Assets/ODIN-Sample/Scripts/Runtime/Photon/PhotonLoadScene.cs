using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonLoadScene : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string sceneToLoad;

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadScene();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Entered room: {newPlayer.NickName}");
        }

        private void LoadScene()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            else
            {
                if (!string.IsNullOrEmpty(sceneToLoad))
                {
                    Debug.Log($"PhotonNetwork : Loading Level : {sceneToLoad}");
                    PhotonNetwork.LoadLevel(sceneToLoad);
                }
            }
        }
    }
}
