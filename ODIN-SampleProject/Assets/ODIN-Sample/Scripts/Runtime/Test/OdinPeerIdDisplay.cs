using System;
using System.Text;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.Test
{
    public class OdinPeerIdDisplay : MonoBehaviour
    {
        [SerializeField] private Text display;
        [SerializeField] private bool logOutput = false;
        
        
        private StringBuilder displayBuilder = new StringBuilder();

        // Update is called once per frame
        void Update()
        {
            if (OdinHandler.Instance)
            {
                foreach (Room room in OdinHandler.Instance.Rooms)
                {
                    foreach (Peer peer in room.RemotePeers)
                    {
                        OdinSampleUserData fromUserData = OdinSampleUserData.FromUserData(peer.UserData);
                        
                        if (null != room.Self && peer.Id == room.Self.Id)
                        {
                            displayBuilder.AppendLine($"Current Name: {fromUserData.name}, Room: {room.Config.Name}, Self peer Id: {peer.Id}");
                        }
                        else
                        {
                            displayBuilder.AppendLine($"Remote Name: {fromUserData.name},Room: {room.Config.Name}, Remote peer Id: {peer.Id}");
                        }
                    }
                    
                    if (null != room.MicrophoneMedia)
                    {
                        int micMediaId = room.MicrophoneMedia.Id;
                        Peer microphoneMediaOwner = default;
                        foreach (Peer peer in room.RemotePeers)
                        {
                            if (peer.Medias.Contains(micMediaId))
                            {
                                microphoneMediaOwner = peer;
                            }
                        }

                        if (null != microphoneMediaOwner)
                        {
                            displayBuilder.AppendLine($"Current Microphone: Microphone Id: {room.MicrophoneMedia.Id}, Owner: {microphoneMediaOwner.Id}, Room: {room.Config.Name}");
                        }
                    }
                    
                }
            }

            string displayString = displayBuilder.ToString();
            displayBuilder.Clear();

            display.text = displayString;
            if(logOutput)
                Debug.Log(displayString);
            
        }
    }
}
