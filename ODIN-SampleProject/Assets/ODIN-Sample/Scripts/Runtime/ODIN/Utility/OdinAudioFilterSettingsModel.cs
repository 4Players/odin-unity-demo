using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

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

        private static string GetCustomSavePath()
        {
            return Application.dataPath + Path.AltDirectorySeparatorChar + "AudioProcessingSettings.json";
        }

        private static string GetDefaultSettingsPath()
        {
            return Application.streamingAssetsPath + Path.AltDirectorySeparatorChar + "AudioProcessingDefaultSettings.json";
        }

        private static void SaveData(OdinAudioFilterSettingsModel model, string savePath)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string json = JsonUtility.ToJson(model, true);
            Debug.Log($"Saving Settings to {savePath}");
            using StreamWriter writer = new StreamWriter(GetCustomSavePath());
            writer.Write(json);
        }

        public static void SaveData(OdinAudioFilterSettingsModel model)
        {
            string savePath = GetCustomSavePath();
            SaveData(model, savePath);
        }

        private static OdinAudioFilterSettingsModel LoadData(string loadPath)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            OdinAudioFilterSettingsModel result = null;
            if (File.Exists(loadPath))
            {
                using StreamReader reader = new StreamReader(loadPath);
                string json = reader.ReadToEnd();

                Debug.Log($"Loading Json from {loadPath}");
                result = JsonUtility.FromJson<OdinAudioFilterSettingsModel>(json);
            }

            return result;
        }

        public static OdinAudioFilterSettingsModel LoadData()
        {
            return LoadData(GetCustomSavePath());
        }

        public static OdinAudioFilterSettingsModel LoadDefaultData()
        {
            return LoadData(GetDefaultSettingsPath());
        }

        public static void OverwriteDefaultData(OdinAudioFilterSettingsModel newDefault)
        {
            SaveData(newDefault, GetDefaultSettingsPath());
        }
    }
}