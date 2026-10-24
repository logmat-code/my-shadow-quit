using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyShadowQuit.Core;

namespace MyShadowQuit.UI
{
    /// <summary>
    /// Independent multitouch-capable virtual button component for mobile touch controls.
    /// Tracks pointer IDs explicitly to guarantee that concurrent multi-finger inputs
    /// (e.g. Right + Jump, Left + Interact, or double-jump while running) never conflict or cancel.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MobileVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum VirtualButtonType
        {
            Left,
            Right,
            Jump,
            Interact,
            SwitchCharacter,
            Attack,
            CycleWeapon
        }

        [SerializeField] private VirtualButtonType buttonType;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;

        private Image _image;
        private RectTransform _rectTransform;
        private int _activePointerId = -999;
        private bool _isPressed;
        private Vector3 _baseScale = Vector3.one;

        public VirtualButtonType Type => buttonType;
        public bool IsPressed => _isPressed;
        public int ActivePointerId => _activePointerId;

        public event System.Action<VirtualButtonType, bool> OnButtonStateChanged;

        public void Initialize(VirtualButtonType type, Sprite normal, Sprite pressed)
        {
            buttonType = type;
            normalSprite = normal;
            pressedSprite = pressed;
            EnsureInitialized();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            _baseScale = transform.localScale;

            if (_image != null && normalSprite != null)
            {
                _image.sprite = normalSprite;
            }
        }

        public void SetOpacity(float alpha)
        {
            EnsureInitialized();
            if (_image != null)
            {
                Color c = _image.color;
                c.a = Mathf.Clamp01(alpha);
                _image.color = c;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed) return;

            _activePointerId = eventData != null ? eventData.pointerId : -1;
            SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerId == _activePointerId)
            {
                SetPressed(false);
                _activePointerId = -999;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // If the finger slides out of the button area, safely release state
            if (eventData != null && eventData.pointerId == _activePointerId)
            {
                SetPressed(false);
                _activePointerId = -999;
            }
        }

        private void SetPressed(bool pressed)
        {
            _isPressed = pressed;

            // Visual feedback: sprite swap and subtle scale press
            if (_image != null)
            {
                if (pressed && pressedSprite != null)
                {
                    _image.sprite = pressedSprite;
                    transform.localScale = _baseScale * 0.96f;
                }
                else if (!pressed && normalSprite != null)
                {
                    _image.sprite = normalSprite;
                    transform.localScale = _baseScale;
                }
            }

            // Input mapping dispatch
            switch (buttonType)
            {
                case VirtualButtonType.Jump:
                    if (pressed)
                    {
                        GameInput.TriggerTouchJump();
                    }
                    else
                    {
                        GameInput.ReleaseTouchJump();
                    }
                    break;

                case VirtualButtonType.Interact:
                    if (pressed)
                    {
                        GameInput.TriggerTouchInteract();
                    }
                    break;

                case VirtualButtonType.SwitchCharacter:
                    if (pressed)
                    {
                        GameInput.TriggerTouchSwitchCharacter();
                    }
                    break;

                case VirtualButtonType.Attack:
                    if (pressed)
                    {
                        GameInput.TriggerTouchAttack();
                    }
                    break;

                case VirtualButtonType.CycleWeapon:
                    if (pressed)
                    {
                        GameInput.TriggerTouchCycleWeapon();
                    }
                    break;

                case VirtualButtonType.Left:
                case VirtualButtonType.Right:
                    // Horizontal dispatch is aggregated deterministically by MobileControlsUI
                    break;
            }

            OnButtonStateChanged?.Invoke(buttonType, pressed);
        }

        public void Release()
        {
            _activePointerId = -999;
            if (_isPressed)
            {
                SetPressed(false);
            }
        }

        private void OnDisable()
        {
            Release();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Release();
            }
        }

        // Test API for simulated QA touches
        public void SimulatePointerDown(int pointerId = -1)
        {
            _activePointerId = pointerId;
            SetPressed(true);
        }

        public void SimulatePointerUp(int pointerId = -1)
        {
            if (pointerId == _activePointerId || _activePointerId == -999)
            {
                SetPressed(false);
                _activePointerId = -999;
            }
        }
    }
}
