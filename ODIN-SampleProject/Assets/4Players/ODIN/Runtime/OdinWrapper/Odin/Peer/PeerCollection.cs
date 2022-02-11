using OdinNative.Odin.Media;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OdinNative.Odin.Peer
{
    /// <summary>
    /// Intern peer dictionary
    /// </summary>
    /// <remarks>Used by <see cref="OdinNative.Odin.Room.Room.RemotePeers"/></remarks>
    public class PeerCollection : IReadOnlyCollection<Peer>, IEqualityComparer<Peer>
    {
        private ConcurrentDictionary<ulong, Peer> _Peers;

        public PeerCollection()
        {
            _Peers = new ConcurrentDictionary<ulong, Peer>();
        }

        /// <summary>
        /// Try to get Peer by id
        /// </summary>
        /// <param name="key">Peer Id</param>
        /// <returns>Peer or null</returns>
        public Peer this[ulong key]
        {
            get
            {
                if (_Peers != null && _Peers.TryGetValue(key, out Peer peer))
                    return peer;

                return null;
            }
        }

        public int Count => _Peers.Count;

        public bool IsReadOnly => false;

        public bool Add(Peer item)
        {
            return _Peers.TryAdd(item.Id, item);
        }

        public void Clear()
        {
            FreeAll();
            _Peers.Clear();
        }

        public bool Contains(ulong id)
        {
            return _Peers.ContainsKey(id);
        }

        public bool Contains(Peer item)
        {
            return _Peers.Values.Contains(item);
        }

        public void CopyTo(Peer[] array, int arrayIndex)
        {
            _Peers.Values.CopyTo(array, arrayIndex);
        }

        public bool Equals(Peer x, Peer y)
        {
            return x.Id == y.Id;
        }

        public IEnumerator<Peer> GetEnumerator()
        {
            return _Peers.Values.GetEnumerator();
        }

        public int GetHashCode(Peer obj)
        {
            return obj.GetHashCode();
        }

        public bool Remove(ulong id)
        {
            return _Peers.TryRemove(id, out _);
        }

        public bool Remove(Peer item)
        {
            return Remove(item.Id);
        }

        internal bool Free(ulong id)
        {
            bool result = _Peers.TryRemove(id, out Peer peer);
            if (result) peer.Dispose();
            return result;
        }

        internal void FreeAll()
        {
            foreach (var kvp in _Peers)
                Free(kvp.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Peers.GetEnumerator();
        }
    }
}