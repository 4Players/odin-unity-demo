using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    ///     Initialises the player's state: if we belong to a remote player, destroy the local-only behaviours, otherwise
    ///     enable the local-only behaviours.
    /// </summary>
    [RequireComponent(typeof(AOdinMultiplayerAdapter))]
    public class InitialisePlayer : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the root object, which contains all the local-only behaviour scripts.
        /// </summary>
        [SerializeField] private GameObject localPlayerOnly;

        private void Start()
        {
            AOdinMultiplayerAdapter multiplayerAdapter = GetComponent<AOdinMultiplayerAdapter>();
            if (!multiplayerAdapter.IsLocalUser())
                Destroy(localPlayerOnly);
            else
                localPlayerOnly.SetActive(true);
        }
    }
}