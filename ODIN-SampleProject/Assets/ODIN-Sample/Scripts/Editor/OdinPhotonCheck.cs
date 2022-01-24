using System;
using UnityEditor;
using UnityEngine;

namespace ODIN_Sample.Scripts.Editor
{
    [InitializeOnLoad]
    static class OdinPhotonCheck
    {
        private static string IsFirstStartupKey => "ODIN-Sample.isFirstStartup";

        static OdinPhotonCheck()
        {
            bool hasPun = HasType("Photon.Pun.PhotonNetwork");
            bool isFirstStartup = SessionState.GetBool(OdinPhotonCheck.IsFirstStartupKey, true);
            if (isFirstStartup && !hasPun)
            {
                Debug.Log("Checking for Photon installation!");
                SessionState.SetBool(IsFirstStartupKey, false);
                
                bool pressedOk = EditorUtility.DisplayDialog("ODIN-Sample Requirements",
                    "The ODIN-Sample uses the \"Photon PUN 2 - FREE\" plugin for Multiplayer Synchronisation. " +
                    "Please install the plugin for free from the asset store to resolve any compilation errors.",
                    "Go to Plugin Page", "Cancel");
                if (pressedOk)
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
                }
            }
        }


        private static bool HasType(string type)
        {
            
            return Type.GetType($"{type}, Assembly-CSharp") != null ||
                Type.GetType($"{type}, Assembly-CSharp-firstpass") != null ||
                Type.GetType($"{type}, PhotonUnityNetworking") != null;
        }
    }
}