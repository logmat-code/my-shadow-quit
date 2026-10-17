using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Unified input reader supporting both Unity Input System and Legacy Input Manager
    /// for reliable desktop & WebGL browser controls.
    /// Controls:
    /// A / D or Left / Right: Horizontal move
    /// SPACE / W / Up: Jump
    /// E: Interact / Pick up or place Torch
    /// TAB: Switch between Protagonist and Shadow
    /// R: Restart current puzzle
    /// ESC / P: Pause
    /// </summary>
    public static class GameInput
    {
        public static float GetHorizontal()
        {
            float val = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) val -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) val += 1f;
                if (Mathf.Abs(val) > 0.01f) return Mathf.Clamp(val, -1f, 1f);
            }
#endif
            try
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) val -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) val += 1f;
            }
            catch { }
            return Mathf.Clamp(val, -1f, 1f);
        }

        public static bool GetJumpDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame ||
                 Keyboard.current.wKey.wasPressedThisFrame ||
                 Keyboard.current.upArrowKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            }
            catch { return false; }
        }

        public static bool GetJumpHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.spaceKey.isPressed ||
                 Keyboard.current.wKey.isPressed ||
                 Keyboard.current.upArrowKey.isPressed))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            }
            catch { return false; }
        }

        public static bool GetInteractDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.E);
            }
            catch { return false; }
        }

        public static bool GetSwitchCharacterDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Tab);
            }
            catch { return false; }
        }

        public static bool GetRestartDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.R);
            }
            catch { return false; }
        }

        public static bool GetPauseDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
            }
            catch { return false; }
        }

        public static bool GetCancelDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Escape);
            }
            catch { return false; }
        }

        public static bool GetConfirmDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            }
            catch { return false; }
        }

        public static bool GetMenuUpDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
            }
            catch { return false; }
        }

        public static bool GetMenuDownDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
            }
            catch { return false; }
        }

        public static bool GetMenuLeftDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
            }
            catch { return false; }
        }

        public static bool GetMenuRightDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            }
            catch { return false; }
        }

        public static Vector2 GetMouseScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
#endif
            try
            {
                return Input.mousePosition;
            }
            catch { return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f); }
        }
    }
}
