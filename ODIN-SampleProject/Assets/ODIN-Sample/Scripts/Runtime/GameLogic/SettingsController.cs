using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Controller script for handling the settings menu.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        /// <summary>
        /// Button for toggling the active status of the settings menu.
        /// </summary>
        [SerializeField] private InputActionReference toggleSettingsButton;
        /// <summary>
        /// The root of the settings menu.
        /// </summary>
        [SerializeField] private GameObject settingsRoot;

        private void Awake()
        {
            Assert.IsNotNull(toggleSettingsButton);
            toggleSettingsButton.action.Enable();
            Assert.IsNotNull(settingsRoot);
        }

        private void Start()
        {
            settingsRoot.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            toggleSettingsButton.action.performed += ActionOnToggleSettings;
        }

        private void OnDisable()
        {
            toggleSettingsButton.action.performed -= ActionOnToggleSettings;
        }

        private void ActionOnToggleSettings(InputAction.CallbackContext context)
        {
            settingsRoot.gameObject.SetActive(!settingsRoot.activeSelf);
        }
    }
}