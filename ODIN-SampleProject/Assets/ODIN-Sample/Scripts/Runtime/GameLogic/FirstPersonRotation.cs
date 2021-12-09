using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class FirstPersonRotation : MonoBehaviour
    {
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";
        [SerializeField] private float rotationSpeed = 200.0f;
        [Range(0.0f, 89.9f)]
        [SerializeField] private float clampPitch = 89.0f;
        
        [SerializeField] private GameObject yawTarget;
        [SerializeField] private GameObject pitchTarget;

        private float _currentYaw;
        private float _currentPitch;

        private void Awake()
        {
            Assert.IsNotNull(yawTarget);
            Assert.IsNotNull(pitchTarget);
        }

        private void OnEnable()
        {
            Vector3 yawEuler = yawTarget.transform.rotation.eulerAngles;
            _currentYaw = yawEuler.y;
            
            Vector3 pitchEuler = pitchTarget.transform.rotation.eulerAngles;
            _currentPitch = pitchEuler.x;


            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            float yaw = Input.GetAxis(mouseXAxis);
            float pitch = -Input.GetAxis(mouseYAxis);

            _currentYaw += yaw * rotationSpeed * Time.deltaTime;
            _currentPitch += pitch * rotationSpeed * Time.deltaTime;
            _currentPitch = Mathf.Clamp(_currentPitch, -clampPitch, clampPitch);
            
            Quaternion yawRotation = Quaternion.Euler(yawTarget.transform.rotation.eulerAngles.x, _currentYaw, 0.0f);
            yawTarget.transform.rotation = yawRotation;
            
            Quaternion pitchRotation = Quaternion.Euler(_currentPitch, pitchTarget.transform.rotation.eulerAngles.y, 0.0f);
            pitchTarget.transform.rotation = pitchRotation;
        }
    }
}
