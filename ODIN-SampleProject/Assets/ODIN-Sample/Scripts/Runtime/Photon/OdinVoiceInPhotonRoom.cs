using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


        private Dictionary<(string, ulong, int), PlaybackComponent> registeredRemoteMedia = new Dictionary<(string, ulong, int), PlaybackComponent>();

        private void Awake()
        {
            Assert.IsNotNull(odinAudioSourcePrefab);
            Assert.IsNotNull(refPlayerName);
        }
        
        public override void OnEnable()
        {
            base.OnEnable();
            // OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            OdinHandler.Instance.OnCreatedMediaObject.AddListener(OnCreatedMediaObject);
        }

       

        public override void OnDisable()
        {
            base.OnDisable();
            // OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            OdinHandler.Instance.OnCreatedMediaObject.RemoveListener(OnCreatedMediaObject);
        }
        
        // private void OnRoomJoined(RoomJoinedEventArgs arg0)
        // {
        //     Debug.Log($"On Room Joined was called.");
        //     BroadcastRemoteUpdate();
        // }

        private void BroadcastRemoteUpdate()
        {
            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                if (null != room.MicrophoneMedia)
                {
                    int microphoneMediaId = room.MicrophoneMedia.Id;
                    ulong peerId = room.MicrophoneMedia.GetPeerId();
                    RequestRemoteMediaObjectCreation(room.Config.Name, peerId, microphoneMediaId);
                }
            }
        }
        
        private void OnCreatedMediaObject(string roomId, ulong peerId, int mediaId)
        {
            BroadcastRemoteUpdate();
            // StartCoroutine(DelayedOnCreatedMediaObject(roomId, peerId, mediaId));
        }

        private IEnumerator DelayedOnCreatedMediaObject(string roomId, ulong peerId, int mediaId)
        {
            yield return null;
            
            Debug.Log($"On Created Media Object - Room: {roomId}, Peer: {peerId}, Media: {mediaId}");
            if (photonView.IsMine && OdinHandler.Instance.Rooms.Contains(roomId))
            {
                bool hasOwnerCreatedMedia = OdinHandler.Instance.Rooms[roomId].MicrophoneMedia.GetPeerId() == peerId;
                if (hasOwnerCreatedMedia)
                {
                    RequestRemoteMediaObjectCreation(roomId, peerId, mediaId);
                }
            }
        }

        private void RequestRemoteMediaObjectCreation(string roomId, ulong peerId, int mediaId)
        {
            Debug.Log($"Requesting remote create Media Object: Room: {roomId}, Peer: {peerId}, Media: {mediaId}");
            photonView.RPC("OnRemoteCreateMedia", RpcTarget.Others, roomId, (long)peerId, mediaId);
        }

        [PunRPC]
        private void OnRemoteCreateMedia(string roomId, long peerId, int mediaId)
        {
            if (!photonView.IsMine)
            {
                Debug.Log($"Remote Created Media Object: Room: {roomId}, Peer: {peerId}, Media: {mediaId}");
                SpawnOdinPlayback(roomId, (ulong)peerId, mediaId);
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
