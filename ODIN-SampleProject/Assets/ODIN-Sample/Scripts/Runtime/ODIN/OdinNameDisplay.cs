using System;
using ODIN_Sample.Scripts.Runtime.Data;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinNameDisplay : MonoBehaviour
    {
        [SerializeField] private AOdinUser odinUser;
        /// <summary>
        /// Room, for which the name should be displayed.
        /// </summary>
        [SerializeField] private StringVariable roomName;
        [SerializeField] private TMP_Text nameDisplay;

        [SerializeField] private int maxDisplayCharacters = 8;
        
        private void Awake()
        {
            Assert.IsNotNull(odinUser);
            Assert.IsNotNull(roomName);
            Assert.IsNotNull(nameDisplay);
            nameDisplay.text = "";
        }

        private void Start()
        {
            if (odinUser.IsLocalUser())
            {
                OdinSampleUserData userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
                DisplayName(userData);
            }
        }

        private void OnEnable()
        {
            if(!odinUser.IsLocalUser())
                odinUser.onPlaybackComponentAdded.AddListener(OnPlaybackComponentAdded);
        }

        private void OnPlaybackComponentAdded(PlaybackComponent added)
        {
            if (added.RoomName == roomName)
            {
                if (OdinHandler.Instance && null != OdinHandler.Instance.Rooms[roomName])
                {
                    Room instanceRoom = OdinHandler.Instance.Rooms[roomName];
                    Peer displayedPeer = instanceRoom.RemotePeers[added.PeerId];
                    if (null != displayedPeer)
                    {
                        OdinSampleUserData displayedPeerUserData = OdinSampleUserData.FromUserData(displayedPeer.UserData);
                        if (null != displayedPeerUserData)
                        {
                            DisplayName(displayedPeerUserData);
                            odinUser.onPlaybackComponentAdded.RemoveListener(OnPlaybackComponentAdded);
                        }
                    }
                }
                
            }
        }

        private void OnDisable()
        {
            if(!odinUser.IsLocalUser())
                odinUser.onPlaybackComponentAdded.RemoveListener(OnPlaybackComponentAdded);
        }

        private void DisplayName(OdinSampleUserData userData)
        {
            if(null != userData)
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