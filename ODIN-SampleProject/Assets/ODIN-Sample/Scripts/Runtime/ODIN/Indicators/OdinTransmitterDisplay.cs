using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin.Utility;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    /// Displays transmissions of all players in a room using UI.
    /// </summary>
    public class OdinTransmitterDisplay : MonoBehaviour
    {

        [SerializeField] private OdinStringVariable displayedRoomName;

        private OdinTransmitterUiElement[] _uiElements;

        private void Awake()
        {
            _uiElements = GetComponentsInChildren<OdinTransmitterUiElement>(true);
            foreach (var element in _uiElements) element.Hide();
        }

        private void OnEnable()
        {
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaActiveStateChanged);
        }


        private void OnDisable()
        {
            if(OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaActiveStateChanged);
        }

        private void OnMediaActiveStateChanged(object sender, MediaActiveStateChangedEventArgs mediaActiveEventArgs)
        {
            if (sender is Room sendingRoom)
            {
                string roomName = sendingRoom.Config.Name;
                if (roomName == displayedRoomName.Value)
                {
                    ulong peerId = mediaActiveEventArgs.PeerId;
                    long mediaId = mediaActiveEventArgs.MediaStreamId;

                    OdinConnectionIdentifier uiKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);

                    if (mediaActiveEventArgs.Active)
                    {
                        var room = OdinHandler.Instance.Rooms[roomName];
                        var peer = room.RemotePeers[peerId];
                        OdinSampleUserData userData = null;
                        if (null != peer)
                        {
                            userData = OdinSampleUserData.FromUserData(peer.UserData);
                        }
                        else
                        {
                            userData = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
                        }
                        
                        if(null != userData)
                            ShowElement(uiKey, userData);
                    }
                    else
                    {
                        HideElement(uiKey);
                    }
                }
            }
        }

        private void ShowElement(OdinConnectionIdentifier key, OdinSampleUserData userData)
        {
            foreach (OdinTransmitterUiElement element in _uiElements)
                if (!element.IsActive())
                {
                    element.Show(key, userData);
                    break;
                }
        }

        private void HideElement(OdinConnectionIdentifier key)
        {
            foreach (OdinTransmitterUiElement element in _uiElements)
                if (element.IsShowing(key))
                    element.Hide();
        }
    }
}