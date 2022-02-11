using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Odin.Media
{
    /// <summary>
    /// Intern media dictionary
    /// </summary>
    /// <remarks>Used by <see cref="OdinNative.Odin.Peer.Peer"/></remarks>
    public class MediaCollection : IReadOnlyCollection<MediaStream>, IEqualityComparer<MediaStream>
    {
        private volatile ConcurrentDictionary<int, MediaStream> _Medias;

        public MediaCollection()
        {
            _Medias = new ConcurrentDictionary<int, MediaStream>();
        }

        /// <summary>
        /// Try to get Media by id
        /// </summary>
        /// <param name="key">MediaId</param>
        /// <returns>MediaStream or null</returns>
        public MediaStream this[int key]
        {
            get
            {
                if (_Medias != null && _Medias.TryGetValue(key, out MediaStream media))
                    return media;

                return null;
            }
        }

        public int Count => _Medias.Count;

        public bool IsReadOnly => false;

        public bool Add(MediaStream item)
        {
            return _Medias.TryAdd(item.Id, item);
        }

        public void Clear()
        {
            FreeAll();
            _Medias.Clear();
        }

        public bool Contains(int id)
        {
            return _Medias.ContainsKey(id);
        }

        public bool Contains(MediaStream item)
        {
            return _Medias.Values.Contains(item);
        }

        public void CopyTo(MediaStream[] array, int arrayIndex)
        {
            _Medias.Values.CopyTo(array, arrayIndex);
        }

        public bool Equals(MediaStream x, MediaStream y)
        {
            return x.Id == y.Id;
        }

        public IEnumerator<MediaStream> GetEnumerator()
        {
            return _Medias.Values.GetEnumerator();
        }

        public int GetHashCode(MediaStream obj)
        {
            return obj.GetHashCode();
        }

        public bool Remove(int id)
        {
            return _Medias.TryRemove(id, out _);
        }

        public bool Remove(MediaStream item)
        {
            return Remove(item.Id);
        }

        internal bool Free(int id)
        {
            bool result = _Medias.TryRemove(id, out MediaStream media);
            if (result) media.Dispose();
            return result;
        }

        internal void FreeAll()
        {
            foreach (var kvp in _Medias)
                Free(kvp.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Medias.GetEnumerator();
        }
    }
}
