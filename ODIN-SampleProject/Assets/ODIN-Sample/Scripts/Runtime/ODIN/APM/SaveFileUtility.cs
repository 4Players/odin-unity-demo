using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    /// <summary>
    /// Utility class for saving files to disk.
    /// </summary>
    public static class SaveFileUtility
    {
        /// <summary>
        /// Returns the full path to a directory, in which the file can be stored.
        /// </summary>
        /// <param name="fileName">The file to store.</param>
        /// <returns>The full path to the save location.</returns>
        public static string GetSavePath(string fileName)
        {
            return Application.persistentDataPath + Path.AltDirectorySeparatorChar + fileName;
        }

        /// <summary>
        /// Returns a path to the streaming assets directory - settings can be accessed easier from this location.
        /// </summary>
        /// <param name="fileName">The file to store</param>
        /// <returns>The full path to the settings directory.</returns>
        public static string GetSettingsPath(string fileName)
        {
            #if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
                return GetSavePath(fileName);
            #else
                return Application.streamingAssetsPath + Path.AltDirectorySeparatorChar + fileName;
            #endif
        }
        
        /// <summary>
        /// Saves given model to savepath.
        /// </summary>
        /// <param name="savePath">Full path to save location.</param>
        /// <param name="model">The object to save.</param>
        public static void SaveData(string savePath, object model)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string json = JsonUtility.ToJson(model, true);
            using (StreamWriter writer = new StreamWriter(savePath))
            {
                writer.Write(json);
            }
        }
        
        /// <summary>
        /// Tries to load the file given by the loadPath as an object of type T.
        /// </summary>
        /// <param name="loadPath">The full path to the file.</param>
        /// <typeparam name="T">Type to load</typeparam>
        /// <returns>The loaded object or the default value, if not available.</returns>
        public static T LoadData<T>(string loadPath)
        {
            // avoids issues e.g. between comma and points when using german culture vs english culture and handling
            // floating point numbers.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            T result = default(T);
            if (File.Exists(loadPath))
            {
                using (StreamReader reader = new StreamReader(loadPath))
                {
                    string json = reader.ReadToEnd();

                    // Debug.Log($"Loading Json from {loadPath}");
                    result = JsonUtility.FromJson<T>(json);
                }
                
            }

            return result;
        }
    }
}