using OdinNative.Odin;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
    /// <summary>
    /// Demo Example where everything in #region Demo does not correlate with Odin
    /// </summary>
    public class GameController : MonoBehaviour
    {
        // Demo Peer
        public GameObject PeerPrefab;
        // Demo GameObjects
        public List<GameObject> PeersObjects;


        // Start is called before the first frame update
        void Start()
        {
            PeersObjects = new List<GameObject>();


            /* Join a Room (with optionally defined userdata)
             * 
             * *The underlying type of UserData is a simple byte-array and
             * can be any format/protocol like Raw-Binary, MessagePack, Protobuf, Json, ...
             */
            // optional set arbitrary data or a unique userdata on join to identify this peer
            var customUserData = new UserData(SystemInfo.deviceUniqueIdentifier);
            OdinHandler.Instance.JoinRoom("Unity-DemoRoom", customUserData);
        }

        /// <summary>
        /// Simple example how to Update own UserData
        /// </summary>
        public void DemoUpdateUserData(RoomJoinedEventArgs args)
        {
            Debug.Log($"{nameof(GameController)} Demo: Room \"{args.Room.Config.Name}\" joined with \"{args.Room.Self.UserData}\"");

            var playerName = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault()?.name ?? "DemoPlayerName123";
            // optional ignore currently joined peers deviceUniqueIdentifier userdata and update with arbitrary data
            if (args.Room.UpdateUserData(new UserData(playerName)))
                Debug.Log($"{nameof(GameController)} Demo: New UserData is \"{playerName}\"");
        }

        /// <summary>
        /// Simple UnityEvent call for Peer joined
        /// </summary>
        /// <remarks>Set by Unity-Inspector of OdinManagerPrefab->OdinHandlerComponent->Peer joined</remarks>
        public void LogHelloPeer(object sender, PeerJoinedEventArgs args)
        {
            Debug.Log($"{nameof(GameController)} Demo: Hello, Peer with ID {args.PeerId}. :)");
        }

        /// <summary>
        /// Simple UnityEvent call for Media added
        /// </summary>
        /// <remarks>Set by Unity-Inspector of OdinManagerPrefab->OdinHandlerComponent->Media added</remarks>
        public void LogNewMedia(object sender, MediaAddedEventArgs args)
        {
            Debug.Log($"{nameof(GameController)} Demo: New Media {args.Media.Id} from Peer {args.PeerId}.");
        }

        /// <summary>
        /// Example to create and assign the PlaybackComponent
        /// </summary>
        /// <remarks>Set by Unity-Inspector of OdinManagerPrefab->OdinHandlerComponent->Media added</remarks>
        public void CreateGameObjectWithMedia(object sender, MediaAddedEventArgs args)
        {
            if (sender is Room == false) return;
            Room room = sender as Room;
            if (room.Self == null || room.Self.Id == args.PeerId) return; // Skip the own media

            // create a prefab as peer container for the PlaybackComponent
            var peerContainer = Instantiate(PeerPrefab, new Vector3(0, 1, 6), Quaternion.identity);
            PlaybackComponent playback = OdinHandler.Instance.AddPlaybackComponent(peerContainer, room.Config.Name, args.PeerId, args.Media.Id);

            // setup the AudioSource attached to the PlaybackComponent for Linear-3D-Rolloff as default
            playback.PlaybackSource.spatialBlend = 1.0f;
            playback.PlaybackSource.rolloffMode = AudioRolloffMode.Linear;
            playback.PlaybackSource.minDistance = 1;
            playback.PlaybackSource.maxDistance = 10;

            // set demo TextMesh with peer info and print optional UserData
            string peerLabel = $"Peer {args.PeerId} Media {args.Media.Id}";
            string data = room.RemotePeers[args.PeerId]?.UserData?.ToString() ?? string.Empty;
            Debug.Log($"{nameof(GameController)} Demo: {peerLabel} Data \"{data}\"");
            playback.gameObject.GetComponentInChildren<TextMesh>().text = peerLabel;

            // optional keep track of objects with a PlaybackComponent because we can easily just Destroy() the object/component to cleanup
            PeersObjects.Add(playback.gameObject);
        }

        /// <summary>
        /// Example to Destroy the PlaybackComponent from a GameObject
        /// </summary>
        /// <remarks>Set by Unity-Inspector of OdinManagerPrefab->OdinHandlerComponent->Media removed</remarks>
        public void DeleteMediaFromGameObject(object sender, MediaRemovedEventArgs args)
        {
            /* In this case, since the media got attached to a Unity GameObject, 
             * we can just Destroy() the MonoBehaviour-Object and Unity will handle the dispose.
             * 
             * To cleanup medias manually from OdinHandler.Instance i.e. Leave() room, Free() peer or just dispose the media object.
             */
            // get PlaybackComponent by media id
            PlaybackComponent playback = PeersObjects
                .Select(gameObj => gameObj.GetComponent<PlaybackComponent>())
                .FirstOrDefault(playbackComponent => playbackComponent != null && playbackComponent.MediaId == args.Media.Id);

            
            if (playback == null) return;

            //Destroy(playback);

            /* 
             * Just in this Demo, we delete the GameObject for cleanup reasons. (Media removed)
             * This would be a agent aka remote player character and should probably not be destroyed here.
             */
            PeersObjects.Remove(playback.gameObject);
            Destroy(playback.gameObject);
        }
    }
}