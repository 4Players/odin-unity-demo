using System;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public abstract class AOdinMultiplayerAdapter : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (IsLocalUser())
            {
                if (OdinHandler.Instance.HasConnections)
                {
                    foreach (Room instanceRoom in OdinHandler.Instance.Rooms)
                    {
                        OnUpdateUniqueUserId(instanceRoom);
                    }
                }
                
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            }
        }
        protected void OnDisable()
        {
            if (IsLocalUser())
            {
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            }
        }

        private void OnRoomJoined(RoomJoinedEventArgs roomJoinedEventArgs)
        {
            if(IsLocalUser())
                OnUpdateUniqueUserId(roomJoinedEventArgs.Room);
        }

        public abstract string GetUniqueUserId();
        public abstract bool IsLocalUser();

        protected abstract void OnUpdateUniqueUserId(Room newRoom);
    }
}