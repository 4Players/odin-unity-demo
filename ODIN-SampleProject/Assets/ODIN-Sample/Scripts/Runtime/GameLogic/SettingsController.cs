using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private OdinStringVariable toggleSettingsButton;
        [SerializeField] private GameObject settingsRoot;

        private void Awake()
        {
            Assert.IsNotNull(toggleSettingsButton);
            Assert.IsNotNull(settingsRoot);
        }

        private void Start()
        {
            settingsRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetButtonDown(toggleSettingsButton))
                settingsRoot.gameObject.SetActive(!settingsRoot.activeSelf);
        }
    }
}