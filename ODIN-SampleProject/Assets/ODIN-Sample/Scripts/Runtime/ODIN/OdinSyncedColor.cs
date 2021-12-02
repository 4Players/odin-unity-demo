using System.Collections;
using OdinNative.Odin;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public class OdinSyncedColor : MonoBehaviour
    {
        [FormerlySerializedAs("odinAdapter")] [SerializeField]
        private AOdinMultiplayerAdapter multiplayerAdapter;

        [SerializeField] private Renderer capsuleRenderer;

        private Color _currentColor;
        private static string ColorKey => "PlayerColor";

        private void Awake()
        {
            Assert.IsNotNull(capsuleRenderer);
            Assert.IsNotNull(multiplayerAdapter);

            if (capsuleRenderer && multiplayerAdapter.IsLocalUser())
            {
                InitializeColor();
                UpdateCapsuleColor();
            }
        }

        private void OnEnable()
        {
            OdinHandler.Instance.OnPeerJoined.AddListener(OnPeerJoined);
            OdinHandler.Instance.OnPeerUpdated.AddListener(OnPeerUpdated);
        }


        private void OnDisable()
        {
            OdinHandler.Instance.OnPeerJoined.RemoveListener(OnPeerJoined);
            OdinHandler.Instance.OnPeerUpdated.RemoveListener(OnPeerUpdated);
        }

        private void OnPeerJoined(object sender, PeerJoinedEventArgs arg1)
        {
            // Debug.Log("SyncColor: OnPeerJoined");
            if (multiplayerAdapter.IsLocalUser() && sender is Room room) StartCoroutine(SendColorUpdateInRoom(room));
        }

        private void OnPeerUpdated(object sender, PeerUpdatedEventArgs peerUpdatedEventArgs)
        {
            var updatedPeer =
                new UserData(peerUpdatedEventArgs.UserData).ToOdinSampleUserData();
            if (!multiplayerAdapter.IsLocalUser() && null != updatedPeer &&
                updatedPeer.uniqueUserId == multiplayerAdapter.GetUniqueUserId())
            {
                // Debug.Log($"SyncColor: Received a color update: {updatedPeer.color} = {_currentColor}");
                _currentColor = GetColorFromHtmlStringRGB(updatedPeer.color);
                UpdateCapsuleColor();
            }
        }

        private void InitializeColor()
        {
            if (PlayerPrefs.HasKey(ColorKey))
            {
                var playerColorString = PlayerPrefs.GetString(ColorKey);
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

        private IEnumerator SendColorUpdateInRoom(Room room)
        {
            yield return new WaitForSeconds(1.0f);
            var userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
            userData.color = GetHtmlStringRGB();
            userData.uniqueUserId = multiplayerAdapter.GetUniqueUserId();
            room.UpdateUserData(userData.ToUserData());

            // Debug.Log($"ColorSync: Sending color update: {userData.color}");
        }

        private Color GetColorFromHtmlStringRGB(string htmlStringRGB)
        {
            ColorUtility.TryParseHtmlString("#" + htmlStringRGB, out var result);
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