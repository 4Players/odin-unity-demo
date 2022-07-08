using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ODIN_Sample.Scripts.Runtime.ODIN.Utility;
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
        /// <summary>
        /// The name of the field
        /// </summary>
        public string configProperty;
        /// <summary>
        /// Reference to the toggle.
        /// </summary>
        public Toggle toggle;
    }

    /// <summary>
    ///     Data structure for setting a float property of the OdinHandler Config from the referenced slider.
    /// </summary>
    [Serializable]
    public class OdinFloatSetting
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string configProperty;
        /// <summary>
        /// Reference to the slider.
        /// </summary>
        public Slider slider;
    }

    /// <summary>
    ///     Data structure for setting a float property of the OdinHandler Config from the referenced slider.
    /// </summary>
    [Serializable]
    public class OdinEnumSetting
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string configProperty;
        /// <summary>
        /// Reference to the dropdown
        /// </summary>
        public TMP_Dropdown dropdown;
    }

    /// <summary>
    ///     Script containing all view references (Unity UI sliders, toggles etc.) for adjusting the ODIN Audio Processing
    ///     settings. Will update views to current settings and will update settings based on user input on views.
    /// </summary>
    public class OdinAudioFilterSettingsController : MonoBehaviour
    {
        /// <summary>
        /// All bool settings
        /// </summary>
        [SerializeField] private OdinBoolSetting[] boolSettings;
        /// <summary>
        /// All float settings
        /// </summary>
        [SerializeField] private OdinFloatSetting[] floatSettings;
        /// <summary>
        /// Noise suppression setting
        /// </summary>
        [SerializeField] private OdinEnumSetting noiseSuppressionSetting;


        private OdinAudioFilterSettingsModel _model;
        
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

        /// <summary>
        /// Initializes View Values for settings to current values.
        /// Will wait for the OdinHandler to be initialized before updating.
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitOnOdinHandlerAvailable()
        {
            // Wait until OdinHandler is available
            while (!OdinHandler.Instance)
                yield return null;
            
            _model = OdinAudioFilterSettingsModel.LoadData();
            if (null == _model)
            {
                _model = OdinAudioFilterSettingsModel.LoadDefaultData();
            }

            if (null == _model)
            {
                _model = new OdinAudioFilterSettingsModel();
                InitModel(_model);
                OdinAudioFilterSettingsModel.OverwriteDefaultData(_model);
            }
            ApplySavedModel(_model);
            UpdateViews();

            // Bind to user input
            foreach (OdinBoolSetting boolSetting in boolSettings)
            {
                boolSetting.toggle.onValueChanged.AddListener(newValue =>
                {
                    if (null != _model)
                    {
                        _model.UpdateBool(boolSetting.configProperty, newValue);
                    }
                    SetBoolValue(boolSetting.configProperty, newValue);
                });
            }
            foreach (OdinFloatSetting setting in floatSettings)
            {
                setting.slider.onValueChanged.AddListener(newValue =>
                {
                    if (null != _model)
                    {
                        _model.UpdateFloat(setting.configProperty, newValue);
                    }
                    SetFloatValue(setting.configProperty, newValue);
                });
            }
            noiseSuppressionSetting.dropdown.onValueChanged.AddListener(newValue =>
            {
                if (null != _model)
                {
                    _model.UpdateEnum(noiseSuppressionSetting.configProperty, newValue);
                }
                SetEnumValueFromInt(noiseSuppressionSetting.configProperty, newValue);
            });
        }

        private void InitModel(OdinAudioFilterSettingsModel model)
        {
            foreach (OdinBoolSetting boolSetting in boolSettings)
            {
                model.UpdateBool(boolSetting.configProperty, GetValue<bool>(boolSetting.configProperty));
            }
            foreach (OdinFloatSetting floatSetting in floatSettings)
            {
                model.UpdateFloat(floatSetting.configProperty, GetValue<float>(floatSetting.configProperty));
            }
            
            model.UpdateEnum(noiseSuppressionSetting.configProperty, (int) GetValue<NativeBindings.OdinNoiseSuppressionLevel>(noiseSuppressionSetting.configProperty));
        }

        private void UpdateViews()
        {
            // Set Bool Settings toggles to current value.
            foreach (OdinBoolSetting boolSetting in boolSettings)
            {
                boolSetting.toggle.isOn = GetValue<bool>(boolSetting.configProperty);
            }

            // Set Float Settings sliders to current value.
            foreach (OdinFloatSetting setting in floatSettings)
            {
                setting.slider.value = GetValue<float>(setting.configProperty);
            }

            // Set Enum dropdowns to current value.
            NativeBindings.OdinNoiseSuppressionLevel noiseSuppressionLevel =
                GetValue<NativeBindings.OdinNoiseSuppressionLevel>(noiseSuppressionSetting.configProperty);
            noiseSuppressionSetting.dropdown.value = (int)noiseSuppressionLevel;
        }

        private void ApplySavedModel(OdinAudioFilterSettingsModel model)
        {
            if (null != model)
            {
                foreach (BoolSettingSchema boolSetting in _model.boolSettings)
                {
                    SetBoolValue(boolSetting.configProperty, boolSetting.value);
                }
                foreach (FloatSettingSchema floatSetting in model.floatSettings)
                {
                    SetFloatValue(floatSetting.configProperty, floatSetting.value);
                }
                foreach (EnumSettingSchema enumSetting in model.enumSettings)
                {
                    SetEnumValueFromInt(enumSetting.configProperty, enumSetting.value);
                }
                
                UpdateViews();
            }
        }

        /// <summary>
        /// Starts the ODIN Settings application logic. Currently we have to leave all rooms and then reenter.
        /// </summary>
        public void ApplyOdinConfig()
        {
            if (OdinHandler.Instance)
            {
                OdinAudioFilterSettingsModel.SaveData(_model);
                StartCoroutine(DelayedApplyRoomConfig());
            }
        }

        public void ResetOdinConfig()
        {
            OdinAudioFilterSettingsModel defaultData = OdinAudioFilterSettingsModel.LoadDefaultData();
            if (null != defaultData)
            {
                _model = defaultData;
                ApplySavedModel(_model);
                UpdateViews();
            }
        }

        /// <summary>
        /// Will leave all ODIN rooms, then reenter all rooms. Will only leave or enter a room once in a frame, so this
        /// operation will be performed over 2*EnteredRooms Frames.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedApplyRoomConfig()
        {
            // Leave all rooms
            List<string> roomNames = new List<string>();
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

            // reenter all rooms
            var userData = OdinHandler.Instance.GetUserData();
            foreach (string roomName in roomNames)
            {
                OdinHandler.Instance.JoinRoom(roomName, userData);
                yield return null;
            }
        }

        
        #pragma region Reflection Based Value Setting
        
        /// <summary>
        /// Retrieves a field value from <c>OdinHandler.Config</c>.
        /// </summary>
        /// <param name="property">Field name.</param>
        /// <typeparam name="T">Expected type of the returned field value.</typeparam>
        /// <returns>Found Value.</returns>
        private T GetValue<T>(string property)
        {
            FieldInfo fieldInfo = GetFieldInfo(property);
            return (T)fieldInfo.GetValue(OdinHandler.Config);
        }

        /// <summary>
        /// Sets a float field in the <c>OdinHandler.Config</c>.
        /// </summary>
        /// <param name="property">Float field name.</param>
        /// <param name="newValue">New Value.</param>
        private void SetFloatValue(string property, float newValue)
        {
            SetFieldInfo(property, newValue);
        }

        /// <summary>
        /// Sets a bool field in the <c>OdinHandler.Config</c>.
        /// </summary>
        /// <param name="property">Bool field name.</param>
        /// <param name="newActive">New Value.</param>
        private void SetBoolValue(string property, bool newActive)
        {
            SetFieldInfo(property, newActive);
        }

        /// <summary>
        /// Sets an enum in the <c>OdinHandler.Config</c> from a given int value.
        /// </summary>
        /// <param name="property">Enum field name.</param>
        /// <param name="value">Enum Value to set to.</param>
        private void SetEnumValueFromInt(string property, int value)
        {
            FieldInfo fieldInfo = GetFieldInfo(property);
            if (Enum.IsDefined(fieldInfo.FieldType, 3))
                SetFieldInfo(property, value);
            else
                Debug.LogError($"Invalid enum range selected for property {property}: Value {value}.");
        }

        /// <summary>
        /// Retrieves reflection field info for the given property from <c>OdinHandler.Config</c>.
        /// </summary>
        /// <param name="property">The property for which we want to retrieve the field info</param>
        /// <returns>Field info or null, if property not found</returns>
        private static FieldInfo GetFieldInfo(string property)
        {
            Type configType = OdinHandler.Config.GetType();
            FieldInfo field = configType.GetField(property);
            return field;
        }

        /// <summary>
        /// Sets the field  of <c> OdinHandler.Config</c> to the given value, if available. Will only work if the type of value is correct.
        /// </summary>
        /// <param name="property">Field to set.</param>
        /// <param name="value">Value to set to.</param>
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
        
        #pragma endregion
    }
}