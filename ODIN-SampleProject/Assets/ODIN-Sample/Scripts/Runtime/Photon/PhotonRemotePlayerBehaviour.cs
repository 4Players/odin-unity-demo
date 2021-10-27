using System;
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
        }

        public override void OnEnable()
        {
            base.OnEnable();
            OdinHandler.Instance.OnCreatedMediaObject.AddListener(Instance_OnCreatedMediaObject);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            OdinHandler.Instance.OnCreatedMediaObject.RemoveListener(Instance_OnCreatedMediaObject);
        }

        private void Instance_OnCreatedMediaObject(string roomName, ulong peerId, int mediaId)
        {
            Room room = OdinHandler.Instance.Rooms[roomName];
            if (null != room && room.Self.Id == peerId)
            {
                
            }
        }

        [PunRPC]
        private void SpawnOdinSound(string roomName, long inPeerId, int mediaId)
        {
            ulong peerId = (ulong)inPeerId;
            
            PlaybackComponent playbackComponent = Instantiate(odinAudioSourcePrefab.gameObject, transform).GetComponent<PlaybackComponent>();

        }

    }
}
