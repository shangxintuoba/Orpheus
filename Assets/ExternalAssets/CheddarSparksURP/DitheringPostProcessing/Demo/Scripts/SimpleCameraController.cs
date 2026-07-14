using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace CheddarSparks.CustomDitheringPostProcessing.Demo
{
    public class SimpleCameraController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float fastSpeedMultiplier = 3f;

        [Header("Look")]
        public float lookSensitivity = 2f;
        public float maxPitch = 89f;

        [Header("Cinematic Mode")]
        public bool cinematicMode = true;
        [Tooltip("How quickly the camera smooths towards its target rotation.")]
        public float rotationSmoothTime = 0.08f;
        [Tooltip("How quickly the camera smooths towards its target position.")]
        public float movementSmoothTime = 0.12f;

        private float _yaw;
        private float _pitch;

        private Vector3 _currentVelocity;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private bool skipMouseFrame;

        public Vector3 CurrentVelocity => _currentVelocity;

        void Start()
        {
            _yaw = transform.eulerAngles.y;
            _pitch = transform.eulerAngles.x;
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
        }

        void LateUpdate()
        {
            HandleLook();
            HandleMovement();
        }

        float GetMouseX()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current != null)
                return Mouse.current.delta.ReadValue().x * 0.1f;
            return 0f;
#else
            return Input.GetAxis("Mouse X");
#endif
        }

        float GetMouseY()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current != null)
                return Mouse.current.delta.ReadValue().y * 0.1f;
            return 0f;
#else
            return Input.GetAxis("Mouse Y");
#endif
        }

        bool GetRightMouse()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
            return Input.GetMouseButton(1);
#endif
        }

        bool GetKey(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            switch (key)
            {
                case KeyCode.W: return keyboard.wKey.isPressed;
                case KeyCode.A: return keyboard.aKey.isPressed;
                case KeyCode.S: return keyboard.sKey.isPressed;
                case KeyCode.D: return keyboard.dKey.isPressed;
                case KeyCode.Q: return keyboard.qKey.isPressed;
                case KeyCode.E: return keyboard.eKey.isPressed;
                case KeyCode.LeftShift: return keyboard.leftShiftKey.isPressed;
            }
            return false;
#else
            return Input.GetKey(key);
#endif
        }

        void HandleLook()
        {
            if (GetRightMouse())
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    skipMouseFrame = true;
                }

                if (skipMouseFrame)
                {
                    skipMouseFrame = false;
                    return;
                }

                float mouseX = GetMouseX() * lookSensitivity;
                float mouseY = GetMouseY() * lookSensitivity;

                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, -maxPitch, maxPitch);

                _targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);

                if (cinematicMode)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        _targetRotation,
                        1f - Mathf.Exp(-rotationSmoothTime * Time.deltaTime * 60f)
                    );
                }
                else
                {
                    transform.rotation = _targetRotation;
                }
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.None;
            }
        }

        void HandleMovement()
        {
            float speed = moveSpeed;
            if (GetKey(KeyCode.LeftShift))
                speed *= fastSpeedMultiplier;

            int inputX = (GetKey(KeyCode.D) ? 1 : 0) - (GetKey(KeyCode.A) ? 1 : 0);
            int inputZ = (GetKey(KeyCode.W) ? 1 : 0) - (GetKey(KeyCode.S) ? 1 : 0);

            Vector3 direction = new Vector3(inputX, 0, inputZ);

            if (GetKey(KeyCode.E)) direction.y += 1;
            if (GetKey(KeyCode.Q)) direction.y -= 1;

            direction.Normalize();

            Vector3 move = transform.TransformDirection(direction) * speed * Time.deltaTime;
            _targetPosition += move;

            if (cinematicMode)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    _targetPosition,
                    ref _currentVelocity,
                    movementSmoothTime
                );
            }
            else
            {
                transform.position = _targetPosition;
            }
        }
    }
}
