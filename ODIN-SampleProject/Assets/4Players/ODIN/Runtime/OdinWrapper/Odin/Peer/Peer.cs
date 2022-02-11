using OdinNative.Odin.Media;
using OdinNative.Odin.Room;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OdinNative.Odin.Peer
{
    /// <summary>
    /// Client/Remote peer
    /// </summary>
    public class Peer : IDisposable
    {
        /// <summary>
        /// Peer id
        /// </summary>
        public ulong Id { get; private set; }
        /// <summary>
        /// Associated room name of this peer
        /// </summary>
        public string RoomName { get; private set; }
        /// <summary>
        /// Peers user id
        /// </summary>
        public string UserId { get; internal set; }
        /// <summary>
        /// Peer userdata
        /// </summary>
        public UserData UserData { get; private set; }
        /// <summary>
        /// Associated medias of this peer
        /// </summary>
        public MediaCollection Medias { get; private set; }

        public Peer(ulong id, string roomName, UserData userData)
        {
            Id = id;
            RoomName = roomName;
            UserData = userData ?? new UserData();
            Medias = new MediaCollection();
        }

        public void AddMedia(PlaybackStream stream)
        {
            Medias.Add(stream);
        }

        public bool RemoveMedia(int mediaId)
        {
            return Medias.Remove(mediaId);
        }

        internal void SetUserData(byte[] newData)
        {
            UserData = new UserData(newData);
        }

        internal void SetUserData(UserData userData)
        {
            UserData = userData;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Medias.FreeAll();
                    UserData = null;
                }

                disposedValue = true;
            }
        }

        ~Peer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}