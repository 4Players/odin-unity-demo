using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Utility class for firing an UnityEvent upon registering a action button being pressed.
    /// </summary>
    public class ActionButtonEvent : MonoBehaviour
    {
        /// <summary>
        /// The button that should be checked.
        /// </summary>
        [SerializeField] private InputActionReference button;
        /// <summary>
        /// Will be fired when button was pressed.
        /// </summary>
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