using System;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    [RequireComponent(typeof(MicrophoneReader))]
    public class OdinPushToTalk : MonoBehaviour
    {
        [SerializeField] private string pushToTalkButton = "PushToTalk";

        private MicrophoneReader _microphoneReader;
        

        private void Awake()
        {
            _microphoneReader = GetComponent<MicrophoneReader>();
            Assert.IsNotNull(_microphoneReader);
        }

        private void Update()
        {
            if (_microphoneReader)
                _microphoneReader.RedirectCapturedAudio = Input.GetButton(pushToTalkButton);
        }
    }
}
