using System;
using ODIN_Sample.Scripts.Runtime.ODIN.Utility;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN.APM
{
    /// <summary>
    /// Scriptable object for storing and saving the selected microphone to file.
    /// </summary>
    [CreateAssetMenu(fileName = "OdinMicrophoneSettings", menuName = "Odin-Demo/MicrophoneSettings", order = 0)]
    public class OdinMicrophoneSettings : ScriptableObject
    {
        /// <summary>
        /// The  file name the settings will be saved to. Will use the 
        /// </summary>
        private const string SaveFileName = "OdinMicrophoneSettings.json";

        public string selectedMicrophone;

        private void OnEnable()
        {
            Load();
        }

        private string GetSavePath()
        {
            return SaveFileUtility.GetSavePath(SaveFileName);
        }

        public void Save()
        {
            SaveFileUtility.SaveData(GetSavePath(), this);
        }

        public void Load()
        {
            string settingsPath = SaveFileUtility.GetSavePath(SaveFileName);
            var saveData = SaveFileUtility.LoadData<OdinMicrophoneSettingsSchema>(settingsPath);
            if (null != saveData) selectedMicrophone = saveData.selectedMicrophone;
        }

        [Serializable]
        class OdinMicrophoneSettingsSchema
        {
            public string selectedMicrophone;
        }
    }
}