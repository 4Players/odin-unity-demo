using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    /// UI element used to display <see cref="OdinSampleUserData"/> like the player's name.
    /// </summary>
    public class OdinTransmitterUiElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private (string, ulong, int) _key;

        private void Awake()
        {
            Assert.IsNotNull(text);
        }

        public bool IsShowing((string, ulong, int) key)
        {
            return IsActive() && _key == key;
        }

        public bool IsActive()
        {
            return gameObject.activeSelf;
        }

        public void Show((string, ulong, int) key, OdinSampleUserData displayData)
        {
            _key = key;
            text.text = displayData.name;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _key = default;
        }
    }
}