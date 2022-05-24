using System.Collections;
using OdinNative.Odin;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    ///     Automatically creates the PlaybackComponents for a single remote player. The remote player will be
    ///     identified by the id provided by the referenced <see cref="AOdinMultiplayerAdapter" />.
    ///     Will only spawn PlaybackComponents for rooms listed in <see cref="connectedOdinRooms" />.
    /// </summary>
    /// <remarks>
    ///     This script is only required for users, whose transmission should be played as 3D audio, e.g. in-game voice
    ///     on a player's character. For 2D voice, which can be heard everywhere, the <see cref="OdinDefaultUser" /> script
    ///     is sufficient and easier to setup.
    /// </remarks>
    [DisallowMultipleComponent]
    public class Odin3dAudioVoiceUser : AOdinUser
    {
        /// <summary>
        ///     Reference to the Multiplayer Adapter, which represents the currently transmitting player in your game.
        /// </summary>
        [FormerlySerializedAs("odinAdapter")] [SerializeField]
        private AOdinMultiplayerAdapter multiplayerAdapter;

        /// <summary>
        ///     All rooms which should be handled by the odin voice user script.
        ///     The script will only create Playback Components for rooms that are listed here.
        /// </summary>
        [SerializeField] private OdinStringVariable[] connectedOdinRooms;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsTrue(connectedOdinRooms.Length > 0);
            Assert.IsNotNull(multiplayerAdapter);
        }

        public void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        public void OnDisable()
        {
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
                OdinHandler.Instance.OnPeerUserDataChanged.RemoveListener(OnPeerUpdated);
            }

            DestroyAllPlaybacks();
        }

        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
            OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
            OdinHandler.Instance.OnRoomJoined.AddListener(OnJoinedRoom);
            OdinHandler.Instance.OnRoomLeft.AddListener(OnLeftRoom);



            OdinHandler.Instance.OnPeerUserDataChanged.AddListener(OnPeerUpdated);

            // yield return new WaitForSeconds(2.0f);
            StartCoroutine(DeferredSpawnPlayback());
        }

        private void OnLeftRoom(RoomLeftEventArgs roomLeftArgs)
        {
            DestroyAllPlaybacksInRoom(roomLeftArgs.RoomName);
        }
        
        private void OnMediaRemoved(object roomObj, MediaRemovedEventArgs mediaRemovedArgs)
        {
            if (roomObj is Room room && null != mediaRemovedArgs.Peer)
            {
                DestroyPlayback(room.Config.Name, mediaRemovedArgs.Peer.Id, mediaRemovedArgs.MediaId);
            }
        }

        private void OnJoinedRoom(RoomJoinedEventArgs arg0)
        {
            StartCoroutine(DeferredSpawnPlayback());
        }

        private IEnumerator DeferredSpawnPlayback()
        {
            yield return null;
            foreach (Room room in OdinHandler.Instance.Rooms)
            foreach (Peer remotePeer in room.RemotePeers)
                UpdateRoomPlayback(room, remotePeer);
        }


        private void OnPeerUpdated(object sender, PeerUserDataChangedEventArgs peerUpdatedEventArgs)
        {
            if (sender is Room room)
            {
                // Debug.Log("3d Odin Voice User: On Peer Updated.");
                UpdateRoomPlayback(room, peerUpdatedEventArgs.Peer);
            }
        }

        /// <summary>
        ///     If this is a remote player and we register that a new media has been added, request the peer id for the room
        ///     in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="roomObj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object roomObj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            // var mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            // if (IsRoomAllowed(mediaRoomName))
            //     if (null != mediaAddedEventArgs.Peer.UserData)
            //     {
            //         var userData = mediaAddedEventArgs.Peer.UserData.ToOdinSampleUserData();
            //         if (userData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
            //             SpawnPlaybackComponent(mediaRoomName, mediaAddedEventArgs.PeerId, mediaAddedEventArgs.Media.Id);
            //     }
            if (roomObj is Room room)
            {
                UpdateRoomPlayback(room, mediaAddedEventArgs.Peer);
            }
        }

        /// <summary>
        ///     Checks for each mediastream connected to the peer <see cref="peer" /> in the room <see cref="room" />
        ///     whether a Playback Components was already created and initialized.
        /// </summary>
        /// <param name="room">The room, for which media streams should be updated.</param>
        /// <param name="peer">The peer, for which media streams should be updated</param>
        private void UpdateRoomPlayback(Room room, Peer peer)
        {
            OdinSampleUserData displayedPeerUserData =
                new UserData(peer.UserData.Buffer).ToOdinSampleUserData();
            if (null != displayedPeerUserData &&
                displayedPeerUserData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                if (room.RemotePeers.Contains(peer.Id) && IsRoomAllowed(room.Config.Name))
                    foreach (var mediaStream in peer.Medias)
                        SpawnPlaybackComponent(room.Config.Name, peer.Id, mediaStream.Id);
        }

        private bool IsRoomAllowed(string roomId)
        {
            foreach (var connectedOdinRoom in connectedOdinRooms)
                if (roomId == connectedOdinRoom)
                    return true;
            return false;
        }
    }
}