using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    [CreateAssetMenu(fileName = "OdinMicrophoneSettings", menuName = "Odin-Demo/MicrophoneSettings", order = 0)]
    public class OdinMicrophoneSettings : ScriptableObject
    {
        private static readonly string SAVE_FILE_NAME = "OdinMicrophoneSettings.json";
        public string selectedMicrophone;

        private void OnEnable()
        {
            Load();
        }

        private string GetSavePath()
        {
            return SaveFileUtility.GetSavePath(SAVE_FILE_NAME);
        }

        public void Save()
        {
            SaveFileUtility.SaveData(GetSavePath(), this);
        }

        public void Load()
        {
            string settingsPath = SaveFileUtility.GetSavePath(SAVE_FILE_NAME);
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