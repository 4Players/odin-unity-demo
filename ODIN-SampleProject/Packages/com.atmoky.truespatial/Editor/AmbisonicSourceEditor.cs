using UnityEngine;
using UnityEditor;

namespace Atmoky
{

    public class AmbisonicSourceGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmoForAtmokySource(AmbisonicSource source, GizmoType gizmoType)
        {
            GUIStyle style = new()
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Gizmos.matrix = source.transform.localToWorldMatrix;
            Handles.matrix = source.transform.localToWorldMatrix;

            Gizmos.DrawWireSphere(new Vector3(0, 0, 0), 1.0f);

            float radius = 1.2f;

            Handles.Label(radius * Vector3.up, "Up", style);
            Handles.Label(-radius * Vector3.up, "Down", style);
            Handles.Label(radius * Vector3.right, "Right", style);
            Handles.Label(-radius * Vector3.right, "Left", style);
            Handles.Label(radius * Vector3.forward, "Front", style);
            Handles.Label(-radius * Vector3.forward, "Back", style);
        }
    }


    [CustomEditor(typeof(AmbisonicSource))]
    [CanEditMultipleObjects]
    public class AmbisonicSourceEditor : Editor
    {
        SerializedProperty headlocked;

        void OnEnable()
        {
            headlocked = serializedObject.FindProperty("headlocked");
        }

        public override void OnInspectorGUI()
        {
            AmbisonicSource source = target as AmbisonicSource;
            AudioSource audioSource = source.GetComponent<AudioSource>();

            if (audioSource != null && audioSource.clip != null &&
                                     !audioSource.clip.ambisonic)
            {
                EditorGUILayout.HelpBox("It seems you are using a non-Ambisonic audio clip. For that use the Atmoky Source script.", MessageType.Error);
            }

            var audioManager = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/AudioManager.asset");
            var serializedObject = new SerializedObject(audioManager);
            serializedObject.Update();
            var ambisonicDecoderProperty = serializedObject.FindProperty("m_AmbisonicDecoderPlugin");

            if (ambisonicDecoderProperty.stringValue != "atmokyAmbisonicDecoder")
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("The atmokyAmbisonicDecoder is not set as the active ambisonic decoder plugin. Please go to the audio project settings and set it as the active ambisonic decoder.", MessageType.Error);
                if (GUILayout.Button(new GUIContent("Set atmoky AmbisonicDecoder", "Sets the atmokyAmbisonicDecoder plug-in as the native decoder of Ambisonic audio sources for this project."), GUILayout.ExpandHeight(true)))
                {
                    ambisonicDecoderProperty.stringValue = "atmokyAmbisonicDecoder";
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(headlocked);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
