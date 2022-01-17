using System;
using System.Collections.Generic;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Base class for different odin user representations. Will manage the handling of the ODIN <see cref="PlaybackComponent"/>,
    /// which is used to transmit the ODIN Audio stream.
    /// </summary>
    public abstract class AOdinUser : MonoBehaviour
    {
        /// <summary>
        /// The prefab containing a PlaybackComponent. This prefab will be spawned as a child of <see cref="instantiationTarget"/>
        /// for each call to <see cref="SpawnPlaybackComponent"/>.
        /// </summary>
        [SerializeField] protected PlaybackComponent playbackComponentPrefab;

        /// <summary>
        /// <see cref="playbackComponentPrefab"/>s will be spawned as a child of this transform.
        /// </summary>
        [SerializeField] protected Transform instantiationTarget;

        /// <summary>
        /// Called when a new <see cref="PlaybackComponent"/> was created by this script
        /// </summary>
        public Action<PlaybackComponent> OnPlaybackComponentAdded;
        
        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (string roomname, ulong peerid, int mediaid) combination.
        /// </summary>
        private Dictionary<(string, ulong, int), PlaybackComponent> _registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();

        protected virtual void Awake()
        {
            Assert.IsNotNull(playbackComponentPrefab);
            Assert.IsNotNull(instantiationTarget);
        }

        /// <summary>
        /// Destroys the <see cref="PlaybackComponent"/> identified by the tuple roomName, peerId and mediaId.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media id the peer is transmitting on.</param>
        /// <returns>True, if an object was destroyed, false if no reference identified by the tuple was found.</returns>
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
        
        /// <summary>
        /// Removes a the playback component identified by the given tuple from the registry, without destroying it.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media id the peer is transmitting on.</param>
        /// <returns>The removed PlaybackComponent or null, if no component with the given tuple was registered.</returns>
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

        /// <summary>
        /// Spawn a new instance of <see cref="playbackComponentPrefab"/> and connect it to an ODIN stream using the
        /// room name, peer id and media id.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media id the peer is transmitting on.</param>
        /// <returns>The spawned PlaybackComponent or null, if the tuple (roomName, peerId, mediaId) was already
        /// registered on this user</returns>
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