using System;
using System.Collections;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
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
        [SerializeField] private StringVariable refPlayerName;
        
        [SerializeField] private PlaybackComponent odinAudioSourcePrefab;

        private ulong _remotePlayerPeerId;

        private Dictionary<int, PlaybackComponent> _connectedMediaPlaybacks = new Dictionary<int, PlaybackComponent>();

        private void Awake()
        {
            Assert.IsNotNull(odinAudioSourcePrefab);
            Assert.IsNotNull(refPlayerName);
        }
        
        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("Start listening to OnRoomJoined");
            OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
        }

        public override void OnJoinedRoom()
        {
            if (!photonView.IsMine)
            {
                UpdateOdinPlaybacks();
            }
        }

        private void OnRoomJoined(RoomJoinedEventArgs roomJoinedEventArgs)
        {
            Debug.Log($"Joined ODIN room: {roomJoinedEventArgs.Room.Config.Name}");
            if (photonView.IsMine)
            {
                Debug.Log($"Sending RPC peer id update: {roomJoinedEventArgs.Room.Self.Id}");
                photonView.RPC("OnUpdatePeerId", RpcTarget.Others, (long) roomJoinedEventArgs.Room.Self.Id);
            }
            else
            {
                UpdateOdinPlaybacks();
            }
        }

        [PunRPC]
        private void OnUpdatePeerId(long peerId)
        {
            if (!photonView.IsMine)
            {
                _remotePlayerPeerId = (ulong)peerId;
                Debug.Log($"Updated remote player peer to: {_remotePlayerPeerId}");
                
                UpdateOdinPlaybacks();
            }
        }

        private void UpdateOdinPlaybacks()
        {
            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                Debug.Log($"Current peer id is: {room.Self.Id} in room: {room.Config.Name}");

                foreach (Peer peer in room.RemotePeers)
                {
                    if (peer.Id == _remotePlayerPeerId)
                    {
                        foreach (MediaStream media in peer.Medias)
                        {
                            
                            if (media is PlaybackStream)
                            {
                                SpawnOdinPlayback(room.Config.Name, media.Id);
                            }
                        }
                    }
                }
            }
        }
        private PlaybackComponent SpawnOdinPlayback(string roomName, int mediaId)
        {
            Debug.Log($"New Odin Sound spawned: {roomName} peerId: {_remotePlayerPeerId} mediaId: {mediaId}");
            PlaybackComponent playbackComponent =
                Instantiate(odinAudioSourcePrefab.gameObject).GetComponent<PlaybackComponent>();
            playbackComponent.RoomName = roomName;
            playbackComponent.PeerId = _remotePlayerPeerId;
            playbackComponent.MediaId = mediaId;

            playbackComponent.transform.SetParent(transform);
            playbackComponent.transform.localPosition = Vector3.zero;
            playbackComponent.transform.localRotation = Quaternion.identity;

            _connectedMediaPlaybacks[mediaId] = playbackComponent;
            
            return playbackComponent;
        }
    }
}
