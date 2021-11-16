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
            if (null != playerPrefab && PhotonNetwork.IsConnectedAndReady)
            {
                Vector3 adjustedSpawnLocation = spawnLocation;
                Collider playerCollider = playerPrefab.GetComponent<Collider>();
                if (playerCollider)
                {
                    Bounds playerBounds = playerCollider.bounds;
                    bool hitSomething = Physics.BoxCast(spawnLocation, playerBounds.extents, Vector3.down);
                    if (hitSomething)
                    {
                        adjustedSpawnLocation.y = adjustedSpawnLocation.y + 2.0f;
                    }
                }
                _instantiatedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, adjustedSpawnLocation, Quaternion.identity);
                
            }
        }
    }
}
