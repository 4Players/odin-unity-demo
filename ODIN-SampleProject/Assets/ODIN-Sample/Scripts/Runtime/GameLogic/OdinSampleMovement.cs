using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Base movement behavior for both the first and third person movement behavior. Inherit from this to implement
    /// your own movement behavior or adjust the settings, if only simple changes are required.
    /// </summary>
    public class OdinSampleMovement : MonoBehaviour
    {

        /// <summary>
        /// The movement input action - e.g. WASD on keyboard or left stick on gamepad.
        /// </summary>
        [Header("Input")]
        [SerializeField] private InputActionReference movement;
        /// <summary>
        /// The spring input action reference.
        /// </summary>
        [SerializeField] protected InputActionReference sprintButton;
        
        /// <summary>
        /// The base movement speed.
        /// </summary>
        [Header("Values")]
        [SerializeField] protected float movementSpeed = 10.0f;
        /// <summary>
        /// The multiplier applied to the base movement speed, when the button <see cref="sprintButton"/> is pressed.
        /// </summary>
        [SerializeField] protected float sprintMultiplier = 1.5f;
        
        /// <summary>
        /// Reference to the Unity <c>CharacterController</c> script, which is used to move around.
        /// </summary>
        [Header("References")]
        [SerializeField] protected CharacterController characterController = null;
        
        protected virtual void Awake()
        {
            Assert.IsNotNull(characterController);
            Assert.IsNotNull(movement);
            Assert.IsNotNull(sprintButton);

            movement.action.Enable();
            sprintButton.action.Enable();
        }

        protected virtual void Update()
        {
            Vector2 input = movement.action.ReadValue<Vector2>();
            float currentSprintMultiplier = sprintButton.action.IsPressed() ? sprintMultiplier : 1.0f;
            
            // don't multiply with Time.deltaTime, it's already included in characterController.SimpleMove()
            Vector3 deltaMovement = (input.x * transform.right + input.y * transform.forward) * (movementSpeed * currentSprintMultiplier);
            characterController.SimpleMove(deltaMovement);
        }
    }
}