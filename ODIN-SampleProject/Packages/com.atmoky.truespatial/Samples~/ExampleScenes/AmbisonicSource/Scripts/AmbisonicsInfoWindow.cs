using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class AmbisonicsInfoWindow : MonoBehaviour
{
#if UNITY_EDITOR

    static public string infoText = "This scene showcases how to render a first-order Ambisonic sound bed.\n\nIn this scene, the distance-based volume rolloff is configured such that the Ambisonic bed is only rendered when the player is inside the dome - so move (WASD or arrow keys) inside to here it in action.";


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
        var windowHeight = 160;
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
