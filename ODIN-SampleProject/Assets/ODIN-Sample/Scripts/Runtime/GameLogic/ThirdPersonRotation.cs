using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Will make the target game object rotate in the direction of the velocity of characterController or when the
    /// rotation axis is used. Used to visualize the current movement direction of the player.
    /// </summary>
    public class ThirdPersonRotation : MonoBehaviour
    {
        /// <summary>
        /// The axis name used for turning the capsule without moving it.
        /// </summary>
        [SerializeField] private string rotationalInputAxis = "Rotational";
        /// <summary>
        /// The target object which should be rotated
        /// </summary>
        [FormerlySerializedAs("target")] [SerializeField] private GameObject rotationTarget;
        /// <summary>
        /// References to the Unity <c>CharacterController</c>. The <see cref="rotationTarget"/> will look in the
        /// direction of the velocity of this controller, if the velocity is non-zero.
        /// </summary>
        [SerializeField] private CharacterController characterController;
        /// <summary>
        /// The rotational speed both when using input or when interpolating towards the velocity direction.
        /// </summary>
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
            // if movement direction is non zero, look in movement direction
            if (movementDirection.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                RotateTowards(targetRotation);
            }
            else
            {
                // listen to rotational input to rotate the target if not moving.
                float customRotation = Input.GetAxis(rotationalInputAxis);
                if (Mathf.Abs(customRotation) > 0.01f)
                {
                    Quaternion deltaRotation = Quaternion.AngleAxis(customRotation * MaxFrameRotation, Vector3.up);
                    Quaternion targetRotation = rotationTarget.transform.rotation * deltaRotation;
                    RotateTowards(targetRotation);
                }
            }
        }


        /// <summary>
        /// Interpolates <see cref="rotationTarget"/> towards <see cref="targetRotation"/> based on <see cref="rotationSpeed"/>
        /// and delta time.
        /// </summary>
        /// <param name="targetRotation">The target rotation.</param>
        private void RotateTowards(Quaternion targetRotation)
        {
            float lerpValue = Time.deltaTime * rotationSpeed;
            rotationTarget.transform.rotation = Quaternion.Lerp(rotationTarget.transform.rotation, targetRotation, lerpValue);
        }
    }
}