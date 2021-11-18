using System;
using ODIN_Sample.Scripts.Runtime.Data;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace ODIN_Sample.Scripts.Runtime.Test
{
    public class AutoLoadScene : MonoBehaviour
    {
        [SerializeField] private StringVariable sceneName;

        private void Awake()
        {
            Assert.IsNotNull(sceneName);
        }

        void Start()
        {
            if(!PhotonNetwork.IsConnected)
                SceneManager.LoadScene(sceneName.Value);
        }
    }
}
