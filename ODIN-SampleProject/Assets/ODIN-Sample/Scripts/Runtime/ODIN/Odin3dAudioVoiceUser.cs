using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Room = OdinNative.Odin.Room.Room;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Automatically creates a PlaybackComponent on a remote player for each room the owning (local) player is connected to.
    /// Will only use rooms listed in <see cref="connectedOdinRooms"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class Odin3dAudioVoiceUser : AOdinUser
    {
        [FormerlySerializedAs("odinAdapter")] [SerializeField] private AOdinMultiplayerAdapter multiplayerAdapter;
        
        /// <summary>
        /// All rooms which should be handled by the odin voice user script.
        /// The script won't automatically create Playback Components for rooms that aren't listed here.
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
            OdinSampleUserData displayedPeerUserData =
                new UserData(peerUpdatedEventArgs.UserData).ToOdinSampleUserData();
            if (null != displayedPeerUserData && displayedPeerUserData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
            {
                if (sender is Room room)
                {
                    UpdateRoomPlayback(room.Config.Name, peerUpdatedEventArgs.PeerId);
                }
            }
        }
        
        /// <summary>
        /// If this is a remote player and we register, that a new media has been added, request the peer id for the room
        /// in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            
            string mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            foreach (OdinStringVariable connectedOdinRoom in connectedOdinRooms)
            {
                if (connectedOdinRoom == mediaRoomName)
                {
                    OdinSampleUserData userData = mediaAddedEventArgs.Peer.UserData.ToOdinSampleUserData();
                    if (userData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                    {
                        SpawnPlaybackComponent(mediaRoomName, mediaAddedEventArgs.PeerId, mediaAddedEventArgs.Media.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Checks for each mediastream connected to the peer <see cref="peerId"/> in the room <see cref="roomId"/>
        /// whether a Playback Components was already created and initialized.
        /// </summary>
        /// <param name="roomId">The room id, for which media streams should be updated.</param>
        /// <param name="peerId">The peer id, for which media streams should be updated</param>
        private void UpdateRoomPlayback(string roomId, ulong peerId)
        {
            Room room = OdinHandler.Instance.Rooms[roomId];
            if (room.RemotePeers.Contains(peerId))
            {
                Peer peer = room.RemotePeers[peerId];
                foreach (MediaStream mediaStream in peer.Medias)
                {
                    SpawnPlaybackComponent(roomId, peer.Id, mediaStream.Id);
                }
            }
        }
    }
}