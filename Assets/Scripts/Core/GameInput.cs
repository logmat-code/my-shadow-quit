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
    /// E: Interact / NPC talk / Switch activate / Dialogue advance
    /// Q: Torch action (Pick up / Drop / Place in Pedestal)
    /// F: Cycle equipped weapon (Emberblade / Crossbow / Greatsword)
    /// TAB: Switch between Protagonist and Shadow
    /// R: Restart current puzzle
    /// ESC / P: Pause
    /// </summary>
    public static class GameInput
    {
        // Virtual Mobile / Touch input state
        private static float s_touchHorizontal = 0f;
        private static bool s_touchJumpHeld = false;
        private static bool s_touchJumpDownPending = false;
        private static bool s_touchInteractDownPending = false;
        private static bool s_touchTorchActionDownPending = false;
        private static bool s_touchSwitchDownPending = false;
        private static bool s_touchRestartDownPending = false;
        private static bool s_touchPauseDownPending = false;
        private static bool s_touchAttackDownPending = false;
        private static bool s_touchCycleWeaponDownPending = false;

        public static float TouchHorizontal => s_touchHorizontal;
        public static bool TouchJumpHeld => s_touchJumpHeld;

        public static void SetTouchHorizontal(float value)
        {
            s_touchHorizontal = Mathf.Clamp(value, -1f, 1f);
        }

        public static void SetTouchJumpState(bool isHeld)
        {
            if (isHeld && !s_touchJumpHeld)
            {
                s_touchJumpDownPending = true;
            }
            s_touchJumpHeld = isHeld;
        }

        public static void TriggerTouchJump()
        {
            s_touchJumpDownPending = true;
            s_touchJumpHeld = true;
        }

        public static void ReleaseTouchJump()
        {
            s_touchJumpHeld = false;
        }

        public static void TriggerTouchInteract()
        {
            s_touchInteractDownPending = true;
        }

        public static void TriggerTouchTorchAction()
        {
            s_touchTorchActionDownPending = true;
        }

        public static void TriggerTouchSwitchCharacter()
        {
            s_touchSwitchDownPending = true;
        }

        public static void TriggerTouchRestart()
        {
            s_touchRestartDownPending = true;
        }

        public static void TriggerTouchPause()
        {
            s_touchPauseDownPending = true;
        }

        public static void TriggerTouchAttack()
        {
            s_touchAttackDownPending = true;
        }

        public static void TriggerTouchCycleWeapon()
        {
            s_touchCycleWeaponDownPending = true;
        }

        public static void ClearFrameTriggers()
        {
            s_touchJumpDownPending = false;
            s_touchInteractDownPending = false;
            s_touchTorchActionDownPending = false;
            s_touchSwitchDownPending = false;
            s_touchRestartDownPending = false;
            s_touchPauseDownPending = false;
            s_touchAttackDownPending = false;
            s_touchCycleWeaponDownPending = false;
        }

        public static void ResetTouchInputs()
        {
            s_touchHorizontal = 0f;
            s_touchJumpHeld = false;
            ClearFrameTriggers();
        }

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

            if (Mathf.Abs(val) > 0.01f) return Mathf.Clamp(val, -1f, 1f);
            return Mathf.Clamp(s_touchHorizontal, -1f, 1f);
        }

        public static bool GetJumpDown()
        {
            if (s_touchJumpDownPending) return true;

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
            if (s_touchJumpHeld) return true;

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
            if (s_touchInteractDownPending) return true;

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

        public static bool GetTorchActionDown()
        {
            if (s_touchTorchActionDownPending) return true;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Q);
            }
            catch { return false; }
        }

        public static bool GetSwitchCharacterDown()
        {
            if (s_touchSwitchDownPending) return true;

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
            if (s_touchRestartDownPending) return true;

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
            if (s_touchPauseDownPending) return true;

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

        public static bool GetAttackDown()
        {
            if (s_touchAttackDownPending) return true;

            // Block mouse attack clicks if pointer is over any UI canvas / virtual buttons
            bool pointerOverUI = false;
            try
            {
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    pointerOverUI = true;
                }
            }
            catch { }

#if ENABLE_INPUT_SYSTEM
            if (!pointerOverUI && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) return true;
#endif
            try
            {
                if (!pointerOverUI && Input.GetMouseButtonDown(0)) return true;
                if (Input.GetKeyDown(KeyCode.J)) return true;
            }
            catch { }

            return false;
        }

        public static bool GetCycleWeaponDown()
        {
            if (s_touchCycleWeaponDownPending) return true;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) return true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.F)) return true;
            }
            catch { }

            return false;
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
