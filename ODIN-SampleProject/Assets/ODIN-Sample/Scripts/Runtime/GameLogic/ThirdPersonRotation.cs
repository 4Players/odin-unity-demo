using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Will make the target gameobject rotate in the direction of the velocity of characterController.
    /// </summary>
    public class ThirdPersonRotation : MonoBehaviour
    {
        [SerializeField] private string rotationalInputAxis = "Rotational";
        [FormerlySerializedAs("target")] [SerializeField] private GameObject rotationTarget;
        [SerializeField] private CharacterController characterController;
        
        [SerializeField] private float rotationSpeed = 5.0f;
        
        /// <summary>
        /// Maximum angle in degrees that the user can turn using the <see cref="rotationalInputAxis"/> in one frame.
        /// </summary>
        private static int MaxFrameRotation => 30;

        private void Awake()
        {
            Assert.IsNotNull(characterController);
            Assert.IsNotNull(rotationTarget);
        }

        void Update()
        {
            Vector3 movementDirection = characterController.velocity;
            movementDirection.y = 0.0f;
            if (movementDirection.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                RotateTowards(targetRotation);
            }
            else
            {
                float customRotation = Input.GetAxis(rotationalInputAxis);
                if (Mathf.Abs(customRotation) > 0.01f)
                {
                    Quaternion deltaRotation = Quaternion.AngleAxis(customRotation * MaxFrameRotation, Vector3.up);
                    Quaternion targetRotation = rotationTarget.transform.rotation * deltaRotation;
                    RotateTowards(targetRotation);
                }
            }
        }


        private void RotateTowards(Quaternion targetRotation)
        {
            float lerpValue = Time.deltaTime * rotationSpeed;
            rotationTarget.transform.rotation = Quaternion.Lerp(rotationTarget.transform.rotation, targetRotation, lerpValue);
        }
    }
}