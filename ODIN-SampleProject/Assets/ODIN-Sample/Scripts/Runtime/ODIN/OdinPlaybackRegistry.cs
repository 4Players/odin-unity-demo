using System.Collections.Generic;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    [CreateAssetMenu(fileName = "PlaybackRegistry", menuName = "Odin-Sample/PlaybackRegistry", order = 0)]
    public class OdinPlaybackRegistry : ScriptableObject
    {
        /// <summary>
        /// Called when a new playbackcomponent was created by this script
        /// </summary>
        public UnityEvent<PlaybackComponent> onPlaybackComponentAdded;
        
        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (roomname, peerid, mediaid) combination.
        /// </summary>
        private Dictionary<(string, ulong, int), PlaybackComponent> _registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();
        
        

        public void AddComponent( string roomName, ulong peerId, int mediaId, PlaybackComponent toAdd)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            _registeredRemoteMedia[dictionaryKey] = toAdd;
            onPlaybackComponentAdded.Invoke(toAdd);
        }

        public bool ContainsComponent(string roomName, ulong peerId, int mediaId)
        {
            return _registeredRemoteMedia.ContainsKey((roomName, peerId, mediaId));
        }

        public PlaybackComponent RemoveComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out PlaybackComponent toRemove))
            {
                _registeredRemoteMedia.Remove(dictionaryKey);
                return toRemove;
            }
            return null;
        }

        public PlaybackComponent GetComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);
            if (_registeredRemoteMedia.TryGetValue(dictionaryKey, out PlaybackComponent playbackComponent))
            {
                return playbackComponent;
            }
            return null;
        }
    }
}