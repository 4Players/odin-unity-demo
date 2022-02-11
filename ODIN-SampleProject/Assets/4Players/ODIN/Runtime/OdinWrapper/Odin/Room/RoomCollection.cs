using OdinNative.Core.Handles;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Odin.Room
{
    /// <summary>
    /// Intern room dictionary
    /// </summary>
    /// <remarks>Used by <see cref="OdinNative.Odin.OdinClient.Rooms"/></remarks>
    public class RoomCollection : IReadOnlyCollection<Room>, IEqualityComparer<Room>
    {
        private volatile ConcurrentDictionary<string, Room> _Rooms;

        public RoomCollection()
        {
            _Rooms = new ConcurrentDictionary<string, Room>();
        }

        /// <summary>
        /// Try to get room by name
        /// </summary>
        /// <param name="key">Room name</param>
        /// <returns>Room or null</returns>
        public Room this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return null;

                if (_Rooms != null && _Rooms.TryGetValue(key, out Room room))
                    return room;

                return null;
            }
        }

        internal Room this[IntPtr handle]
        {
            get
            {
                if (handle == null || IntPtr.Zero == handle || _Rooms == null) return null;

                return _Rooms.Values.FirstOrDefault(r => r.Handle == handle);
            }
        }

        public int Count => _Rooms.Count;

        public bool IsReadOnly => false;

        public bool Add(Room item)
        {
            if(string.IsNullOrEmpty(item.Config.Name))
                return _Rooms.TryAdd(item.Config.Token, item);
            else
                return _Rooms.TryAdd(item.Config.Name, item);
        }

        public void Clear()
        {
            FreeAll();
            _Rooms.Clear();
        }

        public bool Contains(string key)
        {
            return _Rooms.ContainsKey(key);
        }

        public bool Contains(Room item)
        {
            return _Rooms.Values.Contains(item);
        }

        public void CopyTo(Room[] array, int arrayIndex)
        {
            _Rooms.Values.CopyTo(array, arrayIndex);
        }

        public bool Equals(Room x, Room y)
        {
            return x.Config.Name == y.Config.Name;
        }

        public IEnumerator<Room> GetEnumerator()
        {
            return _Rooms.Values.GetEnumerator();
        }

        public int GetHashCode(Room obj)
        {
            return obj.GetHashCode();
        }

        public bool Remove(string key)
        {
            return _Rooms.TryRemove(key, out _);
        }

        public bool Remove(Room item)
        {
            if (string.IsNullOrEmpty(item.Config.Name))
                return Remove(item.Config.Token);
            else
                return Remove(item.Config.Name);
        }

        internal bool Free(string key)
        {
            bool result = _Rooms.TryRemove(key, out Room room);
            if (result) room.Dispose();
            return result;
        }

        internal void FreeAll()
        {
            foreach (var kvp in _Rooms)
                Free(kvp.Key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Rooms.GetEnumerator();
        }
    }
}
