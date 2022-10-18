using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OdinNative.Unity.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    /// <summary>
    ///     Controller script for selecting an input device for ODIN Voice chat.
    /// </summary>
    public class OdinMicrophoneController : MonoBehaviour
    {
        /// <summary>
        ///     The microphone selection dropdown object.
        /// </summary>
        [SerializeField] private TMP_Dropdown selection;

        [SerializeField] private OdinMicrophoneSettings microphoneSettings;

        private List<string> _microphones;

        private void Awake()
        {
            Assert.IsNotNull(microphoneSettings);
            Assert.IsNotNull(selection);
            _microphones = new List<string>(Microphone.devices);
        }

        private void Update()
        {
            bool isListUpToDate = true;
            var currentList = new List<string>(Microphone.devices);
            foreach (string device in currentList) isListUpToDate &= _microphones.Contains(device);
            foreach (string device in _microphones) isListUpToDate &= currentList.Contains(device);

            if (!isListUpToDate) UpdateSelection();
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        private IEnumerator WaitForConnection()
        {
            while (null == OdinHandler.Instance)
                yield return null;

            yield return null;
            UpdateSelection();
            microphoneSettings.Load();
            ActivateDevice(microphoneSettings.selectedMicrophone);
        }

        /// <summary>
        ///     Updates the device list in the dropdown.
        /// </summary>
        private void UpdateSelection()
        {
            selection.ClearOptions();
            _microphones = new List<string>(Microphone.devices);
            selection.AddOptions(_microphones);

            string currentDevice = OdinHandler.Instance.Microphone.InputDevice;
            int foundIndex = _microphones.FindIndex(x => x == currentDevice);
            if (foundIndex > 0) selection.value = foundIndex;
        }


        /// <summary>
        ///     Takes the current microphone selection from the dropdown and tries to activate
        ///     it as the ODIN input device.
        /// </summary>
        public void ApplySelectedMicrophone()
        {
            TMP_Dropdown.OptionData selectionOption = selection.options[selection.value];
            string selectedMicrophone = selectionOption.text;
            ActivateDevice(selectedMicrophone);

            microphoneSettings.selectedMicrophone = selectedMicrophone;
            microphoneSettings.Save();
        }

        /// <summary>
        ///     Stops and restarts the ODIN microphone reader with the new microphone selection.
        /// </summary>
        private void ActivateDevice(string selectedDevice)
        {
            if (Microphone.devices.Contains(selectedDevice))
            {
                MicrophoneReader microphoneReader = OdinHandler.Instance.Microphone;

                if (selectedDevice != microphoneReader.InputDevice)
                {
                    microphoneReader.StopListen();
                    microphoneReader.CustomInputDevice = true;
                    microphoneReader.InputDevice = selectedDevice;
                    microphoneReader.StartListen();
                }
            }
            else
            {
                Debug.LogError($"Selected microphone {selectedDevice} ist not available in Devices list.");
            }
        }
    }
}