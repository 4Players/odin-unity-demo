using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Behavior for toggling the active status of the Game Object referenced as <see cref="toggleTarget"/>. Requires an
    /// attached Collider with <c>isTrigger</c> set to <c>true</c>.
    /// E.g. used to activate or deactivate the Text hovering above radios or activating the <see cref="ToggleRadioBehaviour"/>
    /// script to enable or disable playback of the radio audio.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ToggleActiveStatusOnTrigger : MonoBehaviour
    {
        /// <summary>
        /// Target Game Object to toggle.
        /// </summary>
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
