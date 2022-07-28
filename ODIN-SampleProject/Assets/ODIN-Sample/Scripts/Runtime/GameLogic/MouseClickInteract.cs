using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class MouseClickInteract : MonoBehaviour
    {
        [SerializeField] private InputActionReference clickAction;
        [SerializeField] private InputActionReference clickPositionReference;
        

        private void Awake()
        {
            Assert.IsNotNull(clickAction);
            Assert.IsNotNull(clickPositionReference);
        }

        private void OnEnable()
        {
            clickPositionReference.action.Enable();
            clickAction.action.Enable();
            clickAction.action.performed += OnClickPerformed;
        }

        private void OnDisable()
        {
            clickAction.action.performed -= OnClickPerformed;
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector2 clickPosition = clickPositionReference.action.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(clickPosition);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, 100.0f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            foreach (RaycastHit hit in raycastHits)
            {
                OnClickReact onClickReact = hit.collider.GetComponent<OnClickReact>();
                if (onClickReact)
                {
                    onClickReact.OnClicked();
                }
            }
        }
    }
}