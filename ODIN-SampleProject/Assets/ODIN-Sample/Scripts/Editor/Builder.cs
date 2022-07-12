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
                "sample",
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
        
        private static UnityEditor.Build.Reporting.BuildReport Build(string _method, string name, BuildTarget target,
            BuildOptions options)
        {
            var args = System.Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                try
                {
                    name = args.GetValue(Array.IndexOf(args, _method) + 1).ToString();
                }
                catch
                {
                    Debug.Log($"Invalid argument for {_method}! Using: {name}");
                }
            }

            string[] defaultScene =
            {
                "Assets/ODIN-Sample/Scenes/Lobby.unity",
            };

            return BuildPipeline.BuildPlayer(defaultScene, name, target, options);
        }
    }
}
