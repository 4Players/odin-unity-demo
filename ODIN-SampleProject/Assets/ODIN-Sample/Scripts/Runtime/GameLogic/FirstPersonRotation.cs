using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    ///     Defines the rotation behavior when in first person mode. Also handles hiding / showing the mouse.
    /// </summary>
    public class FirstPersonRotation : MonoBehaviour
    {
        /// <summary>
        /// The action defining for gamepad and mobile onscreen control input.
        /// </summary>
        [SerializeField] private InputActionReference gamepadAxis;
        /// <summary>
        /// The action returning the mouse delta.
        /// </summary>
        [SerializeField] private InputActionReference mouseDelta;


        /// <summary>
        ///     The rotation speed.
        /// </summary>
        [SerializeField] private float rotationSpeed = 200.0f;

        /// <summary>
        ///     On mobile, a slower rotation speed is preferable.
        /// </summary>
        [SerializeField] private float mobileRotationSpeedMultiplier = 0.5f;
        [SerializeField] private float mouseRotationSpeedMultiplier = 0.25f;


        /// <summary>
        ///     The max angle the player can look up or down.
        /// </summary>
        [Range(0.0f, 89.9f)] [SerializeField] private float clampPitch = 89.0f;

        /// <summary>
        ///     The object which should be turned around the local up axis. Can be different from the <see cref="pitchTarget" />,
        ///     if e.g. you want a visible mesh to actually yaw, but not pitch.
        /// </summary>
        [SerializeField] private GameObject yawTarget;

        /// <summary>
        ///     The object which should be turned around the local right axis. Can be different from the <see cref="yawTarget" />,
        ///     if e.g. you want a visible mesh to actually pitch, but not yaw.
        /// </summary>
        [SerializeField] private GameObject pitchTarget;

        private float _currentPitch;

        private float _currentYaw;

        private void Awake()
        {
            Assert.IsNotNull(yawTarget);
            Assert.IsNotNull(pitchTarget);
            Assert.IsNotNull(gamepadAxis);
            Assert.IsNotNull(mouseDelta);
            gamepadAxis.action.Enable();
            mouseDelta.action.Enable();
        }

        private void Update()
        {
            Vector2 gamepadInput = gamepadAxis.action.ReadValue<Vector2>();
            float yaw = gamepadInput.x * Time.deltaTime;
            float pitch = gamepadInput.y * Time.deltaTime;

            // mouse delta is already multiplied by Time.deltaTime.
            Vector2 mouseInput = mouseDelta.action.ReadValue<Vector2>();
            yaw += mouseInput.x * mouseRotationSpeedMultiplier;
            pitch += mouseInput.y * mouseRotationSpeedMultiplier;

            float rotationSpeedMultiplier = rotationSpeed;

#if (UNITY_IPHONE || UNITY_ANDROID) && ! UNITY_EDITOR
            rotationSpeedMultiplier *= mobileRotationSpeedMultiplier;
#endif


            _currentYaw += yaw * rotationSpeedMultiplier;
            _currentPitch += pitch * rotationSpeedMultiplier;
            _currentPitch = Mathf.Clamp(_currentPitch, -clampPitch, clampPitch);

            Quaternion yawRotation = Quaternion.Euler(yawTarget.transform.rotation.eulerAngles.x, _currentYaw, 0.0f);
            yawTarget.transform.rotation = yawRotation;

            Quaternion pitchRotation =
                Quaternion.Euler(_currentPitch, pitchTarget.transform.rotation.eulerAngles.y, 0.0f);
            pitchTarget.transform.rotation = pitchRotation;
        }

        private void OnEnable()
        {
            Vector3 yawEuler = yawTarget.transform.rotation.eulerAngles;
            _currentYaw = yawEuler.y;

            Vector3 pitchEuler = pitchTarget.transform.rotation.eulerAngles;
            _currentPitch = pitchEuler.x;

#if !(UNITY_IPHONE || UNITY_ANDROID) || UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
#endif
        }

        private void OnDisable()
        {
#if !(UNITY_IPHONE || UNITY_ANDROID) || UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#endif
        }
    }
}