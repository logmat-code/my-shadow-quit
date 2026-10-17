using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyShadowQuit.Core;

namespace MyShadowQuit.UI
{
    /// <summary>
    /// Complete dark-fantasy pixel-art Menu & Navigation UI system for MY SHADOW QUIT: THE LAST FLAME.
    /// Strictly designed for 1920x1080 (16:9 Landscape) reference resolution.
    /// Implements:
    /// 1. HOME / MAIN MENU: Cinematic ruins diorama with animated Protagonist, burning Torch, and Shadow,
    ///    floating ember particles, flickering light halos, and CONTINUE, NEW GAME (with overwrite confirmation),
    ///    WORLD SELECT, SETTINGS, CREDITS, and platform-aware QUIT.
    /// 2. WORLD SELECT: Large illustrated pixel-art cards for WORLD 1 (The Ruins of the Last Flame — Available)
    ///    and WORLD 2 (Coming Soon — Locked), progress bar, and smooth ambient lighting shifts.
    /// 3. LEVEL SELECT: Full-screen 16:9 interactive pixel-art Map of the Ruins of World 1 with 10 bespoke
    ///    architectural nodes (01 Awakening through 10 The First Dawn), connected runic pathways,
    ///    Completed/Available/Locked/Selected visual states, mouse & keyboard navigation, and Level Inspector panel.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public event Action OnPlayRequested;
        public event Action OnContinueRequested;
        public event Action OnNewGameRequested;
        public event Action OnWorldSelectRequested;
        public event Action OnLevelSelectRequested;
        public event Action<int, bool> OnStartWorldRequested;
        public event Action<int> OnLevelSelectedFromMap;
        public event Action OnWorldCompleteContinueRequested;
        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnMainMenuRequested;
        public event Action OnPauseRequested;

        public enum ActiveMenuScreen
        {
            None,
            Home,
            WorldSelect,
            LevelSelect,
            Pause,
            Settings,
            Credits,
            World1Complete,
            World2ComingSoon,
            Victory,
            NewGameConfirm
        }

        private Canvas _canvas;
        private GameObject _mainMenuPanel;
        private GameObject _newGameConfirmModal;
        private GameObject _worldSelectPanel;
        private GameObject _levelSelectPanel;
        private GameObject _pauseMenuContainer;
        private GameObject _pauseMenuPanel;
        private GameObject _settingsPanel;
        private GameObject _creditsPanel;
        private GameObject _hudPanel;
        private GameObject _worldCompletePanel;
        private GameObject _world2ComingSoonPanel;
        private GameObject _victoryPanel;

        private ActiveMenuScreen _currentScreen = ActiveMenuScreen.None;
        private ActiveMenuScreen _screenBeforeSubPanel = ActiveMenuScreen.Home;

        // HUD elements
        private Text _levelTitleText;
        private Text _hintText;
        private Text _activeControlText;
        private Text _musicVolumeText;
        private Text _sfxVolumeText;
        private Text _endingQuoteText;
        private Image _fadeOverlay;

        // Home diorama & buttons
        private RectTransform _homeBgRect;
        private Image _homeProtagonistImage;
        private Image _homeShadowImage;
        private Image _homeTorchFlameImage;
        private Image _homeTorchHaloOuter;
        private Image _homeTorchHaloInner;
        private Button _homeContinueButton;
        private Image _homeContinueBtnImage;
        private Text _homeContinueBtnLabel;
        private readonly List<Button> _homeMenuButtons = new List<Button>();
        private int _homeSelectedBtnIndex;

        // In-game pause button & pause menu navigation
        private Button _inGamePauseButton;
        private readonly List<Button> _pauseMenuButtons = new List<Button>();
        private int _pauseSelectedBtnIndex;

        // Floating ember particles shared across menu screens
        private struct MenuEmber
        {
            public RectTransform Rect;
            public Image Img;
            public Vector2 BasePos;
            public float SpeedY;
            public float SwayAmp;
            public float SwayFreq;
            public float Phase;
            public bool IsViolet;
        }
        private readonly List<MenuEmber> _menuEmbers = new List<MenuEmber>();

        // World Select elements
        private Image _worldSelectAmbientGlow;
        private RectTransform _world1CardRect;
        private RectTransform _world2CardRect;
        private Image _world1CardSelectionOutline;
        private Image _world2CardSelectionOutline;
        private Text _world1ProgressText;
        private RectTransform _world1ProgressBarFill;
        private int _selectedWorldCardIndex; // 0 = World 1, 1 = World 2

        // Level Select Map elements
        private class MapNodeUI
        {
            public int LevelIndex;
            public Vector2 MapAnchoredPos;
            public Button NodeButton;
            public RectTransform NodeRect;
            public Image NodeIconImage;
            public Image NodeGlowImage;
            public Text NodeNumberLabel;
            public bool IsUnlocked;
            public bool IsCompleted;
        }
        private readonly List<MapNodeUI> _mapNodes = new List<MapNodeUI>();
        private readonly List<Image> _mapPathSegments = new List<Image>();
        private readonly List<int> _mapPathTargetNodeIndex = new List<int>();
        private RectTransform _mapSelectedRingRect;
        private Image _mapSelectedRingImage;
        private Text _mapHeaderProgressText;
        private Text _inspectorLevelNumberText;
        private Text _inspectorLevelTitleText;
        private Text _inspectorStatusBadgeText;
        private Text _inspectorHintText;
        private Button _inspectorPlayButton;
        private Image _inspectorPlayButtonImage;
        private Text _inspectorPlayButtonLabel;
        private int _selectedMapLevelIndex;

        // Sprites & Fonts
        private Font _uiFont;
        private Sprite _btnNormal;
        private Sprite _btnHover;
        private Sprite _btnSelected;
        private Sprite _btnPressed;
        private Sprite _btnDisabled;
        private Sprite _panelSprite;
        private Sprite _modalFrameSprite;
        private Sprite _homeRuinsBgSprite;
        private Sprite _homeFgArchSprite;
        private Sprite _logoCrestSprite;
        private Sprite _stoneDividerSprite;
        private Sprite _world1ArtSprite;
        private Sprite _world2ArtSprite;
        private Sprite _worldCardActiveSprite;
        private Sprite _worldCardLockedSprite;
        private Sprite _lockSealSprite;
        private Sprite _ruinsMapSprite;
        private Sprite _nodeCompletedSprite;
        private Sprite _nodeAvailableSprite;
        private Sprite _nodeLockedSprite;
        private Sprite _nodeSelectedRingSprite;
        private Sprite _pathLitSprite;
        private Sprite _pathDarkSprite;
        private Sprite _lightHaloSprite;
        private Sprite _emberSprite;
        private Sprite _btnPauseHudNormal;
        private Sprite _btnPauseHudHover;
        private Sprite _btnPauseHudPressed;

        private Sprite[] _protagonistIdleFrames;
        private Sprite[] _shadowIdleFrames;
        private Sprite[] _torchFlameFrames;

        private float _targetFadeAlpha;
        private float _fadeSpeed = 2.5f;
        private bool _isTransitioning;
        private Coroutine _transitionCoroutine;

        // Exact 10 Node coordinates in 480x270 map pixel space
        private static readonly Vector2[] MapNodePixelCoords =
        {
            new Vector2(52f, 212f),   // 01 — AWAKENING
            new Vector2(102f, 168f),  // 02 — THE SEPARATION
            new Vector2(152f, 168f),  // 03 — FOLLOW THE LIGHT
            new Vector2(194f, 204f),  // 04 — THE FORGOTTEN FLAME
            new Vector2(246f, 204f),  // 05 — DARK PASSAGE
            new Vector2(246f, 134f),  // 06 — TWO SIDES
            new Vector2(304f, 134f),  // 07 — THE LAST LIGHT
            new Vector2(346f, 174f),  // 08 — MY SHADOW QUIT
            new Vector2(384f, 114f),  // 09 — ECHOES IN THE DARK
            new Vector2(432f, 86f)    // 10 — THE FIRST DAWN
        };

        public static readonly string[] CanonicalLevelShortNames =
        {
            "AWAKENING",
            "THE SEPARATION",
            "FOLLOW THE LIGHT",
            "THE FORGOTTEN FLAME",
            "DARK PASSAGE",
            "TWO SIDES",
            "THE LAST LIGHT",
            "MY SHADOW QUIT",
            "ECHOES IN THE DARK",
            "THE FIRST DAWN"
        };

        public const string World1CompleteQuote = "\"Beyond the darkness, another light awaits.\"";
        public const string FinalNarrativeQuote = "\"Even shadows need a light to return to.\"";

        public bool IsHomeScreenVisible => _mainMenuPanel != null && _mainMenuPanel.activeSelf;
        public bool IsWorldSelectScreenVisible => _worldSelectPanel != null && _worldSelectPanel.activeSelf;
        public bool IsLevelSelectScreenVisible => _levelSelectPanel != null && _levelSelectPanel.activeSelf;
        public bool IsPauseMenuVisible => _pauseMenuContainer != null && _pauseMenuContainer.activeSelf;
        public bool IsWorld1CompleteScreenVisible => _worldCompletePanel != null && _worldCompletePanel.activeSelf;
        public bool IsWorld2ComingSoonScreenVisible => _world2ComingSoonPanel != null && _world2ComingSoonPanel.activeSelf;
        public bool IsVictoryScreenVisible => _victoryPanel != null && _victoryPanel.activeSelf;
        public bool IsNewGameConfirmVisible => _newGameConfirmModal != null && _newGameConfirmModal.activeSelf;

        public GameObject MainMenuPanel => _mainMenuPanel;
        public GameObject WorldSelectPanel => _worldSelectPanel;
        public GameObject LevelSelectPanel => _levelSelectPanel;
        public GameObject PauseMenuPanel => _pauseMenuPanel;
        public GameObject PauseMenuContainer => _pauseMenuContainer;
        public Button InGamePauseButton => _inGamePauseButton;
        public GameObject WorldCompletePanel => _worldCompletePanel;
        public GameObject World2ComingSoonPanel => _world2ComingSoonPanel;
        public GameObject VictoryPanel => _victoryPanel;
        public int SelectedMapLevelIndex => _selectedMapLevelIndex;
        public int MapNodeCount => _mapNodes.Count;
        public int PauseButtonCount => _pauseMenuButtons.Count;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_canvas != null && _mainMenuPanel != null && _worldSelectPanel != null && _levelSelectPanel != null)
            {
                return;
            }
            LoadUIAssets();
            EnsureEventSystem();
            BuildCanvasAndPanels();
        }

        private void LoadUIAssets()
        {
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_uiFont == null)
            {
                _uiFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            _btnNormal = AssetRegistry.GetSprite("UI/ui_button_normal", 32f);
            _btnHover = AssetRegistry.GetSprite("UI/ui_button_hover", 32f);
            _btnSelected = AssetRegistry.GetSprite("UI/ui_button_selected", 32f) ?? _btnHover;
            _btnPressed = AssetRegistry.GetSprite("UI/ui_button_pressed", 32f) ?? _btnHover;
            _btnDisabled = AssetRegistry.GetSprite("UI/ui_button_disabled", 32f) ?? _btnNormal;
            _panelSprite = AssetRegistry.GetSprite("UI/ui_panel", 32f);
            _modalFrameSprite = AssetRegistry.GetSprite("UI/ui_modal_frame", 32f) ?? _panelSprite;

            _homeRuinsBgSprite = AssetRegistry.GetSprite("UI/home_ruins_bg", 32f);
            _homeFgArchSprite = AssetRegistry.GetSprite("UI/home_foreground_arch", 32f);
            _logoCrestSprite = AssetRegistry.GetSprite("UI/logo_emblem_crest", 32f);
            _stoneDividerSprite = AssetRegistry.GetSprite("UI/stone_ornament_divider", 32f);

            _world1ArtSprite = AssetRegistry.GetSprite("UI/world1_card_art", 32f);
            _world2ArtSprite = AssetRegistry.GetSprite("UI/world2_card_art", 32f);
            _worldCardActiveSprite = AssetRegistry.GetSprite("UI/world_card_frame_active", 32f) ?? _panelSprite;
            _worldCardLockedSprite = AssetRegistry.GetSprite("UI/world_card_frame_locked", 32f) ?? _panelSprite;
            _lockSealSprite = AssetRegistry.GetSprite("UI/icon_lock_seal", 32f);

            _ruinsMapSprite = AssetRegistry.GetSprite("UI/world1_ruins_map", 32f);
            _nodeCompletedSprite = AssetRegistry.GetSprite("UI/map_node_completed", 32f);
            _nodeAvailableSprite = AssetRegistry.GetSprite("UI/map_node_available", 32f);
            _nodeLockedSprite = AssetRegistry.GetSprite("UI/map_node_locked", 32f);
            _nodeSelectedRingSprite = AssetRegistry.GetSprite("UI/map_node_selected_ring", 32f);
            _pathLitSprite = AssetRegistry.GetSprite("UI/map_path_segment_lit", 32f);
            _pathDarkSprite = AssetRegistry.GetSprite("UI/map_path_segment_dark", 32f);

            _lightHaloSprite = AssetRegistry.GetSprite("Torch/light_halo", 32f, FilterMode.Bilinear);
            _emberSprite = AssetRegistry.GetSprite("Torch/ember_particle", 32f);

            _btnPauseHudNormal = AssetRegistry.GetSprite("UI/btn_pause_hud_normal", 32f);
            _btnPauseHudHover = AssetRegistry.GetSprite("UI/btn_pause_hud_hover", 32f) ?? _btnPauseHudNormal;
            _btnPauseHudPressed = AssetRegistry.GetSprite("UI/btn_pause_hud_pressed", 32f) ?? _btnPauseHudNormal;

            _protagonistIdleFrames = AssetRegistry.GetAnimationFrames("Characters/protagonist_idle", 4);
            _shadowIdleFrames = AssetRegistry.GetAnimationFrames("Characters/shadow_idle", 4);
            _torchFlameFrames = AssetRegistry.GetAnimationFrames("Torch/torch_item", 4);
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.transform.SetParent(transform, false);
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void BuildCanvasAndPanels()
        {
            Transform existingCanvas = transform.Find("UICanvas");
            if (existingCanvas != null)
            {
                if (Application.isPlaying)
                {
                    existingCanvas.gameObject.SetActive(false);
                    Destroy(existingCanvas.gameObject);
                }
                else
                {
                    DestroyImmediate(existingCanvas.gameObject);
                }
            }

            _homeMenuButtons.Clear();
            _pauseMenuButtons.Clear();
            _menuEmbers.Clear();
            _mapNodes.Clear();
            _mapPathSegments.Clear();
            _mapPathTargetNodeIndex.Clear();

            GameObject canvasObj = new GameObject("UICanvas");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            BuildHUD(canvasObj.transform);
            BuildMainMenu(canvasObj.transform);
            BuildWorldSelectMenu(canvasObj.transform);
            BuildLevelSelectMapMenu(canvasObj.transform);
            BuildPauseMenu(canvasObj.transform);
            BuildSettingsMenu(canvasObj.transform);
            BuildCreditsMenu(canvasObj.transform);
            BuildWorld1CompleteScreen(canvasObj.transform);
            BuildWorld2ComingSoonScreen(canvasObj.transform);
            BuildVictoryScreen(canvasObj.transform);
            BuildNewGameConfirmModal(canvasObj.transform);
            BuildFadeOverlay(canvasObj.transform);
        }

        // ====================================================================
        // 1. GAMEPLAY HUD (1920x1080)
        // ====================================================================
        private void BuildHUD(Transform parent)
        {
            _hudPanel = CreateFullScreenContainer(parent, "HUDPanel");

            GameObject topBar = CreateImagePanel(_hudPanel.transform, "TopHUD", new Vector2(0f, 480f), new Vector2(1820f, 76f));
            topBar.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.12f, 0.85f);

            _levelTitleText = CreateText(topBar.transform, "LevelTitle", "I. AWAKENING", 30, new Vector2(-560f, 0f), new Vector2(620f, 58f), new Color(1f, 0.82f, 0.35f, 1f), TextAnchor.MiddleLeft);
            _activeControlText = CreateText(topBar.transform, "ActiveControl", "ACTIVE: PROTAGONIST [TAB to switch]", 25, new Vector2(430f, 0f), new Vector2(700f, 58f), new Color(0.85f, 0.78f, 1f, 1f), TextAnchor.MiddleRight);

            // In-Game Pause Button in the top right of TopHUD
            _inGamePauseButton = CreateCustomSpriteButton(topBar.transform, "InGamePauseButton", _btnPauseHudNormal, _btnPauseHudHover, _btnPauseHudPressed, new Vector2(845f, 0f), new Vector2(48f, 48f), () =>
            {
                if (_isTransitioning) return;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.15f);
                }
                OnPauseRequested?.Invoke();
            });

            GameObject bottomBar = CreateImagePanel(_hudPanel.transform, "BottomHUD", new Vector2(0f, -480f), new Vector2(1480f, 64f));
            bottomBar.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.10f, 0.88f);

            _hintText = CreateText(bottomBar.transform, "HintText", "", 25, Vector2.zero, new Vector2(1420f, 54f), new Color(1f, 0.92f, 0.72f, 1f), TextAnchor.MiddleCenter);
        }

        // ====================================================================
        // 2. HOME — MAIN MENU (1920x1080 Cinematic Scene)
        // ====================================================================
        private void BuildMainMenu(Transform parent)
        {
            _mainMenuPanel = CreateFullScreenContainer(parent, "MainMenuPanel");

            // Layer 1: Full-screen ancient ruins background with gentle parallax margin
            GameObject bgObj = CreateSpriteElement(_mainMenuPanel.transform, "HomeRuinsBackground", _homeRuinsBgSprite, Vector2.zero, new Vector2(1980f, 1114f));
            _homeBgRect = bgObj.GetComponent<RectTransform>();

            // Layer 2: Center-left Sanctuary Diorama (Protagonist + Burning Torch on Pedestal + Conscious Shadow)
            GameObject dioramaRoot = new GameObject("SanctuaryDiorama", typeof(RectTransform));
            dioramaRoot.transform.SetParent(_mainMenuPanel.transform, false);
            RectTransform dioRt = dioramaRoot.GetComponent<RectTransform>();
            dioRt.anchoredPosition = new Vector2(-350f, -155f);
            dioRt.sizeDelta = new Vector2(820f, 520f);

            // Outer & Inner flickering torchlight halos illuminating Protagonist and Shadow
            GameObject outerHaloObj = CreateSpriteElement(dioramaRoot.transform, "TorchOuterHalo", _lightHaloSprite, new Vector2(0f, 58f), new Vector2(680f, 680f));
            _homeTorchHaloOuter = outerHaloObj.GetComponent<Image>();
            _homeTorchHaloOuter.color = new Color(1f, 0.64f, 0.18f, 0.34f);
            _homeTorchHaloOuter.raycastTarget = false;

            GameObject innerHaloObj = CreateSpriteElement(dioramaRoot.transform, "TorchInnerHalo", _lightHaloSprite, new Vector2(0f, 68f), new Vector2(340f, 340f));
            _homeTorchHaloInner = innerHaloObj.GetComponent<Image>();
            _homeTorchHaloInner.color = new Color(1f, 0.88f, 0.42f, 0.52f);
            _homeTorchHaloInner.raycastTarget = false;

            // Shadow violet aura behind the Shadow character
            GameObject shadowAuraObj = CreateSpriteElement(dioramaRoot.transform, "ShadowAuraHalo", _lightHaloSprite, new Vector2(195f, 35f), new Vector2(260f, 260f));
            Image shadowAuraImg = shadowAuraObj.GetComponent<Image>();
            shadowAuraImg.color = new Color(0.58f, 0.34f, 0.94f, 0.32f);
            shadowAuraImg.raycastTarget = false;

            // Pedestal in center of dais
            Sprite pedSprite = AssetRegistry.GetSprite("Environment/torch_pedestal_lit", 32f);
            CreateSpriteElement(dioramaRoot.transform, "DioramaPedestal", pedSprite, new Vector2(0f, -24f), new Vector2(140f, 140f));

            // Animated Torch Flame on the pedestal
            Sprite initTorch = (_torchFlameFrames != null && _torchFlameFrames.Length > 0) ? _torchFlameFrames[0] : AssetRegistry.GetSprite("Torch/torch_item_0", 32f);
            GameObject torchObj = CreateSpriteElement(dioramaRoot.transform, "DioramaTorchFlame", initTorch, new Vector2(0f, 64f), new Vector2(128f, 128f));
            _homeTorchFlameImage = torchObj.GetComponent<Image>();

            // Protagonist on the left side of the Torch, illuminated by warm firelight
            Sprite initProt = (_protagonistIdleFrames != null && _protagonistIdleFrames.Length > 0) ? _protagonistIdleFrames[0] : AssetRegistry.GetSprite("Characters/protagonist_idle_0", 32f);
            GameObject protObj = CreateSpriteElement(dioramaRoot.transform, "DioramaProtagonist", initProt, new Vector2(-190f, 22f), new Vector2(210f, 210f));
            _homeProtagonistImage = protObj.GetComponent<Image>();
            _homeProtagonistImage.color = new Color(1f, 0.95f, 0.86f, 1f);

            // Conscious Shadow on the right side of the Torch, facing the flame
            Sprite initShad = (_shadowIdleFrames != null && _shadowIdleFrames.Length > 0) ? _shadowIdleFrames[0] : AssetRegistry.GetSprite("Characters/shadow_idle_0", 32f);
            GameObject shadObj = CreateSpriteElement(dioramaRoot.transform, "DioramaShadow", initShad, new Vector2(190f, 22f), new Vector2(210f, 210f));
            shadObj.GetComponent<RectTransform>().localScale = new Vector3(-1f, 1f, 1f); // Face left toward the torch
            _homeShadowImage = shadObj.GetComponent<Image>();
            _homeShadowImage.color = new Color(0.86f, 0.72f, 1f, 0.94f);

            // Layer 3: Floating ember particles across the ruins
            BuildFloatingEmbers(_mainMenuPanel.transform, 18);

            // Layer 4: Foreground stone archway framing the 16:9 viewport
            GameObject fgArch = CreateSpriteElement(_mainMenuPanel.transform, "HomeForegroundArch", _homeFgArchSprite, Vector2.zero, new Vector2(1920f, 1080f));
            fgArch.GetComponent<Image>().raycastTarget = false;

            // Layer 5: Title & Emblem Crest (Top-Left / Center-Top focal header)
            GameObject headerGroup = new GameObject("HomeTitleHeader", typeof(RectTransform));
            headerGroup.transform.SetParent(_mainMenuPanel.transform, false);
            RectTransform hrt = headerGroup.GetComponent<RectTransform>();
            hrt.anchoredPosition = new Vector2(-350f, 325f);
            hrt.sizeDelta = new Vector2(920f, 240f);

            CreateSpriteElement(headerGroup.transform, "LogoCrest", _logoCrestSprite, new Vector2(0f, 10f), new Vector2(860f, 210f));
            CreateStyledTitleText(headerGroup.transform, "TitleMain", "MY SHADOW QUIT", 54, new Vector2(0f, 36f), new Vector2(820f, 74f), new Color(1f, 0.86f, 0.38f, 1f));
            CreateStyledTitleText(headerGroup.transform, "TitleSub", "THE LAST FLAME", 28, new Vector2(0f, -24f), new Vector2(680f, 44f), new Color(0.80f, 0.64f, 1f, 1f));
            CreateSpriteElement(headerGroup.transform, "HeaderDivider", _stoneDividerSprite, new Vector2(0f, -68f), new Vector2(560f, 32f));

            // Layer 6: Right-Side Sanctuary Command Column
            GameObject menuColumn = CreateImagePanel(_mainMenuPanel.transform, "HomeCommandPanel", new Vector2(520f, -10f), new Vector2(520f, 760f), _modalFrameSprite);
            menuColumn.GetComponent<Image>().color = new Color(0.95f, 0.95f, 1f, 0.94f);

            CreateText(menuColumn.transform, "MenuColumnHeader", "SANCTUARY GATE", 24, new Vector2(0f, 310f), new Vector2(420f, 38f), new Color(1f, 0.82f, 0.36f, 1f), TextAnchor.MiddleCenter);
            CreateSpriteElement(menuColumn.transform, "MenuColumnDivider", _stoneDividerSprite, new Vector2(0f, 276f), new Vector2(380f, 24f));

            _homeContinueButton = CreateMenuButton(menuColumn.transform, "CONTINUE", new Vector2(0f, 200f), () =>
            {
                if (!WorldProgressSave.HasSavedProgress("world_1")) return;
                TransitionToAction(() => OnContinueRequested?.Invoke());
            }, new Vector2(380f, 64f));
            _homeContinueBtnImage = _homeContinueButton.GetComponent<Image>();
            _homeContinueBtnLabel = _homeContinueButton.GetComponentInChildren<Text>();
            _homeMenuButtons.Add(_homeContinueButton);

            Button newGameBtn = CreateMenuButton(menuColumn.transform, "NEW GAME", new Vector2(0f, 118f), () =>
            {
                if (WorldProgressSave.HasSavedProgress("world_1"))
                {
                    ShowNewGameConfirmationModal();
                }
                else
                {
                    TransitionToAction(() => OnNewGameRequested?.Invoke());
                }
            }, new Vector2(380f, 64f));
            _homeMenuButtons.Add(newGameBtn);

            Button worldSelectBtn = CreateMenuButton(menuColumn.transform, "WORLD SELECT", new Vector2(0f, 36f), () =>
            {
                TransitionToAction(() =>
                {
                    OnWorldSelectRequested?.Invoke();
                    ShowWorldSelectScreen();
                });
            }, new Vector2(380f, 64f));
            _homeMenuButtons.Add(worldSelectBtn);

            Button settingsBtn = CreateMenuButton(menuColumn.transform, "SETTINGS", new Vector2(0f, -46f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Home;
                ShowSubPanel(_settingsPanel);
            }, new Vector2(380f, 64f));
            _homeMenuButtons.Add(settingsBtn);

            Button creditsBtn = CreateMenuButton(menuColumn.transform, "CREDITS", new Vector2(0f, -128f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Home;
                ShowSubPanel(_creditsPanel);
            }, new Vector2(380f, 64f));
            _homeMenuButtons.Add(creditsBtn);

            // Platform-aware QUIT button: hidden in WebGL browser builds so it never attempts to close the browser tab
            bool isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            if (!isWebGL)
            {
                Button quitBtn = CreateMenuButton(menuColumn.transform, "QUIT", new Vector2(0f, -210f), () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }, new Vector2(380f, 64f));
                _homeMenuButtons.Add(quitBtn);
            }
        }

        private void BuildNewGameConfirmModal(Transform parent)
        {
            _newGameConfirmModal = CreateFullScreenContainer(parent, "NewGameConfirmModal");
            Image backdrop = _newGameConfirmModal.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.03f, 0.06f, 0.82f);

            GameObject dialog = CreateImagePanel(_newGameConfirmModal.transform, "ConfirmDialogBox", Vector2.zero, new Vector2(720f, 420f), _modalFrameSprite);
            CreateStyledTitleText(dialog.transform, "ConfirmTitle", "BEGIN A NEW FLAME?", 36, new Vector2(0f, 125f), new Vector2(620f, 52f), new Color(1f, 0.84f, 0.36f, 1f));
            CreateSpriteElement(dialog.transform, "ConfirmDivider", _stoneDividerSprite, new Vector2(0f, 78f), new Vector2(460f, 24f));

            CreateText(
                dialog.transform,
                "ConfirmBody",
                "Saved progress exists in World 1: The Ruins of the Last Flame.\nStarting a New Game will reset your world progress back to Level 01 (Awakening).",
                22,
                new Vector2(0f, 8f),
                new Vector2(620f, 110f),
                new Color(0.92f, 0.90f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(dialog.transform, "START NEW GAME", new Vector2(-175f, -115f), () =>
            {
                _newGameConfirmModal.SetActive(false);
                TransitionToAction(() => OnNewGameRequested?.Invoke());
            }, new Vector2(300f, 60f));

            CreateMenuButton(dialog.transform, "CANCEL", new Vector2(175f, -115f), () =>
            {
                PlayMenuBackSound();
                _newGameConfirmModal.SetActive(false);
                _currentScreen = ActiveMenuScreen.Home;
            }, new Vector2(300f, 60f));

            _newGameConfirmModal.SetActive(false);
        }

        public void ShowNewGameConfirmationModal()
        {
            if (_newGameConfirmModal != null)
            {
                _newGameConfirmModal.SetActive(true);
                _currentScreen = ActiveMenuScreen.NewGameConfirm;
            }
        }

        // ====================================================================
        // 3. WORLD SELECT (1920x1080 Large Visual World Cards)
        // ====================================================================
        private void BuildWorldSelectMenu(Transform parent)
        {
            _worldSelectPanel = CreateFullScreenContainer(parent, "WorldSelectPanel");

            // Background ruins + dynamic ambient illumination halo that shifts with selected world
            GameObject bgObj = CreateSpriteElement(_worldSelectPanel.transform, "WorldSelectBg", _homeRuinsBgSprite, Vector2.zero, new Vector2(1920f, 1080f));
            bgObj.GetComponent<Image>().color = new Color(0.55f, 0.55f, 0.68f, 1f);

            GameObject glowObj = CreateSpriteElement(_worldSelectPanel.transform, "WorldSelectAmbientGlow", _lightHaloSprite, new Vector2(-340f, 20f), new Vector2(1100f, 900f));
            _worldSelectAmbientGlow = glowObj.GetComponent<Image>();
            _worldSelectAmbientGlow.color = new Color(1f, 0.68f, 0.24f, 0.28f);
            _worldSelectAmbientGlow.raycastTarget = false;

            GameObject fgArch = CreateSpriteElement(_worldSelectPanel.transform, "WorldSelectFgArch", _homeFgArchSprite, Vector2.zero, new Vector2(1920f, 1080f));
            fgArch.GetComponent<Image>().raycastTarget = false;

            // Header
            CreateStyledTitleText(_worldSelectPanel.transform, "WorldSelectHeader", "SELECT YOUR WORLD", 48, new Vector2(0f, 445f), new Vector2(900f, 64f), new Color(1f, 0.86f, 0.38f, 1f));
            CreateSpriteElement(_worldSelectPanel.transform, "WorldSelectDivider", _stoneDividerSprite, new Vector2(0f, 396f), new Vector2(620f, 28f));

            // ----------------------------------------------------------------
            // WORLD 1 VISUAL CARD (AVAILABLE)
            // ----------------------------------------------------------------
            GameObject w1Card = CreateImagePanel(_worldSelectPanel.transform, "World1Card", new Vector2(-360f, 10f), new Vector2(580f, 650f), _worldCardActiveSprite);
            _world1CardRect = w1Card.GetComponent<RectTransform>();

            GameObject w1Outline = CreateImagePanel(w1Card.transform, "W1SelectionOutline", Vector2.zero, new Vector2(596f, 666f), _btnSelected);
            _world1CardSelectionOutline = w1Outline.GetComponent<Image>();
            _world1CardSelectionOutline.color = new Color(1f, 0.86f, 0.42f, 0.45f);
            _world1CardSelectionOutline.raycastTarget = false;
            w1Outline.transform.SetAsFirstSibling();

            CreateText(w1Card.transform, "W1WorldTag", "WORLD 1   •   AVAILABLE", 21, new Vector2(0f, 278f), new Vector2(500f, 34f), new Color(1f, 0.84f, 0.36f, 1f), TextAnchor.MiddleCenter);
            CreateStyledTitleText(w1Card.transform, "W1Name", "THE RUINS OF THE LAST FLAME", 27, new Vector2(0f, 236f), new Vector2(530f, 42f), new Color(1f, 0.96f, 0.86f, 1f));

            // Large pixel-art representation of World 1
            GameObject w1Art = CreateSpriteElement(w1Card.transform, "W1Artwork", _world1ArtSprite, new Vector2(0f, 58f), new Vector2(496f, 290f));
            Button w1ArtBtn = w1Art.AddComponent<Button>();
            w1ArtBtn.transition = Selectable.Transition.None;
            w1ArtBtn.onClick.AddListener(() => SelectWorldCard(0, openIfUnlocked: true));

            _world1ProgressText = CreateText(w1Card.transform, "W1ProgressLabel", "10 LEVELS   |   COMPLETED: 0 / 10", 21, new Vector2(0f, -118f), new Vector2(500f, 36f), new Color(0.90f, 0.84f, 1f, 1f), TextAnchor.MiddleCenter);

            // Progress Bar
            GameObject barBg = CreateImagePanel(w1Card.transform, "W1ProgressBarBg", new Vector2(0f, -158f), new Vector2(460f, 22f));
            barBg.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.14f, 0.95f);

            GameObject barFill = new GameObject("W1ProgressBarFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            _world1ProgressBarFill = barFill.GetComponent<RectTransform>();
            _world1ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world1ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world1ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world1ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world1ProgressBarFill.sizeDelta = new Vector2(45.4f, -6f);
            barFill.GetComponent<Image>().color = new Color(0.98f, 0.74f, 0.24f, 1f);

            // Button to open Level Select for World 1 (Never starts Level 1 automatically!)
            CreateMenuButton(w1Card.transform, "WORLD 1 — SELECT LEVEL", new Vector2(0f, -238f), () =>
            {
                SelectWorldCard(0, openIfUnlocked: true);
            }, new Vector2(440f, 64f));

            // ----------------------------------------------------------------
            // WORLD 2 VISUAL CARD (LOCKED / COMING SOON)
            // ----------------------------------------------------------------
            GameObject w2Card = CreateImagePanel(_worldSelectPanel.transform, "World2Card", new Vector2(360f, 10f), new Vector2(580f, 650f), _worldCardLockedSprite);
            _world2CardRect = w2Card.GetComponent<RectTransform>();

            GameObject w2Outline = CreateImagePanel(w2Card.transform, "W2SelectionOutline", Vector2.zero, new Vector2(596f, 666f), _btnSelected);
            _world2CardSelectionOutline = w2Outline.GetComponent<Image>();
            _world2CardSelectionOutline.color = new Color(0.62f, 0.48f, 0.92f, 0f);
            _world2CardSelectionOutline.raycastTarget = false;
            w2Outline.transform.SetAsFirstSibling();

            CreateText(w2Card.transform, "W2WorldTag", "WORLD 2   •   LOCKED", 21, new Vector2(0f, 278f), new Vector2(500f, 34f), new Color(0.68f, 0.62f, 0.86f, 1f), TextAnchor.MiddleCenter);
            CreateStyledTitleText(w2Card.transform, "W2Name", "COMING SOON", 27, new Vector2(0f, 236f), new Vector2(500f, 42f), new Color(0.76f, 0.74f, 0.88f, 1f));

            // Mysterious silhouette artwork + Lock seal overlay
            GameObject w2Art = CreateSpriteElement(w2Card.transform, "W2Artwork", _world2ArtSprite, new Vector2(0f, 58f), new Vector2(496f, 290f));
            Button w2ArtBtn = w2Art.AddComponent<Button>();
            w2ArtBtn.transition = Selectable.Transition.None;
            w2ArtBtn.onClick.AddListener(() => SelectWorldCard(1, openIfUnlocked: false));

            CreateSpriteElement(w2Art.transform, "W2LockSeal", _lockSealSprite, new Vector2(0f, 0f), new Vector2(88f, 88f));

            CreateText(
                w2Card.transform,
                "W2MysteryText",
                "\"Beyond the darkness, another light awaits.\"\nUncharted sanctuaries beyond the Ceremonial Gate.",
                20,
                new Vector2(0f, -135f),
                new Vector2(490f, 64f),
                new Color(0.64f, 0.62f, 0.78f, 1f),
                TextAnchor.MiddleCenter
            );

            // Disabled World 2 button
            Button w2DisabledBtn = CreateMenuButton(w2Card.transform, "WORLD 2 (LOCKED)", new Vector2(0f, -238f), () =>
            {
                PlayMenuLockedSound();
                SelectWorldCard(1, openIfUnlocked: false);
            }, new Vector2(440f, 64f));
            SetButtonVisualDisabled(w2DisabledBtn, true);

            // Bottom Navigation Bar
            CreateMenuButton(_worldSelectPanel.transform, "BACK", new Vector2(-240f, -425f), () =>
            {
                PlayMenuBackSound();
                TransitionToPanel(ShowMainMenu);
            }, new Vector2(340f, 62f));

            CreateMenuButton(_worldSelectPanel.transform, "OPEN LEVEL MAP", new Vector2(240f, -425f), () =>
            {
                SelectWorldCard(0, openIfUnlocked: true);
            }, new Vector2(340f, 62f));

            _worldSelectPanel.SetActive(false);
        }

        private void SelectWorldCard(int worldIdx, bool openIfUnlocked)
        {
            _selectedWorldCardIndex = Mathf.Clamp(worldIdx, 0, 1);
            UpdateWorldSelectHighlight();

            if (_selectedWorldCardIndex == 0 && openIfUnlocked)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("torch_ignite", 0.55f, 1.05f);
                }
                TransitionToAction(() =>
                {
                    OnLevelSelectRequested?.Invoke();
                    ShowLevelSelectScreen();
                });
            }
            else if (_selectedWorldCardIndex == 1)
            {
                PlayMenuLockedSound();
                if (openIfUnlocked)
                {
                    TransitionToPanel(ShowWorld2ComingSoonScreen);
                }
            }
        }

        private void UpdateWorldSelectHighlight()
        {
            bool w1Selected = (_selectedWorldCardIndex == 0);
            if (_world1CardSelectionOutline != null)
            {
                _world1CardSelectionOutline.color = w1Selected
                    ? new Color(1f, 0.86f, 0.42f, 0.55f)
                    : new Color(1f, 0.86f, 0.42f, 0f);
            }
            if (_world2CardSelectionOutline != null)
            {
                _world2CardSelectionOutline.color = !w1Selected
                    ? new Color(0.68f, 0.50f, 0.96f, 0.45f)
                    : new Color(0.68f, 0.50f, 0.96f, 0f);
            }
            if (_world1CardRect != null)
            {
                _world1CardRect.localScale = w1Selected ? new Vector3(1.02f, 1.02f, 1f) : Vector3.one;
            }
            if (_world2CardRect != null)
            {
                _world2CardRect.localScale = !w1Selected ? new Vector3(1.01f, 1.01f, 1f) : Vector3.one;
            }
            if (_worldSelectAmbientGlow != null)
            {
                _worldSelectAmbientGlow.rectTransform.anchoredPosition = w1Selected
                    ? new Vector2(-360f, 20f)
                    : new Vector2(360f, 20f);
                _worldSelectAmbientGlow.color = w1Selected
                    ? new Color(1f, 0.68f, 0.24f, 0.30f)
                    : new Color(0.55f, 0.36f, 0.88f, 0.24f);
            }
        }

        public void RefreshWorldSelectState()
        {
            WorldDefinition w1 = WorldCatalog.GetWorld(0);
            int total = w1 != null ? w1.LevelCount : 10;
            int completedCount = WorldProgressSave.GetCompletedLevelsCount("world_1", total);
            bool worldCompleted = WorldProgressSave.IsWorldCompleted("world_1");
            if (worldCompleted) completedCount = total;

            if (_world1ProgressText != null)
            {
                string statusTag = worldCompleted ? "COMPLETED" : "AVAILABLE";
                _world1ProgressText.text = $"10 LEVELS   |   COMPLETED: {completedCount} / {total}   |   {statusTag}";
            }

            if (_world1ProgressBarFill != null)
            {
                float ratio = Mathf.Clamp01(completedCount / (float)Mathf.Max(1, total));
                float barWidth = Mathf.Max(18f, 454f * ratio);
                _world1ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }

            UpdateWorldSelectHighlight();
        }

        // ====================================================================
        // 4. LEVEL SELECT — INTERACTIVE PIXEL-ART RUINS MAP (1920x1080)
        // ====================================================================
        private void BuildLevelSelectMapMenu(Transform parent)
        {
            _levelSelectPanel = CreateFullScreenContainer(parent, "LevelSelectPanel");

            // Dark subterranean backdrop
            Image bgImg = _levelSelectPanel.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.04f, 0.08f, 1f);

            // Top Sanctuary Map Banner
            GameObject topBanner = CreateImagePanel(_levelSelectPanel.transform, "MapTopBanner", new Vector2(0f, 478f), new Vector2(1840f, 82f));
            topBanner.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.14f, 0.96f);

            CreateStyledTitleText(topBanner.transform, "MapTitle", "WORLD 1 — THE RUINS OF THE LAST FLAME", 32, new Vector2(-380f, 0f), new Vector2(960f, 54f), new Color(1f, 0.85f, 0.38f, 1f));
            _mapHeaderProgressText = CreateText(topBanner.transform, "MapProgressSummary", "SANCTUARY PROGRESS: 0 / 10 COMPLETED", 23, new Vector2(520f, 0f), new Vector2(680f, 48f), new Color(0.86f, 0.78f, 1f, 1f), TextAnchor.MiddleRight);

            // ----------------------------------------------------------------
            // LEFT/CENTER: 16:9 ILLUSTRATED PIXEL-ART MAP OF THE RUINS (1360x765)
            // ----------------------------------------------------------------
            const float mapWidth = 1360f;
            const float mapHeight = 765f;
            GameObject mapContainer = CreateSpriteElement(_levelSelectPanel.transform, "World1RuinsMapCanvas", _ruinsMapSprite, new Vector2(-215f, 8f), new Vector2(mapWidth, mapHeight));
            RectTransform mapRect = mapContainer.GetComponent<RectTransform>();

            // Scale factor from 480x270 pixel map to 1360x765 UI RectTransform
            float scaleX = mapWidth / 480f;
            float scaleY = mapHeight / 270f;

            Vector2[] uiPositions = new Vector2[MapNodePixelCoords.Length];
            for (int i = 0; i < MapNodePixelCoords.Length; i++)
            {
                float ux = (MapNodePixelCoords[i].x - 240f) * scaleX;
                float uy = (135f - MapNodePixelCoords[i].y) * scaleY;
                uiPositions[i] = new Vector2(ux, uy);
            }

            // Build connecting runic trails between consecutive nodes (01->02 .. 09->10)
            GameObject pathsLayer = new GameObject("MapConnectingPaths", typeof(RectTransform));
            pathsLayer.transform.SetParent(mapRect, false);
            for (int i = 0; i < uiPositions.Length - 1; i++)
            {
                BuildMapPathBetweenNodes(pathsLayer.transform, uiPositions[i], uiPositions[i + 1], targetNodeIndex: i + 1);
            }

            // Selection Compass Ring (moves to whichever node is selected)
            GameObject ringObj = CreateSpriteElement(mapRect, "MapSelectedNodeRing", _nodeSelectedRingSprite, uiPositions[0], new Vector2(104f, 104f));
            _mapSelectedRingRect = ringObj.GetComponent<RectTransform>();
            _mapSelectedRingImage = ringObj.GetComponent<Image>();
            _mapSelectedRingImage.raycastTarget = false;

            // Build the 10 Interactive Sanctuary Nodes
            for (int i = 0; i < 10; i++)
            {
                int capturedIndex = i;
                Vector2 nPos = uiPositions[i];

                GameObject nodeRoot = new GameObject($"MapNode_{i + 1:D2}", typeof(RectTransform));
                nodeRoot.transform.SetParent(mapRect, false);
                RectTransform nrt = nodeRoot.GetComponent<RectTransform>();
                nrt.anchoredPosition = nPos;
                nrt.sizeDelta = new Vector2(84f, 84f);

                // Glow halo behind unlocked/completed nodes
                GameObject glowObj = CreateSpriteElement(nodeRoot.transform, "NodeHalo", _lightHaloSprite, new Vector2(0f, 6f), new Vector2(115f, 115f));
                Image glowImg = glowObj.GetComponent<Image>();
                glowImg.raycastTarget = false;

                // Node Icon Button
                GameObject iconObj = CreateSpriteElement(nodeRoot.transform, "NodeIcon", _nodeAvailableSprite, Vector2.zero, new Vector2(76f, 76f));
                Image iconImg = iconObj.GetComponent<Image>();
                Button nodeBtn = iconObj.AddComponent<Button>();
                nodeBtn.transition = Selectable.Transition.None;

                // Hover listener
                EventTrigger trigger = iconObj.AddComponent<EventTrigger>();
                EventTrigger.Entry hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                hoverEntry.callback.AddListener((_) => PlayMenuHoverSound());
                trigger.triggers.Add(hoverEntry);

                nodeBtn.onClick.AddListener(() =>
                {
                    OnMapNodeClicked(capturedIndex);
                });

                // Crisp stone badge below node showing 01..10
                GameObject badgeBg = CreateImagePanel(nodeRoot.transform, "NodeBadge", new Vector2(0f, -44f), new Vector2(56f, 26f));
                badgeBg.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.14f, 0.94f);
                badgeBg.GetComponent<Image>().raycastTarget = false;

                Text numLabel = CreateText(
                    badgeBg.transform,
                    "NodeNumber",
                    $"{i + 1:D2}",
                    18,
                    Vector2.zero,
                    new Vector2(52f, 24f),
                    new Color(1f, 0.88f, 0.45f, 1f),
                    TextAnchor.MiddleCenter
                );
                numLabel.raycastTarget = false;

                _mapNodes.Add(new MapNodeUI
                {
                    LevelIndex = i,
                    MapAnchoredPos = nPos,
                    NodeButton = nodeBtn,
                    NodeRect = nrt,
                    NodeIconImage = iconImg,
                    NodeGlowImage = glowImg,
                    NodeNumberLabel = numLabel,
                    IsUnlocked = (i == 0),
                    IsCompleted = false
                });
            }

            // ----------------------------------------------------------------
            // RIGHT SIDE: INTEGRATED SANCTUARY LEVEL INSPECTOR PANEL (430x765)
            // ----------------------------------------------------------------
            GameObject inspector = CreateImagePanel(_levelSelectPanel.transform, "LevelInspectorPanel", new Vector2(695f, 8f), new Vector2(430f, 765f), _modalFrameSprite);

            CreateText(inspector.transform, "InspectorHeaderTag", "SELECTED SANCTUARY", 20, new Vector2(0f, 325f), new Vector2(360f, 32f), new Color(0.80f, 0.66f, 1f, 1f), TextAnchor.MiddleCenter);
            _inspectorLevelNumberText = CreateStyledTitleText(inspector.transform, "InspectorLevelNumber", "LEVEL 01", 40, new Vector2(0f, 276f), new Vector2(370f, 52f), new Color(1f, 0.86f, 0.38f, 1f));
            _inspectorLevelTitleText = CreateStyledTitleText(inspector.transform, "InspectorLevelTitle", "AWAKENING", 28, new Vector2(0f, 222f), new Vector2(380f, 44f), new Color(1f, 0.96f, 0.88f, 1f));

            CreateSpriteElement(inspector.transform, "InspectorDivider1", _stoneDividerSprite, new Vector2(0f, 180f), new Vector2(340f, 24f));

            // Status Badge Box
            GameObject statusBox = CreateImagePanel(inspector.transform, "InspectorStatusBox", new Vector2(0f, 128f), new Vector2(350f, 48f));
            statusBox.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.18f, 0.95f);
            _inspectorStatusBadgeText = CreateText(statusBox.transform, "InspectorStatusText", "STATUS: AVAILABLE", 21, Vector2.zero, new Vector2(330f, 42f), new Color(1f, 0.84f, 0.36f, 1f), TextAnchor.MiddleCenter);

            // Level Lore / Mechanic Hint
            _inspectorHintText = CreateText(
                inspector.transform,
                "InspectorHintBody",
                LevelManager.LevelHints[0],
                21,
                new Vector2(0f, -10f),
                new Vector2(350f, 190f),
                new Color(0.88f, 0.90f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateSpriteElement(inspector.transform, "InspectorDivider2", _stoneDividerSprite, new Vector2(0f, -135f), new Vector2(340f, 24f));

            // Play / Replay Button
            _inspectorPlayButton = CreateMenuButton(inspector.transform, "PLAY", new Vector2(0f, -205f), () =>
            {
                TryLaunchSelectedMapLevel();
            }, new Vector2(350f, 66f));
            _inspectorPlayButtonImage = _inspectorPlayButton.GetComponent<Image>();
            _inspectorPlayButtonLabel = _inspectorPlayButton.GetComponentInChildren<Text>();

            // Back to World Select Button
            CreateMenuButton(inspector.transform, "BACK TO WORLD SELECT", new Vector2(0f, -295f), () =>
            {
                PlayMenuBackSound();
                TransitionToPanel(ShowWorldSelectScreen);
            }, new Vector2(350f, 60f));

            // Bottom Map Legend & Controls Bar
            GameObject bottomBar = CreateImagePanel(_levelSelectPanel.transform, "MapLegendBar", new Vector2(0f, -458f), new Vector2(1840f, 66f));
            bottomBar.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.12f, 0.94f);

            CreateText(
                bottomBar.transform,
                "MapLegendText",
                "GOLDEN SHRINE: COMPLETED     •     PULSING ALTAR: AVAILABLE     •     SEALED OBELISK: LOCKED",
                20,
                Vector2.zero,
                new Vector2(1780f, 52f),
                new Color(0.92f, 0.88f, 0.78f, 1f),
                TextAnchor.MiddleCenter
            );

            _levelSelectPanel.SetActive(false);
        }

        private void BuildMapPathBetweenNodes(Transform parent, Vector2 fromPos, Vector2 toPos, int targetNodeIndex)
        {
            Vector2 delta = toPos - fromPos;
            float dist = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            int segmentCount = Mathf.Max(2, Mathf.RoundToInt(dist / 38f));
            for (int s = 1; s < segmentCount; s++)
            {
                float t = s / (float)segmentCount;
                Vector2 segPos = Vector2.Lerp(fromPos, toPos, t);

                GameObject segObj = CreateSpriteElement(parent, $"Path_{targetNodeIndex}_{s}", _pathDarkSprite, segPos, new Vector2(28f, 10f));
                RectTransform srt = segObj.GetComponent<RectTransform>();
                srt.localRotation = Quaternion.Euler(0f, 0f, angle);
                Image segImg = segObj.GetComponent<Image>();
                segImg.raycastTarget = false;

                _mapPathSegments.Add(segImg);
                _mapPathTargetNodeIndex.Add(targetNodeIndex);
            }
        }

        private void OnMapNodeClicked(int levelIndex)
        {
            if (_selectedMapLevelIndex == levelIndex)
            {
                // Clicking the already-selected node attempts to launch it if unlocked
                TryLaunchSelectedMapLevel();
            }
            else
            {
                SelectMapNode(levelIndex);
            }
        }

        public void SelectMapNode(int levelIndex)
        {
            int clamped = Mathf.Clamp(levelIndex, 0, _mapNodes.Count - 1);
            _selectedMapLevelIndex = clamped;

            MapNodeUI node = _mapNodes[clamped];
            if (_mapSelectedRingRect != null)
            {
                _mapSelectedRingRect.anchoredPosition = node.MapAnchoredPos;
            }

            if (_inspectorLevelNumberText != null)
            {
                _inspectorLevelNumberText.text = $"LEVEL {clamped + 1:D2}";
            }
            if (_inspectorLevelTitleText != null)
            {
                _inspectorLevelTitleText.text = CanonicalLevelShortNames[clamped];
            }
            if (_inspectorHintText != null)
            {
                _inspectorHintText.text = clamped < LevelManager.LevelHints.Length
                    ? LevelManager.LevelHints[clamped]
                    : "";
            }

            if (node.IsCompleted)
            {
                if (_inspectorStatusBadgeText != null)
                {
                    _inspectorStatusBadgeText.text = "STATUS: COMPLETED";
                    _inspectorStatusBadgeText.color = new Color(1f, 0.88f, 0.38f, 1f);
                }
                if (_inspectorPlayButtonLabel != null)
                {
                    _inspectorPlayButtonLabel.text = "REPLAY";
                }
                SetButtonVisualDisabled(_inspectorPlayButton, false);
            }
            else if (node.IsUnlocked)
            {
                if (_inspectorStatusBadgeText != null)
                {
                    _inspectorStatusBadgeText.text = "STATUS: AVAILABLE";
                    _inspectorStatusBadgeText.color = new Color(1f, 0.72f, 0.28f, 1f);
                }
                if (_inspectorPlayButtonLabel != null)
                {
                    _inspectorPlayButtonLabel.text = "PLAY";
                }
                SetButtonVisualDisabled(_inspectorPlayButton, false);
            }
            else
            {
                if (_inspectorStatusBadgeText != null)
                {
                    _inspectorStatusBadgeText.text = $"LOCKED (COMPLETE LEVEL {clamped:D2})";
                    _inspectorStatusBadgeText.color = new Color(0.65f, 0.62f, 0.78f, 1f);
                }
                if (_inspectorPlayButtonLabel != null)
                {
                    _inspectorPlayButtonLabel.text = "LOCKED";
                }
                SetButtonVisualDisabled(_inspectorPlayButton, true);
            }
        }

        public bool TryLaunchSelectedMapLevel()
        {
            if (_selectedMapLevelIndex < 0 || _selectedMapLevelIndex >= _mapNodes.Count) return false;
            MapNodeUI node = _mapNodes[_selectedMapLevelIndex];

            if (!node.IsUnlocked)
            {
                PlayMenuLockedSound();
                return false;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.60f, 1.08f);
            }

            int targetLevel = _selectedMapLevelIndex;
            TransitionToAction(() =>
            {
                OnStartWorldRequested?.Invoke(0, false);
                OnLevelSelectedFromMap?.Invoke(targetLevel);
            });
            return true;
        }

        public void RefreshLevelSelectMap()
        {
            WorldDefinition w1 = WorldCatalog.GetWorld(0);
            int total = w1 != null ? w1.LevelCount : 10;
            int completedCount = WorldProgressSave.GetCompletedLevelsCount("world_1", total);

            if (_mapHeaderProgressText != null)
            {
                _mapHeaderProgressText.text = $"SANCTUARY PROGRESS: {completedCount} / {total} COMPLETED";
            }

            for (int i = 0; i < _mapNodes.Count; i++)
            {
                MapNodeUI node = _mapNodes[i];
                node.IsUnlocked = WorldProgressSave.IsLevelUnlocked("world_1", i, total);
                node.IsCompleted = WorldProgressSave.IsLevelCompleted("world_1", i, total);

                if (node.IsCompleted)
                {
                    node.NodeIconImage.sprite = _nodeCompletedSprite;
                    node.NodeIconImage.color = Color.white;
                    node.NodeGlowImage.gameObject.SetActive(true);
                    node.NodeGlowImage.color = new Color(1f, 0.84f, 0.32f, 0.42f);
                    node.NodeNumberLabel.color = new Color(1f, 0.90f, 0.48f, 1f);
                }
                else if (node.IsUnlocked)
                {
                    node.NodeIconImage.sprite = _nodeAvailableSprite;
                    node.NodeIconImage.color = Color.white;
                    node.NodeGlowImage.gameObject.SetActive(true);
                    node.NodeGlowImage.color = new Color(0.98f, 0.56f, 0.22f, 0.46f);
                    node.NodeNumberLabel.color = new Color(1f, 0.78f, 0.36f, 1f);
                }
                else
                {
                    node.NodeIconImage.sprite = _nodeLockedSprite;
                    node.NodeIconImage.color = new Color(0.72f, 0.74f, 0.86f, 0.92f);
                    node.NodeGlowImage.gameObject.SetActive(false);
                    node.NodeNumberLabel.color = new Color(0.55f, 0.58f, 0.72f, 0.9f);
                }
            }

            // Update connecting path segments based on whether the target node is unlocked
            for (int p = 0; p < _mapPathSegments.Count; p++)
            {
                int targetIdx = _mapPathTargetNodeIndex[p];
                bool pathLit = targetIdx < _mapNodes.Count && _mapNodes[targetIdx].IsUnlocked;
                _mapPathSegments[p].sprite = pathLit ? _pathLitSprite : _pathDarkSprite;
                _mapPathSegments[p].color = pathLit
                    ? new Color(1f, 0.92f, 0.65f, 0.95f)
                    : new Color(0.55f, 0.58f, 0.75f, 0.75f);
            }

            int defaultSelection = WorldProgressSave.GetContinueLevelIndex("world_1", total);
            SelectMapNode(defaultSelection);
        }

        // ====================================================================
        // 5. PAUSE, SETTINGS, CREDITS, WORLD 1 COMPLETE & WORLD 2 COMING SOON
        // ====================================================================
        private void BuildPauseMenu(Transform parent)
        {
            _pauseMenuContainer = CreateFullScreenContainer(parent, "PauseMenuContainer");
            Image backdrop = _pauseMenuContainer.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.03f, 0.06f, 0.72f); // partially dim gameplay behind pause

            _pauseMenuPanel = CreateImagePanel(_pauseMenuContainer.transform, "PauseMenuPanel", Vector2.zero, new Vector2(720f, 680f), _modalFrameSprite);
            CreateStyledTitleText(_pauseMenuPanel.transform, "PauseHeader", "PAUSED", 46, new Vector2(0f, 248f), new Vector2(560f, 60f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateStyledTitleText(_pauseMenuPanel.transform, "PauseSubtitle", "THE FLAME RESTS", 24, new Vector2(0f, 198f), new Vector2(560f, 38f), new Color(0.78f, 0.62f, 0.98f, 1f));
            CreateSpriteElement(_pauseMenuPanel.transform, "PauseDivider", _stoneDividerSprite, new Vector2(0f, 156f), new Vector2(460f, 24f));

            _pauseMenuButtons.Clear();

            Button resumeBtn = CreateMenuButton(_pauseMenuPanel.transform, "RESUME", new Vector2(0f, 86f), () => OnResumeRequested?.Invoke(), new Vector2(360f, 60f));
            _pauseMenuButtons.Add(resumeBtn);

            Button restartBtn = CreateMenuButton(_pauseMenuPanel.transform, "RESTART LEVEL", new Vector2(0f, 14f), () => OnRestartRequested?.Invoke(), new Vector2(360f, 60f));
            _pauseMenuButtons.Add(restartBtn);

            Button levelSelectBtn = CreateMenuButton(_pauseMenuPanel.transform, "LEVEL SELECT", new Vector2(0f, -58f), () =>
            {
                TransitionToAction(() =>
                {
                    OnLevelSelectRequested?.Invoke();
                    ShowLevelSelectScreen();
                });
            }, new Vector2(360f, 60f));
            _pauseMenuButtons.Add(levelSelectBtn);

            Button settingsBtn = CreateMenuButton(_pauseMenuPanel.transform, "SETTINGS", new Vector2(0f, -130f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Pause;
                ShowSubPanel(_settingsPanel);
            }, new Vector2(360f, 60f));
            _pauseMenuButtons.Add(settingsBtn);

            Button mainMenuBtn = CreateMenuButton(_pauseMenuPanel.transform, "MAIN MENU", new Vector2(0f, -202f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            }, new Vector2(360f, 60f));
            _pauseMenuButtons.Add(mainMenuBtn);

            _pauseMenuContainer.SetActive(false);
            _pauseMenuPanel.SetActive(false);
        }

        private void MovePauseButtonSelection(int delta)
        {
            if (_pauseMenuButtons.Count == 0) return;
            _pauseSelectedBtnIndex = (_pauseSelectedBtnIndex + delta + _pauseMenuButtons.Count) % _pauseMenuButtons.Count;
            PlayMenuHoverSound();
            RefreshPauseButtonSelectionVisuals();
        }

        private void RefreshPauseButtonSelectionVisuals()
        {
            for (int i = 0; i < _pauseMenuButtons.Count; i++)
            {
                Button btn = _pauseMenuButtons[i];
                if (btn == null) continue;
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i == _pauseSelectedBtnIndex) ? _btnSelected : _btnNormal;
                }
            }
        }

        private void BuildSettingsMenu(Transform parent)
        {
            _settingsPanel = CreateImagePanel(parent, "SettingsPanel", Vector2.zero, new Vector2(780f, 580f), _modalFrameSprite);
            CreateStyledTitleText(_settingsPanel.transform, "SettingsHeader", "AUDIO & CONTROLS", 38, new Vector2(0f, 210f), new Vector2(620f, 52f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateSpriteElement(_settingsPanel.transform, "SettingsDivider", _stoneDividerSprite, new Vector2(0f, 165f), new Vector2(460f, 24f));

            float initMusic = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1.0f;
            float initSfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f;

            _musicVolumeText = CreateText(_settingsPanel.transform, "MusicVolumeText", $"MUSIC VOLUME: {Mathf.RoundToInt(initMusic * 100f)}%", 24, new Vector2(-150f, 105f), new Vector2(360f, 44f), new Color(1f, 0.90f, 0.55f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "MUSIC + / -", new Vector2(175f, 105f), () =>
            {
                if (AudioManager.Instance != null)
                {
                    float next = AudioManager.Instance.MusicVolume + 0.2f;
                    if (next > 1.05f) next = 0.0f;
                    AudioManager.Instance.MusicVolume = next;
                    RefreshAudioSettingsLabels();
                    AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.05f);
                }
            }, new Vector2(260f, 54f));

            _sfxVolumeText = CreateText(_settingsPanel.transform, "SfxVolumeText", $"SFX VOLUME: {Mathf.RoundToInt(initSfx * 100f)}%", 24, new Vector2(-150f, 25f), new Vector2(360f, 44f), new Color(0.85f, 0.80f, 1f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "SFX + / -", new Vector2(175f, 25f), () =>
            {
                if (AudioManager.Instance != null)
                {
                    float next = AudioManager.Instance.SfxVolume + 0.2f;
                    if (next > 1.05f) next = 0.0f;
                    AudioManager.Instance.SfxVolume = next;
                    RefreshAudioSettingsLabels();
                    AudioManager.Instance.PlaySFX("switch_activate", 0.75f, 1.0f);
                }
            }, new Vector2(260f, 54f));

            CreateText(
                _settingsPanel.transform,
                "ControlsGuide",
                "A / D : Move     SPACE : Jump (Shadow Double Jump)     E : Torch / Activate\nTAB : Switch Protagonist & Shadow     R : Restart Level     ESC : Pause / Back",
                20,
                new Vector2(0f, -75f),
                new Vector2(680f, 84f),
                new Color(0.82f, 0.86f, 0.96f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_settingsPanel.transform, "RETURN", new Vector2(0f, -195f), () =>
            {
                CloseSubPanelAndReturn();
            });
            _settingsPanel.SetActive(false);
        }

        public void RefreshAudioSettingsLabels()
        {
            if (AudioManager.Instance == null) return;
            if (_musicVolumeText != null)
            {
                _musicVolumeText.text = $"MUSIC VOLUME: {Mathf.RoundToInt(AudioManager.Instance.MusicVolume * 100f)}%";
            }
            if (_sfxVolumeText != null)
            {
                _sfxVolumeText.text = $"SFX VOLUME: {Mathf.RoundToInt(AudioManager.Instance.SfxVolume * 100f)}%";
            }
        }

        private void BuildCreditsMenu(Transform parent)
        {
            _creditsPanel = CreateImagePanel(parent, "CreditsPanel", Vector2.zero, new Vector2(760f, 540f), _modalFrameSprite);
            CreateStyledTitleText(_creditsPanel.transform, "CreditsHeader", "CREDITS", 38, new Vector2(0f, 190f), new Vector2(540f, 52f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateSpriteElement(_creditsPanel.transform, "CreditsDivider", _stoneDividerSprite, new Vector2(0f, 145f), new Vector2(420f, 24f));

            CreateText(
                _creditsPanel.transform,
                "CreditsBody",
                "MY SHADOW QUIT: THE LAST FLAME\n\nCreated for AI Browser Game Jam 5\nOfficial Theme: TORCH\n\nOriginal Dark-Fantasy Pixel Art, Procedural Ambient Audio,\nInteractive Ruins Map & Dual-Character Shadow Puzzle Design.",
                22,
                new Vector2(0f, 15f),
                new Vector2(640f, 220f),
                new Color(0.90f, 0.90f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_creditsPanel.transform, "RETURN", new Vector2(0f, -185f), () =>
            {
                CloseSubPanelAndReturn();
            });
            _creditsPanel.SetActive(false);
        }

        private void BuildWorld1CompleteScreen(Transform parent)
        {
            _worldCompletePanel = CreateImagePanel(parent, "World1CompletePanel", Vector2.zero, new Vector2(880f, 700f), _modalFrameSprite);

            GameObject torchDeco = CreateSpriteElement(_worldCompletePanel.transform, "WorldCompleteTorchIcon", AssetRegistry.GetSprite("Torch/torch_item_0", 32f), new Vector2(0f, 268f), new Vector2(68f, 88f));
            torchDeco.GetComponent<Image>().raycastTarget = false;

            CreateStyledTitleText(_worldCompletePanel.transform, "WorldCompleteTitle", "WORLD 1 COMPLETE", 46, new Vector2(0f, 192f), new Vector2(760f, 60f), new Color(1f, 0.86f, 0.38f, 1f));
            CreateStyledTitleText(_worldCompletePanel.transform, "WorldCompleteSub", "THE RUINS OF THE LAST FLAME", 28, new Vector2(0f, 140f), new Vector2(740f, 44f), new Color(0.78f, 0.62f, 0.98f, 1f));
            CreateSpriteElement(_worldCompletePanel.transform, "WorldCompleteDivider", _stoneDividerSprite, new Vector2(0f, 102f), new Vector2(520f, 24f));

            CreateText(
                _worldCompletePanel.transform,
                "WorldCompleteQuote",
                World1CompleteQuote,
                26,
                new Vector2(0f, 48f),
                new Vector2(760f, 64f),
                new Color(1f, 0.94f, 0.80f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_worldCompletePanel.transform, "CONTINUE", new Vector2(0f, -32f), () =>
            {
                OnWorldCompleteContinueRequested?.Invoke();
                TransitionToPanel(ShowWorld2ComingSoonScreen);
            });
            CreateMenuButton(_worldCompletePanel.transform, "LEVEL SELECT", new Vector2(0f, -106f), () =>
            {
                OnLevelSelectRequested?.Invoke();
                TransitionToPanel(ShowLevelSelectScreen);
            });
            CreateMenuButton(_worldCompletePanel.transform, "WORLD SELECT", new Vector2(0f, -180f), () =>
            {
                OnWorldSelectRequested?.Invoke();
                TransitionToPanel(ShowWorldSelectScreen);
            });
            CreateMenuButton(_worldCompletePanel.transform, "MAIN MENU", new Vector2(0f, -254f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            });

            _worldCompletePanel.SetActive(false);
        }

        private void BuildWorld2ComingSoonScreen(Transform parent)
        {
            _world2ComingSoonPanel = CreateImagePanel(parent, "World2ComingSoonPanel", Vector2.zero, new Vector2(860f, 620f), _modalFrameSprite);

            CreateStyledTitleText(_world2ComingSoonPanel.transform, "W2ComingSoonHeader", "WORLD 2 — COMING SOON", 42, new Vector2(0f, 205f), new Vector2(760f, 58f), new Color(1f, 0.86f, 0.38f, 1f));
            CreateStyledTitleText(_world2ComingSoonPanel.transform, "W2ComingSoonSub", "BEYOND THE FIRST DAWN", 26, new Vector2(0f, 150f), new Vector2(700f, 42f), new Color(0.78f, 0.62f, 0.98f, 1f));
            CreateSpriteElement(_world2ComingSoonPanel.transform, "W2Divider", _stoneDividerSprite, new Vector2(0f, 110f), new Vector2(500f, 24f));

            CreateText(
                _world2ComingSoonPanel.transform,
                "W2ComingSoonBody",
                "\"Beyond the darkness, another light awaits.\"\n\nYou have conquered all 10 chambers of World 1:\nThe Ruins of the Last Flame.\n\nYour progress is saved. You may replay any sanctuary chamber\nin Level Select while World 2 is forged.",
                22,
                new Vector2(0f, 5f),
                new Vector2(740f, 180f),
                new Color(0.90f, 0.90f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_world2ComingSoonPanel.transform, "LEVEL SELECT", new Vector2(-190f, -145f), () =>
            {
                OnLevelSelectRequested?.Invoke();
                TransitionToPanel(ShowLevelSelectScreen);
            }, new Vector2(330f, 60f));

            CreateMenuButton(_world2ComingSoonPanel.transform, "WORLD SELECT", new Vector2(190f, -145f), () =>
            {
                OnWorldSelectRequested?.Invoke();
                TransitionToPanel(ShowWorldSelectScreen);
            }, new Vector2(330f, 60f));

            CreateMenuButton(_world2ComingSoonPanel.transform, "MAIN MENU", new Vector2(0f, -225f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            }, new Vector2(340f, 60f));

            _world2ComingSoonPanel.SetActive(false);
        }

        /// <summary>
        /// Reusable Final Game Victory Screen preserved for the future completion of the entire game.
        /// Never executed at Level 8 or Level 10 of World 1.
        /// </summary>
        private void BuildVictoryScreen(Transform parent)
        {
            _victoryPanel = CreateImagePanel(parent, "VictoryPanel", Vector2.zero, new Vector2(820f, 620f), _modalFrameSprite);

            CreateSpriteElement(_victoryPanel.transform, "VictoryTorchIcon", AssetRegistry.GetSprite("Torch/torch_item_0", 32f), new Vector2(0f, 235f), new Vector2(64f, 84f));
            CreateStyledTitleText(_victoryPanel.transform, "VictoryTitleMain", "MY SHADOW QUIT", 46, new Vector2(0f, 162f), new Vector2(720f, 60f), new Color(1f, 0.86f, 0.38f, 1f));
            CreateStyledTitleText(_victoryPanel.transform, "VictoryTitleSub", "THE LAST FLAME", 28, new Vector2(0f, 112f), new Vector2(680f, 44f), new Color(0.76f, 0.58f, 0.98f, 1f));

            CreateText(
                _victoryPanel.transform,
                "VictoryQuote",
                FinalNarrativeQuote,
                25,
                new Vector2(0f, 42f),
                new Vector2(720f, 64f),
                new Color(1f, 0.93f, 0.78f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateText(
                _victoryPanel.transform,
                "VictoryThanks",
                "THANK YOU FOR PLAYING",
                26,
                new Vector2(0f, -22f),
                new Vector2(680f, 48f),
                new Color(0.88f, 0.84f, 1f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_victoryPanel.transform, "PLAY AGAIN", new Vector2(0f, -110f), () => OnPlayRequested?.Invoke());
            CreateMenuButton(_victoryPanel.transform, "MAIN MENU", new Vector2(0f, -192f), () => OnMainMenuRequested?.Invoke());
            _victoryPanel.SetActive(false);
        }

        private void BuildFadeOverlay(Transform parent)
        {
            GameObject fadeObj = CreateFullScreenContainer(parent, "FadeOverlay");
            _fadeOverlay = fadeObj.AddComponent<Image>();
            _fadeOverlay.color = new Color(0.03f, 0.04f, 0.08f, 0f);
            _fadeOverlay.raycastTarget = false;

            _endingQuoteText = CreateText(
                fadeObj.transform,
                "EndingNarrativeQuote",
                World1CompleteQuote,
                32,
                new Vector2(0f, 24f),
                new Vector2(1280f, 120f),
                new Color(1f, 0.92f, 0.76f, 0f),
                TextAnchor.MiddleCenter
            );
            _endingQuoteText.gameObject.SetActive(false);
        }

        private void BuildFloatingEmbers(Transform parent, int count)
        {
            GameObject container = CreateFullScreenContainer(parent, "FloatingEmbersLayer");
            for (int i = 0; i < count; i++)
            {
                bool isViolet = (i % 3 == 0);
                float startX = -860f + (i * 102f) % 1720f;
                float startY = -480f + (i * 137f) % 920f;
                float size = 8f + (i % 4) * 3f;

                GameObject eObj = CreateSpriteElement(container.transform, $"MenuEmber_{i}", _emberSprite, new Vector2(startX, startY), new Vector2(size, size));
                Image eImg = eObj.GetComponent<Image>();
                eImg.raycastTarget = false;
                eImg.color = isViolet
                    ? new Color(0.75f, 0.50f, 1f, 0.65f)
                    : new Color(1f, 0.78f, 0.28f, 0.75f);

                _menuEmbers.Add(new MenuEmber
                {
                    Rect = eObj.GetComponent<RectTransform>(),
                    Img = eImg,
                    BasePos = new Vector2(startX, startY),
                    SpeedY = 18f + (i % 5) * 6f,
                    SwayAmp = 14f + (i % 4) * 5f,
                    SwayFreq = 0.8f + (i % 3) * 0.35f,
                    Phase = i * 0.7f,
                    IsViolet = isViolet
                });
            }
        }

        // ====================================================================
        // 6. UPDATE LOOP: ANIMATIONS, LIGHTING & KEYBOARD NAVIGATION
        // ====================================================================
        private void Update()
        {
            float t = Time.unscaledTime;

            if (_fadeOverlay != null)
            {
                Color c = _fadeOverlay.color;
                c.a = Mathf.MoveTowards(c.a, _targetFadeAlpha, Time.unscaledDeltaTime * _fadeSpeed);
                _fadeOverlay.color = c;
            }

            if (IsHomeScreenVisible)
            {
                UpdateHomeDioramaAnimations(t);
            }

            if (IsLevelSelectScreenVisible)
            {
                UpdateLevelSelectMapAnimations(t);
            }

            if (!_isTransitioning)
            {
                HandleMenuKeyboardNavigation();
            }
        }

        private void UpdateHomeDioramaAnimations(float t)
        {
            // Character & Torch sprite frame animations
            if (_protagonistIdleFrames != null && _protagonistIdleFrames.Length > 0 && _homeProtagonistImage != null)
            {
                int pFrame = Mathf.FloorToInt(t * 5.5f) % _protagonistIdleFrames.Length;
                if (_protagonistIdleFrames[pFrame] != null) _homeProtagonistImage.sprite = _protagonistIdleFrames[pFrame];
            }
            if (_shadowIdleFrames != null && _shadowIdleFrames.Length > 0 && _homeShadowImage != null)
            {
                int sFrame = Mathf.FloorToInt(t * 5.5f + 1f) % _shadowIdleFrames.Length;
                if (_shadowIdleFrames[sFrame] != null) _homeShadowImage.sprite = _shadowIdleFrames[sFrame];
            }
            if (_torchFlameFrames != null && _torchFlameFrames.Length > 0 && _homeTorchFlameImage != null)
            {
                int fFrame = Mathf.FloorToInt(t * 10f) % _torchFlameFrames.Length;
                if (_torchFlameFrames[fFrame] != null) _homeTorchFlameImage.sprite = _torchFlameFrames[fFrame];
            }

            // Flickering torchlight variation
            float flicker = 1f + 0.06f * Mathf.Sin(t * 7.3f) + 0.04f * Mathf.Cos(t * 13.1f);
            if (_homeTorchHaloOuter != null)
            {
                _homeTorchHaloOuter.rectTransform.localScale = new Vector3(flicker, flicker, 1f);
                _homeTorchHaloOuter.color = new Color(1f, 0.64f, 0.18f, 0.30f + 0.05f * Mathf.Sin(t * 6.5f));
            }
            if (_homeTorchHaloInner != null)
            {
                float innerScale = 1f + 0.08f * Mathf.Cos(t * 9.5f);
                _homeTorchHaloInner.rectTransform.localScale = new Vector3(innerScale, innerScale, 1f);
            }

            // Subtle parallax drift on background
            if (_homeBgRect != null)
            {
                _homeBgRect.anchoredPosition = new Vector2(Mathf.Sin(t * 0.35f) * 14f, Mathf.Cos(t * 0.25f) * 8f);
            }

            // Update floating ember particles
            for (int i = 0; i < _menuEmbers.Count; i++)
            {
                MenuEmber e = _menuEmbers[i];
                if (e.Rect == null) continue;
                float yOffset = ((t * e.SpeedY + e.Phase * 60f) % 1060f) - 530f;
                float xOffset = e.BasePos.x + Mathf.Sin(t * e.SwayFreq + e.Phase) * e.SwayAmp;
                e.Rect.anchoredPosition = new Vector2(xOffset, yOffset);

                float alphaPulse = 0.45f + 0.35f * Mathf.Sin(t * 2.8f + e.Phase);
                e.Img.color = e.IsViolet
                    ? new Color(0.75f, 0.50f, 1f, alphaPulse)
                    : new Color(1f, 0.78f, 0.28f, alphaPulse);
            }
        }

        private void UpdateLevelSelectMapAnimations(float t)
        {
            // Pulse selection ring & available node beacons
            if (_mapSelectedRingRect != null)
            {
                float ringScale = 1f + 0.07f * Mathf.Sin(t * 4.8f);
                _mapSelectedRingRect.localScale = new Vector3(ringScale, ringScale, 1f);
            }

            for (int i = 0; i < _mapNodes.Count; i++)
            {
                MapNodeUI node = _mapNodes[i];
                if (node.NodeRect == null) continue;

                if (i == _selectedMapLevelIndex)
                {
                    node.NodeRect.localScale = new Vector3(1.12f, 1.12f, 1f);
                }
                else if (node.IsUnlocked && !node.IsCompleted)
                {
                    float pulse = 1f + 0.05f * Mathf.Sin(t * 4.0f + i);
                    node.NodeRect.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    node.NodeRect.localScale = Vector3.one;
                }
            }
        }

        private void HandleMenuKeyboardNavigation()
        {
            if (_currentScreen == ActiveMenuScreen.NewGameConfirm)
            {
                if (GameInput.GetCancelDown())
                {
                    PlayMenuBackSound();
                    _newGameConfirmModal.SetActive(false);
                    _currentScreen = ActiveMenuScreen.Home;
                }
                else if (GameInput.GetConfirmDown())
                {
                    _newGameConfirmModal.SetActive(false);
                    TransitionToAction(() => OnNewGameRequested?.Invoke());
                }
                return;
            }

            if (_currentScreen == ActiveMenuScreen.Settings || _currentScreen == ActiveMenuScreen.Credits)
            {
                if (GameInput.GetCancelDown())
                {
                    CloseSubPanelAndReturn();
                }
                return;
            }

            if (_currentScreen == ActiveMenuScreen.Home)
            {
                if (GameInput.GetMenuUpDown())
                {
                    MoveHomeButtonSelection(-1);
                }
                else if (GameInput.GetMenuDownDown())
                {
                    MoveHomeButtonSelection(1);
                }
                else if (GameInput.GetConfirmDown())
                {
                    if (_homeSelectedBtnIndex >= 0 && _homeSelectedBtnIndex < _homeMenuButtons.Count)
                    {
                        Button b = _homeMenuButtons[_homeSelectedBtnIndex];
                        if (b != null && b.interactable)
                        {
                            b.onClick.Invoke();
                        }
                    }
                }
            }
            else if (_currentScreen == ActiveMenuScreen.WorldSelect)
            {
                if (GameInput.GetMenuLeftDown())
                {
                    PlayMenuHoverSound();
                    SelectWorldCard(0, openIfUnlocked: false);
                }
                else if (GameInput.GetMenuRightDown())
                {
                    PlayMenuHoverSound();
                    SelectWorldCard(1, openIfUnlocked: false);
                }
                else if (GameInput.GetConfirmDown())
                {
                    SelectWorldCard(_selectedWorldCardIndex, openIfUnlocked: true);
                }
                else if (GameInput.GetCancelDown())
                {
                    PlayMenuBackSound();
                    TransitionToPanel(ShowMainMenu);
                }
            }
            else if (_currentScreen == ActiveMenuScreen.LevelSelect)
            {
                if (GameInput.GetMenuLeftDown() || GameInput.GetMenuDownDown())
                {
                    int prev = Mathf.Max(0, _selectedMapLevelIndex - 1);
                    if (prev != _selectedMapLevelIndex)
                    {
                        PlayMenuHoverSound();
                        SelectMapNode(prev);
                    }
                }
                else if (GameInput.GetMenuRightDown() || GameInput.GetMenuUpDown())
                {
                    int next = Mathf.Min(_mapNodes.Count - 1, _selectedMapLevelIndex + 1);
                    if (next != _selectedMapLevelIndex)
                    {
                        PlayMenuHoverSound();
                        SelectMapNode(next);
                    }
                }
                else if (GameInput.GetConfirmDown())
                {
                    TryLaunchSelectedMapLevel();
                }
                else if (GameInput.GetCancelDown())
                {
                    PlayMenuBackSound();
                    TransitionToPanel(ShowWorldSelectScreen);
                }
            }
            else if (_currentScreen == ActiveMenuScreen.Pause)
            {
                if (GameInput.GetMenuUpDown())
                {
                    MovePauseButtonSelection(-1);
                }
                else if (GameInput.GetMenuDownDown())
                {
                    MovePauseButtonSelection(1);
                }
                else if (GameInput.GetConfirmDown())
                {
                    if (_pauseSelectedBtnIndex >= 0 && _pauseSelectedBtnIndex < _pauseMenuButtons.Count)
                    {
                        Button b = _pauseMenuButtons[_pauseSelectedBtnIndex];
                        if (b != null && b.interactable)
                        {
                            b.onClick.Invoke();
                        }
                    }
                }
                else if (GameInput.GetCancelDown())
                {
                    OnResumeRequested?.Invoke();
                }
            }
        }

        private void MoveHomeButtonSelection(int delta)
        {
            if (_homeMenuButtons.Count == 0) return;
            for (int attempt = 0; attempt < _homeMenuButtons.Count; attempt++)
            {
                _homeSelectedBtnIndex = (_homeSelectedBtnIndex + delta + _homeMenuButtons.Count) % _homeMenuButtons.Count;
                if (_homeMenuButtons[_homeSelectedBtnIndex] != null && _homeMenuButtons[_homeSelectedBtnIndex].interactable)
                {
                    break;
                }
            }
            PlayMenuHoverSound();
            RefreshHomeButtonSelectionVisuals();
        }

        private void RefreshHomeButtonSelectionVisuals()
        {
            bool hasSave = WorldProgressSave.HasSavedProgress("world_1");
            WorldDefinition w1 = WorldCatalog.GetWorld(0);
            int total = w1 != null ? w1.LevelCount : 10;
            int contLvl = WorldProgressSave.GetContinueLevelIndex("world_1", total);

            if (_homeContinueButton != null)
            {
                SetButtonVisualDisabled(_homeContinueButton, !hasSave);
                if (_homeContinueBtnLabel != null)
                {
                    _homeContinueBtnLabel.text = hasSave
                        ? $"CONTINUE (LVL {contLvl + 1:D2})"
                        : "CONTINUE";
                }
            }

            if (!hasSave && _homeSelectedBtnIndex == 0 && _homeMenuButtons.Count > 1)
            {
                _homeSelectedBtnIndex = 1; // Default highlight to NEW GAME when CONTINUE is disabled
            }

            for (int i = 0; i < _homeMenuButtons.Count; i++)
            {
                Button btn = _homeMenuButtons[i];
                if (btn == null || !btn.interactable) continue;
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i == _homeSelectedBtnIndex) ? _btnSelected : _btnNormal;
                }
            }
        }

        // ====================================================================
        // 7. TRANSITIONS & PANEL VISIBILITY API
        // ====================================================================
        private void TransitionToPanel(Action showPanelAction)
        {
            if (!Application.isPlaying || !gameObject.activeInHierarchy)
            {
                showPanelAction?.Invoke();
                return;
            }
            if (_isTransitioning) return;
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(PanelTransitionCoroutine(showPanelAction));
        }

        private void TransitionToAction(Action action)
        {
            if (!Application.isPlaying || !gameObject.activeInHierarchy)
            {
                action?.Invoke();
                return;
            }
            if (_isTransitioning) return;
            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(PanelTransitionCoroutine(action));
        }

        private IEnumerator PanelTransitionCoroutine(Action midTransitionAction)
        {
            _isTransitioning = true;
            _fadeSpeed = 7.5f;
            _targetFadeAlpha = 0.88f;

            float elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            midTransitionAction?.Invoke();

            _targetFadeAlpha = 0f;
            elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _fadeSpeed = 2.5f;
            _isTransitioning = false;
            _transitionCoroutine = null;
        }

        public void TriggerFlashFade()
        {
            if (_fadeOverlay != null)
            {
                _fadeSpeed = 2.5f;
                _fadeOverlay.color = new Color(0.03f, 0.04f, 0.08f, 0.85f);
                _targetFadeAlpha = 0f;
            }
            SetEndingQuoteAlpha(0f);
        }

        public void SetEndingFade(float targetAlpha, float fadeSpeed = 0.85f)
        {
            _targetFadeAlpha = Mathf.Clamp01(targetAlpha);
            _fadeSpeed = Mathf.Max(0.1f, fadeSpeed);
        }

        public void SetEndingQuoteText(string quote)
        {
            if (_endingQuoteText != null)
            {
                _endingQuoteText.text = quote;
            }
        }

        public void SetEndingQuoteAlpha(float alpha)
        {
            if (_endingQuoteText == null) return;
            float clamped = Mathf.Clamp01(alpha);
            _endingQuoteText.gameObject.SetActive(clamped > 0.01f);
            _endingQuoteText.color = new Color(1f, 0.92f, 0.76f, clamped);
        }

        private void HideAllMenuPanels()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_newGameConfirmModal != null) _newGameConfirmModal.SetActive(false);
            if (_worldSelectPanel != null) _worldSelectPanel.SetActive(false);
            if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);
            if (_pauseMenuContainer != null) _pauseMenuContainer.SetActive(false);
            if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
            if (_worldCompletePanel != null) _worldCompletePanel.SetActive(false);
            if (_world2ComingSoonPanel != null) _world2ComingSoonPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_hudPanel != null) _hudPanel.SetActive(false);
        }

        public void ShowMainMenu()
        {
            _targetFadeAlpha = 0f;
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _mainMenuPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.Home;
            _homeSelectedBtnIndex = WorldProgressSave.HasSavedProgress("world_1") ? 0 : 1;
            RefreshHomeButtonSelectionVisuals();
        }

        public void ShowWorldSelectScreen()
        {
            _selectedWorldCardIndex = 0;
            RefreshWorldSelectState();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _worldSelectPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.WorldSelect;
        }

        public void ShowLevelSelectScreen()
        {
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _levelSelectPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.LevelSelect;
        }

        public void ShowWorld1CompleteScreen()
        {
            RefreshWorldSelectState();
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _worldCompletePanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World1Complete;
        }

        public void ShowWorld2ComingSoonScreen()
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _world2ComingSoonPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World2ComingSoon;
        }

        public void ShowGameplayHUD(string levelTitle, string hint, bool isControllingShadow)
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _hudPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.None;

            UpdateHUDText(levelTitle, hint, isControllingShadow);
        }

        public void UpdateHUDText(string levelTitle, string hint, bool isControllingShadow)
        {
            if (_levelTitleText != null) _levelTitleText.text = levelTitle;
            if (_hintText != null) _hintText.text = hint;
            if (_activeControlText != null)
            {
                _activeControlText.text = isControllingShadow
                    ? "ACTIVE: SHADOW [TAB to switch]"
                    : "ACTIVE: PROTAGONIST [TAB to switch]";
                _activeControlText.color = isControllingShadow
                    ? new Color(0.76f, 0.56f, 1f, 1f)
                    : new Color(1f, 0.82f, 0.38f, 1f);
            }
        }

        public void SetPauseMenuVisible(bool visible)
        {
            if (_pauseMenuContainer != null) _pauseMenuContainer.SetActive(visible);
            if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(visible);
            if (!visible)
            {
                if (_settingsPanel != null) _settingsPanel.SetActive(false);
                if (_creditsPanel != null) _creditsPanel.SetActive(false);
                if (_currentScreen == ActiveMenuScreen.Pause || _currentScreen == ActiveMenuScreen.Settings)
                {
                    _currentScreen = ActiveMenuScreen.None;
                }
            }
            else
            {
                _currentScreen = ActiveMenuScreen.Pause;
                _pauseSelectedBtnIndex = 0;
                RefreshPauseButtonSelectionVisuals();
            }
        }

        public void ShowVictoryScreen()
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _victoryPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.Victory;
        }

        private void ShowSubPanel(GameObject panel)
        {
            RefreshAudioSettingsLabels();
            if (_screenBeforeSubPanel == ActiveMenuScreen.Pause && _pauseMenuPanel != null)
            {
                _pauseMenuPanel.SetActive(false);
            }
            _settingsPanel.SetActive(panel == _settingsPanel);
            _creditsPanel.SetActive(panel == _creditsPanel);
            _currentScreen = (panel == _settingsPanel) ? ActiveMenuScreen.Settings : ActiveMenuScreen.Credits;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.60f, 1.1f);
            }
        }

        private void CloseSubPanelAndReturn()
        {
            PlayMenuBackSound();
            _settingsPanel.SetActive(false);
            _creditsPanel.SetActive(false);
            if (_screenBeforeSubPanel == ActiveMenuScreen.Pause)
            {
                if (_pauseMenuContainer != null) _pauseMenuContainer.SetActive(true);
                if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(true);
                _currentScreen = ActiveMenuScreen.Pause;
                RefreshPauseButtonSelectionVisuals();
            }
            else
            {
                _currentScreen = ActiveMenuScreen.Home;
            }
        }

        // ====================================================================
        // 8. UI FACTORY & AUDIO HELPERS
        // ====================================================================
        private void PlayMenuHoverSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.28f, 1.24f);
            }
        }

        private void PlayMenuBackSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.48f, 0.82f);
            }
        }

        private void PlayMenuLockedSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("land", 0.45f, 0.76f);
            }
        }

        private void SetButtonVisualDisabled(Button btn, bool disabled)
        {
            if (btn == null) return;
            btn.interactable = !disabled;
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = disabled ? _btnDisabled : _btnNormal;
                img.color = disabled ? new Color(0.65f, 0.65f, 0.72f, 0.85f) : Color.white;
            }
            Text lbl = btn.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.color = disabled
                    ? new Color(0.52f, 0.54f, 0.64f, 0.75f)
                    : new Color(1f, 0.94f, 0.80f, 1f);
            }
        }

        private GameObject CreateFullScreenContainer(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private GameObject CreateSpriteElement(Transform parent, string name, Sprite sprite, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            return go;
        }

        private GameObject CreateImagePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Sprite overrideSprite = null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.sprite = overrideSprite != null ? overrideSprite : _panelSprite;
            img.type = Image.Type.Simple;
            return go;
        }

        private Text CreateStyledTitleText(Transform parent, string name, string content, int fontSize, Vector2 pos, Vector2 size, Color color)
        {
            Text txt = CreateText(parent, name, content, fontSize, pos, size, color, TextAnchor.MiddleCenter);
            txt.fontStyle = FontStyle.Bold;
            Shadow sh = txt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0.03f, 0.02f, 0.06f, 0.95f);
            sh.effectDistance = new Vector2(3f, -3f);
            return txt;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 pos, Vector2 size, Color color, TextAnchor align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = go.GetComponent<Text>();
            txt.font = _uiFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private Button CreateMenuButton(Transform parent, string label, Vector2 pos, Action onClick, Vector2? customSize = null)
        {
            GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            Vector2 btnSize = customSize ?? new Vector2(350f, 60f);
            rt.sizeDelta = btnSize;

            Image img = go.GetComponent<Image>();
            img.sprite = _btnNormal;

            Button btn = go.GetComponent<Button>();
            SpriteState st = new SpriteState
            {
                highlightedSprite = _btnSelected,
                pressedSprite = _btnPressed,
                selectedSprite = _btnSelected,
                disabledSprite = _btnDisabled
            };
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = st;

            Text lbl = CreateText(go.transform, "Label", label, 22, Vector2.zero, new Vector2(btnSize.x - 24f, btnSize.y - 8f), new Color(1f, 0.94f, 0.80f, 1f), TextAnchor.MiddleCenter);
            lbl.fontStyle = FontStyle.Bold;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            EventTrigger.Entry hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            hoverEntry.callback.AddListener((_) =>
            {
                if (btn.interactable) PlayMenuHoverSound();
            });
            trigger.triggers.Add(hoverEntry);

            btn.onClick.AddListener(() =>
            {
                if (_isTransitioning) return;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.05f);
                }
                onClick?.Invoke();
            });

            return btn;
        }

        private Button CreateCustomSpriteButton(Transform parent, string name, Sprite normal, Sprite hover, Sprite pressed, Vector2 pos, Vector2 size, Action onClick)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.sprite = normal;

            Button btn = go.GetComponent<Button>();
            SpriteState st = new SpriteState
            {
                highlightedSprite = hover,
                pressedSprite = pressed,
                selectedSprite = hover,
                disabledSprite = normal
            };
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = st;

            EventTrigger trigger = go.AddComponent<EventTrigger>();
            EventTrigger.Entry hoverEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            hoverEntry.callback.AddListener((_) =>
            {
                if (btn.interactable) PlayMenuHoverSound();
            });
            trigger.triggers.Add(hoverEntry);

            btn.onClick.AddListener(() =>
            {
                if (_isTransitioning) return;
                onClick?.Invoke();
            });

            return btn;
        }
    }
}
