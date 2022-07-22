using System.Text;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using TMPro;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    ///     Utility script for displaying useful ODIN debugging data, like the Peer and room data for each connected user.
    /// </summary>
    public class OdinPeerIdDisplay : MonoBehaviour
    {
        private const float Alpha = 0.03f;

        /// <summary>
        ///     Text on which the data should be displayed.
        /// </summary>
        [SerializeField] private TMP_Text display;

        /// <summary>
        ///     Whether to log the output using Debug.Log.
        /// </summary>
        [SerializeField] private bool logOutput;

        private readonly StringBuilder displayBuilder = new();

        private float _smoothedFPS;

        // Update is called once per frame
        private void Update()
        {
            if (Time.smoothDeltaTime > 0.0f)
            {
                float fps = 1.0f / Time.smoothDeltaTime;
                _smoothedFPS = Alpha * fps + _smoothedFPS * (1 - Alpha);

                displayBuilder.AppendLine($"FPS: {Mathf.RoundToInt(_smoothedFPS)}");
            }

            if (OdinHandler.Instance)
                foreach (Room room in OdinHandler.Instance.Rooms)
                {
                    displayBuilder.AppendLine($"Room: {room.Config.Name}");
                    
                    if (null != room.MicrophoneMedia)
                    {
                        long micMediaId = room.MicrophoneMedia.Id;
                        Peer microphoneMediaOwner = default;
                        foreach (Peer peer in room.RemotePeers)
                            if (peer.Medias.Contains(micMediaId))
                                microphoneMediaOwner = peer;

                        if (null != microphoneMediaOwner)
                            displayBuilder.AppendLine(
                                $"Local Users Microphone Id: {room.MicrophoneMedia.Id}, Peer Id: {microphoneMediaOwner.Id}");
                    }
                    
                    foreach (Peer peer in room.RemotePeers)
                    {
                        OdinSampleUserData fromUserData = OdinSampleUserData.FromUserData(peer.UserData);
                        if (null != room.Self && peer.Id == room.Self.Id)
                            displayBuilder.Append("Local Username: ");
                        else
                            displayBuilder.Append("Remote Username: ");

                        displayBuilder.Append(
                            $"{fromUserData.name}, Unique Id: {fromUserData.uniqueUserId}, Peer Id: {peer.Id}; ");

                        displayBuilder.Append(" Medias: ");
                        foreach (MediaStream mediaStream in peer.Medias) displayBuilder.Append($" ID {mediaStream.Id}");

                        displayBuilder.AppendLine();
                    }

                    
                }

            string displayString = displayBuilder.ToString();
            displayBuilder.Clear();

            display.text = displayString;
            if (logOutput)
                Debug.Log(displayString);
        }
    }
}