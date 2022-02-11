﻿#if UNITY_EDITOR
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEditor;

namespace OdinNative.Unity.UIEditor
{
    /// <summary>
    /// Adds a custom layout to the MicrophoneReader component
    /// </summary>
    [CustomEditor(typeof(MicrophoneReader))]
    public class MicrophoneReaderEditor : Editor
    {
        SerializedProperty CaptureAudio;
        SerializedProperty RecordingLoop;

        SerializedProperty AudioClipLength;
        SerializedProperty OverrideSampleRate;
        SerializedProperty SampleRate;
        SerializedProperty StartListener;

        SerializedProperty LoopbackTestRunner;

        private bool toggleMicSettings;
        private bool toggleShowTest;

        private GUIStyle FoldoutLabelStyle;

        void OnEnable()
        {
            CaptureAudio = serializedObject.FindProperty("RedirectCapturedAudio");
            RecordingLoop = serializedObject.FindProperty("ContinueRecording");

            AudioClipLength = serializedObject.FindProperty("AudioClipLength");
            OverrideSampleRate = serializedObject.FindProperty("OverrideSampleRate");
            SampleRate = serializedObject.FindProperty("SampleRate");
            StartListener = serializedObject.FindProperty("AutostartListen");

            LoopbackTestRunner = serializedObject.FindProperty("Loopback");
        }

        /// <summary>
        /// Implementation for the Unity custom inspector of MicrophoneReader.
        /// </summary>
        public override void OnInspectorGUI()
        {
            changeStyles();
            MicrophoneReader micReader = (target as MicrophoneReader);
            if (micReader == null)
            {
                DrawDefaultInspector(); // fallback
                return;
            }

            EditorGUILayout.PropertyField(CaptureAudio, new GUIContent("Redirect Captured Audio", "Redirect the captured audio to all rooms."));
            EditorGUILayout.PropertyField(RecordingLoop, new GUIContent("Continue Recording", "Indicates whether the recording should continue recording if AudioClipLength is reached, and wrap around and record from the beginning of the AudioClip."));
            GUILayout.Space(10);
            CreateMicSettingsLayout();
            if (toggleShowTest)
            {
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(RecordingLoop, new GUIContent("Loopback Test", "Start/Stop Audio- Loopback"));
            }
            serializedObject.ApplyModifiedProperties();

        }

        private void changeStyles()
        {
            FoldoutLabelStyle = new GUIStyle(EditorStyles.foldout);
            FoldoutLabelStyle.fontStyle = FontStyle.Bold;
            FoldoutLabelStyle.fontSize = 14;
        }

        private static void drawRect(int height)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            GUILayout.Space(3);

        }

        private void CreateMicSettingsLayout()
        {
            toggleMicSettings = EditorGUILayout.Foldout(toggleMicSettings, "Mic-AudioClip Settings", FoldoutLabelStyle);
            drawRect(2);
            if (toggleMicSettings)
            {
                EditorGUILayout.PropertyField(AudioClipLength, new GUIContent("AudioClip length", "Set the length of the AudioClip produced by the recording"));
                EditorGUILayout.PropertyField(OverrideSampleRate, new GUIContent("Override SampleRate", "Enable, to override \"Capture device sample rate\" from Odin Editor Config for this Gameobject"));
                if (OverrideSampleRate.boolValue)
                    EditorGUILayout.PropertyField(SampleRate, new GUIContent("SampleRate", "The new SampleRate to use for microphone capture"));
                EditorGUILayout.PropertyField(StartListener, new GUIContent("Autostart Microphone-Listen", "Automatical microphone start on Start()"));
            }
        }
    }
}
#endif
