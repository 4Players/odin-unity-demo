using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    [Serializable]
    public class BoolSettingSchema
    {
        public string configProperty;
        public bool value;
    }

    [Serializable]
    public class FloatSettingSchema
    {
        public string configProperty;
        public float value;
    }

    [Serializable]
    public class EnumSettingSchema
    {
        public string configProperty;
        public int value;
    }

    [Serializable]
    public class OdinAudioFilterSettingsModel
    {
        public List<BoolSettingSchema> boolSettings = new List<BoolSettingSchema>();
        public List<FloatSettingSchema> floatSettings = new List<FloatSettingSchema>();
        public List<EnumSettingSchema> enumSettings = new List<EnumSettingSchema>();

        public void UpdateBool(string fieldName, bool newValue)
        {
            foreach (BoolSettingSchema boolSetting in boolSettings)
                if (boolSetting.configProperty == fieldName)
                {
                    boolSetting.value = newValue;
                    return;
                }

            boolSettings.Add(new BoolSettingSchema { configProperty = fieldName, value = newValue });
        }

        public void UpdateFloat(string fieldName, float newValue)
        {
            foreach (FloatSettingSchema floatSetting in floatSettings)
                if (floatSetting.configProperty == fieldName)
                {
                    floatSetting.value = newValue;
                    return;
                }

            floatSettings.Add(new FloatSettingSchema { configProperty = fieldName, value = newValue });
        }

        public void UpdateEnum(string fieldName, int newValue)
        {
            foreach (EnumSettingSchema enumSetting in enumSettings)
                if (enumSetting.configProperty == fieldName)
                {
                    enumSetting.value = newValue;
                    return;
                }

            enumSettings.Add(new EnumSettingSchema { configProperty = fieldName, value = newValue });
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