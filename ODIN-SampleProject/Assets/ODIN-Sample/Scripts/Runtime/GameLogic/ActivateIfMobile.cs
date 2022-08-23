using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    ///     Script deactivate this game, if the current platform is a mobile platform. Can be overriden using
    ///     <see cref="enableInEditor" />.
    /// </summary>
    public class ActivateIfMobile : MonoBehaviour
    {
        /// <summary>
        ///     If false, will disable the mobile controls in editor, even if on a mobile plattform
        /// </summary>
        [SerializeField] private bool enableInEditor;

        // Start is called before the first frame update
        private void Awake()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            gameObject.SetActive(enableInEditor);
#else
            gameObject.SetActive(false);
#endif
        }
    }
}