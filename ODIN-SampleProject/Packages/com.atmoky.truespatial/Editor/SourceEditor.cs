using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System.Linq;

namespace Atmoky
{
    [EditorTool("Directivity Tool", typeof(Atmoky.Source))]
    class AtmokyDirectivityTool : EditorTool
    {
        GUIContent m_Icon;

        public override GUIContent toolbarIcon => m_Icon;

        private void OnEnable()
        {
            Texture tex = (Texture)AssetDatabase.LoadAssetAtPath("Packages/com.atmoky.truespatial/Editor/Icons/Directivity.tiff", typeof(Texture));
            m_Icon = new GUIContent(tex, "Directivity Tool - adjust directivity settings for the selected Atmoky Source.");

            InnerHandles = new ArcHandle[targets.Count()];
            OuterHandles = new ArcHandle[targets.Count()];

            for (int i = 0; i < targets.Count(); i++)
            {
                InnerHandles[i] = new ArcHandle();
                OuterHandles[i] = new ArcHandle();

                InnerHandles[i].radius = 1;
                InnerHandles[i].fillColor = new Color(0.0f, 1.0f, 0.0f, 0.0f);
                OuterHandles[i].radius = 1;
                OuterHandles[i].fillColor = new Color(0.0f, 1.0f, 0.0f, 0.0f);
            }
        }

        private void OnDisable()
        {
            m_Icon = null;

            InnerHandles = null;
            OuterHandles = null;
        }

        [Shortcut("Activate atmoky Directivity Tool", typeof(SceneView), KeyCode.D)]
        static void Shortcut()
        {
            if (Selection.GetFiltered<Source>(SelectionMode.TopLevel).Length > 0)
                ToolManager.SetActiveTool<AtmokyDirectivityTool>();
        }

        ArcHandle[] InnerHandles;
        ArcHandle[] OuterHandles;


        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView sceneView))
                return;

            var i = 0;

            foreach (var obj in targets)
            {
                if (obj is not Source source)
                    continue;

                var InnerHandle = InnerHandles[i];
                var OuterHandle = OuterHandles[i];

                InnerHandle.angle = source.innerAngle / 2;
                OuterHandle.angle = source.outerAngle / 2;

                Vector3 handleDirection = source.transform.forward;
                Vector3 handleNormal = Vector3.Cross(handleDirection, Vector3.up);
                Matrix4x4 handleMatrix = Matrix4x4.TRS(
                    source.transform.position,
                    Quaternion.LookRotation(handleDirection, handleNormal),
                    Vector3.one
                );


                using (new Handles.DrawingScope(handleMatrix))
                {
                    // sphere
                    Handles.color = new Color(0.0f, 0.0f, 0.0f, 0.4f);
                    const int numCircles = 64;
                    for (var j = 0; j < numCircles; ++j)
                    {
                        var y = 1.0f - 2.0f * j / numCircles;
                        var radius = Mathf.Sqrt(1 - y * y);
                        Handles.DrawWireDisc((1.0f - 2.0f * j / numCircles) * Vector3.forward, Vector3.forward, radius);
                    }

                    // inner handle
                    Handles.color = Color.green;
                    DrawWireArc(source.innerAngle / 2, source.transform);

                    EditorGUI.BeginChangeCheck();
                    InnerHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(source, "Change Inner Angle");
                        var newAngle = Math.Clamp(InnerHandle.angle * 2, 0, 360);
                        source.innerAngle = newAngle;

                        if (source.innerAngle > source.outerAngle)
                        {
                            source.outerAngle = source.innerAngle;
                            OuterHandle.angle = InnerHandle.angle;
                        }
                    }

                    // outer handle
                    Handles.color = Color.blue;
                    DrawWireArc(source.outerAngle / 2, source.transform);
                    EditorGUI.BeginChangeCheck();
                    OuterHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(source, "Change Outer Angle");
                        var newAngle = Math.Clamp(OuterHandle.angle * 2, 0, 360);
                        source.outerAngle = newAngle;

                        if (source.innerAngle > source.outerAngle)
                        {
                            source.innerAngle = source.outerAngle;
                            InnerHandle.angle = OuterHandle.angle;
                        }
                    }


                    var target = -source.outerGain * Vector3.forward;
                    Handles.DrawLine(Vector3.zero, target);
                    float size = HandleUtility.GetHandleSize(target) * 0.1f;
                    EditorGUI.BeginChangeCheck();
                    var newPosition = Handles.Slider(target, -Vector3.forward, size, Handles.CubeHandleCap, 0.0f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(source, "Change Outer Gain");
                        var mag = Vector3.Dot(newPosition, -Vector3.forward);
                        source.outerGain = Mathf.Clamp(mag, 0.0f, 1.0f);
                    }

                }
                ++i;
            }
        }

        static void DrawWireArc(float angle, Transform transform)
        {

            var rot = Quaternion.AngleAxis(angle, Vector3.up);
            var vertex = rot * Vector3.forward;

            Handles.DrawWireArc(Vector3.zero, Vector3.forward, vertex, 360, vertex.magnitude, 1.0f);
        }

    }

    public class SourceGizmoDrawer
    {
        readonly static Vector3[] Points = null;
        readonly static Vector3[] ScaledPoints = null;

        static SourceGizmoDrawer()
        {
            const int nPoints = 64;
            Points = new Vector3[nPoints];
            ScaledPoints = new Vector3[nPoints];

            const float dPhi = Mathf.PI / (nPoints - 1);

            for (var i = 0; i < nPoints; ++i)
            {
                float phi = i * dPhi;
                var z = Mathf.Cos(phi);
                var x = Mathf.Sin(phi);
                var y = 0.0f;
                var vertex = new Vector3(x, y, z);
                Points[i] = vertex;
                ScaledPoints[i] = vertex;
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmoForSource(Source source, GizmoType gizmoType)
        {
            Vector3 position = source.transform.position;

            const int numRotations = 128;

            for (var j = 0; j < Points.Length; ++j)
            {
                float angle = Vector3.Angle(Points[j], Vector3.forward);
                ScaledPoints[j] = Points[j] * GetDirectivityValue(source, angle);
            }

            Gizmos.color = Color.red;
            for (var i = 0; i < numRotations; ++i)
            {
                var rot = Quaternion.AngleAxis(360.0f / numRotations * i, Vector3.forward);
                Gizmos.matrix = Matrix4x4.TRS(position, source.transform.rotation * rot, Vector3.one);

#if UNITY_2022_1_OR_NEWER
                Gizmos.DrawLineStrip(ScaledPoints, false);
#else
                for (var j = 0; j < ScaledPoints.Length - 1; ++j)
                {
                    Gizmos.DrawLine(ScaledPoints[j], ScaledPoints[j + 1]);
                }
#endif
            }
        }

        public static float GetDirectivityValue(Source source, float angle)
        {
            float innerAngleHalf = source.innerAngle * 0.5f;
            float outerAngleHalf = source.outerAngle * 0.5f;

            var vertexGain = 1.0f;
            var vertexLowpass = 0.0f;
            if (angle > innerAngleHalf && angle < outerAngleHalf)
            {
                // NOTE: limit to t < 1.0 because DrawAAPolyLine anti aliasing is weird
                vertexGain = Mathf.Lerp(1.0f, source.outerGain, Mathf.Min((angle - innerAngleHalf) / (outerAngleHalf - innerAngleHalf), 0.9975f));
                vertexLowpass = Mathf.Lerp(0.0f, source.outerLowpass, (angle - innerAngleHalf) / (outerAngleHalf - innerAngleHalf));
            }
            else if (angle >= outerAngleHalf)
            {
                vertexGain = source.outerGain;
                vertexLowpass = source.outerLowpass;
            }

            var vertexHighcut = 1.0f - vertexLowpass;
            vertexHighcut = (vertexHighcut * 0.4f) + 0.6f;

            var gainLinear = vertexGain * vertexHighcut;
            var gainDb = 20.0f * Mathf.Log10(gainLinear);
            const float dbRange = 20.0f;

            return Mathf.Clamp01((gainDb + dbRange) / dbRange);
        }
    }


    class DirectivityPreset
    {
        public string name;
        public float innerAngle;
        public float outerAngle;
        public float outerGain;
        public float outerLowpass;
    }
}

[CustomEditor(typeof(Atmoky.Source))]
[CanEditMultipleObjects]
public class SourceEditor : Editor
{
    static readonly int[] rendererIndexOptions;
    static readonly GUIContent[] rendererIndexOptionStrings;

    static readonly int[] receiverIndexOptions;
    static readonly GUIContent[] receiverIndexOptionStrings;

    static readonly int[] sendLevelSourceOptions;
    static readonly GUIContent[] sendLevelSourceOptionStrings;


    static SourceEditor()
    {
        rendererIndexOptions = new int[101];
        rendererIndexOptionStrings = new GUIContent[101];
        rendererIndexOptions[0] = 0;
        rendererIndexOptionStrings[0] = new GUIContent("Binaural");
        for (int i = 1; i < 101; ++i)
        {
            rendererIndexOptions[i] = i;
            rendererIndexOptionStrings[i] = new GUIContent($"Send to Renderer/{i}");
        }

        receiverIndexOptions = new int[101];
        receiverIndexOptionStrings = new GUIContent[101];
        receiverIndexOptions[0] = 0;
        receiverIndexOptionStrings[0] = new GUIContent("No Send");
        for (int i = 1; i < 101; ++i)
        {
            receiverIndexOptions[i] = i;
            receiverIndexOptionStrings[i] = new GUIContent(i.ToString());
        }

        sendLevelSourceOptions = new int[2];
        sendLevelSourceOptionStrings = new GUIContent[2];
        sendLevelSourceOptions[0] = 0;
        sendLevelSourceOptionStrings[0] = new GUIContent("Parameter");
        sendLevelSourceOptions[1] = 1;
        sendLevelSourceOptionStrings[1] = new GUIContent("Reverb Zone Mix");

        const int nPoints = 64 * 10;
        points = new Vector3[nPoints];
        scaledPoints = new Vector3[nPoints];

        const float dPhi = 2 * Mathf.PI / (nPoints - 1);

        for (var i = 0; i < nPoints; ++i)
        {
            float phi = i * dPhi;
            var x = Mathf.Sin(phi);
            var y = Mathf.Cos(phi);
            var vertex = new Vector3(y, x, 0.0f);
            points[i] = vertex;
            scaledPoints[i] = vertex;
        }
    }

    SerializedProperty rendererIndex;
    SerializedProperty occlusion;

    SerializedProperty receiverIndex;
    SerializedProperty sendLevelSrc;
    SerializedProperty sendLevel;

    SerializedProperty innerAngle;
    SerializedProperty outerAngle;
    SerializedProperty outerGain;
    SerializedProperty outerLowpass;

    SerializedProperty nfeDistance;
    SerializedProperty nfeGain;
    SerializedProperty nfeBassBoost;

    Atmoky.DirectivityPreset[] directivityPresets = new Atmoky.DirectivityPreset[]
{
        new Atmoky.DirectivityPreset
        {
            name = "Omni",
            innerAngle = 360.0f,
            outerAngle = 360.0f,
            outerGain = 1.0f,
            outerLowpass = 0.0f
        },
        new Atmoky.DirectivityPreset
        {
            name = "Cardioid",
            innerAngle = 0.0f,
            outerAngle = 360.0f,
            outerGain = 0.0f,
            outerLowpass = 0.0f
        },
        new Atmoky.DirectivityPreset
        {
            name = "Voice",
            innerAngle = 130.0f,
            outerAngle = 300f,
            outerGain = 0.7f,
            outerLowpass = 0.4f
        },
        new Atmoky.DirectivityPreset
        {
            name = "Loudspeaker",
            innerAngle = 90f,
            outerAngle = 360.0f,
            outerGain = 0.6f,
            outerLowpass = 0.3f
        },
        new Atmoky.DirectivityPreset
        {
            name = "NarrowBeam",
            innerAngle = 45.0f,
            outerAngle = 180.0f,
            outerGain = 0.0f,
            outerLowpass = 1.0f
        },
        new Atmoky.DirectivityPreset
        {
            name = "WideBeam",
            innerAngle = 60.0f,
            outerAngle = 270.0f,
            outerGain = 0.1f,
            outerLowpass = 0.5f
        }
};

    void OnEnable()
    {
        rendererIndex = serializedObject.FindProperty("rendererIndex");
        occlusion = serializedObject.FindProperty("occlusion");

        receiverIndex = serializedObject.FindProperty("receiverIndex");
        sendLevelSrc = serializedObject.FindProperty("sendLevelSrc");
        sendLevel = serializedObject.FindProperty("sendLevel");

        innerAngle = serializedObject.FindProperty("innerAngle");
        outerAngle = serializedObject.FindProperty("outerAngle");
        outerGain = serializedObject.FindProperty("outerGain");
        outerLowpass = serializedObject.FindProperty("outerLowpass");

        nfeDistance = serializedObject.FindProperty("nfeDistance");
        nfeGain = serializedObject.FindProperty("nfeGain");
        nfeBassBoost = serializedObject.FindProperty("nfeBassBoost");
    }

    public override void OnInspectorGUI()
    {
        Atmoky.Source source = target as Atmoky.Source;
        AudioSource audioSource = source.GetComponent<AudioSource>();

        if (audioSource != null && audioSource.clip != null &&
                                 audioSource.clip.ambisonic)
        {
            EditorGUILayout.HelpBox("It seems you are using an Ambisonic audio clip. For that use the AtmokyAmbisonicSource script.", MessageType.Error);
        }

        if (AudioSettings.GetSpatializerPluginName() != "atmokySpatializer")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("The atmokySpatializer is not set as the active spatializer. Please go to the audio project settings and set it as the active spatializer. ", MessageType.Error);
            if (GUILayout.Button(new GUIContent("Set atmoky Spatializer", "Sets the atmokySpatializer plug-in as the native spatializer of audio sources for this project."), GUILayout.ExpandHeight(true)))
            {
                var audioManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/AudioManager.asset");
                var serializedObject = new SerializedObject(audioManager);
                serializedObject.Update();
                var spatializerProperty = serializedObject.FindProperty("m_SpatializerPlugin");
                spatializerProperty.stringValue = "atmokySpatializer";
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (audioSource.spatialBlend != 1.0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("The AudioSource's spatial blend property isn't set fully to 3D. ", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Set to 3D", "Sets the AudioSource's spatial blend property to 3D."), GUILayout.ExpandHeight(true)))
            {
                audioSource.spatialBlend = 1.0f;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (audioSource.spatialize == false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("The AudioSource's Spatialize setting is not enabled. Binaural rendering is disabled for this source.", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Enable spatialization", "Enables the AudioSource's spatialization setting."), GUILayout.ExpandHeight(true)))
            {
                audioSource.spatialize = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.Update();

        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

        int selectedRenderer = rendererIndex.intValue;
        int newselectedRenderer = EditorGUILayout.IntPopup(new GUIContent("Output Mode", "Switch between direct binaural rendering or sending to a bulk renderer with the specified index."), selectedRenderer, rendererIndexOptionStrings, rendererIndexOptions);
        if (newselectedRenderer != selectedRenderer)
        {
            rendererIndex.intValue = newselectedRenderer;
        }



        EditorGUILayout.PropertyField(occlusion);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Send Configuration", EditorStyles.boldLabel);

        int selectedReceiver = receiverIndex.intValue;
        int newselectedReceiver = EditorGUILayout.IntPopup(new GUIContent("Receiver Index", "Index of a receiver instance to send the dry audio to."), selectedReceiver, receiverIndexOptionStrings, receiverIndexOptions);
        if (newselectedReceiver != selectedReceiver)
        {
            receiverIndex.intValue = newselectedReceiver;
        }

        int selectedSendLevelSource = sendLevelSrc.intValue;
        int newSelectedSendLevelSource = EditorGUILayout.IntPopup(new GUIContent("Send Level Source", "Parameter source to use for the send level."), selectedSendLevelSource, sendLevelSourceOptionStrings, sendLevelSourceOptions);
        if (newSelectedSendLevelSource != selectedSendLevelSource)
        {
            sendLevelSrc.intValue = newSelectedSendLevelSource;
        }


        if (selectedSendLevelSource == 0)
        {
            EditorGUILayout.PropertyField(sendLevel);
        }

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Directivity Settings", EditorStyles.boldLabel);

        void OnDirectivityPresetSelected(object preset)
        {
            Atmoky.DirectivityPreset selectedPreset = (Atmoky.DirectivityPreset)preset;

            innerAngle.floatValue = selectedPreset.innerAngle;
            outerAngle.floatValue = selectedPreset.outerAngle;
            outerGain.floatValue = selectedPreset.outerGain;
            outerLowpass.floatValue = selectedPreset.outerLowpass;

            serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(innerAngle);
        if (EditorGUI.EndChangeCheck() && innerAngle.floatValue > outerAngle.floatValue)
            outerAngle.floatValue = innerAngle.floatValue;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(outerAngle);
        if (EditorGUI.EndChangeCheck() && outerAngle.floatValue < innerAngle.floatValue)
            innerAngle.floatValue = outerAngle.floatValue;

        EditorGUILayout.PropertyField(outerGain);
        EditorGUILayout.PropertyField(outerLowpass);

        EditorGUILayout.BeginHorizontal();

        DrawDirectivity();

        GUILayout.Space(20);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Directivity Presets", GUILayout.MinWidth(130), GUILayout.MaxWidth(250)))
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < directivityPresets.Length; ++i)
            {
                Atmoky.DirectivityPreset preset = directivityPresets[i];
                menu.AddItem(new GUIContent(preset.name), false, OnDirectivityPresetSelected, preset);
            }
            menu.ShowAsContext();
        }

        GUILayout.Space(50);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Nearfield Effects", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(nfeDistance, new GUIContent("Start Distance"));
        EditorGUILayout.PropertyField(nfeGain, new GUIContent("Overall Gain"));
        EditorGUILayout.PropertyField(nfeBassBoost, new GUIContent("Bass Boost"));

        serializedObject.ApplyModifiedProperties();
    }

    private static readonly Vector3[] points = null;
    static private readonly Vector3[] scaledPoints = null;

    private void DrawDirectivity()
    {
        Rect rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));

        if (Event.current.type != EventType.Repaint)
            return;

        GUI.BeginClip(rect);

        float offset = rect.width * 0.5f;
        Handles.matrix = Matrix4x4.TRS(new Vector3(offset, offset, 0), Quaternion.identity, 0.95f * offset * Vector3.one);
        DrawSolidArc(outerAngle.floatValue, new Color(0.25f, 0.35f, 0.61f));
        DrawSolidArc(innerAngle.floatValue, new Color(0.35f, 0.61f, 0.35f));

        Handles.color = Color.gray;
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 1.0f);

        for (var i = 0; i < points.Length; ++i)
        {
            float angle = Vector3.Angle(points[i], Vector3.down);
            float value = Atmoky.SourceGizmoDrawer.GetDirectivityValue(target as Atmoky.Source, angle);
            scaledPoints[i] = value * points[i];
        }

        Handles.color = Color.red;
        Handles.DrawAAPolyLine(3.0f, scaledPoints);
        Handles.matrix = Matrix4x4.identity;

        GUI.EndClip();
    }

    private void DrawSolidArc(float angleInDegrees, Color color)
    {
        float angleInRad = angleInDegrees * Mathf.Deg2Rad;

        Handles.color = color;
        Handles.DrawSolidArc(Vector3.zero, new Vector3(0, 0, 1), new Vector3(-Mathf.Sin(angleInRad / 2), -Mathf.Cos(angleInRad / 2), 0), angleInDegrees, 1.0f);
    }
}
