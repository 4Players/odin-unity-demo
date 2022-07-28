using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    ///     Behavior for toggling the active status of the Game Object referenced as <see cref="toggleTargets" />. Requires an
    ///     attached Collider with <c>isTrigger</c> set to <c>true</c>.
    ///     E.g. used to activate or deactivate the Text hovering above radios or activating the
    ///     <see cref="ToggleRadioBehaviour" />
    ///     script to enable or disable playback of the radio audio.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ToggleActiveStatusOnTrigger : MonoBehaviour
    {
        /// <summary>
        ///     Target Game Object to toggle.
        /// </summary>
        [SerializeField] private GameObject[] toggleTargets;

        private void Awake()
        {
            Assert.IsNotNull(toggleTargets);
        }

        private void Start()
        {
            SetToggleState(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            AOdinMultiplayerAdapter otherView = other.GetComponent<AOdinMultiplayerAdapter>();
            if (otherView && otherView.IsLocalUser()) SetToggleState(true);
        }

        private void OnTriggerExit(Collider other)
        {
            AOdinMultiplayerAdapter otherView = other.GetComponent<AOdinMultiplayerAdapter>();
            if (otherView && otherView.IsLocalUser()) SetToggleState(false);
        }

        private void SetToggleState(bool newActive)
        {
            foreach (GameObject target in toggleTargets)
            {
                target.SetActive(newActive);
            }
        }
    }
}