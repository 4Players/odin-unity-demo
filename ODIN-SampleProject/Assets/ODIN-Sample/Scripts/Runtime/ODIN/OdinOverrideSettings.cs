using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    ///     Overrides settings for ODIN handler instance.
    /// </summary>
    public class OdinOverrideSettings : MonoBehaviour
    {
        private void Awake()
        {
            var overrideGatewayURL = GetCommandLineArg("override-gateway-url");
            if (!String.IsNullOrEmpty(overrideGatewayURL)) {
                Debug.Log("Overriding default ODIN gateway URL: " + overrideGatewayURL);

                OdinHandler.Config.Server = overrideGatewayURL;
            }

            var overrideAccessKey = GetCommandLineArg("override-access-key");
            if (!String.IsNullOrEmpty(overrideAccessKey)) {
                Debug.Log("Overriding default ODIN access key: " + overrideAccessKey);

                OdinHandler.Config.AccessKey = overrideAccessKey;
            }
        }

        private string GetCommandLineArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                    return args[i + 1];
            }
            
            return null;
        }
    }
}