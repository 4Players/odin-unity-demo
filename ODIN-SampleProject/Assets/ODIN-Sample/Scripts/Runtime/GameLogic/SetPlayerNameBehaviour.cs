using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Behaviour for reading and storing the player name from a Unity <c>InputField</c>. Uses <c>PlayerPrefs</c> to
    /// store the name on disk, and stores it in the <see cref="OdinStringVariable"/> object <see cref="playerName"/> for
    /// access in other scripts.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class SetPlayerNameBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Reference to the <see cref="OdinStringVariable"/> which should contain the player name.
        /// </summary>
        [SerializeField] private OdinStringVariable playerName;

        private InputField _playerNameInput = null;

        /// <summary>
        /// The key used for storing the player name in <c>PlayerPrefs</c>.
        /// </summary>
        private static string PlayerNameKey => "PlayerName";

        private void Awake()
        {
            _playerNameInput = GetComponent<InputField>();
            Assert.IsNotNull(playerName);
            
            string savedName = PlayerPrefs.GetString(PlayerNameKey, "Player");
            _playerNameInput.text = savedName;
        }

        private void OnEnable()
        {
            _playerNameInput.onEndEdit.AddListener(SetPlayerName);
        }

        private void OnDisable()
        {
            _playerNameInput.onEndEdit.RemoveListener(SetPlayerName);
        }

        /// <summary>
        /// Reads the player name from the connected <c>InputField</c> and stores it.
        /// </summary>
        public void UpdatePlayerName()
        {
            SetPlayerName(_playerNameInput.text);
        }

        /// <summary>
        /// Stores the given player name <see cref="newName"/>.
        /// </summary>
        /// <param name="newName">The new player name.</param>
        public void SetPlayerName(string newName)
        {
            playerName.Value = newName;
            PlayerPrefs.SetString(PlayerNameKey, newName);
            PlayerPrefs.Save();
        }
    }
}