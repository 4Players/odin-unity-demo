using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{

    /// <summary>
    /// Activates / deactivates push to talk based on the current toggle value. Requires a Toggle script on the same
    /// GameObject.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class OdinPushToTalkSettingsController : MonoBehaviour
    {
        /// <summary>
        /// The room for which the push to talk settings should be set
        /// </summary>
        [SerializeField] private OdinStringVariable targetRoom;
        /// <summary>
        /// A reference to the settings object for updating the settings on Toggle changes.
        /// </summary>
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
            if (null != odinPushToTalkData) _toggleView.isOn = odinPushToTalkData.pushToTalkIsActive;
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
            settings.SetPushToTalkActive(targetRoom.Value, newActive);
            settings.Save();
        }
    }
}