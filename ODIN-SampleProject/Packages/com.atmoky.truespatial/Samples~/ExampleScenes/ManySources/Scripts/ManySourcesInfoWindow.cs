using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ManySourcesInfoWindow : MonoBehaviour
{
#if UNITY_EDITOR

    static public string infoText = "This scene features a large number of spatialized audio sources that can be efficiently rendered using atmoky trueSpatial.";


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

        var windowWidth = 220;
        var windowHeight = 100;
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
