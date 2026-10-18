using UnityEngine;
using UnityEngine.UI;
using MyShadowQuit.Core;

namespace MyShadowQuit.UI
{
    /// <summary>
    /// Master controller for responsive mobile virtual touch controls.
    /// Manages the layout, multitouch button states, ergonomic positioning, safe area,
    /// and visual themes for Android and touch devices.
    /// Integrates seamlessly with GameInput without duplicating any movement or physics logic.
    /// </summary>
    public class MobileControlsUI : MonoBehaviour
    {
        public static MobileControlsUI Instance { get; private set; }

        public const string PrefKeySize = "MSQ_MobileControls_Size";
        public const string PrefKeyOpacity = "MSQ_MobileControls_Opacity";
        public const string PrefKeyPosition = "MSQ_MobileControls_Position";
        public const string PrefKeyEnabled = "MSQ_MobileControls_Enabled";

        [Header("Buttons")]
        [SerializeField] private MobileVirtualButton leftButton;
        [SerializeField] private MobileVirtualButton rightButton;
        [SerializeField] private MobileVirtualButton jumpButton;
        [SerializeField] private MobileVirtualButton interactButton;
        [SerializeField] private MobileVirtualButton switchButton;

        private RectTransform _rootRect;
        private CanvasGroup _canvasGroup;
        private bool _isGameplayActive;
        private float _lastHorizontalDir = 0f;

        // Configuration state
        private float _currentScale = 1.0f;
        private float _currentOpacity = 0.75f;
        private int _currentPositionMode = 0; // 0: Default, 1: Wide (notches/bezel compensation)

        public MobileVirtualButton LeftButton => leftButton;
        public MobileVirtualButton RightButton => rightButton;
        public MobileVirtualButton JumpButton => jumpButton;
        public MobileVirtualButton InteractButton => interactButton;
        public MobileVirtualButton SwitchButton => switchButton;
        public float CurrentScale => _currentScale;
        public float CurrentOpacity => _currentOpacity;
        public int CurrentPositionMode => _currentPositionMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_rootRect == null) _rootRect = GetComponent<RectTransform>();
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            LoadPreferences();
            HookButtonEvents();
            ApplyLayoutAndStyle();
        }

        private void HookButtonEvents()
        {
            if (leftButton != null)
            {
                leftButton.OnButtonStateChanged -= HandleDirectionChanged;
                leftButton.OnButtonStateChanged += HandleDirectionChanged;
            }
            if (rightButton != null)
            {
                rightButton.OnButtonStateChanged -= HandleDirectionChanged;
                rightButton.OnButtonStateChanged += HandleDirectionChanged;
            }
        }

        private void HandleDirectionChanged(MobileVirtualButton.VirtualButtonType type, bool pressed)
        {
            bool leftHeld = leftButton != null && leftButton.IsPressed;
            bool rightHeld = rightButton != null && rightButton.IsPressed;

            if (pressed)
            {
                if (type == MobileVirtualButton.VirtualButtonType.Left) _lastHorizontalDir = -1f;
                else if (type == MobileVirtualButton.VirtualButtonType.Right) _lastHorizontalDir = 1f;
            }

            if (leftHeld && !rightHeld)
            {
                GameInput.SetTouchHorizontal(-1f);
            }
            else if (rightHeld && !leftHeld)
            {
                GameInput.SetTouchHorizontal(1f);
            }
            else if (leftHeld && rightHeld)
            {
                // Deterministic conflict resolution: prioritize newest pressed direction with zero jitter
                GameInput.SetTouchHorizontal(_lastHorizontalDir);
            }
            else
            {
                GameInput.SetTouchHorizontal(0f);
            }
        }

        private void LateUpdate()
        {
            // Clear single-frame impulse triggers after all game Update methods have executed
            GameInput.ClearFrameTriggers();
        }

        public void SetGameplayActive(bool active)
        {
            _isGameplayActive = active;
            UpdateVisibility();
        }

        public void UpdateVisibility()
        {
            bool shouldShow = _isGameplayActive && ShouldDisplayControlsOnPlatform();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = shouldShow ? _currentOpacity : 0f;
                _canvasGroup.interactable = shouldShow;
                _canvasGroup.blocksRaycasts = shouldShow;
            }

            if (!shouldShow)
            {
                ReleaseAll();
            }
        }

        public static bool ShouldDisplayControlsOnPlatform()
        {
            int forcePref = PlayerPrefs.GetInt(PrefKeyEnabled, 0);
            if (forcePref == 1) return true;  // Manually forced on in Settings
            if (forcePref == -1) return false; // Manually forced off

#if UNITY_EDITOR
            // Enabled by default in Editor so developers can test touch controls directly
            return true;
#elif UNITY_ANDROID || UNITY_IOS
            return true;
#elif UNITY_WEBGL
            // On WebGL, show if touch screen is supported, otherwise default to hidden on desktop
            return Input.touchSupported;
#else
            return false;
#endif
        }

        public void LoadPreferences()
        {
            _currentScale = PlayerPrefs.GetFloat(PrefKeySize, 1.0f);
            _currentOpacity = PlayerPrefs.GetFloat(PrefKeyOpacity, 0.75f);
            _currentPositionMode = PlayerPrefs.GetInt(PrefKeyPosition, 0);
        }

        public void SavePreferences()
        {
            PlayerPrefs.SetFloat(PrefKeySize, _currentScale);
            PlayerPrefs.SetFloat(PrefKeyOpacity, _currentOpacity);
            PlayerPrefs.SetInt(PrefKeyPosition, _currentPositionMode);
            PlayerPrefs.Save();
        }

        public void SetScale(float scale)
        {
            _currentScale = Mathf.Clamp(scale, 0.75f, 1.35f);
            ApplyLayoutAndStyle();
            SavePreferences();
        }

        public void SetOpacity(float opacity)
        {
            _currentOpacity = Mathf.Clamp(opacity, 0.25f, 1.0f);
            ApplyLayoutAndStyle();
            SavePreferences();
        }

        public void SetPositionMode(int mode)
        {
            _currentPositionMode = mode;
            ApplyLayoutAndStyle();
            SavePreferences();
        }

        public void CycleSize()
        {
            if (_currentScale < 0.9f) SetScale(1.0f);
            else if (_currentScale < 1.1f) SetScale(1.2f);
            else SetScale(0.8f);
        }

        public void CycleOpacity()
        {
            if (_currentOpacity < 0.6f) SetOpacity(0.75f);
            else if (_currentOpacity < 0.9f) SetOpacity(1.0f);
            else SetOpacity(0.5f);
        }

        public void CyclePosition()
        {
            SetPositionMode(_currentPositionMode == 0 ? 1 : 0);
        }

        public void ApplyLayoutAndStyle()
        {
            if (_canvasGroup != null && _isGameplayActive)
            {
                _canvasGroup.alpha = _currentOpacity;
            }

            // Offset adjustment for wide screen notches / bezels
            float sideMarginOffset = (_currentPositionMode == 1) ? 65f : 0f;

            // 1. Bottom-Left Cluster (Left & Right movement)
            if (leftButton != null)
            {
                RectTransform rt = leftButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.zero;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(110f, 110f) * _currentScale;
                    rt.anchoredPosition = new Vector2((95f + sideMarginOffset) * _currentScale, 100f * _currentScale);
                }
                leftButton.SetOpacity(_currentOpacity);
            }

            if (rightButton != null)
            {
                RectTransform rt = rightButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.zero;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(110f, 110f) * _currentScale;
                    rt.anchoredPosition = new Vector2((230f + sideMarginOffset) * _currentScale, 100f * _currentScale);
                }
                rightButton.SetOpacity(_currentOpacity);
            }

            // 2. Bottom-Right Cluster (Jump & Interact)
            if (jumpButton != null)
            {
                RectTransform rt = jumpButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(135f, 135f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-115f - sideMarginOffset) * _currentScale, 115f * _currentScale);
                }
                jumpButton.SetOpacity(_currentOpacity);
            }

            if (interactButton != null)
            {
                RectTransform rt = interactButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(105f, 105f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-265f - sideMarginOffset) * _currentScale, 95f * _currentScale);
                }
                interactButton.SetOpacity(_currentOpacity);
            }

            // 3. Right-Mid Cluster (Switch Character)
            if (switchButton != null)
            {
                RectTransform rt = switchButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(100f, 100f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-95f - sideMarginOffset) * _currentScale, 30f * _currentScale);
                }
                switchButton.SetOpacity(_currentOpacity);
            }
        }

        public void ReleaseAll()
        {
            if (leftButton != null) leftButton.Release();
            if (rightButton != null) rightButton.Release();
            if (jumpButton != null) jumpButton.Release();
            if (interactButton != null) interactButton.Release();
            if (switchButton != null) switchButton.Release();
            GameInput.ResetTouchInputs();
            _lastHorizontalDir = 0f;
        }

        private void OnDisable()
        {
            ReleaseAll();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ReleaseAll();
            }
        }

        public static MobileControlsUI Create(Transform parent)
        {
            GameObject container = new GameObject("MobileControlsRoot", typeof(RectTransform), typeof(CanvasGroup));
            container.transform.SetParent(parent, false);

            RectTransform rt = container.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            MobileControlsUI ui = container.AddComponent<MobileControlsUI>();

            // Load button textures from Resources
            Sprite leftNormal = AssetRegistry.GetSprite("UI/mobile_left_normal", 32f);
            Sprite leftPressed = AssetRegistry.GetSprite("UI/mobile_left_pressed", 32f);
            Sprite rightNormal = AssetRegistry.GetSprite("UI/mobile_right_normal", 32f);
            Sprite rightPressed = AssetRegistry.GetSprite("UI/mobile_right_pressed", 32f);
            Sprite jumpNormal = AssetRegistry.GetSprite("UI/mobile_jump_normal", 32f);
            Sprite jumpPressed = AssetRegistry.GetSprite("UI/mobile_jump_pressed", 32f);
            Sprite interactNormal = AssetRegistry.GetSprite("UI/mobile_interact_normal", 32f);
            Sprite interactPressed = AssetRegistry.GetSprite("UI/mobile_interact_pressed", 32f);
            Sprite switchNormal = AssetRegistry.GetSprite("UI/mobile_switch_normal", 32f);
            Sprite switchPressed = AssetRegistry.GetSprite("UI/mobile_switch_pressed", 32f);

            ui.leftButton = CreateButton(container.transform, "Btn_Left", MobileVirtualButton.VirtualButtonType.Left, leftNormal, leftPressed);
            ui.rightButton = CreateButton(container.transform, "Btn_Right", MobileVirtualButton.VirtualButtonType.Right, rightNormal, rightPressed);
            ui.jumpButton = CreateButton(container.transform, "Btn_Jump", MobileVirtualButton.VirtualButtonType.Jump, jumpNormal, jumpPressed);
            ui.interactButton = CreateButton(container.transform, "Btn_Interact", MobileVirtualButton.VirtualButtonType.Interact, interactNormal, interactPressed);
            ui.switchButton = CreateButton(container.transform, "Btn_Switch", MobileVirtualButton.VirtualButtonType.SwitchCharacter, switchNormal, switchPressed);

            ui.EnsureInitialized();
            return ui;
        }

        private static MobileVirtualButton CreateButton(Transform parent, string name, MobileVirtualButton.VirtualButtonType type, Sprite normal, Sprite pressed)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(MobileVirtualButton));
            obj.transform.SetParent(parent, false);

            Image img = obj.GetComponent<Image>();
            img.raycastTarget = true;
            img.preserveAspect = true;

            MobileVirtualButton btn = obj.GetComponent<MobileVirtualButton>();
            btn.Initialize(type, normal, pressed);
            return btn;
        }
    }
}
