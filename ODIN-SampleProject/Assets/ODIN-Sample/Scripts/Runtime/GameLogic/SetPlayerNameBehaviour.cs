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
        [SerializeField] private OdinStringVariable playerName;

        private InputField _playerNameInput = null;

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