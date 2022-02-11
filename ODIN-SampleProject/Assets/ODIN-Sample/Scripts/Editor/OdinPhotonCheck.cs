using System;
using UnityEditor;
using UnityEngine;

namespace ODIN_Sample.Scripts.Editor
{
    public static class OdinPhotonCheck
    {
        private static string IsFirstStartupKey => "ODIN-Sample.OdinPhotonCheck.isFirstStartup";

        private static string PhotonAssetStoreUrl =>
            "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var hasPun = HasType("Photon.Pun.PhotonNetwork");
            var isFirstStartup = SessionState.GetBool(IsFirstStartupKey, true);
            if (isFirstStartup && !hasPun)
            {
                SessionState.SetBool(IsFirstStartupKey, false);

                var pressedOk = EditorUtility.DisplayDialog("ODIN-Sample Requirements",
                    "The ODIN-Sample uses the \"Photon PUN 2 - FREE\" plugin for Multiplayer Synchronisation. Please install the plugin for free from the asset store to make the sample scenes work.",
                    "Go to Plugin Page", "Cancel");
                if (pressedOk) Application.OpenURL(PhotonAssetStoreUrl);
                else
                    Debug.LogError(
                        $"ODIN-Sample: No Photon installation detected. Please install the \"Photon PUN 2 - FREE\" plugin from the asset store to make the sample scenes work. Link to plugin: {PhotonAssetStoreUrl}");
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