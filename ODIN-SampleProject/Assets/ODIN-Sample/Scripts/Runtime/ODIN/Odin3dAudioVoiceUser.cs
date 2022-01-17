using OdinNative.Odin;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    ///     Automatically creates the PlaybackComponents for a single remote player. The remote player will be
    ///     identified by the id provided by the referenced <see cref="AOdinMultiplayerAdapter"/>.
    ///     Will only spawn PlaybackComponents for rooms listed in <see cref="connectedOdinRooms" />.
    /// </summary>
    /// <remarks>
    ///     This script is only required for users, whose transmission should be played as 3D audio, e.g. in-game voice
    ///     on a player's character. For 2D voice, which can be heard everywhere, the <see cref="OdinDefaultUser"/> script
    ///     is sufficient and easier to setup.
    /// </remarks>
    [DisallowMultipleComponent]
    public class Odin3dAudioVoiceUser : AOdinUser
    {
        /// <summary>
        /// Reference to the Multiplayer Adapter, which represents the currently transmitting player in your game.
        /// </summary>
        [FormerlySerializedAs("odinAdapter")] [SerializeField]
        private AOdinMultiplayerAdapter multiplayerAdapter;

        /// <summary>
        ///     All rooms which should be handled by the odin voice user script.
        ///     The script won't automatically create Playback Components for rooms that aren't listed here.
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
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
                OdinHandler.Instance.OnPeerUpdated.AddListener(OnPeerUpdated);
            }
        }

        public void OnDisable()
        {
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
                OdinHandler.Instance.OnPeerUpdated.RemoveListener(OnPeerUpdated);
            }
        }

        private void OnPeerUpdated(object sender, PeerUpdatedEventArgs peerUpdatedEventArgs)
        {
            var displayedPeerUserData =
                new UserData(peerUpdatedEventArgs.UserData).ToOdinSampleUserData();
            if (null != displayedPeerUserData &&
                displayedPeerUserData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                if (sender is Room room)
                    UpdateRoomPlayback(room.Config.Name, peerUpdatedEventArgs.PeerId);
        }

        /// <summary>
        ///     If this is a remote player and we register that a new media has been added, request the peer id for the room
        ///     in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            var mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            if (IsRoomInAllowedConnectionList(mediaRoomName))
            {
                var userData = mediaAddedEventArgs.Peer.UserData.ToOdinSampleUserData();
                if (userData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                    SpawnPlaybackComponent(mediaRoomName, mediaAddedEventArgs.PeerId, mediaAddedEventArgs.Media.Id);
            }
        }

        /// <summary>
        ///     Checks for each mediastream connected to the peer <see cref="peerId" /> in the room <see cref="roomId" />
        ///     whether a Playback Components was already created and initialized.
        /// </summary>
        /// <param name="roomId">The room id, for which media streams should be updated.</param>
        /// <param name="peerId">The peer id, for which media streams should be updated</param>
        private void UpdateRoomPlayback(string roomId, ulong peerId)
        {
            var room = OdinHandler.Instance.Rooms[roomId];
            if (room.RemotePeers.Contains(peerId) && IsRoomInAllowedConnectionList(roomId))
            {
                var peer = room.RemotePeers[peerId];
                foreach (var mediaStream in peer.Medias) SpawnPlaybackComponent(roomId, peer.Id, mediaStream.Id);
            }
        }

        private bool IsRoomInAllowedConnectionList(string roomId)
        {
            foreach (var connectedOdinRoom in connectedOdinRooms)
                if (roomId == connectedOdinRoom)
                    return true;
            return false;
        }
    }
}