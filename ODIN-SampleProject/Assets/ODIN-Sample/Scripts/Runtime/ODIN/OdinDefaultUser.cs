using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Media;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Used to automatically spawn playback components for other users, independent of their position. Could be used
    /// e.g. for playing audio from radio units / headsets
    /// </summary>
    public class OdinDefaultUser : AOdinUser
    {
        [SerializeField] private OdinStringVariable odinRoomName;
        [SerializeField] private OdinPlaybackRegistry odinPlaybackRegistry;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(odinRoomName);
        }

        private void OnEnable()
        {
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
            OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
        }

        private void OnDisable()
        {
            OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
            OdinHandler.Instance.OnMediaRemoved.RemoveListener(OnMediaRemoved);
        }

        private void OnMediaAdded(object arg0, MediaAddedEventArgs mediaAddedEventArgs)
        {
            string mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            ulong mediaPeerId = mediaAddedEventArgs.PeerId;

            MicrophoneStream microphoneStream = OdinHandler.Instance.Rooms[mediaRoomName].MicrophoneMedia;
            ulong localPeerId = microphoneStream?.GetPeerId() ?? 0;
            if (odinRoomName == mediaRoomName && localPeerId != mediaPeerId)
            {
                int mediaId = mediaAddedEventArgs.Media.Id;
                PlaybackComponent spawnedComponent = SpawnPlaybackComponent(mediaRoomName, mediaPeerId, mediaId);
                if(odinPlaybackRegistry)
                    odinPlaybackRegistry.AddComponent(mediaRoomName, mediaPeerId, mediaId, spawnedComponent);
            }
        }
        
        private void OnMediaRemoved(object arg0, MediaRemovedEventArgs mediaRemovedArgs)
        {
            string mediaRoomName = mediaRemovedArgs.Peer.RoomName;
            ulong mediaPeerId = mediaRemovedArgs.Peer.Id;
            int mediaId = mediaRemovedArgs.MediaId;

            PlaybackComponent removedComponent = RemovePlaybackComponent(mediaRoomName, mediaPeerId, mediaId);
            if (removedComponent)
            {
                Destroy(removedComponent.gameObject);
            }

            if (odinPlaybackRegistry)
                odinPlaybackRegistry.RemoveComponent(mediaRoomName, mediaPeerId, mediaId);
        }
    }
}