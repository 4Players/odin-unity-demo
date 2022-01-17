using System.Collections.Generic;
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

        private HashSet<PlaybackComponent> _playbackComponents = new HashSet<PlaybackComponent>();

        private OdinTransmitterUiElement[] _uiElements;

        private void Awake()
        {
            Assert.IsNotNull(playbackRegistry);

            _uiElements = GetComponentsInChildren<OdinTransmitterUiElement>(true);
            foreach (OdinTransmitterUiElement element in _uiElements)
            {
                element.Hide();
            }
        }

        private void OnEnable()
        {
            playbackRegistry.OnPlaybackComponentAdded += OnPlaybackComponentAdded;
            playbackRegistry.OnPlaybackComponentRemoved += OnPlaybackComponentRemoved;
        }
        
        private void OnDisable()
        {
            playbackRegistry.OnPlaybackComponentAdded -= OnPlaybackComponentAdded;
            playbackRegistry.OnPlaybackComponentRemoved -= OnPlaybackComponentRemoved;
        }

        private void OnDestroy()
        {
            foreach (PlaybackComponent component in _playbackComponents)
            {
                component.OnPlaybackPlayingStatusChanged -= OnPlaybackPlayingStatusChanged;
            }
        }

        private void OnPlaybackComponentAdded(PlaybackComponent added)
        {
            if (null != added)
            {
                added.OnPlaybackPlayingStatusChanged += OnPlaybackPlayingStatusChanged;
                _playbackComponents.Add(added);
            }
        }

        private void OnPlaybackComponentRemoved(PlaybackComponent removed)
        {
            if (null != removed)
            {
                removed.OnPlaybackPlayingStatusChanged -= OnPlaybackPlayingStatusChanged;
                _playbackComponents.Remove(removed);
            }
        }
        
        private void OnPlaybackPlayingStatusChanged(PlaybackComponent component, bool isplaying)
        {
            if (!component)
                return;

            string roomName = component.RoomName;
            ulong peerId = component.PeerId;
            int mediaId = component.MediaId;

            var uiKey = (roomName, peerId, mediaId);

            if (isplaying)
            {
                Room room = OdinHandler.Instance.Rooms[roomName];
                Peer peer = room.RemotePeers[peerId];
                OdinSampleUserData userData = OdinSampleUserData.FromUserData(peer.UserData);
                ShowElement(uiKey, userData);
            }
            else
            {
                HideElement(uiKey);
            }
        }

        private void ShowElement((string, ulong, int) key, OdinSampleUserData userData)
        {
            foreach (OdinTransmitterUiElement element in _uiElements)
            {
                if (!element.IsActive())
                {
                    element.Show(key, userData);
                    break;
                }
            }
        }

        private void HideElement((string, ulong, int) key)
        {
            foreach (OdinTransmitterUiElement element in _uiElements)
            {
                if (element.IsShowing(key))
                {
                    element.Hide();
                }
            }
        }
    }
}