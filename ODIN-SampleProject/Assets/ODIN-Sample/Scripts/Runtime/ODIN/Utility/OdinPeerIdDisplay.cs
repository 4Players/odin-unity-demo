using System.Text;
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

        private float _smoothedFPS;

        private readonly StringBuilder displayBuilder = new StringBuilder();

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
                    foreach (Peer peer in room.RemotePeers)
                    {
                        OdinSampleUserData fromUserData = OdinSampleUserData.FromUserData(peer.UserData);
                        if (null != room.Self && peer.Id == room.Self.Id)
                            displayBuilder.Append("Current Name: ");
                        else
                            displayBuilder.Append("Remote Name:");

                        displayBuilder.AppendLine(
                            $"{fromUserData.name}, Room: {room.Config.Name}, Self peer Id: {peer.Id}, Unique Id: {fromUserData.uniqueUserId}");
                    }

                    if (null != room.MicrophoneMedia)
                    {
                        int micMediaId = room.MicrophoneMedia.Id;
                        Peer microphoneMediaOwner = default;
                        foreach (Peer peer in room.RemotePeers)
                            if (peer.Medias.Contains(micMediaId))
                                microphoneMediaOwner = peer;

                        if (null != microphoneMediaOwner)
                            displayBuilder.AppendLine(
                                $"Current Microphone: Microphone Id: {room.MicrophoneMedia.Id}, Owner: {microphoneMediaOwner.Id}, Room: {room.Config.Name}");
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