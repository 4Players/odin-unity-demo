using System;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinNameDisplay : MonoBehaviour
    {
        [FormerlySerializedAs("odinAdapter")] [SerializeField] private AOdinMultiplayerAdapter multiplayerAdapter;

        /// <summary>
        /// Room, for which the name should be displayed.
        /// </summary>
        [SerializeField] private OdinStringVariable roomName;

        [SerializeField] private TMP_Text nameDisplay;

        [SerializeField] private int maxDisplayCharacters = 8;

        private void Awake()
        {
            Assert.IsNotNull(multiplayerAdapter);
            Assert.IsNotNull(roomName);
            Assert.IsNotNull(nameDisplay);
            nameDisplay.text = "";
        }

        private void Start()
        {
            if (multiplayerAdapter.IsLocalUser())
            {
                OdinSampleUserData userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
                DisplayName(userData);
            }
        }

        private void OnEnable()
        {
            if (!multiplayerAdapter.IsLocalUser())
            {
                OdinHandler.Instance.OnPeerUpdated.AddListener(OnPeerUpdated);
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            }
        }

        private void OnRoomJoined(RoomJoinedEventArgs roomJoinedEventArgs)
        {
            foreach (Peer remotePeer in roomJoinedEventArgs.Room.RemotePeers)
            {
                OdinSampleUserData userData = remotePeer.UserData.ToOdinSampleUserData();
                // Debug.Log($"OdinNameDisplay - OnRoomJoined - Name: {userData.name} ID: {userData.uniqueUserId}");
                if (userData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
                {
                    DisplayName(userData);
                }
            }
        }

        private void OnPeerUpdated(object sender, PeerUpdatedEventArgs peerUpdatedEventArgs)
        {
            OdinSampleUserData userData =
                new UserData(peerUpdatedEventArgs.UserData).ToOdinSampleUserData();
            
            // Debug.Log($"OdinNameDisplay - OnPeerUpdated - Name: {userData.name} ID: {userData.uniqueUserId}");
            if (null != userData && userData.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
            {
                DisplayName(userData);
            }
        }

        private void OnDisable()
        {
            if (!multiplayerAdapter.IsLocalUser())
            {
                OdinHandler.Instance.OnPeerUpdated.RemoveListener(OnPeerUpdated);
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            }
        }

        private void DisplayName(OdinSampleUserData userData)
        {
            if (null != userData)
                nameDisplay.text = AdjustName(userData.name);
        }

        private string AdjustName(string displayedName)
        {
            if (string.IsNullOrEmpty(displayedName)) displayedName = "Player";

            if (displayedName.Length > maxDisplayCharacters)
                displayedName = displayedName.Substring(0, maxDisplayCharacters) + "...";

            return displayedName;
        }
    }
}