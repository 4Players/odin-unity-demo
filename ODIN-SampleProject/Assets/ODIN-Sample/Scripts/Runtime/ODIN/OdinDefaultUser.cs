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
        [SerializeField] private StringVariable odinRoomName;

        private void Awake()
        {
            Assert.IsNotNull(odinRoomName);
            Assert.IsNotNull(playbackComponentPrefab);
        }

        private void OnEnable()
        {
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        private void OnDisable()
        {
            OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
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
                SpawnPlaybackComponent(mediaRoomName, mediaPeerId, mediaId);
            }
        }
        
        
    }
}