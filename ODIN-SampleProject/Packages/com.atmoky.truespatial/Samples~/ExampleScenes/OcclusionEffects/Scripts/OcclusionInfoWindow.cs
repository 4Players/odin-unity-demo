using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class OcclusionInfoWindow : MonoBehaviour
{
#if UNITY_EDITOR

    static public string infoText = "This scene showcases automatic and smooth occlusion computation using the Atmoky Occlusion Probe Group component.\n\nWhen adding this component to an audio source, a resulting occlusion value is automatically computed and applied for the source by casting rays between all occlusion probes and the Audio Listener.\n\nAll game objects that should act as occluders need to have an Atmoky Occluder component that defines the amount of occlusion that is added by the game object.";


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

        var windowWidth = 280;
        var windowHeight = 220;
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
