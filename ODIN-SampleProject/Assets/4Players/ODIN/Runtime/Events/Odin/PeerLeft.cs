using System;
using UnityEngine.Events;
using OdinNative.Odin.Room;

namespace OdinNative.Unity.Events
{
    [Serializable]
    public class PeerLeftProxy : UnityEvent<object, PeerLeftEventArgs>
    {
    }
}

