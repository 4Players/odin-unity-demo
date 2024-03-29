﻿using System.Collections;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    ///     Used to automatically spawn audio playback components for all ODIN users connected to the room with name
    ///     <see cref="odinRoomName" />, independent of their position. Could be used e.g. for playing audio from headsets or
    ///     "voice of god" transmissions.
    /// </summary>
    public class OdinDefaultUser : AOdinUser
    {
        /// <summary>
        ///     The room for which the Playback Components should be spawned.
        /// </summary>
        [SerializeField] private OdinStringVariable odinRoomName;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(odinRoomName);
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        private void OnDisable()
        {
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
                OdinHandler.Instance.OnMediaRemoved.RemoveListener(OnMediaRemoved);
            }
            
            DestroyAllPlaybacks();
        }

        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;
            
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
            OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
            OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            OdinHandler.Instance.OnRoomLeft.AddListener(OnRoomLeft);
            
        }

        private void OnRoomLeft(RoomLeftEventArgs arg0)
        {
            DestroyAllPlaybacksInRoom(arg0.RoomName);
        }

        private void OnRoomJoined(RoomJoinedEventArgs arg0)
        {
            StartCoroutine(DeferredOnRoomJoined());
        }

        private IEnumerator DeferredOnRoomJoined()
        {
            yield return null;
            UpdateRoomPlayback();
            
        }

        /// <summary>
        ///     Checks for each mediastream connected to any peer in the room <see cref="odinRoomName" />
        ///     whether a Playback Components was already created and initialized.
        /// </summary>
        private void UpdateRoomPlayback()
        {
            if (OdinHandler.Instance && OdinHandler.Instance.Rooms.Contains(odinRoomName))
            {
                Room room = OdinHandler.Instance.Rooms[odinRoomName];
                foreach (Peer remotePeer in room.RemotePeers)
                {
                    foreach (var mediaStream in remotePeer.Medias) SpawnPlaybackComponent(room.Config.Name, remotePeer.Id, mediaStream.Id);
                }
            }
        }

        private void OnMediaAdded(object arg0, MediaAddedEventArgs mediaAddedEventArgs)
        {
            string mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            ulong mediaPeerId = mediaAddedEventArgs.PeerId;

            MicrophoneStream microphoneStream = OdinHandler.Instance.Rooms[mediaRoomName].MicrophoneMedia;
            ulong localPeerId = microphoneStream?.GetPeerId() ?? 0;
            // Debug.Log($"OnMedia Added for {mediaRoomName} and peer {mediaPeerId}");
            if (odinRoomName == mediaRoomName && localPeerId != mediaPeerId)
            {
                long mediaId = mediaAddedEventArgs.Media.Id;
                // Debug.Log($"Trying to spawn playback component for {mediaRoomName} with peer {mediaPeerId} and media {mediaId}");
                PlaybackComponent spawnedComponent = SpawnPlaybackComponent(mediaRoomName, mediaPeerId, mediaId);
                // Debug.Log($"After spawning component: {spawnedComponent}");
            }
        }

        private void OnMediaRemoved(object roomObject, MediaRemovedEventArgs mediaRemovedArgs)
        {
            
            
            if (null != mediaRemovedArgs.Peer)
            {
                string mediaRoomName = mediaRemovedArgs.Peer.RoomName;
                ulong mediaPeerId = mediaRemovedArgs.Peer.Id;
                long mediaId = mediaRemovedArgs.MediaStreamId;

                DestroyPlayback(mediaRoomName, mediaPeerId, mediaId);
            }
            else
            {
                if (roomObject is Room room)
                {
                    DestroyPlaybacks(room.Config.Name, mediaRemovedArgs.MediaStreamId);
                }
            }
        }
    }
}