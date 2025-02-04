#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ManySourcesSetSpatializerPlugin : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        var availableSpatializerPlugins = AudioSettings.GetSpatializerPluginNames();

        bool atmokySpatializerAvailable = false;
        foreach (var plugin in availableSpatializerPlugins)
        {
            if (plugin.Contains("atmokySpatializer"))
            {
                atmokySpatializerAvailable = true;
                break;
            }
        }

        if (!atmokySpatializerAvailable)
        {
            Debug.LogError("atmokySpatializer is not available.");
            return;
        }

        AudioSettings.SetSpatializerPluginName("atmokySpatializer");
    }
#endif
}
