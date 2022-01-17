using System;
using System.Collections.Generic;
using OdinNative.Unity.Audio;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Scriptable object for registering playback components. Can be used to store play back components spawned by an
    /// <see cref="AOdinUser"/> implementation and visualizing all connected players using an UI script, without having
    /// to provide direct references.
    /// </summary>
    [CreateAssetMenu(fileName = "PlaybackRegistry", menuName = "Odin-Sample/PlaybackRegistry", order = 0)]
    public class OdinPlaybackRegistry : ScriptableObject
    {
        /// <summary>
        /// Called when a new PlaybackComponent was registered.
        /// </summary>
        public Action<PlaybackComponent> OnPlaybackComponentAdded;

        /// <summary>
        /// Called when a PlaybackComponent was removed from the registry.
        /// </summary>
        public Action<PlaybackComponent> OnPlaybackComponentRemoved;

        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (roomname, peerid, mediaid) combination.
        /// </summary>
        private readonly Dictionary<(string, ulong, int), PlaybackComponent> _registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();


        /// <summary>
        /// Adds the given PlaybackComponent to the registry. PlaybackComponents are identified by the connected
        /// room name, peer id and media id, so please ensure that those are correctly initialized.
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddComponent(PlaybackComponent toAdd)
        {
            var roomName = toAdd.RoomName;
            var peerId = toAdd.PeerId;
            int mediaId = toAdd.MediaId;
            var dictionaryKey = (roomName, peerId, mediaId);

            if (_registeredRemoteMedia.ContainsKey(dictionaryKey))
                Debug.LogWarning(
                    "Registering PlaybackComponent with identical (room name, peer id and media id) to another PlaybackComponent - the old PlaybackComponent will be replaced in the registry. ");
            _registeredRemoteMedia[dictionaryKey] = toAdd;
            OnPlaybackComponentAdded?.Invoke(toAdd);
        }

        /// <summary>
        /// Determines whether a component identified by the room Name, peer id and media id was already added to the registry.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The peer id of the user in the room.</param>
        /// <param name="mediaId">The media id of media transmitting the peer's voice.</param>
        /// <returns>True, if a component was already registered for the given identifiers, false otherwise.</returns>
        public bool ContainsComponent(string roomName, ulong peerId, int mediaId)
        {
            return _registeredRemoteMedia.ContainsKey((roomName, peerId, mediaId));
        }

        /// <summary>
        /// Removes the component identified by the room name, peer id and media id.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The peer id of the user in the room.</param>
        /// <param name="mediaId">The media id of media transmitting the peer's voice.</param>
        /// <returns>The removed PlaybackComponent or null, if no component was registered for the given identifiers.</returns>
        public PlaybackComponent RemoveComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out var toRemove))
            {
                _registeredRemoteMedia.Remove(dictionaryKey);
                OnPlaybackComponentRemoved.Invoke(toRemove);
                return toRemove;
            }

            return null;
        }

        /// <summary>
        /// Returns a component identified by the room name, peer id and media id.
        /// </summary>
        /// <param name="roomName">The ODIN room name.</param>
        /// <param name="peerId">The peer id of the user in the room.</param>
        /// <param name="mediaId">The media id of media transmitting the peer's voice.</param>
        /// <returns>The PlaybackComponent given by the identifiers or null, if no component was registered for the identifiers..</returns>
        public PlaybackComponent GetComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out var playbackComponent)) return playbackComponent;
            return null;
        }
    }
}