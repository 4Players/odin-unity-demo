using System.Collections;
using System.Collections.Generic;
using System;
using OdinNative.Core;

namespace OdinNative.Odin.Room
{
    /// <summary>
    /// A set of values that are used when creating Rooms
    /// </summary>
    public class RoomConfig
    {
        /// <summary>
        /// Room associated AccessKey 
        /// </summary>
        public string AccessKey;
        /// <summary>
        /// Room associated Token lifetime 
        /// </summary>
        public ulong TokenLifetime;
        /// <summary>
        /// Room name
        /// </summary>
        public string Name;
        /// <summary>
        /// Room token
        /// </summary>
        public string Token;
        /// <summary>
        /// Room associated endpoint 
        /// </summary>
        public string Server;
        /// <summary>
        /// true if <see cref="OdinNative.Odin.Room.Room.RegisterEventCallback"/> where set and registered in ODIN ffi
        /// </summary>
        public bool HasEventCallbacks;
        /// <summary>
        /// Configuration for <see cref="OdinNative.Core.Imports.NativeBindings.OdinApmConfig"/>
        /// </summary>
        public OdinRoomConfig ApmConfig;
        /// <summary>
        /// Configuration for <see cref="OdinNative.Odin.Media.MediaStream"/> on new medias
        /// </summary>
        public OdinMediaConfig PlaybackMediaConfig;
    }
}
