#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SetAmbisonicsDecoder : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        var audioManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/AudioManager.asset");
        var serializedObject = new SerializedObject(audioManager);
        serializedObject.Update();
        var ambisonicDecoderProperty = serializedObject.FindProperty("m_AmbisonicDecoderPlugin");

        if (ambisonicDecoderProperty.stringValue != "atmokyAmbisonicDecoder")
        {
            ambisonicDecoderProperty.stringValue = "atmokyAmbisonicDecoder";
            serializedObject.ApplyModifiedProperties();

            Debug.Log("Ambisonic decoder plugin set to atmokyAmbisonicDecoder.");
        }
    }
#endif
}
