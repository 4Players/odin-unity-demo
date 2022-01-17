using System;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    ///  It is used to implement the adapter pattern for interfacing between ODIN and the Multiplayer solution
    /// (e.g. Photon, Mirror, Unity Netcode, etc.) of your choice. This class should be the connection between your
    /// multiplayer game logic and the ODIN logic, representing a single player (either local or remote). It will
    /// couple the transmissions of a player in an ODIN room with the visual representation in your game.
    /// </summary>
    /// <remarks>
    /// A concrete sample for implementing Photon can be found in the script <c>PhotonToOdinAdapter</c>.
    /// </remarks>
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
                        UpdateUniqueUserId(instanceRoom);
                    }
                }
                
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            }
        }
        protected virtual void OnDisable()
        {
            if (IsLocalUser())
            {
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            }
        }
        
        protected virtual void OnRoomJoined(RoomJoinedEventArgs roomJoinedEventArgs)
        {
            if(IsLocalUser())
                UpdateUniqueUserId(roomJoinedEventArgs.Room);
        }

        /// <summary>
        /// Returns a unique user id, usually provided by your multiplayer solution. This will couple transmissions by
        /// a certain user in an ODIN room to their visual/physical representation in your game.
        /// </summary>
        /// <remarks>
        /// E.g. in photon you could use the photonView's ViewID.
        /// </remarks>
        /// <returns>A unique user Id.</returns>
        public abstract string GetUniqueUserId();
        /// <summary>
        /// Whether the user represented by <see cref="GetUniqueUserId"/> is a local or remote user.
        /// </summary>
        /// <returns>True, if it is a local user, false otherwise.</returns>
        public abstract bool IsLocalUser();


        
        /// <summary>
        /// Transmits the unique user Id to the given ODIN room. Will be automatically called in OnRoomJoined.
        /// </summary>
        /// <param name="newRoom">The room for which the unique user Id should be updated.</param>
        protected virtual void UpdateUniqueUserId(Room newRoom)
        {
            OdinSampleUserData userData = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
            userData.uniqueUserId = GetUniqueUserId();
            newRoom.UpdateUserData(userData.ToUserData());
        }
    }
}