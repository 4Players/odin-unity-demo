using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    [RequireComponent(typeof(Toggle))]
    public class OdinPushToTalkSettingsController : MonoBehaviour
    {
        [SerializeField] private OdinStringVariable targetRoom;
        [SerializeField] private OdinPushToTalkSettings settings;

        private Toggle _toggleView;

        private void Awake()
        {
            Assert.IsNotNull(settings);
            Assert.IsNotNull(targetRoom);
            _toggleView = GetComponent<Toggle>();
            Assert.IsNotNull(_toggleView);
        }

        private void OnEnable()
        {
            settings.Load();
            OdinPushToTalkSettings.OdinPushToTalkData odinPushToTalkData = settings.GetData(targetRoom);
            if (null != odinPushToTalkData) _toggleView.isOn = !odinPushToTalkData.pushToTalkIsActive;
            else
                Debug.LogError($"Could not find Push To Talk Data in referenced settings for ODIN room {targetRoom}");

            _toggleView.onValueChanged.AddListener(SetPushToTalkActive);
        }

        private void OnDisable()
        {
            _toggleView.onValueChanged.RemoveListener(SetPushToTalkActive);
        }

        public void SetPushToTalkActive(bool newActive)
        {
            settings.SetPushToTalkActive(targetRoom.Value, !newActive);
            settings.Save();
        }
    }
}