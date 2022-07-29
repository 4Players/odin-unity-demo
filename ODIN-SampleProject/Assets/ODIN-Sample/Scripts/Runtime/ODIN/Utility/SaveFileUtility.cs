using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    public static class SaveFileUtility
    {
        public static string GetCustomSavePath(string fileName)
        {
            return Application.persistentDataPath + Path.AltDirectorySeparatorChar + fileName;
        }

        public static string GetDefaultSettingsPath(string fileName)
        {
            #if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
                return GetCustomSavePath(fileName);
            #else
                return Application.streamingAssetsPath + Path.AltDirectorySeparatorChar + fileName;
            #endif
        }
        
        public static void SaveData(string savePath, object model)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string json = JsonUtility.ToJson(model, true);
            using StreamWriter writer = new StreamWriter(savePath);
            writer.Write(json);
        }
        
        public static T LoadData<T>(string loadPath)
        {
            // avoids issues e.g. between comma and points when using german culture vs english culture and handling
            // floating point numbers.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            T result = default(T);
            if (File.Exists(loadPath))
            {
                using StreamReader reader = new StreamReader(loadPath);
                string json = reader.ReadToEnd();

                Debug.Log($"Loading Json from {loadPath}");
                result = JsonUtility.FromJson<T>(json);
            }

            return result;
        }
    }
}