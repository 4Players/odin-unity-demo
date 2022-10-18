using System;
using System.Collections.Generic;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    /// <summary>
    /// Schema for saving filter settings data to file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AudioFilterSettingsSchema<T>
    {
        public string configProperty;
        public T value;
    }

    /// <summary>
    /// The model for saving settings data to file.
    /// </summary>
    [Serializable]
    public class OdinAudioFilterSettingsModel
    {
        public List<AudioFilterSettingsSchema<bool>> boolSettings = new List<AudioFilterSettingsSchema<bool>>();
        public List<AudioFilterSettingsSchema<float>> floatSettings = new List<AudioFilterSettingsSchema<float>>();
        public List<AudioFilterSettingsSchema<int>> enumSettings = new List<AudioFilterSettingsSchema<int>>();

        /// <summary>
        /// The save file name for settings that were applied by the user.
        /// </summary>
        private static readonly string SAVE_FILE_NAME = "AudioProcessingSettings.json";
        /// <summary>
        /// The save file name containing default settings, if no user setting file is available.
        /// </summary>
        private static readonly string DEFAULT_SAVE_FILE_NAME = "AudioProcessingDefaultSettings.json";
        
        /// <summary>
        /// Updates the a bool setting.
        /// </summary>
        /// <param name="fieldName">The field name to update.</param>
        /// <param name="newValue">The new value of the field.</param>
        public void UpdateBool(string fieldName, bool newValue)
        {
            UpdateValue(fieldName, newValue, boolSettings);
        }

        /// <summary>
        /// Generic method for updating a field.
        /// </summary>
        /// <param name="fieldname">The field name to update.</param>
        /// <param name="newValue">The new value of the field.</param>
        /// <param name="settings">The settings list which will be updated.</param>
        /// <typeparam name="T">The field value type.</typeparam>
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

        /// <summary>
        /// Updates the a float setting.
        /// </summary>
        /// <param name="fieldName">The field name to update.</param>
        /// <param name="newValue">The new value of the field.</param>
        public void UpdateFloat(string fieldName, float newValue)
        {
            UpdateValue(fieldName, newValue, floatSettings);
        }

        /// <summary>
        /// Updates the an enum setting.
        /// </summary>
        /// <param name="fieldName">The field name to update.</param>
        /// <param name="newValue">The new value of the field.</param>
        public void UpdateEnum(string fieldName, int newValue)
        {
            UpdateValue(fieldName, newValue, enumSettings);
        }
        
        /// <summary>
        /// Saves the given model data to the user save file.
        /// </summary>
        /// <param name="model">Model data to save.</param>
        public static void SaveData(OdinAudioFilterSettingsModel model)
        {
            string savePath = SaveFileUtility.GetSavePath(SAVE_FILE_NAME);
            SaveFileUtility.SaveData(savePath, model);
        }

        /// <summary>
        /// Tries to load user save data from file. Will fall back to default data, if user data is not available.
        /// </summary>
        /// <returns>Loaded Model data</returns>
        public static OdinAudioFilterSettingsModel LoadCustomOrDefaultData()
        {
            OdinAudioFilterSettingsModel loadResult = SaveFileUtility.LoadData<OdinAudioFilterSettingsModel>(SaveFileUtility.GetSavePath(SAVE_FILE_NAME));
            if (null == loadResult)
                loadResult = LoadDefaultData();
            return loadResult;
        }

        /// <summary>
        /// Loads the default model from file.
        /// </summary>
        /// <returns>The loaded default model.</returns>
        public static OdinAudioFilterSettingsModel LoadDefaultData()
        {
            return SaveFileUtility.LoadData<OdinAudioFilterSettingsModel>(SaveFileUtility.GetSettingsPath(DEFAULT_SAVE_FILE_NAME));
        }

        /// <summary>
        /// Overwrites the default model data. This should only be used, if no default save file is available on disk.
        /// </summary>
        /// <param name="newDefault">The new default model.</param>
        public static void OverwriteDefaultData(OdinAudioFilterSettingsModel newDefault)
        {
            SaveFileUtility.SaveData(SaveFileUtility.GetSettingsPath(DEFAULT_SAVE_FILE_NAME), newDefault);
        }
    }
}