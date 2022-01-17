using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Automatically loads the scene given by <see cref="sceneToLoad"/> using the Photon Load method, which allows
    /// us to load the Photon-Synchronised version of the scene. 
    /// </summary>
    public class PhotonLoadGameSceneOnJoin : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// The unity scene to load on join.
        /// </summary>
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
            Debug.Log($"Entered Photon room: {newPlayer.NickName}");
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