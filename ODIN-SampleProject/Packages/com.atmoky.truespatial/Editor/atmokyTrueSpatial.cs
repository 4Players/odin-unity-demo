using System;
using UnityEditor;
using UnityEngine;

public class atmokyRendererGUI : IAudioEffectPluginGUI
{
    public override string Name
    {
        get { return "atmokyRenderer"; }
    }

    public override string Description
    {
        get { return "Renderer GUI"; }
    }

    public override string Vendor
    {
        get { return "atmoky"; }
    }


    static float[] parameters = new float[2];

    public override bool OnGUI(IAudioEffectPlugin plugin)
    {
        var success = plugin.GetFloatBuffer("dump", out parameters, 2);
        if (!success)
        {
            EditorGUILayout.HelpBox("Couldn't retrieve plug-in state. Please restart the editor.", MessageType.Error);
            return false;
        }

        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);



        // renderer index
        float rendererIndex = parameters[0];

        int selectedRenderer = Mathf.RoundToInt(rendererIndex);
        int[] options = new int[101];
        GUIContent[] optionStrings = new GUIContent[101];
        for (int i = 0; i <= 100; ++i)
        {
            options[i] = i;
            optionStrings[i] = new GUIContent(i.ToString());
        }
        optionStrings[0] = new GUIContent("0 (Disabled)");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Renderer Index", "Index of this renderer instance. All audio sources with the same index will be rendered by this renderer."));
        int newselected = EditorGUILayout.IntPopup(selectedRenderer, optionStrings, options);

        if (GUILayout.Button("Next available renderer"))
        {
            float[] nextRenderer = new float[1];
            bool found = plugin.GetFloatBuffer("NextRendererIndex", out nextRenderer, 1);

            newselected = Mathf.RoundToInt(nextRenderer[0]);
        }
        if (newselected != selectedRenderer)
        {
            plugin.SetFloatParameter("Renderer Index", newselected);
        }

        EditorGUILayout.EndHorizontal();

        { // passthrough
            float passthrough = parameters[1];
            bool previousPassthrough = passthrough > 0.0f;
            bool newPassthrough = EditorGUILayout.Toggle(new GUIContent("Input Passthrough", "Lets the input pass through to the output"),
                previousPassthrough);
            if (newPassthrough != previousPassthrough)
            {
                plugin.SetFloatParameter("Passthrough", newPassthrough ? 1.0f : 0.0f);
            }
        }

        EditorGUILayout.Separator();


        GUILayout.Space(5f);
        Rect r = GUILayoutUtility.GetRect(200, 30, GUILayout.ExpandWidth(true));

        GUILayout.Space(5f);

        // atmoky pentatope
        Handles.matrix = Matrix4x4.TRS(r.min, Quaternion.identity, new Vector3(0.25f, 0.25f, 0.25f));
        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(0, 0, 0);
        points[1] = new Vector3(75, 23, 0);
        points[2] = new Vector3(35, 98, 0);
        points[3] = new Vector3(0, 0, 0);

        Handles.color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        points[1] = new Vector3(49, 21, 0);
        Handles.color = new Vector4(0.6f, 0.4f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        points[1] = new Vector3(33, 47, 0);
        Handles.color = new Vector4(0.6f, 0.0f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        points[1] = new Vector3(75, 23, 0);
        points[2] = new Vector3(49, 21, 0);
        Handles.color = new Vector4(0.4f, 0.0f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        points[0] = new Vector3(33, 47, 0);
        points[1] = new Vector3(75, 23, 0);
        points[2] = new Vector3(35, 98, 0);
        points[3] = new Vector3(33, 47, 0);
        Handles.color = new Vector4(0.4f, 0.0f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        points[0] = new Vector3(33, 47, 0);
        points[1] = new Vector3(49, 21, 0);
        points[2] = new Vector3(35, 98, 0);
        points[3] = new Vector3(33, 47, 0);
        Handles.color = new Vector4(0.8f, 0.6f, 1.0f, 1.0f);
        Handles.DrawAAConvexPolygon(points);

        return false;
    }
}


public class atmokyReceiverGUI : IAudioEffectPluginGUI
{
    public override string Name
    {
        get { return "atmokyReceiver"; }
    }

    public override string Description
    {
        get { return "Receiver GUI"; }
    }

    public override string Vendor
    {
        get { return "atmoky"; }
    }


    static float[] parameters = new float[1];

    public override bool OnGUI(IAudioEffectPlugin plugin)
    {
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

        plugin.GetFloatBuffer("dump", out parameters, 1);

        // receiver index
        float receiverIndex = parameters[0];

        int selectedReceiver = Mathf.RoundToInt(receiverIndex);
        int[] options = new int[100];
        GUIContent[] optionStrings = new GUIContent[100];
        for (int i = 0; i < 100; ++i)
        {
            options[i] = i + 1;
            optionStrings[i] = new GUIContent((i + 1).ToString());
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Receiver Index", "Index of this receiver instance."));
        int newselected = EditorGUILayout.IntPopup(selectedReceiver, optionStrings, options);

        if (GUILayout.Button("Next available Receiver"))
        {
            float[] nextReceiver = new float[1];
            bool found = plugin.GetFloatBuffer("NextReceiverIndex", out nextReceiver, 1);

            newselected = Mathf.RoundToInt(nextReceiver[0]);
        }
        if (newselected != selectedReceiver)
        {
            plugin.SetFloatParameter("Receiver Index", newselected);
        }

        EditorGUILayout.EndHorizontal();

        return false;
    }
}


public class atmokyExternalizerGUI : IAudioEffectPluginGUI
{
    public override string Name
    {
        get { return "atmokyExternalizer"; }
    }

    public override string Description
    {
        get { return "Externalizer GUI"; }
    }

    public override string Vendor
    {
        get { return "atmoky"; }
    }


    static float[] parameters = new float[2];

    public override bool OnGUI(IAudioEffectPlugin plugin)
    {
        plugin.GetFloatBuffer("dump", out parameters, 2);

        // externalizer amount
        float extAmount = parameters[0];
        float extAmountMin, extAmountMax, extAmountDef;
        plugin.GetFloatParameterInfo("Amount", out extAmountMin, out extAmountMax, out extAmountDef);
        float updatedExtAmount = EditorGUILayout.Slider(new GUIContent("Amount", "Intensity of the Externalizer. 0 will bypass the Externalizer engine."), extAmount, extAmountMin, extAmountMax);
        if (updatedExtAmount != extAmount)
        {
            plugin.SetFloatParameter("Amount", updatedExtAmount);
        }

        // externalizer character
        float extCharacter = parameters[1];
        float extCharacterMin, extCharacterMax, extCharacterDef;
        plugin.GetFloatParameterInfo("Character", out extCharacterMin, out extCharacterMax, out extCharacterDef);
        float updatedExtCharacter = EditorGUILayout.Slider(new GUIContent("Character", "Character of the Externalizer. 0 = warmer; 100 = airy"), extCharacter, extCharacterMin, extCharacterMax);
        if (updatedExtCharacter != extCharacter)
        {
            plugin.SetFloatParameter("Character", updatedExtCharacter);
        }

        return false;
    }
}
