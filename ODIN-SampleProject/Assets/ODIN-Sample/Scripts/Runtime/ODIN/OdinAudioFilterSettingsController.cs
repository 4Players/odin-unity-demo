using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OdinNative.Core.Imports;
using OdinNative.Odin;
using OdinNative.Odin.Room;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    ///     Data structure for setting a boolean property of the OdinHandler Config from the referenced toggle.
    /// </summary>
    [Serializable]
    public class OdinBoolSetting
    {
        public string configProperty;
        public Toggle toggle;
    }

    /// <summary>
    ///     Data structure for setting a float property of the OdinHandler Config from the referenced slider.
    /// </summary>
    [Serializable]
    public class OdinFloatSetting
    {
        public string configProperty;
        public Slider slider;
    }

    /// <summary>
    ///     Data structure for setting a float property of the OdinHandler Config from the referenced slider.
    /// </summary>
    [Serializable]
    public class OdinEnumSetting
    {
        public string configProperty;
        public TMP_Dropdown dropdown;
    }

    /// <summary>
    ///     Script containing all view references (Unity UI sliders, toggles etc.) for adjusting the ODIN Audio Processing
    ///     settings.
    /// </summary>
    public class OdinAudioFilterSettingsController : MonoBehaviour
    {
        [SerializeField] private OdinBoolSetting[] boolSettings;
        [SerializeField] private OdinFloatSetting[] floatSettings;
        [SerializeField] private OdinEnumSetting noiseSuppressionSetting;


        private void Awake()
        {
            foreach (OdinBoolSetting boolSetting in boolSettings) Assert.IsNotNull(boolSetting.toggle);
            foreach (OdinFloatSetting floatSetting in floatSettings) Assert.IsNotNull(floatSetting.slider);
            Assert.IsNotNull(noiseSuppressionSetting.dropdown);
        }

        private void OnEnable()
        {
            StartCoroutine(InitOnOdinHandlerAvailable());
        }

        private void OnDisable()
        {
            foreach (OdinBoolSetting boolSetting in boolSettings)
                boolSetting.toggle.onValueChanged.RemoveAllListeners();

            foreach (OdinFloatSetting setting in floatSettings)
                setting.slider.onValueChanged.RemoveAllListeners();

            noiseSuppressionSetting.dropdown.onValueChanged.RemoveAllListeners();
        }

        private IEnumerator InitOnOdinHandlerAvailable()
        {
            while (!OdinHandler.Instance)
                yield return null;

            foreach (OdinBoolSetting boolSetting in boolSettings)
            {
                boolSetting.toggle.isOn = GetValue<bool>(boolSetting.configProperty);
                boolSetting.toggle.onValueChanged.AddListener(v => { SetBoolValue(boolSetting.configProperty, v); });
            }

            foreach (OdinFloatSetting setting in floatSettings)
            {
                setting.slider.value = GetValue<float>(setting.configProperty);
                setting.slider.onValueChanged.AddListener(newValue =>
                {
                    SetFloatValue(setting.configProperty, newValue);
                });
            }

            NativeBindings.OdinNoiseSuppressionLevel noiseSuppressionLevel =
                GetValue<NativeBindings.OdinNoiseSuppressionLevel>(noiseSuppressionSetting.configProperty);
            noiseSuppressionSetting.dropdown.value = (int)noiseSuppressionLevel;
            noiseSuppressionSetting.dropdown.onValueChanged.AddListener(newValue =>
            {
                SetEnumValueFromInt(noiseSuppressionSetting.configProperty, newValue);
            });
        }

        public void ApplyOdinConfig()
        {
            if (OdinHandler.Instance) StartCoroutine(DelayedApplyRoomConfig());
        }

        private IEnumerator DelayedApplyRoomConfig()
        {
            List<string> roomNames = new List<string>();
            UserData userData = OdinHandler.Instance.GetUserData();

            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                roomNames.Add(room.Config.Name);
            }

            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                OdinHandler.Instance.LeaveRoom(room.Config.Name);
                yield return null;
            }
            yield return null;

            foreach (string roomName in roomNames)
            {
                OdinHandler.Instance.JoinRoom(roomName, userData);
                yield return null;
            }
        }

        public T GetValue<T>(string property)
        {
            FieldInfo fieldInfo = GetFieldInfo(property);
            return (T)fieldInfo.GetValue(OdinHandler.Config);
        }

        public void SetFloatValue(string property, float newValue)
        {
            SetFieldInfo(property, newValue);
        }

        public void SetBoolValue(string property, bool newActive)
        {
            SetFieldInfo(property, newActive);
        }

        public void SetEnumValueFromInt(string property, int value)
        {
            FieldInfo fieldInfo = GetFieldInfo(property);
            if (Enum.IsDefined(fieldInfo.FieldType, 3))
                SetFieldInfo(property, value);
            else
                Debug.LogError($"Invalid enum range selected for property {property}: Value {value}.");
        }

        private static FieldInfo GetFieldInfo(string property)
        {
            Type configType = OdinHandler.Config.GetType();
            FieldInfo field = configType.GetField(property);
            return field;
        }

        private static void SetFieldInfo(string property, object value)
        {
            FieldInfo field = GetFieldInfo(property);
            if (null != field)
            {
                if (field.FieldType != value.GetType() && !Enum.IsDefined(field.FieldType, value))
                    Debug.LogError(
                        $"Tried setting field info of type {field.GetType()} to value {value} of type {value.GetType()}");
                else
                    field.SetValue(OdinHandler.Config, value);
            }
            else
            {
                Debug.LogError($"Could not find field of property {property}");
            }
        }
    }
}