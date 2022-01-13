using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Turns the current game object towards an active main camera.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        /// <summary>
        /// Rotation speed towards the target camera
        /// </summary>
        [SerializeField] private float rotationSpeed = 5.0f;

        private Camera _current;
        
        private void OnEnable()
        {
            transform.rotation = GetTargetRotation();
        }

        /// <summary>
        /// Set the target camera, at which the game object should look at.
        /// </summary>
        /// <param name="newTarget">The new target camera.</param>
        public void SetCamera(Camera newTarget)
        {
            _current = newTarget;
        }

        void Update()
        {
            if (!_current || !_current.enabled || !_current.gameObject.activeInHierarchy)
            {
                _current = Camera.main;
            }

            if (_current)
            {
                var targetRotation = GetTargetRotation();
                float deltaSpeed = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, deltaSpeed);
            }
        }

        /// <summary>
        /// Retrieves the rotation for this object to look at the current target camera.
        /// </summary>
        /// <returns>Target rotation as quaternion.</returns>
        private Quaternion GetTargetRotation()
        {
            Quaternion targetRotation = Quaternion.identity;
            if (_current)
            {
                Vector3 lookRotationForward = transform.position - _current.transform.position;
                targetRotation = Quaternion.LookRotation(lookRotationForward, Vector3.up);
            }

            return targetRotation;
        }
    }
}