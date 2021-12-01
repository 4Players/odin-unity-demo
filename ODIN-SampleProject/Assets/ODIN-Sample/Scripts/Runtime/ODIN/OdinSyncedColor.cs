using System;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinSyncedColor : MonoBehaviour
    {
        [SerializeField] private AOdinMultiplayerAdapter odinAdapter;

        [SerializeField] private Renderer capsuleRenderer = null;
        private static string ColorKey => "PlayerColor";

        private Color _currentColor;

        private void Awake()
        {
            Assert.IsNotNull(capsuleRenderer);
            Assert.IsNotNull(odinAdapter);

            if (capsuleRenderer && odinAdapter.IsLocalUser())
            {
                InitializeColor();
                UpdateCapsuleColor();
                if (OdinHandler.Instance.HasConnections)
                {
                    foreach (Room room in OdinHandler.Instance.Rooms)
                    {
                        SendColorUpdateInRoom(room);
                    }
                }
            }
        }

        private void OnEnable()
        {
            OdinHandler.Instance.OnPeerJoined.AddListener(OnPeerJoined);
        }

        private void OnDisable()
        {
            OdinHandler.Instance.OnPeerJoined.RemoveListener(OnPeerJoined);
        }

        private void OnPeerJoined(object arg0, PeerJoinedEventArgs peerJoinedEventArgs)
        {
            string joinedRoom = peerJoinedEventArgs.Peer.RoomName;
            Room remoteRoom = OdinHandler.Instance.Rooms[joinedRoom];
            if (null != remoteRoom)
            {
                Peer remotePeer = peerJoinedEventArgs.Peer;
                if (null != remotePeer)
                {
                    OdinSampleUserData odinSampleUserData = remotePeer.UserData.ToOdinSampleUserData();
                    if (odinSampleUserData.playerId == odinAdapter.GetUniqueUserId())
                    {
                        _currentColor = GetColorFromHtmlStringRGB(odinSampleUserData.color);
                        UpdateCapsuleColor();
                    }
                }
            }
        }

        private void InitializeColor()
        {
            if (PlayerPrefs.HasKey(ColorKey))
            {
                string playerColorString = PlayerPrefs.GetString(ColorKey);
                ColorUtility.TryParseHtmlString("#" + playerColorString, out _currentColor);
            }
            else
            {
                Random.InitState(SystemInfo.deviceUniqueIdentifier.GetHashCode());
                _currentColor = Random.ColorHSV();
                PlayerPrefs.SetString(ColorKey, GetHtmlStringRGB());
                PlayerPrefs.Save();
            }
        }

        private void SendColorUpdateInRoom(Room room)
        {
            OdinSampleUserData userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
            userData.color = GetHtmlStringRGB();
            room.UpdateUserData(userData.ToUserData());
        }

        private Color GetColorFromHtmlStringRGB(string htmlStringRGB)
        {
            ColorUtility.TryParseHtmlString("#" + htmlStringRGB, out Color result);
            return result;
        }

        private string GetHtmlStringRGB()
        {
            return ColorUtility.ToHtmlStringRGB(_currentColor);
        }

        private void UpdateCapsuleColor()
        {
            capsuleRenderer.material.color = _currentColor;
        }
    }
}