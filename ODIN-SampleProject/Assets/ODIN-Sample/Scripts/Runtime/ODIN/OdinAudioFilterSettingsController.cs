using System;
using System.Collections;
using System.Reflection;
using ODIN_Sample.Scripts.Runtime.ODIN.Utility;
using OdinNative.Core;
using OdinNative.Core.Imports;
using OdinNative.Odin.Room;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [Serializable]
    public class FloatSettingSideEffect
    {
        public string originProperty;
        public string targetProperty;
        public float offset;
    }

    /// <summary>
    ///     Data structure for setting a boolean property of the OdinHandler Config from the referenced toggle.
    /// </summary>
    [Serializable]
    public class OdinBoolSetting
    {
        /// <summary>
        ///     The name of the field
        /// </summary>
        public string configProperty;

        /// <summary>
        ///     Reference to the toggle.
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
        ///     The name of the field
        /// </summary>
        public string configProperty;

        /// <summary>
        ///     Reference to the slider.
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
        ///     The name of the field
        /// </summary>
        public string configProperty;

        /// <summary>
        ///     Reference to the dropdown
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
        ///     All bool settings
        /// </summary>
        [Header("Bool Settings")]
        [SerializeField]
        private OdinBoolSetting[] boolSettings;

        /// <summary>
        ///     All float settings
        /// </summary>
        [Header("Float Settings")]
        [SerializeField]
        private OdinFloatSetting[] floatSettings;

        [SerializeField] private FloatSettingSideEffect[] sideEffects;

        /// <summary>
        ///     Noise suppression setting
        /// </summary>
        [Header("Enum Settings")]
        [SerializeField]
        private OdinEnumSetting noiseSuppressionSetting;

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
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnOdinRoomJoined);
        }

        /// <summary>
        ///     Initializes View Values for settings to current values.
        ///     Will wait for the OdinHandler to be initialized before updating.
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitOnOdinHandlerAvailable()
        {
            // Wait until OdinHandler is available
            while (!OdinHandler.Instance)
                yield return null;

            _model = OdinAudioFilterSettingsModel.LoadCustomOrDefaultData();

            // if theres no default or custom save data, create a new one
            if (null == _model)
            {
                _model = new OdinAudioFilterSettingsModel();
                InitModelFromOdinConfig();
                OdinAudioFilterSettingsModel.OverwriteDefaultData(_model);
            }

            ApplyModelToOdinHandler(_model);
            UpdateViews();

            // Bind to user input
            foreach (OdinBoolSetting boolSetting in boolSettings)
                boolSetting.toggle.onValueChanged.AddListener(newValue =>
                {
                    UpdateBoolSetting(boolSetting.configProperty, newValue);
                });
            foreach (OdinFloatSetting setting in floatSettings)
                setting.slider.onValueChanged.AddListener(newValue =>
                {
                    UpdateFloatSetting(setting.configProperty, newValue);
                });
            noiseSuppressionSetting.dropdown.onValueChanged.AddListener(newValue =>
            {
                UpdateEnumSetting(noiseSuppressionSetting.configProperty, newValue);
            });

            OdinHandler.Instance.OnRoomJoined.AddListener(OnOdinRoomJoined);
        }

        /// <summary>
        /// Updates the model, sets the odin config settings and applies side effects, if any exist.
        /// </summary>
        /// <param name="property">Property to apply settings to</param>
        /// <param name="newValue">New property value.</param>
        private void UpdateFloatSetting(string property, float newValue)
        {
            if (null != _model)
            {
                _model.UpdateFloat(property, newValue);
                SetValue(property, newValue);

                ApplyFloatSideEffects(property, newValue);
            }
        }

        /// <summary>
        /// Applies side effects, if any were defined, to the given property.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="newValue">New value of the property.</param>
        private void ApplyFloatSideEffects(string property, float newValue)
        {
            foreach (FloatSettingSideEffect sideEffect in sideEffects)
            {
                if (sideEffect.originProperty == property)
                {
                    if (sideEffect.originProperty == sideEffect.targetProperty)
                    {
                        Debug.LogError("Loop in sideEffect definition detected, will not apply side effect.");
                    }
                    else
                    {
                        float newTargetValue = newValue + sideEffect.offset;
                        UpdateFloatSetting(sideEffect.targetProperty, newTargetValue);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the model and applies setting to Odin Config.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="newValue">New property value</param>
        private void UpdateEnumSetting(string property, int newValue)
        {
            if (null != _model)
            {
                _model.UpdateEnum(property, newValue);
                SetValue(property, newValue);
            }
        }

        /// <summary>
        /// Updates the model and applies setting to Odin Config.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="newValue">New property value</param>
        private void UpdateBoolSetting(string property, bool newValue)
        {
            if (null != _model)
            {
                _model.UpdateBool(property, newValue);
                SetValue(property, newValue);
            }
        }

        /// <summary>
        /// Applies the current audio processing settings to the newly joined room.
        /// </summary>
        /// <param name="eventArgs"></param>
        private void OnOdinRoomJoined(RoomJoinedEventArgs eventArgs)
        {
            if (null != eventArgs && null != eventArgs.Room)
                eventArgs.Room.SetApmConfig(GetCurrentConfigFromOdinHandler());
        }

        /// <summary>
        ///     Applies the current settings of the OdinHandler.Config Instance to the current model
        /// </summary>
        private void InitModelFromOdinConfig()
        {
            foreach (OdinBoolSetting boolSetting in boolSettings)
                UpdateBoolSetting(boolSetting.configProperty, GetValue<bool>(boolSetting.configProperty));
            foreach (OdinFloatSetting floatSetting in floatSettings)
                UpdateFloatSetting(floatSetting.configProperty, GetValue<float>(floatSetting.configProperty));

            UpdateEnumSetting(noiseSuppressionSetting.configProperty,
                (int)GetValue<NativeBindings.OdinNoiseSuppressionLevel>(noiseSuppressionSetting.configProperty));
        }

        /// <summary>
        ///     Updates the views (toggles, sliders and dropdowns) to display the currently set values in the OdinHandler.Config
        /// </summary>
        private void UpdateViews()
        {
            // Set Bool Settings toggles to current value.
            foreach (OdinBoolSetting boolSetting in boolSettings)
                boolSetting.toggle.isOn = GetValue<bool>(boolSetting.configProperty);

            // Set Float Settings sliders to current value.
            foreach (OdinFloatSetting floatSetting in floatSettings)
                floatSetting.slider.value = GetValue<float>(floatSetting.configProperty);

            // Set Enum dropdowns to current value.
            NativeBindings.OdinNoiseSuppressionLevel noiseSuppressionLevel =
                GetValue<NativeBindings.OdinNoiseSuppressionLevel>(noiseSuppressionSetting.configProperty);
            noiseSuppressionSetting.dropdown.value = (int)noiseSuppressionLevel;
        }

        /// <summary>
        ///     Apply the values from te given settings model to the OdinHandler.Config Instance.
        /// </summary>
        /// <param name="model">Model containing the data to apply</param>
        private void ApplyModelToOdinHandler(OdinAudioFilterSettingsModel model)
        {
            if (null != model)
            {
                foreach (var boolSetting in _model.boolSettings)
                    SetValue(boolSetting.configProperty, boolSetting.value);
                foreach (var floatSetting in model.floatSettings)
                    SetValue(floatSetting.configProperty, floatSetting.value);
                foreach (var enumSetting in model.enumSettings) SetValue(enumSetting.configProperty, enumSetting.value);

                UpdateViews();
            }
        }

        /// <summary>
        ///     Applies the new Audio Processing settings to each room.
        /// </summary>
        public void ApplyOdinConfig()
        {
            if (OdinHandler.Instance)
            {
                OdinAudioFilterSettingsModel.SaveData(_model);
                foreach (Room room in OdinHandler.Instance.Rooms)
                {
                    OdinRoomConfig config = GetCurrentConfigFromOdinHandler();
                    room.SetApmConfig(config);
                }
            }
        }

        /// <summary>
        ///     Resets the audio filter settings to the default values defined in the default configuration file.
        /// </summary>
        public void ResetOdinConfig()
        {
            OdinAudioFilterSettingsModel defaultData = OdinAudioFilterSettingsModel.LoadDefaultData();
            if (null != defaultData)
            {
                _model = defaultData;
                ApplyModelToOdinHandler(_model);
                UpdateViews();
            }
        }

        private OdinRoomConfig GetCurrentConfigFromOdinHandler()
        {
            return new OdinRoomConfig
            {
                EchoCanceller = OdinHandler.Config.EchoCanceller,
                HighPassFilter = OdinHandler.Config.HighPassFilter,
                NoiseSuppressionLevel = OdinHandler.Config.NoiseSuppressionLevel,
                PreAmplifier = OdinHandler.Config.PreAmplifier,
                TransientSuppressor = OdinHandler.Config.TransientSuppressor,
                VoiceActivityDetection = OdinHandler.Config.VoiceActivityDetection,
                VoiceActivityDetectionAttackProbability =
                    OdinHandler.Config.VoiceActivityDetectionAttackProbability,
                VoiceActivityDetectionReleaseProbability =
                    OdinHandler.Config.VoiceActivityDetectionReleaseProbability,
                VolumeGate = OdinHandler.Config.VolumeGate,
                VolumeGateAttackLoudness = OdinHandler.Config.VolumeGateAttackLoudness,
                VolumeGateReleaseLoudness = OdinHandler.Config.VolumeGateReleaseLoudness
            };
        }

        /// <summary>
        ///     Retrieves a field value from <c>OdinHandler.Config</c>.
        /// </summary>
        /// <param name="property">Field name.</param>
        /// <typeparam name="T">Expected type of the returned field value.</typeparam>
        /// <returns>Found Value.</returns>
        private T GetValue<T>(string property)
        {
            FieldInfo fieldInfo = GetFieldInfo(property);
            return (T)fieldInfo.GetValue(OdinHandler.Config);
        }

        private void SetValue<T>(string property, T newValue)
        {
            SetFieldInfo(property, newValue);
        }

        /// <summary>
        ///     Retrieves reflection field info for the given property from <c>OdinHandler.Config</c>.
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
        ///     Sets the field  of <c> OdinHandler.Config</c> to the given value, if available. Will only work if the type of value
        ///     is correct.
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
    }
}