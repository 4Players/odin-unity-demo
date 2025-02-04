using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Atmoky
{
    [EditorTool("Atmoky Occlusion Probe Tool", typeof(OcclusionProbeGroup))]
    public class OcclusionProbeGroupEditorTool : EditorTool
    {
        GUIContent m_Icon;
        public override GUIContent toolbarIcon => m_Icon;

        private List<int> probeSelection = new List<int>();

        private OcclusionProbeGroup group;

        private int previousNumberOfProbes = 0;

        void OnEnable()
        {
            Texture tex = (Texture)
                AssetDatabase.LoadAssetAtPath(
                    "Packages/com.atmoky.truespatial/Editor/Icons/Probes.tiff",
                    typeof(Texture)
                );
            m_Icon = new GUIContent(
                tex,
                "Occlusion Probe Group Tool - adjust the number and position of occlusion probes for the selected object."
            );

            group = target as OcclusionProbeGroup;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (!(window is SceneView sceneView))
                return;

            if (group.probes.Count != previousNumberOfProbes)
            {
                ClearSelection();
                previousNumberOfProbes = group.probes.Count;
            }

#if UNITY_EDITOR_OSX
            var modifier = Event.current.command;
#else
            var modifier = Event.current.control;
#endif

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A && modifier)
            {
                ClearSelection();
                for (var i = 0; i < group.probes.Count; i++)
                    SelectProbe(i);
                Event.current.Use();
            }

            updateProbes();
            drawProbeGroupGUI();
        }

        private void drawProbeGroupGUI()
        {
            Handles.BeginGUI();

#if UNITY_2022_1_OR_NEWER
            var width = SceneView.currentDrawingSceneView.cameraViewport.width;
            var height = SceneView.currentDrawingSceneView.cameraViewport.height;
#else
            var width = Screen.width / EditorGUIUtility.pixelsPerPoint;
            var height = Screen.height / EditorGUIUtility.pixelsPerPoint - 50;
#endif

            var windowWidth = 210;
            var windowHeight = 55;
            var margin = 10;

            GUILayout.Window(
                0,
                new Rect(
                    width - windowWidth - margin,
                    height - windowHeight - margin,
                    windowWidth,
                    windowHeight
                ),
                (id) =>
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Add Probe", GUILayout.Width(80)))
                    {
                        var index = group.AddProbe();
                        ClearSelection();
                        SelectProbe(index);
                        previousNumberOfProbes = group.probes.Count;
                    }

                    GUILayout.FlexibleSpace();

                    GUI.enabled = probeSelection.Count > 0;
                    if (GUILayout.Button("Remove Selected", GUILayout.Width(120)))
                    {
                        probeSelection.Sort();
                        for (var i = probeSelection.Count - 1; i >= 0; --i)
                        {
                            group.RemoveProbe(probeSelection[i]);
                            previousNumberOfProbes = group.probes.Count;
                        }
                        ClearSelection();
                    }

                    GUILayout.EndHorizontal();
                },
                "Atmoky Occlusion Probe Group"
            );
            Handles.EndGUI();
        }

        private void updateProbes()
        {
            var matrix = group.transform.localToWorldMatrix;

            var meanPosition = Vector3.zero;
            if (probeSelection.Count > 0)
            {
                foreach (var index in probeSelection)
                    meanPosition += group.probes[index].position;
                meanPosition /= probeSelection.Count;
                meanPosition = matrix.MultiplyPoint(meanPosition);
            }

            for (var i = 0; i < group.probes.Count; i++)
            {
                var probe = group.probes[i];
                var position = matrix.MultiplyPoint(probe.position);

                if (probeSelection.Contains(i))
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(position, meanPosition);
                }
                else
                    Handles.color = Color.white;

                Vector3 normal = Handles.inverseMatrix.MultiplyVector(Camera.current.transform.forward);
                Handles.DrawWireDisc(position, normal, 0.05f);


                if (Handles.Button(position, Quaternion.identity, 0.05f, 0.05f, Handles.SphereHandleCap))
                {
#if UNITY_EDITOR_OSX
                    if (!Event.current.shift && !Event.current.command)
                        ClearSelection();
#else
                    if (!Event.current.shift && !Event.current.control)
                        ClearSelection();
#endif
                    SelectProbe(i);
                }
            }

            if (probeSelection.Count == 0)
                return;

            Handles.color = Color.yellow;
            var newPosition = Handles.PositionHandle(meanPosition, Quaternion.identity);
            if (newPosition != meanPosition)
            {
                var delta = matrix.inverse.MultiplyPoint(newPosition) - matrix.inverse.MultiplyPoint(meanPosition);
                foreach (var index in probeSelection)
                    group.probes[index].position += delta;
            }
        }

        private void SelectProbe(int index)
        {
            if (!probeSelection.Contains(index))
                probeSelection.Add(index);
            else
                probeSelection.Remove(index);
        }

        private void ClearSelection()
        {
            probeSelection.Clear();
        }
    }

    public class OcclusionProbeGroupGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmo(OcclusionProbeGroup group, GizmoType gizmoType)
        {
            if (!(Selection.Contains(group.gameObject)))
                return;

            if (group.probes.Count == 0)
                return;

            var matrix = group.transform.localToWorldMatrix;

            foreach (var probe in group.probes)
            {
                var position = matrix.MultiplyPoint(probe.position);

                float occlusion = probe.occlusion;
                if (probe.IsOccluding())
                    Gizmos.color = new Color(1.0f, 1.0f - occlusion, 0.4f, 1);
                else
                    Gizmos.color = Color.green;

                Gizmos.DrawSphere(position, 0.05f);

                var referenceCamera = group.GetReferenceCamera();
                if (referenceCamera != null)
                    Gizmos.DrawLine(position, referenceCamera.transform.position);
            }
        }
    }


    [CustomEditor(typeof(OcclusionProbeGroup))]
    public class OcclusionProbeGroupEditor : Editor
    {
        SerializedProperty overrideSource;
        SerializedProperty useRaycastNonAlloc;
        SerializedProperty maxNumberOfHits;
        SerializedProperty occlusionLayerMask;
        SerializedProperty probes;
        SerializedProperty occlusionSensitivity;

        void OnEnable()
        {
            overrideSource = serializedObject.FindProperty("overrideSource");
            useRaycastNonAlloc = serializedObject.FindProperty("useRaycastNonAlloc");
            maxNumberOfHits = serializedObject.FindProperty("maxNumberOfHits");
            occlusionLayerMask = serializedObject.FindProperty("occlusionLayerMask");
            probes = serializedObject.FindProperty("probes");
            occlusionSensitivity = serializedObject.FindProperty("occlusionSensitivity");
        }

        public override void OnInspectorGUI()
        {
            OcclusionProbeGroup probeGroup = target as OcclusionProbeGroup;

            if (probeGroup.GetSource() == null)
            {
                EditorGUILayout.HelpBox("There's no Atmoky Source to control. Either add an Atmoky Source script to this GameObject or use the Source Override to define one.", MessageType.Error);
                if (GUILayout.Button(new GUIContent("Add AtmokySource component."), GUILayout.ExpandHeight(true)))
                {
                    probeGroup.gameObject.AddComponent<Source>();
                }
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(overrideSource);

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Raycasting", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(occlusionLayerMask);

            EditorGUILayout.PropertyField(useRaycastNonAlloc, new GUIContent("Use non-allocating raycast"));

            if (!useRaycastNonAlloc.boolValue)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.PropertyField(maxNumberOfHits);

            if (!useRaycastNonAlloc.boolValue)
            {
                GUI.enabled = true;
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Probes", EditorStyles.boldLabel);

            for (var i = 0; i < probes.arraySize; i++)
            {
                var probe = probes.GetArrayElementAtIndex(i);
                float occlusion = probe.FindPropertyRelative("occlusion").floatValue * 100;
                Vector3 pos = probe.FindPropertyRelative("position").vector3Value;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(5.0f, false);

                GUILayout.Label("Probe " + (i + 1).ToString() + ": ", GUILayout.Width(50));
                GUILayout.FlexibleSpace();

                GUI.enabled = false;
                GUILayout.Label(occlusion.ToString("0.0") + "%", GUILayout.Width(50));
                GUI.enabled = true;

                GUILayout.FlexibleSpace();

                var newPos = EditorGUILayout.Vector3Field("", probe.FindPropertyRelative("position").vector3Value);
                if (newPos != pos)
                {
                    probe.FindPropertyRelative("position").vector3Value = newPos;
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(30)))
                {
                    probeGroup.RemoveProbe(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Probe", GUILayout.Width(100)))
            {
                probeGroup.AddProbe();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Occlusion Sensitivity", EditorStyles.boldLabel);
            GUIStyle textStyle = EditorStyles.label;
            textStyle.wordWrap = true;
            GUILayout.Label("Defines how sensitive the final occlusion value is to the individual probe occlusion values.", textStyle);

            GUI.enabled = false;
            GUILayout.Label("Final Occlusion Value: " + (probeGroup.GetFinalOcclusionValue() * 100).ToString("0.0") + "%");
            GUI.enabled = true;

            EditorGUILayout.Slider(occlusionSensitivity, 0.0f, 100.0f, "");
            GUILayout.Space(-10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("min", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("mean", GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("max", GUILayout.Width(30));
            EditorGUILayout.Space(EditorGUIUtility.fieldWidth, false);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
