using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 5.0f;

        private Camera _current;
        
        private void OnEnable()
        {
            transform.rotation = GetTargetRotation();
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