using ODIN_Sample.Scripts.Runtime.Odin.Utility;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Displays transmissions of all players in a room using ui elements in the hierarchy.
    /// </summary>
    public class OdinTransmitterDisplay : MonoBehaviour
    {
        /// <summary>
        ///     The room name for which transmissions will be shown.
        /// </summary>
        [FormerlySerializedAs("displayedRoomName")] [SerializeField]
        private OdinStringVariable roomName;


        private OdinTransmitterUiElement[] _uiElements;

        private void Awake()
        {
            _uiElements = GetComponentsInChildren<OdinTransmitterUiElement>(true);
            foreach (var element in _uiElements) element.Hide();
        }

        private void OnEnable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaActiveStateChanged);
        }


        private void OnDisable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaActiveStateChanged);
        }

        private void OnMediaActiveStateChanged(object sender, MediaActiveStateChangedEventArgs mediaActiveEventArgs)
        {
            if (sender is Room sendingRoom)
            {
                string roomName = sendingRoom.Config.Name;
                if (roomName == this.roomName.Value)
                {
                    ulong peerId = mediaActiveEventArgs.PeerId;
                    long mediaId = mediaActiveEventArgs.MediaStreamId;

                    OdinConnectionIdentifier uiKey = new OdinConnectionIdentifier(roomName, peerId, mediaId);

                    // if media is now active, try to show element
                    if (mediaActiveEventArgs.Active)
                    {
                        var room = OdinHandler.Instance.Rooms[roomName];
                        var peer = room.RemotePeers[peerId];
                        // retrieve user data
                        OdinSampleUserData userData = null;
                        if (null != peer)
                            userData = OdinSampleUserData.FromUserData(peer.UserData);
                        else
                            userData = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();

                        // if user data was retrieved
                        if (null != userData)
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