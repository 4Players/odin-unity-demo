using System.Collections.Generic;
using System.Linq;
using OdinNative.Unity.Audio;
using UnityEngine;
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
        public UnityEvent<PlaybackComponent> onPlaybackComponentAdded;
        
        /// <summary>
        /// Contains all constructed PlaybackComponents, identified by their (roomname, peerid, mediaid) combination.
        /// </summary>
        protected Dictionary<(string, ulong, int), PlaybackComponent> registeredRemoteMedia =
            new Dictionary<(string, ulong, int), PlaybackComponent>();
        
        protected void SpawnPlaybackComponent(string roomName, ulong peerId, int mediaId)
        {
            var dictionaryKey = (roomName, peerId, mediaId);

            if (!registeredRemoteMedia.ContainsKey(dictionaryKey))
            {
                Transform parentTransform = null == instantiationTarget ? transform : instantiationTarget;
                PlaybackComponent playbackComponent = Instantiate(playbackComponentPrefab.gameObject, parentTransform)
                    .GetComponent<PlaybackComponent>();

                playbackComponent.RoomName = roomName;
                playbackComponent.PeerId = peerId;
                playbackComponent.MediaId = mediaId;

                registeredRemoteMedia.Add(dictionaryKey, playbackComponent);
                onPlaybackComponentAdded.Invoke(playbackComponent);
            }
        }
    }
}