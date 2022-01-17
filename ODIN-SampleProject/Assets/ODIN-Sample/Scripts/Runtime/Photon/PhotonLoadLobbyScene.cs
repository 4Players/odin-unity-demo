using ODIN_Sample.Scripts.Runtime.Odin;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace ODIN_Sample.Scripts.Runtime.Photon
{
    /// <summary>
    ///     If we're not connected to the photon network on Start,
    ///     load the scene given by <see cref="sceneName" /> to start
    ///     Photon connection.
    /// </summary>
    /// <remarks>
    ///     Used for faster prototyping: we don't have to switch to the lobby scene in the editor to start playmode.
    /// </remarks>
    public class PhotonLoadLobbyScene : MonoBehaviour
    {
        /// <summary>
        /// The lobby scene name.
        /// </summary>
        [SerializeField] private OdinStringVariable sceneName;

        private void Awake()
        {
            Assert.IsNotNull(sceneName);
        }

        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
                SceneManager.LoadScene(sceneName.Value);
        }
    }
}