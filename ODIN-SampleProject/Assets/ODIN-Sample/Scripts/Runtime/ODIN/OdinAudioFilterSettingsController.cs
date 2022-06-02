using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [Serializable]
    public class OdinBoolSetting
    {
        public string configProperty;
        public Toggle toggle;
    }

    [Serializable]
    public class OdinFloatSetting
    {
        public string configProperty;
        public Slider slider;
    }

    public class OdinAudioFilterSettingsController : MonoBehaviour
    {
        [SerializeField] private OdinBoolSetting[] boolSettings;
        [SerializeField] private OdinFloatSetting[] floatSettings;

        private void Awake()
        {
            foreach (OdinBoolSetting boolSetting in boolSettings) Assert.IsNotNull(boolSetting.toggle);
            foreach (OdinFloatSetting floatSetting in floatSettings) Assert.IsNotNull(floatSetting.slider);
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
                if (field.FieldType != value.GetType())
                    Debug.LogError(
                        $"Tried setting field info of type {field.GetType()} to value of type {value.GetType()}");
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