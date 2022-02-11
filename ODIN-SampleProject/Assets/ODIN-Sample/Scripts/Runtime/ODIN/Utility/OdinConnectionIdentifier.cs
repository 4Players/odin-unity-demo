using System;
using OdinNative.Unity.Audio;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    /// Structure used to uniquely compare roomname, peerid and mediaid tuples. These three values uniquely identify a media
    /// stream.
    /// </summary>
    public struct OdinConnectionIdentifier : IEquatable<OdinConnectionIdentifier>
    {
        public string RoomName;
        public ulong PeerId;
        public int MediaId;

        public OdinConnectionIdentifier(PlaybackComponent playbackComponent) : this(playbackComponent.RoomName, playbackComponent.PeerId, playbackComponent.MediaId)
        {
            
        }

        public OdinConnectionIdentifier(string roomName, ulong peerId, int mediaId)
        {
            RoomName = roomName;
            PeerId = peerId;
            MediaId = mediaId;
        }


        public bool Equals(OdinConnectionIdentifier other)
        {
            return RoomName == other.RoomName && PeerId == other.PeerId && MediaId == other.MediaId;
        }

        public override bool Equals(object obj)
        {
            return obj is OdinConnectionIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RoomName != null ? RoomName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PeerId.GetHashCode();
                hashCode = (hashCode * 397) ^ MediaId;
                return hashCode;
            }
        }
        
        public static bool operator ==(OdinConnectionIdentifier id1, OdinConnectionIdentifier id2)
        {
            return id1.Equals(id2);
        }

        public static bool operator !=(OdinConnectionIdentifier id1, OdinConnectionIdentifier id2)
        {
            return !(id1 == id2);
        }
    }
}