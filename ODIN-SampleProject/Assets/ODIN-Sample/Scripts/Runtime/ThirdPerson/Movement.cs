using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    /// <summary>
    /// Will use the given input and the linked CharacterController to implement basic movement.
    /// </summary>
    public class Movement : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private string horizontalMovement = "Horizontal";
        [SerializeField] private string verticalMovement = "Vertical";
        [SerializeField] private string sprintButton = "Sprint";
        
        [Header("Values")]
        [SerializeField] private float movementSpeed = 10.0f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        
        [Header("References")]
        [SerializeField] private CharacterController characterController = null;

        private void Awake()
        {
            Assert.IsNotNull(characterController);
        }

        // Update is called once per frame
        void Update()
        {
            float horizontal = Input.GetAxis(horizontalMovement);
            float vertical = Input.GetAxis(verticalMovement);

            // Debug.Log(Input.GetButton(sprintButton));
            float currentSprintMultiplier = Input.GetButton(sprintButton) ? sprintMultiplier : 1.0f;
            Vector3 input = new Vector3(horizontal, 0.0f, vertical);
            Vector3 deltaMovement = (input.x * transform.right + input.z * transform.forward) * movementSpeed * currentSprintMultiplier;
            
            characterController.SimpleMove(deltaMovement);

            
        }
    }
}
