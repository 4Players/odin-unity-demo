using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Hides the player mesh from being visible in first person mode, by adjusting the layers of all child objects of
    /// the GameObject <see cref="playerMeshRoot"/>. The First Person Camera needs to ignore the layer defined in
    /// <see cref="firstPersonLayer"/> for this script to work.
    /// </summary>
    public class FirstPersonMeshVisibility : MonoBehaviour
    {
        
        /// <summary>
        /// The root of the player mesh - this object and all children will be hidden from the first person camera view.
        /// </summary>
        [SerializeField] private GameObject playerMeshRoot;
        /// <summary>
        /// The layer, which is ignored by the first person camera. Please make sure, that the layer mask only contains
        /// a single layer.
        /// </summary>
        [SerializeField] private LayerMask firstPersonLayer;

        private int _originalMask;
        private void Awake()
        {
            Assert.IsNotNull(playerMeshRoot);
        }

        private void OnEnable()
        {
            _originalMask = playerMeshRoot.layer;
            int layer = (int) Mathf.Log(firstPersonLayer.value, 2);
            SetLayerOnAll(playerMeshRoot.gameObject, layer);
        }

        private void OnDisable()
        {
            SetLayerOnAll(playerMeshRoot.gameObject, _originalMask);
        }

        private static void SetLayerOnAll(GameObject obj, int layer) {
            foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true)) {
                trans.gameObject.layer = layer;
            }
        }
    }
}