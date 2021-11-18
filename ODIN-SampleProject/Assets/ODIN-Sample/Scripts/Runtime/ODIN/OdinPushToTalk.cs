using System;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    public class OdinPushToTalk : MonoBehaviour
    {
        [SerializeField] private OdinPushToTalkData[] pushToTalkSettings;

        private void Update()
        {
            if (!(OdinHandler.Instance && null != OdinHandler.Instance.Rooms))
                return;
            
            foreach (OdinPushToTalkData pushToTalkData in pushToTalkSettings)
            {
                if (OdinHandler.Instance.Rooms.Contains(pushToTalkData.connectedRoom))
                {
                    Room roomToCheck = OdinHandler.Instance.Rooms[pushToTalkData.connectedRoom];

                    if (null != roomToCheck.MicrophoneMedia)
                    {
                        bool isPushToTalkPressed = Input.GetButton(pushToTalkData.pushToTalkButton);
                        roomToCheck.MicrophoneMedia.SetMute(!isPushToTalkPressed);
                    }
                }
            }
            
        }
    }

    [Serializable]
    public class OdinPushToTalkData
    {
        public StringVariable connectedRoom;
        public string pushToTalkButton;
    }
}
