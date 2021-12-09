using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class FirstPersonMeshVisibility : MonoBehaviour
    {
        [SerializeField] private GameObject playerMeshRoot;
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

        static void SetLayerOnAll(GameObject obj, int layer) {
            foreach (Transform trans in obj.GetComponentsInChildren<Transform>(true)) {
                trans.gameObject.layer = layer;
            }
        }
    }
}