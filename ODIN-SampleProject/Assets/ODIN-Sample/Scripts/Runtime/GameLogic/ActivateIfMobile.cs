using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Script deactivate this game, if the current platform is a mobile platform. Can be overriden using <see cref="enableInEditor"/>.
    /// </summary>
    public class ActivateIfMobile : MonoBehaviour
    {
        /// <summary>
        /// If true, will enable the object in editor, even if not on a mobile platform.
        /// </summary>
        [SerializeField] private bool enableInEditor;

        // Start is called before the first frame update
        private void Awake()
        {
            bool bShouldEnableInEditor = enableInEditor && Application.isEditor;
            bool bShouldBeActive = Application.isMobilePlatform || bShouldEnableInEditor;
            gameObject.SetActive(bShouldBeActive);
        }
    }
}