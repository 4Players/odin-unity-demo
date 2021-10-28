using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    public class PhotonPlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 spawnLocation;

        private GameObject _instantiatedPlayer = null;

        private void Awake()
        {
            Assert.IsNotNull(playerPrefab);
        }

        private void Start()
        {
            InstantiatePlayer();
        }

        private void InstantiatePlayer()
        {
            if (null != playerPrefab)
            {
                if (PhotonNetwork.IsConnectedAndReady)
                {
                    _instantiatedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation, Quaternion.identity);
                }
            }
        }
    }
}
