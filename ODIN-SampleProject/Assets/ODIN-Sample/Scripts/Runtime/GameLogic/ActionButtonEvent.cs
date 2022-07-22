using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class ActionButtonEvent : MonoBehaviour
    {
        [SerializeField] private InputActionReference button;
        [SerializeField] private UnityEvent onButtonPressed;
        
        private void Awake()
        {
            Assert.IsNotNull(button);
            button.action.Enable();
        }

        private void OnEnable()
        {
            button.action.performed += OnButtonPerformed;
        }

        private void OnDisable()
        {
            button.action.performed -= OnButtonPerformed;
        }

        private void OnButtonPerformed(InputAction.CallbackContext context)
        {
            onButtonPressed.Invoke();
        }
    }
}