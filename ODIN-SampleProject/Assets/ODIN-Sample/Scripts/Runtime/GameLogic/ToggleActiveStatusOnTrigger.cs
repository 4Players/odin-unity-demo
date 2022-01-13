using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ToggleActiveStatusOnTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject toggleTarget;

        private void Awake()
        {
            Assert.IsNotNull(toggleTarget);
        }

        private void Start()
        {
            toggleTarget.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            PhotonView otherView = other.GetComponent<PhotonView>();
            if (otherView && otherView.IsMine)
            {
                SetToggleState(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PhotonView otherView = other.GetComponent<PhotonView>();
            if (otherView && otherView.IsMine)
            {
                SetToggleState(false);
            }
        }

        private void SetToggleState(bool newActive)
        {
            toggleTarget.SetActive(newActive);
        }
    }
}
