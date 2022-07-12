using System;
using UnityEditor;
using UnityEngine;

namespace ODIN_Sample.Scripts.Editor
{
    public class Builder
    {
        static void BuildAndroid()
        {
            Build($"{nameof(Builder)}.{nameof(Builder.BuildAndroid)}",
                "sample.apk",
                BuildTarget.Android,
                BuildOptions.None);
        }
        
        static void BuildiOS()
        {
            Build($"{nameof(Builder)}.{nameof(Builder.BuildiOS)}",
                "sample",
                BuildTarget.iOS,
                BuildOptions.None);
        }
        
        private static UnityEditor.Build.Reporting.BuildReport Build(string method, string name, BuildTarget target,
            BuildOptions options)
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                try
                {
                    name = args.GetValue(Array.IndexOf(args, method) + 1).ToString();
                }
                catch
                {
                    Debug.Log($"Invalid argument for {method}! Using: {name}");
                }
            }

            string[] scenes =
            {
                "Assets/ODIN-Sample/Scenes/Lobby.unity",
                "Assets/ODIN-Sample/Scenes/DemoLevel.unity"
            };

            Debug.Log($"Running {method} with: {name}");
            return BuildPipeline.BuildPlayer(scenes, name, target, options);
        }
    }
}
