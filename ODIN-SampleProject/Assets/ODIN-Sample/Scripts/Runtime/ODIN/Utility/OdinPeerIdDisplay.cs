using System.Linq;
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
        /// <summary>
        /// Used for averaging out the shown fps value.
        /// </summary>
        private const float Alpha = 0.03f;

        /// <summary>
        ///     Text on which the data should be displayed.
        /// </summary>
        [SerializeField] private TMP_Text display;

        /// <summary>
        ///     Whether to log the output using Debug.Log.
        /// </summary>
        [SerializeField] private bool logOutput;

        private readonly StringBuilder displayBuilder = new StringBuilder();

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
            {
                foreach (Room room in OdinHandler.Instance.Rooms)
                {
                    int streamsActive = room.PlaybackMedias.Count();
                    displayBuilder.AppendLine($"<b><size=125%>Connections in {room.Config.Name}: {streamsActive}</size></b>");
                }

                displayBuilder.AppendLine();

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

                    AppendPeer(room.Self, "Local", false);
                    foreach (Peer peer in room.RemotePeers) AppendPeer(peer);
                }
            }


            string displayString = displayBuilder.ToString();
            displayBuilder.Clear();

            display.text = displayString;
            if (logOutput)
                Debug.Log(displayString);
        }

        private void AppendPeer(Peer peer, string suffix = "Remote", bool showMedias = true)
        {
            if (null != peer)
            {
                displayBuilder.Append("\t");
                OdinSampleUserData fromUserData = OdinSampleUserData.FromUserData(peer.UserData);
                displayBuilder.AppendLine(
                    $"<b>{fromUserData.name}</b> ({suffix})");


                displayBuilder.AppendLine($"\t \tUnique Id: {fromUserData.uniqueUserId}, Peer Id: {peer.Id}");

                if (showMedias)
                {
                    displayBuilder.Append("\t \tMedias: ");
                    foreach (MediaStream mediaStream in peer.Medias) displayBuilder.Append($" ID {mediaStream.Id}");

                    displayBuilder.AppendLine();
                }
            }
        }
    }
}