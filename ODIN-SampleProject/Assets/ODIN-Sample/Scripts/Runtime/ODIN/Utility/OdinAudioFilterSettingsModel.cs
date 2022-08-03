using System;
using System.Collections.Generic;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{

    [Serializable]
    public class AudioFilterSettingsSchema<T>
    {
        public string configProperty;
        public T value;
    }

    [Serializable]
    public class OdinAudioFilterSettingsModel
    {
        public List<AudioFilterSettingsSchema<bool>> boolSettings = new List<AudioFilterSettingsSchema<bool>>();
        public List<AudioFilterSettingsSchema<float>> floatSettings = new List<AudioFilterSettingsSchema<float>>();
        public List<AudioFilterSettingsSchema<int>> enumSettings = new List<AudioFilterSettingsSchema<int>>();

        private static readonly string SAVE_FILE_NAME = "AudioProcessingSettings.json";
        private static readonly string DEFAULT_SAVE_FILE_NAME = "AudioProcessingDefaultSettings.json";
        
        public void UpdateBool(string fieldName, bool newValue)
        {
            UpdateValue(fieldName, newValue, boolSettings);
        }

        public void UpdateValue<T>(string fieldname, T newValue, List<AudioFilterSettingsSchema<T>> settings)
        {
            foreach (var setting in settings)
            {
                if (setting.configProperty == fieldname)
                {
                    setting.value = newValue;
                    return;
                }
            } 
            settings.Add(new AudioFilterSettingsSchema<T>(){configProperty = fieldname,value = newValue});
        }  

        public void UpdateFloat(string fieldName, float newValue)
        {
            UpdateValue(fieldName, newValue, floatSettings);
        }

        public void UpdateEnum(string fieldName, int newValue)
        {
            UpdateValue(fieldName, newValue, enumSettings);
        }
        
        public static void SaveData(OdinAudioFilterSettingsModel model)
        {
            string savePath = SaveFileUtility.GetCustomSavePath(SAVE_FILE_NAME);
            SaveFileUtility.SaveData(savePath, model);
        }

        public static OdinAudioFilterSettingsModel LoadCustomOrDefaultData()
        {
            OdinAudioFilterSettingsModel loadResult = SaveFileUtility.LoadData<OdinAudioFilterSettingsModel>(SaveFileUtility.GetCustomSavePath(SAVE_FILE_NAME));
            if (null == loadResult)
                loadResult = LoadDefaultData();
            return loadResult;
        }

        public static OdinAudioFilterSettingsModel LoadDefaultData()
        {
            return SaveFileUtility.LoadData<OdinAudioFilterSettingsModel>(SaveFileUtility.GetDefaultSettingsPath(DEFAULT_SAVE_FILE_NAME));
        }

        public static void OverwriteDefaultData(OdinAudioFilterSettingsModel newDefault)
        {
            SaveFileUtility.SaveData(SaveFileUtility.GetDefaultSettingsPath(DEFAULT_SAVE_FILE_NAME), newDefault);
        }
    }
}