using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    /// <summary>
    /// Will make the target gameobject rotate in the direction of the velocity of characterController.
    /// </summary>
    public class RotateToVelocity : MonoBehaviour
    {
        [SerializeField] private string rotationalInputAxis = "Rotational";
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
                    Quaternion deltaRotation = Quaternion.AngleAxis(customRotation * 30, Vector3.up);
                    Quaternion targetRotation = target.transform.rotation * deltaRotation;
                    RotateTowards(targetRotation);
                }
            }
        }

        private void RotateTowards(Quaternion targetRotation)
        {
            float lerpValue = Time.deltaTime * rotationSpeed;
            target.transform.rotation = Quaternion.Lerp(target.transform.rotation, targetRotation, lerpValue);
        }
    }
}