using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    /// <summary>
    /// Will make the target gameobject rotate in the direction of the velocity of characterController.
    /// </summary>
    public class RotateToVelocity : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float rotationSpeed = 5.0f;
    
        private void Awake()
        {
            Assert.IsNotNull(characterController);
            Assert.IsNotNull(target);
        }

        void Update()
        {
            Transform targetTransform = target.transform;
            
            Vector3 movementDirection = characterController.velocity;
            movementDirection.y = 0.0f;
            if (movementDirection.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                float lerpValue = Time.deltaTime * rotationSpeed;
                target.transform.rotation = Quaternion.Lerp(targetTransform.rotation, targetRotation, lerpValue);
            }
        }
    }
}
