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
using Room = OdinNative.Odin.Room.Room;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    [DisallowMultipleComponent]
    public class OdinVoiceInPhotonRoom : MonoBehaviourPunCallbacks
    {
        [SerializeField] private StringVariable refRoomName;
        [SerializeField] private StringVariable refPlayerName;
        [SerializeField] private PlaybackComponent odinAudioSourcePrefab;

        private Dictionary<string, ulong> roomToPeerIds = new Dictionary<string, ulong>();
        
        private Dictionary<(string, ulong, int), PlaybackComponent> registeredRemoteMedia = new Dictionary<(string, ulong, int), PlaybackComponent>();

        private void Awake()
        {
            Assert.IsNotNull(odinAudioSourcePrefab);
            Assert.IsNotNull(refPlayerName);
        }

        private void Start()
        {
            string userId = refPlayerName.Value + DateTime.Now.Ticks;

            if (!PhotonNetwork.IsConnected)
                return;

            if (photonView.IsMine)
            {
                if (OdinHandler.Instance && !OdinHandler.Instance.Rooms.Contains(refRoomName.Value))
                {
                    Debug.Log($"Odin - joining room {refRoomName.Value}");
                    OdinHandler.Instance.JoinRoom(refRoomName.Value, userId);
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }

        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            Debug.Log($"OnMediaAdded with obj {obj}, Peer: {mediaAddedEventArgs.PeerId}, MediaId: {mediaAddedEventArgs.Media.Id}");
            if (!photonView.IsMine)
            {
                Debug.Log("Requesting Peer Ids.");
                photonView.RPC("RequestPeerIds", RpcTarget.Others);
            }
        }


        [PunRPC]
        private void RequestPeerIds()
        {
            if (photonView.IsMine)
            {
                Debug.Log("Received Peer Id Request.");
                BroadcastRemoteUpdate();
            }
        }

        private void BroadcastRemoteUpdate()
        {
            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                if (null != room.MicrophoneMedia)
                {
                    ulong peerId = room.MicrophoneMedia.GetPeerId();
                    BroadcastPeerId(room.Config.Name, peerId);
                }
                else
                {
                    Debug.LogError($"MicrophoneMedia in room {room.Config.Name} is null.");
                }
            }
        }

        private void BroadcastPeerId(string roomId, ulong peerId)
        {
            Debug.Log($"RequestBroadcastPeerId: Room: {roomId}, Peer: {peerId}");
            photonView.RPC("OnUpdatePeerId", RpcTarget.Others, roomId, (long) peerId);
        }

        [PunRPC]
        private void OnUpdatePeerId(string roomId, long peerId)
        {
            if (!photonView.IsMine)
            {
                Debug.Log($"OnUpdatePeerId: {roomId}, Peer: {peerId}");
                roomToPeerIds[roomId] = (ulong)peerId;
                UpdatePlaybacks();
            }
        }

        private void UpdatePlaybacks()
        {
            foreach (string roomId in roomToPeerIds.Keys)
            {
                ulong peerId = roomToPeerIds[roomId];
                if (OdinHandler.Instance.Rooms.Contains(roomId))
                {
                    Room room = OdinHandler.Instance.Rooms[roomId];
                    foreach (Peer peer in room.RemotePeers)
                    {
                        if (peer.Id == peerId)
                        {
                            UpdatePlaybackComponents(peer, roomId);
                        }
                    }
                }
            }
        }

        private void UpdatePlaybackComponents(Peer peer, string roomId)
        {
            foreach (MediaStream mediaStream in peer.Medias)
            {
                SpawnOdinPlayback(roomId, peer.Id, mediaStream.Id);
            }
        }

        private PlaybackComponent SpawnOdinPlayback(string roomName, ulong peerId,  int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (registeredRemoteMedia.ContainsKey(dictionaryKey))
                return null;
            
            Debug.Log($"New Odin Sound spawned: {roomName} peerId: {peerId} mediaId: {mediaId}");
            PlaybackComponent playbackComponent =
                Instantiate(odinAudioSourcePrefab.gameObject).GetComponent<PlaybackComponent>();
            playbackComponent.RoomName = roomName;
            playbackComponent.PeerId = peerId;
            playbackComponent.MediaId = mediaId;

            playbackComponent.transform.SetParent(transform);
            playbackComponent.transform.localPosition = Vector3.zero;
            playbackComponent.transform.localRotation = Quaternion.identity;
            
            registeredRemoteMedia.Add(dictionaryKey, playbackComponent);
            
            return playbackComponent;
        }
    }
}
