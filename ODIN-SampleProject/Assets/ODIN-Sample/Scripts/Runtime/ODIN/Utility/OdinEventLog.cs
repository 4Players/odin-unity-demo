using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin.Utility
{
    /// <summary>
    /// Utility script for displaying messages for relevant ODIN events, like OnRoomJoined or OnPeerLeft.
    /// </summary>
    public class OdinEventLog : MonoBehaviour
    {
        private void OnEnable()
        {
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
                OdinHandler.Instance.OnPeerJoined.AddListener(OnPeerJoined);
                OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
                OdinHandler.Instance.OnPeerUpdated.AddListener(OnPeerUpdated);
            
                OdinHandler.Instance.OnRoomLeft.AddListener(OnRoomLeft);
                OdinHandler.Instance.OnPeerLeft.AddListener(OnPeerLeft);
                OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
            }
        }

        private void OnPeerUpdated(object sender, PeerUpdatedEventArgs peerUpdatedEventArgs)
        {
            Room room = sender as Room;
            if (null != room)
            {
                Peer remotePeer = room.RemotePeers[peerUpdatedEventArgs.PeerId];
                if (null != remotePeer)
                {
                    OdinSampleUserData userData = remotePeer.UserData.ToOdinSampleUserData();
                    Debug.Log($"Updated Peer {remotePeer.Id} in Room {room.Config.Name} with Unique Id: {userData.uniqueUserId}");
                }
            }
        }

        private void OnDisable()
        {
            if (OdinHandler.Instance)
            {
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
                OdinHandler.Instance.OnPeerJoined.RemoveListener(OnPeerJoined);
                OdinHandler.Instance.OnMediaAdded.RemoveListener(OnMediaAdded);
            
                OdinHandler.Instance.OnRoomLeft.RemoveListener(OnRoomLeft);
                OdinHandler.Instance.OnPeerLeft.RemoveListener(OnPeerLeft);
                OdinHandler.Instance.OnMediaRemoved.RemoveListener(OnMediaRemoved);
            }
        }
        
        private void OnRoomJoined(RoomJoinedEventArgs arg0)
        {
            Debug.Log($"OnRoomJoined: {arg0.Room.Config.Name}, Peer:{arg0.Room.Self.Id}");
            Debug.Log("Self User Data: " + arg0.Room.Self.UserData);
        }
        
        private void OnPeerJoined(object arg0, PeerJoinedEventArgs arg1)
        {
            Debug.Log($"OnPeerJoined: {arg1.PeerId}, Room: {arg1.Peer.RoomName}");
        }
        
        private void OnMediaAdded(object arg0, MediaAddedEventArgs arg1)
        {
            Debug.Log($"OnMediaAdded: {arg1.Media.Id}, Peer: {arg1.Peer.Id}, Room: {arg1.Peer.RoomName}");
        }

        private void OnRoomLeft(RoomLeftEventArgs arg0)
        {
            Debug.Log($"OnRoomLeft: {arg0.RoomName}");
        }
        
        private void OnPeerLeft(object arg0, PeerLeftEventArgs arg1)
        {
            Debug.Log($"OnPeerLeft: {arg1.PeerId}");
        }
        
        private void OnMediaRemoved(object arg0, MediaRemovedEventArgs arg1)
        {
            Debug.Log($"OnMediaRemoved: {arg1.MediaId}, Peer: {arg1.Peer.Id}, Room: {arg1.Peer.RoomName}");
        }
    }
}