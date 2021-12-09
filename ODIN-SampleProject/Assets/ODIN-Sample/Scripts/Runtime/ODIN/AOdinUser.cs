using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Called when a new playbackcomponent was created by this script
        /// </summary>
        public Action<PlaybackComponent> OnPlaybackComponentAdded;
        
        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (roomname, peerid, mediaid) combination.
        /// </summary>
        private Dictionary<(string, ulong, int), PlaybackComponent> _registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();

        protected virtual void Awake()
        {
            Assert.IsNotNull(playbackComponentPrefab);
            Assert.IsNotNull(instantiationTarget);
        }

        protected bool DestroyPlaybackAudioSource(string roomName, ulong peerId, int mediaId)
        {
            PlaybackComponent removed = RemovePlaybackComponent(roomName, peerId, mediaId);
            if(removed)
            {
                Destroy(removed.gameObject);
                return true;
            }
            return false;
        }
        
        protected PlaybackComponent RemovePlaybackComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out PlaybackComponent toRemove))
            {
                _registeredRemoteMedia.Remove(dictionaryKey);
                return toRemove;
            }
            return null;
        }

        protected PlaybackComponent SpawnPlaybackComponent(string roomName, ulong peerId, ushort mediaId)
        {
            PlaybackComponent spawned = null;
            var dictionaryKey = (roomName, peerId, mediaId);
            if (!_registeredRemoteMedia.ContainsKey(dictionaryKey))
            {
                Transform parentTransform = null == instantiationTarget ? transform : instantiationTarget;
                spawned = Instantiate(playbackComponentPrefab.gameObject, parentTransform)
                    .GetComponent<PlaybackComponent>();
                
                Debug.Log($"Spawned PlaybackComponent for {dictionaryKey}: {spawned}");


                spawned.RoomName = roomName;
                spawned.PeerId = peerId;
                spawned.MediaId = mediaId;

                _registeredRemoteMedia.Add(dictionaryKey, spawned);
                OnPlaybackComponentAdded?.Invoke(spawned);
            }

            return spawned;
        }
    }
}