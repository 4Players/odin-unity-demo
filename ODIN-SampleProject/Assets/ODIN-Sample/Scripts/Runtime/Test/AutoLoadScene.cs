using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ODIN_Sample.Scripts.Runtime.Test
{
    public class AutoLoadScene : MonoBehaviour
    {

        [SerializeField] private string sceneName;

    
        void Start()
        {
            if(!PhotonNetwork.IsConnected)
                SceneManager.LoadScene(sceneName);
        }

    
    }
}
