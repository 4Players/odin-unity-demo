using System;
using OdinNative.Unity.Audio;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    ///     Structure used to uniquely compare roomname, peerid and mediaid tuples. These three values uniquely identify a
    ///     media
    ///     stream.
    /// </summary>
    public struct OdinConnectionIdentifier : IEquatable<OdinConnectionIdentifier>
    {
        /// <summary>
        /// The room name.
        /// </summary>
        public string RoomName;
        /// <summary>
        /// The peer id.
        /// </summary>
        public ulong PeerId;
        /// <summary>
        /// The media id.
        /// </summary>
        public long MediaId;

        public OdinConnectionIdentifier(PlaybackComponent playbackComponent) : this(playbackComponent.RoomName,
            playbackComponent.PeerId, playbackComponent.MediaStreamId)
        {
        }

        public OdinConnectionIdentifier(string roomName, ulong peerId, long mediaId)
        {
            RoomName = roomName;
            PeerId = peerId;
            MediaId = mediaId;
        }


        /// <summary>
        /// Checks for equal field values.
        /// </summary>
        /// <param name="other">Tht other identifier.</param>
        /// <returns>True, if all field values are equal.</returns>
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
                var hashCode = RoomName != null ? RoomName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ PeerId.GetHashCode();
                hashCode = (hashCode * 397) ^ MediaId.GetHashCode();
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

        public override string ToString()
        {
            return $"Room: {RoomName}, Peer: {PeerId}, Media: {MediaId}";
        }
    }
}