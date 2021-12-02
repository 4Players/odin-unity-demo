using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 5.0f;
        [SerializeField] private Camera targetCamera;

        private void OnEnable()
        {
            if (null == targetCamera)
                targetCamera = Camera.main;
            var targetRotation = GetTargetRotation();
            transform.rotation = targetRotation;
        }

        void Update()
        {
            if (null == targetCamera)
            {
                targetCamera = Camera.main;
            }
            else
            {
                var targetRotation = GetTargetRotation();
                float deltaSpeed = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, deltaSpeed);
            }
        }

        private Quaternion GetTargetRotation()
        {
            Quaternion targetRotation = Quaternion.identity;
            if (targetCamera)
            {
                Vector3 lookRotationForward = transform.position - targetCamera.transform.position;
                targetRotation = Quaternion.LookRotation(lookRotationForward, Vector3.up);
            }

            return targetRotation;
        }
    }
}