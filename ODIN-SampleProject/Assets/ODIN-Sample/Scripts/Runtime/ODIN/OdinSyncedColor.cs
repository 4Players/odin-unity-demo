using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinSyncedColor : MonoBehaviour
    {
        [SerializeField] private AOdinUser odinUser;

        [SerializeField] private Renderer capsuleRenderer = null;
        private static string ColorKey => "PlayerColor";

        private Color _currentColor;

        private void Awake()
        {
            Assert.IsNotNull(capsuleRenderer);
            Assert.IsNotNull(odinUser);

            if (capsuleRenderer && odinUser.IsLocalUser())
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

                UpdateCapsuleColor();
                if (OdinHandler.Instance.HasConnections)
                {
                    foreach (Room room in OdinHandler.Instance.Rooms)
                    {
                        UpdateColorInRoom(room);
                    }
                }
                
                OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);
            }
            else
            {
                odinUser.onPlaybackComponentAdded.AddListener(OnPlaybackComponentAdded);
            }
        }

        private void UpdateColorInRoom(Room room)
        {
            OdinSampleUserData userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
            userData.color = GetHtmlStringRGB();
            room.UpdateUserData(userData.ToUserData());
        }
        
        private void OnRoomJoined(RoomJoinedEventArgs roomJoinedEventArgs)
        {
            if (odinUser.IsLocalUser())
            {
                UpdateColorInRoom(roomJoinedEventArgs.Room);
                OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            }
        }

        private void OnPlaybackComponentAdded(PlaybackComponent added)
        {
            if (added)
            {
                Room remoteRoom = OdinHandler.Instance.Rooms[added.RoomName];
                if (null != remoteRoom)
                {
                    Peer remotePeer = remoteRoom.RemotePeers[added.PeerId];
                    if (null != remotePeer)
                    {
                        OdinSampleUserData odinSampleUserData = remotePeer.UserData.ToOdinSampleUserData();
                        _currentColor = GetColorFromHtmlStringRGB(odinSampleUserData.color);
                        UpdateCapsuleColor();
                    }
                }
            }
        }
        

        private void OnDestroy()
        {
            OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);
            odinUser.onPlaybackComponentAdded.RemoveListener(OnPlaybackComponentAdded);
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