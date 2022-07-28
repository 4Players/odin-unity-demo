using UnityEngine;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    [RequireComponent(typeof(Collider))]
    public class OnClickReact : MonoBehaviour
    {

        public UnityEvent onClickEvent;
        public void OnClicked()
        {
            if (enabled)
            {
                onClickEvent.Invoke();
            }
        }
    }
}