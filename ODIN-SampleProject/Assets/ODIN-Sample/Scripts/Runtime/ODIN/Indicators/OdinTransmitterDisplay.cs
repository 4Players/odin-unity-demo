using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin.Utility;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    /// Displays transmissions of all players in a room using UI.
    /// </summary>
    public class OdinTransmitterDisplay : MonoBehaviour
    {
        /// <summary>
        /// Reference to a scriptable object containing references to all ODIN <c>PlaybackComponent</c>s.
        /// </summary>
        [SerializeField] private OdinPlaybackRegistry playbackRegistry;

        [SerializeField] private OdinStringVariable displayedRoomName;


        private HashSet<PlaybackComponent> _playbackComponents = new HashSet<PlaybackComponent>();

        private OdinTransmitterUiElement[] _uiElements;

        private void Awake()
        {
            Assert.IsNotNull(playbackRegistry);

            _uiElements = GetComponentsInChildren<OdinTransmitterUiElement>(true);
            foreach (var element in _uiElements) element.Hide();
        }

        private void OnEnable()
        {
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaActiveStateChanged);
        }


        private void OnDisable()
        {
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaActiveStateChanged);
        }

        private void OnMediaActiveStateChanged(object sender, MediaActiveStateChangedEventArgs mediaActiveEventArgs)
        {
            if (sender is Room sendingRoom)
            {
                var roomName = sendingRoom.Config.Name;
                if (roomName == displayedRoomName.Value)
                {
                    var peerId = mediaActiveEventArgs.PeerId;
                    int mediaId = mediaActiveEventArgs.MediaId;

                    var uiKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);

                    if (mediaActiveEventArgs.Active)
                    {
                        var room = OdinHandler.Instance.Rooms[roomName];
                        var peer = room.RemotePeers[peerId];
                        if (null != peer)
                        {
                            var userData = OdinSampleUserData.FromUserData(peer.UserData);
                            ShowElement(uiKey, userData);
                        }
                    }
                    else
                    {
                        HideElement(uiKey);
                    }
                }
            }
        }

        private void ShowElement(OdinConnectionIdentifier key, OdinSampleUserData userData)
        {
            foreach (var element in _uiElements)
                if (!element.IsActive())
                {
                    element.Show(key, userData);
                    break;
                }
        }

        private void HideElement(OdinConnectionIdentifier key)
        {
            foreach (var element in _uiElements)
                if (element.IsShowing(key))
                    element.Hide();
        }
    }
}