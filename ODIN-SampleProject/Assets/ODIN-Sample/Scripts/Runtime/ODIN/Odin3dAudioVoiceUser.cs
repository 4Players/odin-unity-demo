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


        /// <summary>
        ///     If this is a remote player and we register that a new media has been added, request the peer id for the room
        ///     in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="roomObj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object roomObj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            if (roomObj is Room room)
            {
                UpdateRoomPlayback(room, mediaAddedEventArgs.Peer);
                Debug.Log(
                    $"On Media removed: {room.Config.Name}, {mediaAddedEventArgs.Peer.Id}, {mediaAddedEventArgs.Media.Id}");
            }
        }

        /// <summary>
        /// Destroys playback components connected to the media stream that was just removed.
        /// </summary>
        /// <param name="roomObj"></param>
        /// <param name="mediaRemovedArgs"></param>
        private void OnMediaRemoved(object roomObj, MediaRemovedEventArgs mediaRemovedArgs)
        {
            if (roomObj is Room room && null != mediaRemovedArgs.Peer)
            {
                DestroyPlayback(room.Config.Name, mediaRemovedArgs.Peer.Id, mediaRemovedArgs.MediaStreamId);
                Debug.Log(
                    $"On Media removed: {room.Config.Name}, {mediaRemovedArgs.Peer.Id}, {mediaRemovedArgs.MediaStreamId}");
            }
        }

        /// <summary>
        /// Spawns a playback component for each media stream in the newly joined room.
        /// </summary>
        /// <param name="arg0"></param>
        private void OnJoinedRoom(RoomJoinedEventArgs arg0)
        {
            Debug.Log($"On Joined room: {arg0.Room.Config.Name}");
            StartCoroutine(DeferredSpawnPlayback());
        }

        /// <summary>
        /// Destroys all playbacks that are connected to the left room.
        /// </summary>
        /// <param name="roomLeftArgs"></param>
        private void OnLeftRoom(RoomLeftEventArgs roomLeftArgs)
        {
            Debug.Log($"On Left room: {roomLeftArgs.RoomName}");
            DestroyAllPlaybacksInRoom(roomLeftArgs.RoomName);
        }

        /// <summary>
        ///     Defer spawning in cases where the room was not yet updated.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DeferredSpawnPlayback()
        {
            yield return null;
            foreach (Room room in OdinHandler.Instance.Rooms)
            foreach (Peer remotePeer in room.RemotePeers)
                UpdateRoomPlayback(room, remotePeer);
        }


        /// <summary>
        /// Checks if a playback should be created, based on changes to the user data. E.g. if the users unique user id
        /// was transmitted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="peerUpdatedEventArgs"></param>
        private void OnPeerUpdated(object sender, PeerUserDataChangedEventArgs peerUpdatedEventArgs)
        {
            if (sender is Room room)
                // Debug.Log("3d Odin Voice User: On Peer Updated.");
                UpdateRoomPlayback(room, peerUpdatedEventArgs.Peer);
        }

        /// <summary>
        ///     Checks for each mediastream connected to the peer <see cref="peer" /> in the room <see cref="room" />
        ///     whether a Playback Components was already created and initialized.
        /// </summary>
        /// <param name="room">The room, for which media streams should be updated.</param>
        /// <param name="peer">The peer, for which media streams should be updated</param>
        private void UpdateRoomPlayback(Room room, Peer peer)
        {
            if (null != peer && null != peer.UserData)
            {
                OdinSampleUserData displayedPeerUserData =
                    new UserData(peer.UserData.Buffer).ToOdinSampleUserData();
                if (null != displayedPeerUserData &&
                    displayedPeerUserData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                    if (room.RemotePeers.Contains(peer.Id) && IsRoomAllowed(room.Config.Name))
                        foreach (var mediaStream in peer.Medias)
                            SpawnPlaybackComponent(room.Config.Name, peer.Id, mediaStream.Id);
            }
        }

        /// <summary>
        /// Checks, whether the room id is contained in the <see cref="connectedOdinRooms"/> list.
        /// </summary>
        /// <param name="roomId">Room id to check</param>
        /// <returns>True, if playbacks should be created for this room.</returns>
        private bool IsRoomAllowed(string roomId)
        {
            foreach (var connectedOdinRoom in connectedOdinRooms)
                if (roomId == connectedOdinRoom)
                    return true;
            return false;
        }
    }
}