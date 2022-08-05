using UnityEngine;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    ///     Behaviour for reacting to clicks on this object. Will be activated by a <see cref="MouseClickInteract" /> behaviour
    ///     in the scene and
    ///     invoke the <see cref="onClickEvent" />.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OnClickReact : MonoBehaviour
    {
        /// <summary>
        ///     Will be invoked when clicked and the script is enabled.
        /// </summary>
        public UnityEvent onClickEvent;

        public void OnClicked()
        {
            if (enabled) onClickEvent.Invoke();
        }
    }
}