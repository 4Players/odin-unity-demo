using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class OdinSampleMovement : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] protected string horizontalMovement = "Horizontal";
        [SerializeField] protected string verticalMovement = "Vertical";
        [SerializeField] protected string sprintButton = "Sprint";
        
        [Header("Values")]
        [SerializeField] protected float movementSpeed = 10.0f;
        [SerializeField] protected float sprintMultiplier = 1.5f;
        
        [Header("References")]
        [SerializeField] protected CharacterController characterController = null;

        protected float CurrentSprintMultiplier;
        protected Vector3 PlayerInput;
        
        protected virtual void Awake()
        {
            Assert.IsNotNull(characterController);
        }

        protected virtual void Update()
        {
            UpdateInput();
            // don't multiply with Time.deltaTime, it's already included in characterController.SimpleMove()
            Vector3 deltaMovement = (PlayerInput.x * transform.right + PlayerInput.z * transform.forward) * movementSpeed *
                                    CurrentSprintMultiplier;
            characterController.SimpleMove(deltaMovement);
        }

        protected virtual void UpdateInput()
        {
            float horizontal = Input.GetAxis(horizontalMovement);
            float vertical = Input.GetAxis(verticalMovement);

            CurrentSprintMultiplier = Input.GetButton(sprintButton) ? sprintMultiplier : 1.0f;
            PlayerInput = new Vector3(horizontal, 0.0f, vertical);
        }
    }
}