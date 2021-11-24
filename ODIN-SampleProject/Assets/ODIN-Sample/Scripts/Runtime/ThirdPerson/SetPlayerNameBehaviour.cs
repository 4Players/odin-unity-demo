using System;
using ODIN_Sample.Scripts.Runtime.Data;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    [RequireComponent(typeof(InputField))]
    public class SetPlayerNameBehaviour : MonoBehaviour
    {
        [SerializeField] private StringVariable playerName;

        private InputField _playerNameInput = null;

        private static string PlayerNameKey => "PlayerName";

        private void Awake()
        {
            _playerNameInput = GetComponent<InputField>();
            _playerNameInput.onEndEdit.AddListener(SetPlayerName);
            Assert.IsNotNull(playerName);
        }

        private void Start()
        {
            string savedName = PlayerPrefs.GetString(PlayerNameKey, "Player");
            _playerNameInput.text = savedName;
        }

        public void UpdatePlayerName()
        {
            SetPlayerName(_playerNameInput.text);
        }

        public void SetPlayerName(string newName)
        {
            playerName.Value = newName;
            PlayerPrefs.SetString(PlayerNameKey, newName);
            PlayerPrefs.Save();
        }
    }
}