using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace CheddarSparks.CustomDitheringPostProcessing.Demo
{
    public class ResolutionSwitcher : MonoBehaviour
    {
        private FullScreenMode _currentScreenMode = FullScreenMode.Windowed;

        void Update()
        {
            if (GetKeyDown(KeyCode.Alpha1))
            {
                SetResolution(1920, 1080);
                Debug.Log("Resolution: Full HD (1920x1080)");
            }

            if (GetKeyDown(KeyCode.Alpha2))
            {
                SetResolution(3840, 2160);
                Debug.Log("Resolution: 4K (3840x2160)");
            }

            if (GetKeyDown(KeyCode.Alpha3))
            {
                SetResolution(1280, 1024);
                Debug.Log("Resolution: 5:4 (1280x1024)");
            }

            if (GetKeyDown(KeyCode.Alpha4))
            {
                SetResolution(1080, 1920);
                Debug.Log("Resolution: Portrait Full HD (1080x1920)");
            }

            if (GetKeyDown(KeyCode.Alpha5))
            {
                SetResolution(1440, 2560);
                Debug.Log("Resolution: Portrait QHD (1440x2560)");
            }

            if (GetKeyDown(KeyCode.Alpha6))
            {
                SetResolution(1366, 768);
                Debug.Log("Resolution: WXGA (1366x768)");
            }

            if (GetKeyDown(KeyCode.F))
            {
                ToggleScreenMode();
            }
        }

        bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            switch (key)
            {
                case KeyCode.Alpha1: return keyboard.digit1Key.wasPressedThisFrame;
                case KeyCode.Alpha2: return keyboard.digit2Key.wasPressedThisFrame;
                case KeyCode.Alpha3: return keyboard.digit3Key.wasPressedThisFrame;
                case KeyCode.Alpha4: return keyboard.digit4Key.wasPressedThisFrame;
                case KeyCode.Alpha5: return keyboard.digit5Key.wasPressedThisFrame;
                case KeyCode.Alpha6: return keyboard.digit6Key.wasPressedThisFrame;
                case KeyCode.F: return keyboard.fKey.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(key);
#endif
        }

        void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, _currentScreenMode);
        }

        void ToggleScreenMode()
        {
            _currentScreenMode = _currentScreenMode switch
            {
                FullScreenMode.Windowed => FullScreenMode.FullScreenWindow,
                FullScreenMode.FullScreenWindow => FullScreenMode.ExclusiveFullScreen,
                FullScreenMode.ExclusiveFullScreen => FullScreenMode.Windowed,
                _ => FullScreenMode.Windowed
            };

            Screen.SetResolution(Screen.width, Screen.height, _currentScreenMode);

            Debug.Log($"Screen Mode: {_currentScreenMode}");
        }
    }
}