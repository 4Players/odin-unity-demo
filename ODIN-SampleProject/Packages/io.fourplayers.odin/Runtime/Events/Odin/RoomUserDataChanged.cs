using System;
using UnityEngine.Events;
using OdinNative.Odin.Room;

namespace OdinNative.Unity.Events
{
    /// <summary>
    /// This class provides the base functionality for UnityEvents based <see cref="OdinNative.Odin.Room.RoomUserDataChangedEventHandler"/>.
    /// A persistent callback that can be saved with the Scene.
    /// Unity Inspector event wrapper <see href="https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html">(UnityEvent)</see>
    /// </summary>
    /// <remarks>Changing a Room's UserData is only possible via request to Odin server API, but not supported inside the client SDK.</remarks>
    [Serializable]
    public class RoomUserDataChangedProxy : UnityEvent<object, RoomUserDataChangedEventArgs>
    {
    }
}