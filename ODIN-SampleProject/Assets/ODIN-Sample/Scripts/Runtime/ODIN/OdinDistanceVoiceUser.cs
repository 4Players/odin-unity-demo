using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using Room = OdinNative.Odin.Room.Room;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Automatically creates a PlaybackComponent on a remote player for each room the owning (local) player is connected to.
    /// Will only use rooms listed in <see cref="connectedOdinRooms"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class OdinDistanceVoiceUser : AOdinUser
    {
        [SerializeField] private AOdinMultiplayerAdapter odinAdapter;
        
        /// <summary>
        /// All rooms which should be handled by the odin voice user script.
        /// The script won't automatically create Playback Components for rooms that aren't listed here.
        /// </summary>
        [SerializeField] private OdinStringVariable[] connectedOdinRooms;
        
        /// <summary>
        /// Contains all pairs of roomname to peer ids associated to the player owning the photonView
        /// </summary>
        private readonly Dictionary<string, ulong> _roomToPeerIds = new Dictionary<string, ulong>();
        
        protected override void Awake()
        {
            base.Awake();
            Assert.IsTrue(connectedOdinRooms.Length > 0);
            Assert.IsNotNull(odinAdapter);
        }
        
        public void OnEnable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        }

        public void OnDisable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
        }
        
        /// <summary>
        /// If this is a remote player and we register, that a new media has been added, request the peer id for the room
        /// in which the media has been created from the actual owner of the Photon View (= the actual player).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="mediaAddedEventArgs"></param>
        private void OnMediaAdded(object obj, MediaAddedEventArgs mediaAddedEventArgs)
        {
            if (null != mediaAddedEventArgs)
            {
                string mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
                foreach (OdinStringVariable connectedOdinRoom in connectedOdinRooms)
                {
                    if (connectedOdinRoom == mediaRoomName)
                    {
                        OdinSampleUserData userData = mediaAddedEventArgs.Peer.UserData.ToOdinSampleUserData();
                        if (userData.playerId == odinAdapter.GetUniqueUserId())
                        {
                            SpawnPlaybackComponent(mediaRoomName, mediaAddedEventArgs.PeerId, mediaAddedEventArgs.Media.Id);
                        }
                    }
                }
            }
        }
        //
        // /// <summary>
        // /// Handles a request from a remote player for the peer id of the current user in the room <see cref="roomName"/>.
        // /// </summary>
        // /// <remarks>
        // /// Because room.Self.Id currently does not give reliable results when opening multiple clients on the same pc,
        // /// we're using the room.MicrophoneMedia.GetPeerId() to retrieve the peer id for the current user in a specific room.
        // /// </remarks>
        // /// <param name="roomName"></param>
        // [PunRPC]
        // private void OnRequestedPeerIds(string roomName)
        // {
        //     if (photonView.IsMine)
        //     {
        //         if (OdinHandler.Instance.Rooms.Contains(roomName))
        //         {
        //             Room room = OdinHandler.Instance.Rooms[roomName];
        //             if (null != room.MicrophoneMedia)
        //             {
        //                 ulong peerId = room.MicrophoneMedia.GetPeerId();
        //                 if (PhotonNetwork.IsConnectedAndReady)
        //                     photonView.RPC("OnReceivedPeerIdUpdate", RpcTarget.Others, room.Config.Name, (long)peerId);
        //             }
        //             else
        //             {
        //                 Debug.LogError($"MicrophoneMedia in room {room.Config.Name} is null.");
        //             }
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// Message containing the peer id for a given room, which was sent for synchronization by the owning photonView
        // /// (the actual player).
        // /// </summary>
        // /// <param name="roomId"></param>
        // /// <param name="peerId"></param>
        // [PunRPC]
        // private void OnReceivedPeerIdUpdate(string roomId, long peerId)
        // {
        //     if (!photonView.IsMine)
        //     {
        //         Debug.Log($"OnUpdatePeerId: {roomId}, Peer: {peerId}");
        //         _roomToPeerIds[roomId] = (ulong)peerId;
        //         UpdateAllPlaybacks();
        //     }
        // }
        //
        // /// <summary>
        // /// Iterates through all room+peer combinations registered for this player and creates playbackcomponents for
        // /// that combination, if not yet available.
        // /// </summary>
        // private void UpdateAllPlaybacks()
        // {
        //     foreach (string roomId in _roomToPeerIds.Keys)
        //     {
        //         ulong peerId = _roomToPeerIds[roomId];
        //         if (OdinHandler.Instance.Rooms.Contains(roomId))
        //         {
        //             UpdateRoomPlayback(roomId, peerId);
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// Checks for each mediastream connected to the peer <see cref="peerId"/> in the room <see cref="roomId"/>
        // /// whether a Playback Components was already created and initialized.
        // /// </summary>
        // /// <param name="roomId"></param>
        // /// <param name="peerId"></param>
        // private void UpdateRoomPlayback(string roomId, ulong peerId)
        // {
        //     Room room = OdinHandler.Instance.Rooms[roomId];
        //     if (room.RemotePeers.Contains(peerId))
        //     {
        //         Peer peer = room.RemotePeers[peerId];
        //         foreach (MediaStream mediaStream in peer.Medias)
        //         {
        //             SpawnPlaybackComponent(roomId, peer.Id, mediaStream.Id);
        //         }
        //     }
        // }
    }
}