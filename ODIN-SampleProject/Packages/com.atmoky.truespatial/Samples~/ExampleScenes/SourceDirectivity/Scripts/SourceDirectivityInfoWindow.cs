using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SourceDirectivityInfoWindow : MonoBehaviour
{
#if UNITY_EDITOR

    static public string infoText = "This scene showcases how to apply a directivity pattern to an audio source.\n\nAdditionally, the the source signal is also sent to an atmokyReceiver instance on the Reverb bus that feeds into a SFX Reverb plugin, such that the sound is more reverberant when the source faces away from the listener and more direct when the source faces towards the listener.";


    private void OnEnable()
    {
        if (!Application.isEditor)
        {
            Destroy(this);
        }
        SceneView.duringSceneGui += OnScene;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    void OnScene(SceneView scene)
    {
        var width = SceneView.currentDrawingSceneView.cameraViewport.width;
        var height = SceneView.currentDrawingSceneView.cameraViewport.height;

        var windowWidth = 250;
        var windowHeight = 180;
        var margin = 10;

        GUILayout.Window(0, new Rect(
               width - windowWidth - margin,
               height - windowHeight - margin,
               windowWidth,
               windowHeight
           ), ShowWindow, "");
    }

    private static void ShowWindow(int windowID)
    {
        GUIStyle textStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
        GUILayout.Label(infoText, textStyle);
    }

#endif
}
