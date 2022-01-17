using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    /// Spawns the player object on start, using the Photon Spawn method.
    /// </summary>
    public class PhotonPlayerSpawner : MonoBehaviour
    {
        /// <summary>
        /// Prefab of the player object. Has to be located in a resources folder! (Photon requirement)
        /// </summary>
        [SerializeField] private GameObject playerPrefab;
        /// <summary>
        /// The location at which we should spawn the player.
        /// </summary>
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
                // Try to adjust spawn location, if we find a collision. Doesn't work very well when using
                // Unity's CharacterController script as collider.
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
                // instantiate player using the Photon synchronised instantiation method.
                _instantiatedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, adjustedSpawnLocation, Quaternion.identity);
            }
        }
    }
}
