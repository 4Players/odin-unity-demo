using OdinNative.Odin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Odin.Room
{
    /// <summary>
    /// Room join arguments before the room is actually joined
    /// </summary>
    public class RoomJoinEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Room joined arguments after a room is joined
    /// </summary>
    public class RoomJoinedEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Room leave arguments before the room is destroyed
    /// </summary>
    public class RoomLeaveEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Room left arguments after the room is destroyed
    /// </summary>
    public class RoomLeftEventArgs : EventArgs
    {
        public string RoomName;
    }

    /// <summary>
    /// Peer joined arguments after a peer used <see cref="OdinHandler.JoinRoom"/>
    /// </summary>
    public class PeerJoinedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public string UserId { get; internal set; }
        public Peer.Peer Peer;
    }
    public delegate void RoomPeerJoinedEventHandler(object sender, PeerJoinedEventArgs e);

    /// <summary>
    /// Peer left arguments
    /// </summary>
    public class PeerLeftEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
    }
    public delegate void RoomPeerLeftEventHandler(object sender, PeerLeftEventArgs e);
    /// <summary>
    /// Peer updated arguments with arbitrary data
    /// </summary>
    public class PeerUserDataChangedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public Peer.Peer Peer;
        public UserData UserData;
    }
    public delegate void RoomPeerUserDataChangedEventHandler(object sender, PeerUserDataChangedEventArgs e);

    /// <summary>
    /// Media added arguments in the current room
    /// </summary>
    public class MediaAddedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public Peer.Peer Peer;
        public PlaybackStream Media;
    }
    public delegate void RoomMediaAddedEventHandler(object sender, MediaAddedEventArgs e);

    /// <summary>
    /// Media removed arguments in the current room
    /// </summary>
    public class MediaRemovedEventArgs : EventArgs
    {
        public ushort MediaId { get; internal set; }
        public Peer.Peer Peer;
        public MediaStream Media;
    }
    public delegate void RoomMediaRemovedEventHandler(object sender, MediaRemovedEventArgs e);

    /// <summary>
    /// Media activity changed arguments
    /// </summary>
    public class MediaActiveStateChangedEventArgs : EventArgs
    {
        public ushort MediaId { get; internal set; }
        public ulong PeerId { get; internal set; }
        public bool Active { get; internal set; }
    }
    public delegate void MediaActiveStateChangedEventHandler(object sender, MediaActiveStateChangedEventArgs e);

    /// <summary>
    /// RoomUserData changed arguments in the current room
    /// </summary>
    public class RoomUserDataChangedEventArgs : EventArgs
    {
        public string RoomName { get; internal set; }
        public UserData Data;
    }
    public delegate void RoomUserDataChangedEventHandler(object sender, RoomUserDataChangedEventArgs e);

    /// <summary>
    /// Message received arguments in the current room
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public byte[] Data;
    }
    public delegate void RoomMessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
}
