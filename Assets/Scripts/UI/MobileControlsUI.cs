using UnityEngine;
using UnityEngine.UI;
using MyShadowQuit.Core;
using MyShadowQuit.Combat;

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
        [SerializeField] private MobileVirtualButton torchActionButton;
        [SerializeField] private MobileVirtualButton switchButton;
        [SerializeField] private MobileVirtualButton attackButton;
        [SerializeField] private MobileVirtualButton cycleWeaponButton;

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
        public MobileVirtualButton TorchActionButton => torchActionButton;
        public MobileVirtualButton SwitchButton => switchButton;
        public MobileVirtualButton AttackButton => attackButton;
        public MobileVirtualButton CycleWeaponButton => cycleWeaponButton;
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

        private void Update()
        {
            UpdateCombatButtons();
            UpdateInteractButton();
        }

        private void UpdateInteractButton()
        {
            if (interactButton == null) return;

            // Contextual visibility: only show Interact button when a character is in range of an NPC or ShadowSwitch
            bool hasValidInteraction = false;

            // 1. Check NPC interaction range
            Narrative.NPCInteraction[] npcs = FindObjectsByType<Narrative.NPCInteraction>(FindObjectsSortMode.None);
            if (npcs != null && npcs.Length > 0)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector2 playerPos = player.transform.position;
                    foreach (var npc in npcs)
                    {
                        if (npc != null && !npc.IsConversationCompleted())
                        {
                            if (Vector2.Distance(playerPos, npc.transform.position) <= 1.8f)
                            {
                                hasValidInteraction = true;
                                break;
                            }
                        }
                    }
                }
            }

            // 2. Check ShadowSwitch interaction range (if controlling Shadow)
            if (!hasValidInteraction)
            {
                Entities.ShadowController shadow = FindAnyObjectByType<Entities.ShadowController>();
                if (shadow != null && shadow.IsControlActive)
                {
                    Puzzle.ShadowSwitch[] switches = FindObjectsByType<Puzzle.ShadowSwitch>(FindObjectsSortMode.None);
                    if (switches != null)
                    {
                        Vector2 shadowPos = shadow.transform.position;
                        foreach (var sw in switches)
                        {
                            if (sw != null && !sw.IsActivated)
                            {
                                if (Vector2.Distance(shadowPos, sw.transform.position) <= 1.6f)
                                {
                                    hasValidInteraction = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (interactButton.gameObject.activeSelf != hasValidInteraction)
            {
                interactButton.gameObject.SetActive(hasValidInteraction);
            }
        }

        public void UpdateCombatButtons()
        {
            int unlockedCount = WeaponCatalog.GetUnlockedWeaponsCount();
            bool hasWeapons = unlockedCount > 0;
            bool multipleWeapons = unlockedCount > 1;

            if (attackButton != null)
            {
                attackButton.gameObject.SetActive(hasWeapons);
            }

            if (cycleWeaponButton != null)
            {
                cycleWeaponButton.gameObject.SetActive(multipleWeapons);
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

#if UNITY_WEBGL
            // Permanently disabled on WebGL itch.io desktop browser builds
            return false;
#elif UNITY_ANDROID || UNITY_IOS
            return true;
#elif UNITY_EDITOR
            // Enabled by default in Editor so developers can test touch controls directly
            return true;
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
                    rt.sizeDelta = new Vector2(124f, 124f) * _currentScale;
                    rt.anchoredPosition = new Vector2((105f + sideMarginOffset) * _currentScale, 105f * _currentScale);
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
                    rt.sizeDelta = new Vector2(124f, 124f) * _currentScale;
                    rt.anchoredPosition = new Vector2((250f + sideMarginOffset) * _currentScale, 105f * _currentScale);
                }
                rightButton.SetOpacity(_currentOpacity);
            }

            // 2. Bottom-Right Cluster (Jump, Attack, Torch)
            if (jumpButton != null)
            {
                RectTransform rt = jumpButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(136f, 136f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-110f - sideMarginOffset) * _currentScale, 110f * _currentScale);
                }
                jumpButton.SetOpacity(_currentOpacity);
            }

            if (attackButton != null)
            {
                RectTransform rt = attackButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(120f, 120f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-240f - sideMarginOffset) * _currentScale, 110f * _currentScale);
                }
                attackButton.SetOpacity(_currentOpacity);
            }

            if (torchActionButton != null)
            {
                RectTransform rt = torchActionButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(110f, 110f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-360f - sideMarginOffset) * _currentScale, 110f * _currentScale);
                }
                torchActionButton.SetOpacity(_currentOpacity);
            }

            // Contextual Interact button: sits cleanly above the Torch button when valid
            if (interactButton != null)
            {
                RectTransform rt = interactButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(104f, 104f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-360f - sideMarginOffset) * _currentScale, 235f * _currentScale);
                }
                interactButton.SetOpacity(_currentOpacity);
            }

            // 3. Right-Mid Cluster (Switch Character, Switch Weapon)
            if (switchButton != null)
            {
                RectTransform rt = switchButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(114f, 114f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-105f - sideMarginOffset) * _currentScale, 50f * _currentScale);
                }
                switchButton.SetOpacity(_currentOpacity);
            }

            if (cycleWeaponButton != null)
            {
                RectTransform rt = cycleWeaponButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0.5f);
                    rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(104f, 104f) * _currentScale;
                    rt.anchoredPosition = new Vector2((-230f - sideMarginOffset) * _currentScale, 50f * _currentScale);
                }
                cycleWeaponButton.SetOpacity(_currentOpacity);
            }
        }

        public void ReleaseAll()
        {
            if (leftButton != null) leftButton.Release();
            if (rightButton != null) rightButton.Release();
            if (jumpButton != null) jumpButton.Release();
            if (interactButton != null) interactButton.Release();
            if (torchActionButton != null) torchActionButton.Release();
            if (switchButton != null) switchButton.Release();
            if (attackButton != null) attackButton.Release();
            if (cycleWeaponButton != null) cycleWeaponButton.Release();
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
            if (parent != null)
            {
                Transform existing = parent.Find("MobileControlsRoot");
                if (existing != null)
                {
                    MobileControlsUI existingUI = existing.GetComponent<MobileControlsUI>();
                    if (existingUI != null) return existingUI;
                    if (Application.isPlaying) Destroy(existing.gameObject);
                    else DestroyImmediate(existing.gameObject);
                }
            }

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
            Sprite torchNormal = AssetRegistry.GetSprite("UI/mobile_torch_normal", 32f) ?? AssetRegistry.GetSprite("UI/mobile_interact_normal", 32f);
            Sprite torchPressed = AssetRegistry.GetSprite("UI/mobile_torch_pressed", 32f) ?? AssetRegistry.GetSprite("UI/mobile_interact_pressed", 32f);
            Sprite switchCharNormal = AssetRegistry.GetSprite("UI/mobile_switch_char_normal", 32f) ?? AssetRegistry.GetSprite("UI/mobile_switch_normal", 32f);
            Sprite switchCharPressed = AssetRegistry.GetSprite("UI/mobile_switch_char_pressed", 32f) ?? AssetRegistry.GetSprite("UI/mobile_switch_pressed", 32f);
            Sprite attackNormal = AssetRegistry.GetSprite("UI/mobile_attack_normal", 32f) ?? AssetRegistry.GetSprite("UI/mobile_interact_normal", 32f);
            Sprite attackPressed = AssetRegistry.GetSprite("UI/mobile_attack_pressed", 32f) ?? AssetRegistry.GetSprite("UI/mobile_interact_pressed", 32f);
            Sprite cycleWeaponNormal = AssetRegistry.GetSprite("UI/mobile_cycle_weapon_normal", 32f) ?? AssetRegistry.GetSprite("UI/mobile_switch_normal", 32f);
            Sprite cycleWeaponPressed = AssetRegistry.GetSprite("UI/mobile_cycle_weapon_pressed", 32f) ?? AssetRegistry.GetSprite("UI/mobile_switch_pressed", 32f);
            Sprite interactNormal = AssetRegistry.GetSprite("UI/mobile_interact_normal", 32f);
            Sprite interactPressed = AssetRegistry.GetSprite("UI/mobile_interact_pressed", 32f);

            ui.leftButton = CreateButton(container.transform, "Btn_Left", MobileVirtualButton.VirtualButtonType.Left, leftNormal, leftPressed);
            ui.rightButton = CreateButton(container.transform, "Btn_Right", MobileVirtualButton.VirtualButtonType.Right, rightNormal, rightPressed);
            ui.jumpButton = CreateButton(container.transform, "Btn_Jump", MobileVirtualButton.VirtualButtonType.Jump, jumpNormal, jumpPressed);
            ui.torchActionButton = CreateButton(container.transform, "Btn_TorchAction", MobileVirtualButton.VirtualButtonType.TorchAction, torchNormal, torchPressed);
            ui.interactButton = CreateButton(container.transform, "Btn_Interact", MobileVirtualButton.VirtualButtonType.Interact, interactNormal, interactPressed);
            ui.switchButton = CreateButton(container.transform, "Btn_Switch", MobileVirtualButton.VirtualButtonType.SwitchCharacter, switchCharNormal, switchCharPressed);
            ui.attackButton = CreateButton(container.transform, "Btn_Attack", MobileVirtualButton.VirtualButtonType.Attack, attackNormal, attackPressed);
            ui.cycleWeaponButton = CreateButton(container.transform, "Btn_CycleWeapon", MobileVirtualButton.VirtualButtonType.CycleWeapon, cycleWeaponNormal, cycleWeaponPressed);

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
