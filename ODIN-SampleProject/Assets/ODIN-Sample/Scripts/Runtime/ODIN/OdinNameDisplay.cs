using System;
using OdinNative.Odin;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// UI Behaviour for displaying the name of an ODIN user. Uses the <see cref="OdinSampleUserData"/> to read the name
    /// of the user identified by the connected <see cref="AOdinMultiplayerAdapter"/> for the given room name.
    /// Requires only ODIN dependencies for synchronizing names.
    /// </summary>
    public class OdinNameDisplay : MonoBehaviour
    {
        /// <summary>
        /// The Adapter used to identify the user, for which the name should be displayed.
        /// </summary>
        [FormerlySerializedAs("odinAdapter")] [SerializeField] private AOdinMultiplayerAdapter multiplayerAdapter;

        /// <summary>
        /// ODIN room, for which the name should be displayed.
        /// </summary>
        [SerializeField] private OdinStringVariable roomName;

        /// <summary>
        /// UI component for visualizing the name.
        /// </summary>
        [SerializeField] private TMP_Text nameDisplay;

        /// <summary>
        /// The maximum number of name characters that will be displayed. Names with more characters will be cut off.
        /// </summary>
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

        /// <summary>
        /// Shortens the name if necessary and returns the cut-off version for display.
        /// </summary>
        /// <param name="fullName">The full name</param>
        /// <returns>The truncated version, if <c>fullName.Length</c> > <see cref="maxDisplayCharacters"/>, the full name otherwise.</returns>
        private string AdjustName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) fullName = "Player";

            if (fullName.Length > maxDisplayCharacters)
                fullName = fullName.Substring(0, maxDisplayCharacters) + "...";

            return fullName;
        }
    }
}