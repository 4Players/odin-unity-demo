using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OdinNative.Unity.Audio;

public class SimplePushToTalk : MonoBehaviour
{
    public string RoomName;
    [SerializeField]
    public KeyCode PushToTalkHotkey;
    public bool UsePushToTalk = true;
    public MicrophoneReader AudioSender;

    private void Reset()
    {
        RoomName = "default";
        PushToTalkHotkey = KeyCode.C;
        UsePushToTalk = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (AudioSender == null) 
            AudioSender = FindObjectOfType<MicrophoneReader>();

        OdinHandler.Instance.JoinRoom(RoomName);
    }

    // Update is called once per frame
    void Update()
    {
        if (AudioSender)
            AudioSender.RedirectCapturedAudio = UsePushToTalk ? Input.GetKey(PushToTalkHotkey) : true;
    }
}
