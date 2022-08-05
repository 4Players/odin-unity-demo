using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin.Utility;
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
        /// Called when a new Media Stream was added for this Odin User (both remote or local)
        /// </summary>
        public Action<OdinConnectionIdentifier> OnMediaStreamEstablished;
        
        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (string roomname, ulong peerid, int mediaid) combination.
        /// </summary>
        private Dictionary<OdinConnectionIdentifier, PlaybackComponent> _registeredRemoteMedia =
            new Dictionary<OdinConnectionIdentifier, PlaybackComponent>();

        protected virtual void Awake()
        {
            Assert.IsNotNull(playbackComponentPrefab);
            Assert.IsNotNull(instantiationTarget);
        }

        /// <summary>
        /// Destroys all gameobjects with registered playback components.
        /// </summary>
        protected void DestroyAllPlaybacks()
        {
            foreach (PlaybackComponent playbackComponent in _registeredRemoteMedia.Values)
            {
                if(playbackComponent)
                    Destroy(playbackComponent.gameObject);
            }
            _registeredRemoteMedia.Clear();
        }

        /// <summary>
        /// Destroys all gameobjects with registered playback components for a given room.
        /// </summary>
        /// <param name="roomName">The room for which the playback objects should be destroyed.</param>
        protected void DestroyAllPlaybacksInRoom(string roomName)
        {
            List<OdinConnectionIdentifier> idsToRemove = new List<OdinConnectionIdentifier>();
            foreach (var idToPlayback in _registeredRemoteMedia)
            {
                if (idToPlayback.Key.RoomName == roomName)
                {
                    idsToRemove.Add(idToPlayback.Key);
                    Destroy(idToPlayback.Value.gameObject);
                }
            }

            foreach (OdinConnectionIdentifier identifier in idsToRemove)
            {
                _registeredRemoteMedia.Remove(identifier);
            }
        }

        /// <summary>
        /// Destroys the playback gameobject based on the given connection identifier.
        /// </summary>
        /// <param name="odinId">The identifier uniquely identifying a playback component.</param>
        /// <returns>True, if a playback gameobject was found and destroyed.</returns>
        protected bool DestroyPlayback(OdinConnectionIdentifier odinId)
        {
            return DestroyPlayback(odinId.RoomName, odinId.PeerId,
                odinId.MediaId);
        }

        /// <summary>
        /// Destroys the <see cref="PlaybackComponent"/> identified by the tuple roomName, peerId and mediaId.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media id the peer is transmitting on.</param>
        /// <returns>True, if an object was destroyed, false if no reference identified by the tuple was found.</returns>
        protected bool DestroyPlayback(string roomName, ulong peerId, long mediaId)
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
        /// <param name="mediaId">The media stream id the peer is transmitting on.</param>
        /// <returns>The removed PlaybackComponent or null, if no component with the given tuple was registered.</returns>
        protected PlaybackComponent RemovePlaybackComponent(string roomName, ulong peerId, long mediaId)
        {
            var dictionaryKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out PlaybackComponent toRemove))
            {
                _registeredRemoteMedia.Remove(dictionaryKey);
                return toRemove;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the playback component identified by identified by the given tuple from the registry or null, if none was found.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media stream id the peer is transmitting on.</param>
        /// <returns></returns>
        protected PlaybackComponent GetPlaybackComponent(string roomName, ulong peerId, long mediaId)
        {
            var dictionaryKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out PlaybackComponent foundPlaybackComponent))
            {
                return foundPlaybackComponent;
            }

            return null;
        }

        /// <summary>
        /// Spawn a new instance of <see cref="playbackComponentPrefab"/> and connect it to an ODIN stream using the
        /// room name, peer id and media id.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The ODIN peers Id.</param>
        /// <param name="mediaId">The media stream id the peer is transmitting on.</param>
        /// <returns>The spawned PlaybackComponent or null, if the tuple (roomName, peerId, mediaId) was already
        /// registered on this user</returns>
        protected PlaybackComponent SpawnPlaybackComponent(string roomName, ulong peerId, long mediaId)
        {
            PlaybackComponent spawned = null;
            var dictionaryKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);
            if (!_registeredRemoteMedia.ContainsKey(dictionaryKey))
            {
                Transform parentTransform = null == instantiationTarget ? transform : instantiationTarget;
                spawned = Instantiate(playbackComponentPrefab.gameObject, parentTransform)
                    .GetComponent<PlaybackComponent>();
                
                Debug.Log($"ODIN: Spawned PlaybackComponent for {dictionaryKey}: {spawned}");


                spawned.RoomName = roomName;
                spawned.PeerId = peerId;
                spawned.MediaStreamId = mediaId;

                _registeredRemoteMedia.Add(dictionaryKey, spawned);
                OnMediaStreamEstablished?.Invoke(dictionaryKey);
            }

            return spawned;
        }
        
        
    }
}