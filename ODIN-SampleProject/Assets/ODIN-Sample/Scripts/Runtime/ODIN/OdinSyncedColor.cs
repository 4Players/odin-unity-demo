using System.Collections;
using OdinNative.Odin;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Behaviour for replicating the player's capsule color using ODIN.
    /// </summary>
    public class OdinSyncedColor : MonoBehaviour
    {
        /// <summary>
        /// Reference to the adapter used for identifying the current player.
        /// </summary>
        [FormerlySerializedAs("odinAdapter")] [SerializeField]
        private AOdinMultiplayerAdapter multiplayerAdapter;

        /// <summary>
        /// The renderer, for which the color should be synced.
        /// </summary>
        [SerializeField] private Renderer capsuleRenderer;

        private Color _currentColor;
        /// <summary>
        /// The Player Pref key for accessing the locally stored player color.
        /// </summary>
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

        /// <summary>
        /// When a new peer enters the room, send a color update to the entire room.
        /// </summary>
        /// <param name="sender">The sending ODIN room.</param>
        /// <param name="peerJoinedEventArgs">The event arguments containing additional data.</param>
        private void OnPeerJoined(object sender, PeerJoinedEventArgs peerJoinedEventArgs)
        {
            // Debug.Log("SyncColor: OnPeerJoined");
            if (multiplayerAdapter.IsLocalUser() && sender is Room room) StartCoroutine(SendColorUpdateInRoom(room));
        }

        /// <summary>
        /// Called when a peer's user data was updated. If this behaviour is connected to a remote user's multiplayer adapter
        /// and the UserData uniqueUserId corresponds to the adapters unique user id, the referenced <see cref="capsuleRenderer"/>'s
        /// color will be updated to the transmitted color.
        /// </summary>
        /// <param name="sender">The sending ODIN room.</param>
        /// <param name="peerUpdatedEventArgs">The event arguments.</param>
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

        /// <summary>
        /// If available, loads the color from player prefs, otherwise a new random color will be created and saved
        /// to player prefs.
        /// </summary>
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

        /// <summary>
        /// Sends the local user's capsule color to all other users in the room. 
        /// </summary>
        /// <param name="room">The room in which we want to broadcast the color updated.</param>
        /// <param name="delay">An additional time to wait before sending. This is used to sidestep some synchronization issues.</param>
        /// <returns></returns>
        private IEnumerator SendColorUpdateInRoom(Room room, float delay = 1.0f)
        {
            // Wait for a short time to let 
            yield return new WaitForSeconds(delay);
            var userData = OdinSampleUserData.FromUserData(OdinHandler.Instance.GetUserData());
            userData.color = GetHtmlStringRGB();
            userData.uniqueUserId = multiplayerAdapter.GetUniqueUserId();
            room.UpdateUserData(userData.ToUserData());

            // Debug.Log($"ColorSync: Sending color update: {userData.color}");
        }

        /// <summary>
        /// Converts an html rbg string into a Unity Color struct.
        /// </summary>
        /// <param name="htmlStringRGB">The color as an html rgb string.</param>
        /// <returns>The converted color.</returns>
        private Color GetColorFromHtmlStringRGB(string htmlStringRGB)
        {
            ColorUtility.TryParseHtmlString("#" + htmlStringRGB, out var result);
            return result;
        }

        /// <summary>
        /// Converts the <see cref="_currentColor"/> into an html rgb string for transmission using ODIN.
        /// </summary>
        /// <returns>The <see cref="_currentColor"/> as an html rgb string.</returns>
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