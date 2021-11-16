using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity;
using OdinNative.Unity.Audio;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Room = OdinNative.Odin.Room.Room;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    [DisallowMultipleComponent]
    public class OdinVoiceUser : MonoBehaviourPunCallbacks
    {
        [FormerlySerializedAs("odinAudioSourcePrefab")] [SerializeField] private PlaybackComponent remoteAudioSourcePrefab;
        [Tooltip("The transform on which the remoteAudioSourcePrefab will be instantiated.")]
        [SerializeField] private Transform instantiationTarget;
        

        public UnityEvent<PlaybackComponent> onPlaybackComponentAdded;

        private Dictionary<string, ulong> roomToPeerIds = new Dictionary<string, ulong>();

        private Dictionary<(string, ulong, int), PlaybackComponent> registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();

        private void Awake()
        {
            Assert.IsNotNull(remoteAudioSourcePrefab);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }


        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            if (PhotonNetwork.IsConnectedAndReady && !photonView.IsMine)
            {
                photonView.RPC("OnRequestedPeerIds", RpcTarget.Others);
            }
        }

        [PunRPC]
        private void OnRequestedPeerIds()
        {
            if (photonView.IsMine)
            {
                foreach (Room room in OdinHandler.Instance.Rooms)
                {
                    if (null != room.MicrophoneMedia)
                    {
                        ulong peerId = room.MicrophoneMedia.GetPeerId();
                        if(PhotonNetwork.IsConnectedAndReady)
                            photonView.RPC("OnReceivedPeerIdUpdate", RpcTarget.Others, room.Config.Name, (long)peerId);
                    }
                    else
                    {
                        Debug.LogError($"MicrophoneMedia in room {room.Config.Name} is null.");
                    }
                }
            }
        }

        [PunRPC]
        private void OnReceivedPeerIdUpdate(string roomId, long peerId)
        {
            if (!photonView.IsMine)
            {
                Debug.Log($"OnUpdatePeerId: {roomId}, Peer: {peerId}");
                roomToPeerIds[roomId] = (ulong)peerId;
                UpdateAllPlaybacks();
            }
        }

        private void UpdateAllPlaybacks()
        {
            foreach (string roomId in roomToPeerIds.Keys)
            {
                ulong peerId = roomToPeerIds[roomId];
                if (OdinHandler.Instance.Rooms.Contains(roomId))
                {
                    UpdateRoomPlayback(roomId, peerId);
                }
            }
        }

        private void UpdateRoomPlayback(string roomId, ulong peerId)
        {
            Room room = OdinHandler.Instance.Rooms[roomId];
            foreach (Peer peer in room.RemotePeers)
            {
                if (peer.Id == peerId)
                {
                    foreach (MediaStream mediaStream in peer.Medias)
                    {
                        TrySpawnOdinPlayback(roomId, peer.Id, mediaStream.Id);
                    }
                }
            }
        }

        private PlaybackComponent TrySpawnOdinPlayback(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (registeredRemoteMedia.ContainsKey(dictionaryKey))
                return null;

            Debug.Log($"New Odin Sound spawned: {roomName} peerId: {peerId} mediaId: {mediaId}");
            PlaybackComponent playbackComponent =
                Instantiate(remoteAudioSourcePrefab.gameObject).GetComponent<PlaybackComponent>();
            playbackComponent.RoomName = roomName;
            playbackComponent.PeerId = peerId;
            playbackComponent.MediaId = mediaId;

            Transform parentTransform = null == instantiationTarget ? transform : instantiationTarget;
            playbackComponent.transform.SetParent(parentTransform);
            playbackComponent.transform.localPosition = Vector3.zero;
            playbackComponent.transform.localRotation = Quaternion.identity;

            registeredRemoteMedia.Add(dictionaryKey, playbackComponent);

            onPlaybackComponentAdded?.Invoke(playbackComponent);

            return playbackComponent;
        }
    }
}