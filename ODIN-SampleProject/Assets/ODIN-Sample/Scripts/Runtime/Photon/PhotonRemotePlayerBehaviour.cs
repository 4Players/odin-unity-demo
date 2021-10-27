using System;
using System.Collections;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    [DisallowMultipleComponent]
    public class PhotonRemotePlayerBehaviour : MonoBehaviourPunCallbacks
    {
        [SerializeField] private PlaybackComponent odinAudioSourcePrefab;

        private void Awake()
        {
            Assert.IsNotNull(odinAudioSourcePrefab);
            if (photonView.IsMine)
                this.enabled = false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            // StartCoroutine(DelayedStartListening());
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnCreatedMediaObject?.AddListener(Instance_OnCreatedMediaObject);
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            }

        }

        private void OnRoomJoined(RoomJoinedEventArgs arg0)
        {
            Debug.Log($"Joined room: {arg0.Room.Config.Name}"); 
        }

        private IEnumerator DelayedStartListening()
        {
            yield return null;
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnCreatedMediaObject?.AddListener(Instance_OnCreatedMediaObject);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnCreatedMediaObject?.RemoveListener(Instance_OnCreatedMediaObject);
        }

        private void Instance_OnCreatedMediaObject(string roomName, ulong peerId, int mediaId)
        {
            Debug.Log($"On created media object: roomName: {roomName} peerId: {peerId} mediaId: {mediaId}"); 

            Room room = OdinHandler.Instance.Rooms[roomName];
            if (null != room && room.Self.Id == peerId)
            {
                photonView.RPC("SpawnOdinSound", RpcTarget.Others, roomName, (long) peerId, mediaId);
            }
        }

        [PunRPC]
        private void SpawnOdinSound(string roomName, long inPeerId, int mediaId)
        {
            ulong peerId = (ulong)inPeerId;
            
            PlaybackComponent playbackComponent = Instantiate(odinAudioSourcePrefab.gameObject).GetComponent<PlaybackComponent>();
            playbackComponent.RoomName = roomName;
            playbackComponent.PeerId = peerId;
            playbackComponent.MediaId = mediaId;
            
            playbackComponent.transform.SetParent(transform);
            playbackComponent.transform.localPosition = Vector3.zero;
            playbackComponent.transform.localRotation = Quaternion.identity;
        }

    }
}
