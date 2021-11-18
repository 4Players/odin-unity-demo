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
    /// <summary>
    /// Automatically creates a PlaybackComponent on a remote player for each room the owning (local) player is connected to.
    /// Will only use rooms listed in <see cref="connectedOdinRooms"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class OdinVoiceUser : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// All rooms which should be handled by the odin voice user script.
        /// The script won't automatically create Playback Components for rooms that aren't listed here.
        /// </summary>
        [SerializeField] private StringVariable[] connectedOdinRooms;

        [SerializeField]
        private PlaybackComponent remoteAudioSourcePrefab;

        [Tooltip("The transform on which the remoteAudioSourcePrefab will be instantiated.")] [SerializeField]
        private Transform instantiationTarget;


        /// <summary>
        /// Called when a new playbackcomponent was created by this script
        /// </summary>
        public UnityEvent<PlaybackComponent> onPlaybackComponentAdded;

        /// <summary>
        /// Contains all pairs of roomname to peer ids associated to the player owning the photonView
        /// </summary>
        private Dictionary<string, ulong> roomToPeerIds = new Dictionary<string, ulong>();

        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (roomname, peerid, mediaid) combination.
        /// </summary>
        private Dictionary<(string, ulong, int), PlaybackComponent> registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();

        private void Awake()
        {
            Assert.IsNotNull(remoteAudioSourcePrefab);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }
        
        /// <summary>
        /// If this is a remote player and we register, that a new media has been added, request the peer id for the room
        /// in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            if (PhotonNetwork.IsConnectedAndReady && !photonView.IsMine)
            {
                photonView.RPC("OnRequestedPeerIds", RpcTarget.Others, mediaAddedEventArgs.Peer.RoomName);
            }
        }

        /// <summary>
        /// Handles a request from a remote player for the peer id of the current user in the room <see cref="roomName"/>.
        /// </summary>
        /// <remarks>
        /// Because room.Self.Id currently does not give reliable results when opening multiple clients on the same pc,
        /// we're using the room.MicrophoneMedia.GetPeerId() to retrieve the peer id for the current user in a specific room.
        /// </remarks>
        /// <param name="roomName"></param>
        [PunRPC]
        private void OnRequestedPeerIds(string roomName)
        {
            if (photonView.IsMine)
            {
                if (OdinHandler.Instance.Rooms.Contains(roomName))
                {
                    Room room = OdinHandler.Instance.Rooms[roomName];
                    if (null != room.MicrophoneMedia)
                    {
                        ulong peerId = room.MicrophoneMedia.GetPeerId();
                        if (PhotonNetwork.IsConnectedAndReady)
                            photonView.RPC("OnReceivedPeerIdUpdate", RpcTarget.Others, room.Config.Name, (long)peerId);
                    }
                    else
                    {
                        Debug.LogError($"MicrophoneMedia in room {room.Config.Name} is null.");
                    }
                }
            }
        }

        /// <summary>
        /// Message containing the peer id for a given room, which was sent for synchronization by the owning photonView
        /// (the actual player).
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="peerId"></param>
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

        /// <summary>
        /// Iterates through all room+peer combinations registered for this player and creates playbackcomponents for
        /// that combination, if not yet available.
        /// </summary>
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

        /// <summary>
        /// Checks for each mediastream connected to the peer <see cref="peerId"/> in the room <see cref="roomId"/>
        /// whether a Playback Components was already created and initialized.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="peerId"></param>
        private void UpdateRoomPlayback(string roomId, ulong peerId)
        {
            Room room = OdinHandler.Instance.Rooms[roomId];
            if (room.RemotePeers.Contains(peerId))
            {
                Peer peer = room.RemotePeers[peerId];
                foreach (MediaStream mediaStream in peer.Medias)
                {
                    TrySpawnOdinPlayback(roomId, peer.Id, mediaStream.Id);
                }
            }
        }

        private PlaybackComponent TrySpawnOdinPlayback(string roomName, ulong peerId, int mediaId)
        {
            // Skip, if we've already created the Playback component for this media
            var dictionaryKey = (roomName, peerId, mediaId);
            if (registeredRemoteMedia.ContainsKey(dictionaryKey))
                return null;
            
            // Create and initialize the Playback Component
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

            // add to registered playback components
            registeredRemoteMedia.Add(dictionaryKey, playbackComponent);
            
            // notify listeners, that a new playback component was registered
            onPlaybackComponentAdded?.Invoke(playbackComponent);
            return playbackComponent;
        }
    }
}