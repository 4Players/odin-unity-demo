using System;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Defines the rotation behavior when in first person mode. Also handles hiding / showing the mouse.
    /// </summary>
    public class FirstPersonRotation : MonoBehaviour
    {
        [SerializeField] private InputActionReference lookAxis;
        /// <summary>
        /// The rotation speed.
        /// </summary>
        [SerializeField] private float rotationSpeed = 200.0f;
        /// <summary>
        /// The max angle the player can look up or down.
        /// </summary>
        [Range(0.0f, 89.9f)]
        [SerializeField] private float clampPitch = 89.0f;
        
        /// <summary>
        /// The object which should be turned around the local up axis. Can be different from the <see cref="pitchTarget"/>,
        /// if e.g. you want a visible mesh to actually yaw, but not pitch.
        /// </summary>
        [SerializeField] private GameObject yawTarget;
        /// <summary>
        /// The object which should be turned around the local right axis. Can be different from the <see cref="yawTarget"/>,
        /// if e.g. you want a visible mesh to actually pitch, but not yaw.
        /// </summary>
        [SerializeField] private GameObject pitchTarget;

        private float _currentYaw;
        private float _currentPitch;

        private void Awake()
        {
            Assert.IsNotNull(yawTarget);
            Assert.IsNotNull(pitchTarget);
            Assert.IsNotNull(lookAxis);
            lookAxis.action.Enable();
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
            Vector2 lookInput = lookAxis.action.ReadValue<Vector2>();
            float yaw = lookInput.x;
            float pitch = lookInput.y;

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
