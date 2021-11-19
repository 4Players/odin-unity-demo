using System;
using System.Collections.Generic;
using System.Linq;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public abstract class AOdinUser : MonoBehaviour
    {
        
        [SerializeField] protected PlaybackComponent playbackComponentPrefab;

        /// <summary>
        /// Reference to the transform the <see cref="playbackComponentPrefab"/> should be instantiated on.
        /// </summary>
        [SerializeField] protected Transform instantiationTarget;

        [SerializeField] protected OdinPlaybackRegistry playbackRegistry;

        protected virtual void Awake()
        {
            Assert.IsNotNull(playbackComponentPrefab);
            Assert.IsNotNull(instantiationTarget);
            Assert.IsNotNull(playbackRegistry);
        }

        protected void SpawnPlaybackComponent(string roomName, ulong peerId, int mediaId)
        {
            if (!playbackRegistry.ContainsComponent(roomName, peerId, mediaId))
            {
                Transform parentTransform = null == instantiationTarget ? transform : instantiationTarget;
                PlaybackComponent playbackComponent = Instantiate(playbackComponentPrefab.gameObject, parentTransform)
                    .GetComponent<PlaybackComponent>();

                playbackComponent.RoomName = roomName;
                playbackComponent.PeerId = peerId;
                playbackComponent.MediaId = mediaId;

                playbackRegistry.AddComponent(roomName, peerId, mediaId, playbackComponent);
            }
        }
    }
}