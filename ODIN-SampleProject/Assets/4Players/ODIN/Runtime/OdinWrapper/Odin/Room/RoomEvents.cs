using OdinNative.Odin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Odin.Room
{
    /// <summary>
    /// Arguments for RoomJoin events right before the room is joined
    /// </summary>
    public class RoomJoinEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Arguments for RoomJoined events when the room was joined successfully
    /// </summary>
    public class RoomJoinedEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Arguments for RoomLeave events right before the room handle is destroyed
    /// </summary>
    public class RoomLeaveEventArgs : EventArgs
    {
        public Room Room;
    }

    /// <summary>
    /// Arguments for RoomLeft events when the room handle was destroyed
    /// </summary>
    public class RoomLeftEventArgs : EventArgs
    {
        public string RoomName;
    }

    /// <summary>
    /// Arguments for PeerJoined events in the current room
    /// </summary>
    public class PeerJoinedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public string UserId { get; internal set; }
        public Peer.Peer Peer;
    }
    public delegate void RoomPeerJoinedEventHandler(object sender, PeerJoinedEventArgs e);

    /// <summary>
    /// Arguments for PeerLeft events in the current room
    /// </summary>
    public class PeerLeftEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
    }
    public delegate void RoomPeerLeftEventHandler(object sender, PeerLeftEventArgs e);

    /// <summary>
    /// Arguments for PeerUserDataChanged events in the current room
    /// </summary>
    public class PeerUserDataChangedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public Peer.Peer Peer;
        public UserData UserData;
    }
    public delegate void RoomPeerUserDataChangedEventHandler(object sender, PeerUserDataChangedEventArgs e);

    /// <summary>
    /// Arguments for MediaAdded events in the current room
    /// </summary>
    public class MediaAddedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public Peer.Peer Peer;
        public PlaybackStream Media;
    }
    public delegate void RoomMediaAddedEventHandler(object sender, MediaAddedEventArgs e);

    /// <summary>
    /// Arguments for MediaRemoved events in the current room
    /// </summary>
    public class MediaRemovedEventArgs : EventArgs
    {
        public ushort MediaId { get; internal set; }
        public Peer.Peer Peer;
    }
    public delegate void RoomMediaRemovedEventHandler(object sender, MediaRemovedEventArgs e);

    /// <summary>
    /// Arguments for MediaActiveStateChanged events in the current room
    /// </summary>
    public class MediaActiveStateChangedEventArgs : EventArgs
    {
        public ushort MediaId { get; internal set; }
        public ulong PeerId { get; internal set; }
        public bool Active { get; internal set; }
    }
    public delegate void MediaActiveStateChangedEventHandler(object sender, MediaActiveStateChangedEventArgs e);

    /// <summary>
    /// Arguments for RoomUserDataChanged events in the current room
    /// </summary>
    public class RoomUserDataChangedEventArgs : EventArgs
    {
        public string RoomName { get; internal set; }
        public UserData Data;
    }
    public delegate void RoomUserDataChangedEventHandler(object sender, RoomUserDataChangedEventArgs e);

    /// <summary>
    /// Arguments for MessageReceived events in the current room
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public ulong PeerId { get; internal set; }
        public byte[] Data;
    }
    public delegate void RoomMessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
}
