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
        /// <summary>
        /// Reference to the text display.
        /// </summary>
        [SerializeField] private TextMeshProUGUI text;

        private (string, ulong, int) _key;

        private void Awake()
        {
            Assert.IsNotNull(text);
        }

        /// <summary>
        /// Whether the text element is showing user data uniquely identified by the given
        /// key (room name, peer id, media id).
        /// </summary>
        /// <param name="key">The (room name, peer id, media id) key.</param>
        /// <returns>Whether the ui element is displaying data identified by the given key.</returns>
        public bool IsShowing((string, ulong, int) key)
        {
            return IsActive() && _key == key;
        }

        /// <summary>
        /// Is the ui element active?
        /// </summary>
        /// <returns>True if active, false otherwise.</returns>
        public bool IsActive()
        {
            return gameObject.activeSelf;
        }

        /// <summary>
        /// Display the given <see cref="displayData"/> and store the (room name, peer id, media id) key for later identification.
        /// </summary>
        /// <param name="key">The (room name, peer id, media id) key uniquely identifying the peer and media connected to the <see cref="displayData"/>. </param>
        /// <param name="displayData">The data to display.</param>
        public void Show((string, ulong, int) key, OdinSampleUserData displayData)
        {
            _key = key;
            text.text = displayData.name;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the ui element.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _key = default;
        }
    }
}