using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class NearfieldEffectsInfoWindow : MonoBehaviour
{
#if UNITY_EDITOR

    static public string infoText = "This scene showcases acoustic nearfield effects.\n\nWhen a source is close to the listener (the start distance can be controlled using the Atmoky Source script) the gain, the low-frequency energy, and the interaural level differnces are altered to simulate these effects. ";


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

        var windowWidth = 240;
        var windowHeight = 170;
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
