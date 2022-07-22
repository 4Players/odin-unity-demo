using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleSettingsButton;
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