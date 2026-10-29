using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyShadowQuit.Core;
using MyShadowQuit.Cosmetics;
using MyShadowQuit.Economy;

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
    /// 4. SHOP / RELICS CHAMBER: Full cosmetic skin store with 15 skins, animated live preview stage,
    ///    Soul Coins balance, tabbed categories (Player, Shadow, Torch), and debounced transactions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
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
        public event Action OnShopRequested;

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
            World2Complete,
            World3Complete,
            World4Complete,
            World2ComingSoon,
            Victory,
            NewGameConfirm,
            Shop,
            LoreJournal
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
        private GameObject _world2CompletePanel;
        private GameObject _world3CompletePanel;
        private GameObject _world4CompletePanel;
        private GameObject _world2ComingSoonPanel;
        private GameObject _victoryPanel;
        private GameObject _shopPanel;
        private GameObject _loreJournalPanel;

        // Combat & Health HUD
        private Image[] _heartIcons;
        private Image _equippedWeaponIcon;
        private Text _loreJournalContentText;
        private Text _loreJournalListText;

        private ActiveMenuScreen _currentScreen = ActiveMenuScreen.None;
        private ActiveMenuScreen _screenBeforeSubPanel = ActiveMenuScreen.Home;

        public ActiveMenuScreen CurrentScreen => _currentScreen;

        // HUD elements
        private Text _levelTitleText;
        private Text _hintText;
        private Text _activeControlText;
        private Text _musicVolumeText;
        private Text _sfxVolumeText;
        private Text _mobileSizeText;
        private Text _mobileOpacityText;
        private Text _mobilePositionText;
        private Text _mobileToggleText;
        private MobileControlsUI _mobileControlsUI;
        private Text _endingQuoteText;
        private Image _fadeOverlay;

        public MobileControlsUI MobileControls => _mobileControlsUI;

        // Home diorama & buttons
        private RectTransform _homeBgRect;
        private Image _homeProtagonistImage;
        private Image _homeShadowImage;
        private Image _homeTorchFlameImage;
        private Image _homeTorchHaloOuter;
        private Image _homeTorchHaloInner;
        private Color _homeTorchHaloBaseColor = new Color(1f, 0.64f, 0.18f, 1f);
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
        private Image _world2CardBgImage;
        private Text _world2WorldTagText;
        private Text _world2NameText;
        private GameObject _world2LockSealObj;
        private Text _world2ProgressText;
        private RectTransform _world2ProgressBarFill;
        private Button _world2ActionButton;
        private Text _world2ActionBtnText;
        private RectTransform _world3CardRect;
        private Image _world3CardBgImage;
        private Image _world3CardSelectionOutline;
        private Text _world3WorldTagText;
        private Text _world3NameText;
        private GameObject _world3LockSealObj;
        private Text _world3ProgressText;
        private RectTransform _world3ProgressBarFill;
        private Button _world3ActionButton;
        private Text _world3ActionBtnText;

        private RectTransform _world4CardRect;
        private Image _world4CardBgImage;
        private Image _world4CardSelectionOutline;
        private Text _world4WorldTagText;
        private Text _world4NameText;
        private GameObject _world4LockSealObj;
        private Text _world4ProgressText;
        private RectTransform _world4ProgressBarFill;
        private Button _world4ActionButton;
        private Text _world4ActionBtnText;

        private RectTransform _world5CardRect;
        private Image _world5CardBgImage;
        private Image _world5CardSelectionOutline;
        private Text _world5WorldTagText;
        private Text _world5NameText;
        private GameObject _world5LockSealObj;
        private Text _world5ProgressText;
        private RectTransform _world5ProgressBarFill;
        private Button _world5ActionButton;
        private Text _world5ActionBtnText;
        private int _selectedWorldCardIndex; // 0 = World 1, 1 = World 2, 2 = World 3, 3 = World 4, 4 = World 5

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
        private int _selectedMapWorldIndex = 0; // 0 = World 1, 1 = World 2, 2 = World 3
        private Image _mapContainerImage;
        private Text _mapTitleBannerText;
        private Sprite _forestMapSprite;
        private Sprite _clocktowerMapSprite;
        private Sprite _cathedralMapSprite;
        private Sprite _abyssMapSprite;

        // Shop UI elements
        private Text _shopCoinBalanceText;
        private readonly Button[] _shopCategoryTabButtons = new Button[3];
        private readonly Image[] _shopCategoryTabImages = new Image[3];
        private readonly Text[] _shopCategoryTabTexts = new Text[3];
        private int _shopSelectedTab = 0; // 0 = Player, 1 = Shadow, 2 = Torch

        private class ShopSkinCardUI
        {
            public Button CardButton;
            public Image CardBgImage;
            public Image CardIconImage;
            public Text CardTitleText;
            public Text CardRarityText;
            public Text CardStatusText;
            public Image CardCoinIcon;
        }
        private readonly List<ShopSkinCardUI> _shopCards = new List<ShopSkinCardUI>();
        private int _shopSelectedSkinIndex = 0; // 0..4

        // Shop Preview Stage
        private Image _shopPreviewImage;
        private Image _shopPreviewHaloImage;
        private Image _shopPreviewContactShadow;
        private float _shopJumpTimer = 0f;
        private Text _shopPreviewTitleText;
        private Text _shopPreviewRarityText;
        private Text _shopPreviewLoreText;
        private Button _shopActionButton;
        private Image _shopActionBtnImage;
        private Text _shopActionBtnText;
        private int _shopPreviewMode = 0; // 0 = Idle, 1 = Move/Run, 2 = Jump
        private readonly Button[] _shopPreviewModeButtons = new Button[3];
        private readonly Text[] _shopPreviewModeTexts = new Text[3];
        private Text _shopToastText;
        private Coroutine _shopToastCoroutine;

        // Gameplay Reward Toast Banner
        private GameObject _rewardToastBanner;
        private Text _rewardToastText;
        private Coroutine _rewardToastCoroutine;

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
        private Sprite _world3ArtSprite;
        private Sprite _world4ArtSprite;
        private Sprite _world5ArtSprite;
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

        private Sprite _coinSoulIconSprite;
        private Sprite _shopPedestalSprite;
        private Sprite _shopCardFrameNormal;
        private Sprite _shopCardFrameSelected;
        private Sprite _shopCardFrameEquipped;

        private Sprite[] _protagonistIdleFrames;
        private Sprite[] _shadowIdleFrames;
        private Sprite[] _torchFlameFrames;
        private Sprite[] _shopCachedPreviewFrames;

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

        private static readonly Vector2[] World2MapNodePixelCoords =
        {
            new Vector2(60f, 215f),   // 11 — WHISPERS IN THE ROOTS
            new Vector2(105f, 175f),  // 12 — THE RISING PATH
            new Vector2(148f, 135f),  // 13 — ROOTS OF LIGHT
            new Vector2(195f, 165f),  // 14 — FADING FOOTSTEPS
            new Vector2(242f, 195f),  // 15 — THE HOLLOW CANOPY
            new Vector2(290f, 145f),  // 16 — A BRIDGE OF EMBERS
            new Vector2(335f, 110f),  // 17 — THE SILENT THORNS
            new Vector2(365f, 160f),  // 18 — THE FRACTURED GROVE
            new Vector2(405f, 120f),  // 19 — TWO PATHS, ONE FLAME
            new Vector2(440f, 75f)    // 20 — HEART OF THE INVERTED FOREST
        };

        private static readonly Vector2[] World3MapNodePixelCoords =
        {
            new Vector2(55f, 220f),   // 21 — THE SILENT HANDS
            new Vector2(98f, 185f),   // 22 — THE FIRST TICK
            new Vector2(145f, 150f),  // 23 — BROKEN GEARS
            new Vector2(188f, 180f),  // 24 — THE SWINGING HALL
            new Vector2(236f, 140f),  // 25 — THE FROZEN MINUTE
            new Vector2(285f, 175f),  // 26 — THE LENS OF EMBERS
            new Vector2(330f, 130f),  // 27 — BETWEEN TWO TICKS
            new Vector2(370f, 165f),  // 28 — THE FALLEN BELLS
            new Vector2(408f, 115f),  // 29 — THE LAST HOUR
            new Vector2(445f, 70f)    // 30 — HEART OF THE CLOCKTOWER
        };

        private static readonly Vector2[] World4MapNodePixelCoords =
        {
            new Vector2(50f, 215f),   // 31 — BENEATH THE BLACK TIDE
            new Vector2(95f, 180f),   // 32 — THE FLOODED NAVE
            new Vector2(140f, 145f),  // 33 — DROWNED STEPS
            new Vector2(185f, 180f),  // 34 — GLASS BENEATH THE SEA
            new Vector2(230f, 140f),  // 35 — THE SILENT CHOIR
            new Vector2(280f, 175f),  // 36 — RISING WATERS
            new Vector2(325f, 130f),  // 37 — THE BROKEN BAPTISTERY
            new Vector2(365f, 165f),  // 38 — BENEATH THE BELLS
            new Vector2(405f, 115f),  // 39 — THE SUNKEN RELIQUARY
            new Vector2(440f, 70f)    // 40 — THE LAST CATHEDRAL LIGHT
        };

        private static readonly Vector2[] World5MapNodePixelCoords =
        {
            new Vector2(50f, 220f),   // 41 — EDGE OF OBLIVION
            new Vector2(92f, 185f),   // 42 — WHERE LIGHT BREAKS
            new Vector2(138f, 150f),  // 43 — ECHOES WITHOUT FORM
            new Vector2(182f, 180f),  // 44 — THE FLAME ANCHORS
            new Vector2(230f, 135f),  // 45 — BRIDGES OF NOTHING
            new Vector2(278f, 175f),  // 46 — FRACTURED REFLECTIONS
            new Vector2(322f, 130f),  // 47 — THE FORGOTTEN DIVIDE
            new Vector2(362f, 165f),  // 48 — A WORLD WITHOUT FIRE
            new Vector2(402f, 115f),  // 49 — BEFORE THE LAST DAWN
            new Vector2(445f, 70f)    // 50 — THE LAST FLAME
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

        public static readonly string[] World2CanonicalLevelShortNames =
        {
            "WHISPERS IN THE ROOTS",
            "THE RISING PATH",
            "ROOTS OF LIGHT",
            "FADING FOOTSTEPS",
            "THE HOLLOW CANOPY",
            "A BRIDGE OF EMBERS",
            "THE SILENT THORNS",
            "THE FRACTURED GROVE",
            "TWO PATHS, ONE FLAME",
            "HEART OF THE INVERTED FOREST"
        };

        public static readonly string[] World3CanonicalLevelShortNames =
        {
            "THE SILENT HANDS",
            "THE FIRST TICK",
            "BROKEN GEARS",
            "THE SWINGING HALL",
            "THE FROZEN MINUTE",
            "THE LENS OF EMBERS",
            "BETWEEN TWO TICKS",
            "THE FALLEN BELLS",
            "THE LAST HOUR",
            "HEART OF THE CLOCKTOWER"
        };

        public static readonly string[] World4CanonicalLevelShortNames =
        {
            "BENEATH THE BLACK TIDE",
            "THE FLOODED NAVE",
            "DROWNED STEPS",
            "GLASS BENEATH THE SEA",
            "THE SILENT CHOIR",
            "RISING WATERS",
            "THE BROKEN BAPTISTERY",
            "BENEATH THE BELLS",
            "THE SUNKEN RELIQUARY",
            "THE LAST CATHEDRAL LIGHT"
        };

        public static readonly string[] World5CanonicalLevelShortNames =
        {
            "EDGE OF OBLIVION",
            "WHERE LIGHT BREAKS",
            "ECHOES WITHOUT FORM",
            "THE FLAME ANCHORS",
            "BRIDGES OF NOTHING",
            "FRACTURED REFLECTIONS",
            "THE FORGOTTEN DIVIDE",
            "A WORLD WITHOUT FIRE",
            "BEFORE THE LAST DAWN",
            "THE LAST FLAME"
        };

        public const string World1CompleteQuote = "\"Beyond the darkness, another light awaits.\"";
        public const string World2CompleteQuote = "\"The roots remember the first spark. Light and Shadow breathe as one.\"";
        public const string World3CompleteQuote = "\"When the gears fall silent, the flame rings eternal.\"";
        public const string World4CompleteQuote = "\"Beneath the black tide, the last choir still remembers the light.\"";
        public const string FinalNarrativeQuote = "\"Even shadows need a light to return to.\"";

        public bool IsHomeScreenVisible => _mainMenuPanel != null && _mainMenuPanel.activeSelf;
        public bool IsWorldSelectScreenVisible => _worldSelectPanel != null && _worldSelectPanel.activeSelf;
        public bool IsLevelSelectScreenVisible => _levelSelectPanel != null && _levelSelectPanel.activeSelf;
        public bool IsPauseMenuVisible => _pauseMenuContainer != null && _pauseMenuContainer.activeSelf;
        public bool IsWorld1CompleteScreenVisible => _worldCompletePanel != null && _worldCompletePanel.activeSelf;
        public bool IsWorld2CompleteScreenVisible => _world2CompletePanel != null && _world2CompletePanel.activeSelf;
        public bool IsWorld3CompleteScreenVisible => _world3CompletePanel != null && _world3CompletePanel.activeSelf;
        public bool IsWorld4CompleteScreenVisible => _world4CompletePanel != null && _world4CompletePanel.activeSelf;
        public bool IsWorld2ComingSoonScreenVisible => _world2ComingSoonPanel != null && _world2ComingSoonPanel.activeSelf;
        public bool IsVictoryScreenVisible => _victoryPanel != null && _victoryPanel.activeSelf;
        public bool IsNewGameConfirmVisible => _newGameConfirmModal != null && _newGameConfirmModal.activeSelf;
        public bool IsShopScreenVisible => _shopPanel != null && _shopPanel.activeSelf;

        public GameObject MainMenuPanel => _mainMenuPanel;
        public GameObject WorldSelectPanel => _worldSelectPanel;
        public GameObject LevelSelectPanel => _levelSelectPanel;
        public GameObject ShopPanel => _shopPanel;
        public GameObject PauseMenuPanel => _pauseMenuPanel;
        public GameObject PauseMenuContainer => _pauseMenuContainer;
        public Button InGamePauseButton => _inGamePauseButton;
        public GameObject WorldCompletePanel => _worldCompletePanel;
        public GameObject World2CompletePanel => _world2CompletePanel;
        public GameObject World3CompletePanel => _world3CompletePanel;
        public GameObject World4CompletePanel => _world4CompletePanel;
        public GameObject World2ComingSoonPanel => _world2ComingSoonPanel;
        public GameObject VictoryPanel => _victoryPanel;
        public int SelectedMapLevelIndex => _selectedMapLevelIndex;
        public int SelectedMapWorldIndex => _selectedMapWorldIndex;
        public int MapNodeCount => _mapNodes.Count;
        public int PauseButtonCount => _pauseMenuButtons.Count;
        public int ShopSelectedTab => _shopSelectedTab;
        public int ShopSelectedSkinIndex => _shopSelectedSkinIndex;
        public int ShopCardCount => _shopCards.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Preserve single active reference
            }
            Instance = this;
            EnsureInitialized();
        }

        private void OnEnable()
        {
            Combat.WeaponCatalog.OnWeaponEquippedChanged -= HandleWeaponEquippedChanged;
            Combat.WeaponCatalog.OnWeaponEquippedChanged += HandleWeaponEquippedChanged;
        }

        private void OnDisable()
        {
            Combat.WeaponCatalog.OnWeaponEquippedChanged -= HandleWeaponEquippedChanged;
        }

        private void HandleWeaponEquippedChanged(Combat.WeaponType newWeapon)
        {
            RefreshWeaponHUD();
        }

        private void OnDestroy()
        {
            Combat.WeaponCatalog.OnWeaponEquippedChanged -= HandleWeaponEquippedChanged;
            if (Instance == this) Instance = null;
        }

        public void EnsureInitialized()
        {
            if (_canvas != null && _mainMenuPanel != null && _worldSelectPanel != null && _levelSelectPanel != null && _shopPanel != null)
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
            _world3ArtSprite = AssetRegistry.GetSprite("UI/world3_card_art", 32f);
            _world4ArtSprite = AssetRegistry.GetSprite("UI/world4_card_art", 32f);
            _world5ArtSprite = AssetRegistry.GetSprite("UI/world5_card_art", 32f);
            if (_world3ArtSprite == null)
            {
                // Never fall back to another world's illustration: a missing asset must be visible, not masked.
                Debug.LogError("[UIManager] Missing WORLD 3 card artwork at Resources/Sprites/UI/world3_card_art.png");
            }
            _worldCardActiveSprite = AssetRegistry.GetSprite("UI/world_card_frame_active", 32f) ?? _panelSprite;
            _worldCardLockedSprite = AssetRegistry.GetSprite("UI/world_card_frame_locked", 32f) ?? _panelSprite;
            _lockSealSprite = AssetRegistry.GetSprite("UI/icon_lock_seal", 32f);

            _ruinsMapSprite = AssetRegistry.GetSprite("UI/world1_ruins_map", 32f);
            _forestMapSprite = AssetRegistry.GetSprite("UI/world2_forest_map", 32f) ?? _ruinsMapSprite;
            _clocktowerMapSprite = AssetRegistry.GetSprite("UI/world3_clocktower_map", 32f) ?? AssetRegistry.GetSprite("World3/world3_clocktower_map", 32f) ?? _ruinsMapSprite;
            _cathedralMapSprite = AssetRegistry.GetSprite("UI/world4_cathedral_map", 32f) ?? _ruinsMapSprite;
            _abyssMapSprite = AssetRegistry.GetSprite("UI/world5_abyss_map", 32f) ?? _ruinsMapSprite;
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

            _coinSoulIconSprite = AssetRegistry.GetSprite("UI/coin_soul_icon", 32f);
            _shopPedestalSprite = AssetRegistry.GetSprite("UI/shop_pedestal", 32f);
            _shopCardFrameNormal = AssetRegistry.GetSprite("UI/shop_card_frame_normal", 32f) ?? _panelSprite;
            _shopCardFrameSelected = AssetRegistry.GetSprite("UI/shop_card_frame_selected", 32f) ?? _btnSelected;
            _shopCardFrameEquipped = AssetRegistry.GetSprite("UI/shop_card_frame_equipped", 32f) ?? _btnSelected;

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
            BuildShopMenu(canvasObj.transform);
            BuildPauseMenu(canvasObj.transform);
            BuildLoreJournalMenu(canvasObj.transform);
            BuildSettingsMenu(canvasObj.transform);
            BuildCreditsMenu(canvasObj.transform);
            BuildWorld1CompleteScreen(canvasObj.transform);
            BuildWorld2CompleteScreen(canvasObj.transform);
            BuildWorld3CompleteScreen(canvasObj.transform);
            BuildWorld4CompleteScreen(canvasObj.transform);
            BuildWorld2ComingSoonScreen(canvasObj.transform);
            BuildVictoryScreen(canvasObj.transform);
            BuildNewGameConfirmModal(canvasObj.transform);
            BuildFadeOverlay(canvasObj.transform);
            RefreshHomeEquippedSkins();
        }

        // ====================================================================
        // 1. GAMEPLAY HUD (1920x1080)
        // ====================================================================
        private void BuildHUD(Transform parent)
        {
            _hudPanel = CreateFullScreenContainer(parent, "HUDPanel");

            GameObject topBar = CreateImagePanel(_hudPanel.transform, "TopHUD", new Vector2(0f, 480f), new Vector2(1820f, 76f));
            topBar.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.12f, 0.85f);

            _levelTitleText = CreateText(topBar.transform, "LevelTitle", "LEVEL 1: AWAKENING", 30, new Vector2(-560f, 0f), new Vector2(620f, 58f), new Color(1f, 0.82f, 0.35f, 1f), TextAnchor.MiddleLeft);
            _activeControlText = CreateText(topBar.transform, "ActiveControl", "ACTIVE: PROTAGONIST [TAB to switch]", 25, new Vector2(430f, 0f), new Vector2(700f, 58f), new Color(0.85f, 0.78f, 1f, 1f), TextAnchor.MiddleRight);

            // Health Hearts Container (up to 6 hearts)
            GameObject healthContainer = new GameObject("HealthContainer", typeof(RectTransform));
            healthContainer.transform.SetParent(topBar.transform, false);
            RectTransform hrt = healthContainer.GetComponent<RectTransform>();
            hrt.anchoredPosition = new Vector2(-120f, 0f);
            hrt.sizeDelta = new Vector2(240f, 40f);

            _heartIcons = new Image[6];
            Sprite hFull = Resources.Load<Sprite>("Sprites/UI/heart_full");
            for (int i = 0; i < 6; i++)
            {
                GameObject hObj = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
                hObj.transform.SetParent(healthContainer.transform, false);
                RectTransform rt = hObj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(i * 36f - 90f, 0f);
                rt.sizeDelta = new Vector2(28f, 28f);
                Image img = hObj.GetComponent<Image>();
                img.sprite = hFull;
                img.preserveAspect = true;
                _heartIcons[i] = img;
            }

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
            bottomBar.SetActive(false); // Removed redundant bottom description overlay during gameplay

            _hintText = CreateText(bottomBar.transform, "HintText", "", 25, Vector2.zero, new Vector2(1420f, 54f), new Color(1f, 0.92f, 0.72f, 1f), TextAnchor.MiddleCenter);

            // Center-top floating reward toast notification for Soul Coins
            _rewardToastBanner = CreateImagePanel(_hudPanel.transform, "RewardToastBanner", new Vector2(0f, 380f), new Vector2(560f, 68f), _modalFrameSprite);
            _rewardToastBanner.GetComponent<Image>().color = new Color(0.08f, 0.07f, 0.16f, 0.95f);
            if (_coinSoulIconSprite != null)
            {
                CreateSpriteElement(_rewardToastBanner.transform, "RewardToastCoinIcon", _coinSoulIconSprite, new Vector2(-230f, 0f), new Vector2(36f, 36f));
            }
            _rewardToastText = CreateText(_rewardToastBanner.transform, "RewardToastText", "+100 SOUL COINS", 22, new Vector2(20f, 0f), new Vector2(460f, 50f), new Color(1f, 0.86f, 0.35f, 1f), TextAnchor.MiddleCenter);
            _rewardToastBanner.SetActive(false);

            // Multitouch Mobile Controls UI
            _mobileControlsUI = MobileControlsUI.Create(_hudPanel.transform);
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

            _homeContinueButton = CreateMenuButton(menuColumn.transform, "CONTINUE", new Vector2(0f, 215f), () =>
            {
                if (!WorldProgressSave.HasAnySavedProgress()) return;
                TransitionToAction(() => OnContinueRequested?.Invoke());
            }, new Vector2(380f, 56f));
            _homeContinueBtnImage = _homeContinueButton.GetComponent<Image>();
            _homeContinueBtnLabel = _homeContinueButton.GetComponentInChildren<Text>();
            _homeMenuButtons.Add(_homeContinueButton);

            Button newGameBtn = CreateMenuButton(menuColumn.transform, "NEW GAME", new Vector2(0f, 145f), () =>
            {
                if (WorldProgressSave.HasSavedProgress("world_1"))
                {
                    ShowNewGameConfirmationModal();
                }
                else
                {
                    TransitionToAction(() => OnNewGameRequested?.Invoke());
                }
            }, new Vector2(380f, 56f));
            _homeMenuButtons.Add(newGameBtn);

            Button worldSelectBtn = CreateMenuButton(menuColumn.transform, "WORLD SELECT", new Vector2(0f, 75f), () =>
            {
                TransitionToAction(() =>
                {
                    OnWorldSelectRequested?.Invoke();
                    ShowWorldSelectScreen();
                });
            }, new Vector2(380f, 56f));
            _homeMenuButtons.Add(worldSelectBtn);

            Button shopBtn = CreateMenuButton(menuColumn.transform, "SHOP", new Vector2(0f, 5f), () =>
            {
                TransitionToAction(() =>
                {
                    OnShopRequested?.Invoke();
                    ShowShopScreen();
                });
            }, new Vector2(380f, 56f));
            _homeMenuButtons.Add(shopBtn);

            Button settingsBtn = CreateMenuButton(menuColumn.transform, "SETTINGS", new Vector2(0f, -65f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Home;
                ShowSubPanel(_settingsPanel);
            }, new Vector2(380f, 56f));
            _homeMenuButtons.Add(settingsBtn);

            Button creditsBtn = CreateMenuButton(menuColumn.transform, "CREDITS", new Vector2(0f, -135f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Home;
                ShowSubPanel(_creditsPanel);
            }, new Vector2(380f, 56f));
            _homeMenuButtons.Add(creditsBtn);

            // Platform-aware QUIT button: hidden in WebGL browser builds so it never attempts to close the browser tab
            bool isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
            if (!isWebGL)
            {
                Button quitBtn = CreateMenuButton(menuColumn.transform, "QUIT", new Vector2(0f, -205f), () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }, new Vector2(380f, 56f));
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
            GameObject w1Card = CreateImagePanel(_worldSelectPanel.transform, "World1Card", new Vector2(-550f, 10f), new Vector2(510f, 630f), _worldCardActiveSprite);
            _world1CardRect = w1Card.GetComponent<RectTransform>();

            GameObject w1Outline = CreateImagePanel(w1Card.transform, "W1SelectionOutline", Vector2.zero, new Vector2(526f, 646f), _btnSelected);
            _world1CardSelectionOutline = w1Outline.GetComponent<Image>();
            _world1CardSelectionOutline.color = new Color(1f, 0.86f, 0.42f, 0.45f);
            _world1CardSelectionOutline.raycastTarget = false;
            w1Outline.transform.SetAsFirstSibling();

            CreateText(w1Card.transform, "W1WorldTag", "WORLD 1   •   AVAILABLE", 19, new Vector2(0f, 265f), new Vector2(460f, 32f), new Color(1f, 0.84f, 0.36f, 1f), TextAnchor.MiddleCenter);
            CreateStyledTitleText(w1Card.transform, "W1Name", "THE RUINS OF THE LAST FLAME", 24, new Vector2(0f, 226f), new Vector2(480f, 38f), new Color(1f, 0.96f, 0.86f, 1f));

            // Large pixel-art representation of World 1
            GameObject w1Art = CreateSpriteElement(w1Card.transform, "W1Artwork", _world1ArtSprite, new Vector2(0f, 62f), new Vector2(440f, 250f));
            Button w1ArtBtn = w1Art.AddComponent<Button>();
            w1ArtBtn.transition = Selectable.Transition.None;
            w1ArtBtn.onClick.AddListener(() => SelectWorldCard(0, openIfUnlocked: true));

            _world1ProgressText = CreateText(w1Card.transform, "W1ProgressLabel", "10 LEVELS   |   COMPLETED: 0 / 10", 18, new Vector2(0f, -105f), new Vector2(460f, 32f), new Color(0.90f, 0.84f, 1f, 1f), TextAnchor.MiddleCenter);

            // Progress Bar
            GameObject barBg = CreateImagePanel(w1Card.transform, "W1ProgressBarBg", new Vector2(0f, -145f), new Vector2(400f, 20f));
            barBg.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.14f, 0.95f);

            GameObject barFill = new GameObject("W1ProgressBarFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            _world1ProgressBarFill = barFill.GetComponent<RectTransform>();
            _world1ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world1ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world1ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world1ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world1ProgressBarFill.sizeDelta = new Vector2(39.4f, -6f);
            barFill.GetComponent<Image>().color = new Color(0.98f, 0.74f, 0.24f, 1f);

            // Button to open Level Select for World 1
            CreateMenuButton(w1Card.transform, "WORLD 1 — SELECT LEVEL", new Vector2(0f, -225f), () =>
            {
                SelectWorldCard(0, openIfUnlocked: true);
            }, new Vector2(390f, 58f));

            // ----------------------------------------------------------------
            // WORLD 2 VISUAL CARD (DYNAMIC: UNLOCKED OR LOCKED)
            // ----------------------------------------------------------------
            GameObject w2Card = CreateImagePanel(_worldSelectPanel.transform, "World2Card", new Vector2(0f, 10f), new Vector2(510f, 630f), _worldCardLockedSprite);
            _world2CardRect = w2Card.GetComponent<RectTransform>();
            _world2CardBgImage = w2Card.GetComponent<Image>();

            GameObject w2Outline = CreateImagePanel(w2Card.transform, "W2SelectionOutline", Vector2.zero, new Vector2(526f, 646f), _btnSelected);
            _world2CardSelectionOutline = w2Outline.GetComponent<Image>();
            _world2CardSelectionOutline.color = new Color(0.62f, 0.48f, 0.92f, 0f);
            _world2CardSelectionOutline.raycastTarget = false;
            w2Outline.transform.SetAsFirstSibling();

            _world2WorldTagText = CreateText(w2Card.transform, "W2WorldTag", "WORLD 2   •   LOCKED", 19, new Vector2(0f, 265f), new Vector2(460f, 32f), new Color(0.68f, 0.62f, 0.86f, 1f), TextAnchor.MiddleCenter);
            _world2NameText = CreateStyledTitleText(w2Card.transform, "W2Name", "THE INVERTED FOREST", 24, new Vector2(0f, 226f), new Vector2(480f, 38f), new Color(0.76f, 0.74f, 0.88f, 1f));

            // Artwork + Lock seal overlay
            GameObject w2Art = CreateSpriteElement(w2Card.transform, "W2Artwork", _world2ArtSprite, new Vector2(0f, 62f), new Vector2(440f, 250f));
            Button w2ArtBtn = w2Art.AddComponent<Button>();
            w2ArtBtn.transition = Selectable.Transition.None;
            w2ArtBtn.onClick.AddListener(() => SelectWorldCard(1, openIfUnlocked: true));

            _world2LockSealObj = CreateSpriteElement(w2Art.transform, "W2LockSeal", _lockSealSprite, new Vector2(0f, 0f), new Vector2(80f, 80f));

            _world2ProgressText = CreateText(
                w2Card.transform,
                "W2ProgressLabel",
                "10 LEVELS   |   COMPLETED: 0 / 10",
                18,
                new Vector2(0f, -105f),
                new Vector2(460f, 32f),
                new Color(0.72f, 0.95f, 0.85f, 1f),
                TextAnchor.MiddleCenter
            );

            // Progress Bar
            GameObject w2BarBg = CreateImagePanel(w2Card.transform, "W2ProgressBarBg", new Vector2(0f, -145f), new Vector2(400f, 20f));
            w2BarBg.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.08f, 0.95f);

            GameObject w2BarFill = new GameObject("W2ProgressBarFill", typeof(RectTransform), typeof(Image));
            w2BarFill.transform.SetParent(w2BarBg.transform, false);
            _world2ProgressBarFill = w2BarFill.GetComponent<RectTransform>();
            _world2ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world2ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world2ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world2ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world2ProgressBarFill.sizeDelta = new Vector2(18f, -6f);
            w2BarFill.GetComponent<Image>().color = new Color(0.24f, 0.88f, 0.65f, 1f);

            // Action button
            _world2ActionButton = CreateMenuButton(w2Card.transform, "WORLD 2 — SELECT LEVEL", new Vector2(0f, -225f), () =>
            {
                SelectWorldCard(1, openIfUnlocked: true);
            }, new Vector2(390f, 58f));
            _world2ActionBtnText = _world2ActionButton.GetComponentInChildren<Text>();

            // ----------------------------------------------------------------
            // WORLD 3 VISUAL CARD (DYNAMIC: UNLOCKED OR LOCKED)
            // ----------------------------------------------------------------
            GameObject w3Card = CreateImagePanel(_worldSelectPanel.transform, "World3Card", new Vector2(550f, 10f), new Vector2(510f, 630f), _worldCardLockedSprite);
            _world3CardRect = w3Card.GetComponent<RectTransform>();
            _world3CardBgImage = w3Card.GetComponent<Image>();

            GameObject w3Outline = CreateImagePanel(w3Card.transform, "W3SelectionOutline", Vector2.zero, new Vector2(526f, 646f), _btnSelected);
            _world3CardSelectionOutline = w3Outline.GetComponent<Image>();
            _world3CardSelectionOutline.color = new Color(0.85f, 0.72f, 0.38f, 0f);
            _world3CardSelectionOutline.raycastTarget = false;
            w3Outline.transform.SetAsFirstSibling();

            _world3WorldTagText = CreateText(w3Card.transform, "W3WorldTag", "WORLD 3   •   LOCKED", 19, new Vector2(0f, 265f), new Vector2(460f, 32f), new Color(0.68f, 0.62f, 0.86f, 1f), TextAnchor.MiddleCenter);
            _world3NameText = CreateStyledTitleText(w3Card.transform, "W3Name", "THE HOLLOW CLOCKTOWER", 24, new Vector2(0f, 226f), new Vector2(480f, 38f), new Color(0.76f, 0.74f, 0.88f, 1f));

            // Artwork + Lock seal overlay
            GameObject w3Art = CreateSpriteElement(w3Card.transform, "W3Artwork", _world3ArtSprite, new Vector2(0f, 62f), new Vector2(440f, 250f));
            Button w3ArtBtn = w3Art.AddComponent<Button>();
            w3ArtBtn.transition = Selectable.Transition.None;
            w3ArtBtn.onClick.AddListener(() => SelectWorldCard(2, openIfUnlocked: true));

            _world3LockSealObj = CreateSpriteElement(w3Art.transform, "W3LockSeal", _lockSealSprite, new Vector2(0f, 0f), new Vector2(80f, 80f));

            _world3ProgressText = CreateText(
                w3Card.transform,
                "W3ProgressLabel",
                "10 LEVELS   |   COMPLETED: 0 / 10",
                18,
                new Vector2(0f, -105f),
                new Vector2(460f, 32f),
                new Color(0.92f, 0.85f, 0.65f, 1f),
                TextAnchor.MiddleCenter
            );

            // Progress Bar
            GameObject w3BarBg = CreateImagePanel(w3Card.transform, "W3ProgressBarBg", new Vector2(0f, -145f), new Vector2(400f, 20f));
            w3BarBg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            GameObject w3BarFill = new GameObject("W3ProgressBarFill", typeof(RectTransform), typeof(Image));
            w3BarFill.transform.SetParent(w3BarBg.transform, false);
            _world3ProgressBarFill = w3BarFill.GetComponent<RectTransform>();
            _world3ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world3ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world3ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world3ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world3ProgressBarFill.sizeDelta = new Vector2(18f, -6f);
            w3BarFill.GetComponent<Image>().color = new Color(0.95f, 0.78f, 0.35f, 1f);

            // Action button
            _world3ActionButton = CreateMenuButton(w3Card.transform, "WORLD 3 — SELECT LEVEL", new Vector2(0f, -225f), () =>
            {
                SelectWorldCard(2, openIfUnlocked: true);
            }, new Vector2(390f, 58f));
            _world3ActionBtnText = _world3ActionButton.GetComponentInChildren<Text>();

            // ----------------------------------------------------------------
            // WORLD 4 VISUAL CARD (DYNAMIC: UNLOCKED OR LOCKED)
            // ----------------------------------------------------------------
            GameObject w4Card = CreateImagePanel(_worldSelectPanel.transform, "World4Card", new Vector2(0f, 10f), new Vector2(510f, 630f), _worldCardLockedSprite);
            _world4CardRect = w4Card.GetComponent<RectTransform>();
            _world4CardBgImage = w4Card.GetComponent<Image>();

            GameObject w4Outline = CreateImagePanel(w4Card.transform, "W4SelectionOutline", Vector2.zero, new Vector2(526f, 646f), _btnSelected);
            _world4CardSelectionOutline = w4Outline.GetComponent<Image>();
            _world4CardSelectionOutline.color = new Color(0.35f, 0.75f, 0.95f, 0f);
            _world4CardSelectionOutline.raycastTarget = false;
            w4Outline.transform.SetAsFirstSibling();

            _world4WorldTagText = CreateText(w4Card.transform, "W4WorldTag", "WORLD 4   •   LOCKED", 19, new Vector2(0f, 265f), new Vector2(460f, 32f), new Color(0.68f, 0.62f, 0.86f, 1f), TextAnchor.MiddleCenter);
            _world4NameText = CreateStyledTitleText(w4Card.transform, "W4Name", "THE DROWNED CATHEDRAL", 24, new Vector2(0f, 226f), new Vector2(480f, 38f), new Color(0.76f, 0.84f, 0.98f, 1f));

            GameObject w4Art = CreateSpriteElement(w4Card.transform, "W4Artwork", _world4ArtSprite, new Vector2(0f, 62f), new Vector2(440f, 250f));
            Button w4ArtBtn = w4Art.AddComponent<Button>();
            w4ArtBtn.transition = Selectable.Transition.None;
            w4ArtBtn.onClick.AddListener(() => SelectWorldCard(3, openIfUnlocked: true));

            _world4LockSealObj = CreateSpriteElement(w4Art.transform, "W4LockSeal", _lockSealSprite, new Vector2(0f, 0f), new Vector2(80f, 80f));

            _world4ProgressText = CreateText(
                w4Card.transform,
                "W4ProgressLabel",
                "10 LEVELS   |   COMPLETED: 0 / 10",
                18,
                new Vector2(0f, -105f),
                new Vector2(460f, 32f),
                new Color(0.75f, 0.92f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            GameObject w4BarBg = CreateImagePanel(w4Card.transform, "W4ProgressBarBg", new Vector2(0f, -145f), new Vector2(400f, 20f));
            w4BarBg.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.12f, 0.95f);

            GameObject w4BarFill = new GameObject("W4ProgressBarFill", typeof(RectTransform), typeof(Image));
            w4BarFill.transform.SetParent(w4BarBg.transform, false);
            _world4ProgressBarFill = w4BarFill.GetComponent<RectTransform>();
            _world4ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world4ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world4ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world4ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world4ProgressBarFill.sizeDelta = new Vector2(18f, -6f);
            w4BarFill.GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.95f, 1f);

            _world4ActionButton = CreateMenuButton(w4Card.transform, "WORLD 4 — SELECT LEVEL", new Vector2(0f, -225f), () =>
            {
                SelectWorldCard(3, openIfUnlocked: true);
            }, new Vector2(390f, 58f));
            _world4ActionBtnText = _world4ActionButton.GetComponentInChildren<Text>();

            // ----------------------------------------------------------------
            // WORLD 5 VISUAL CARD (DYNAMIC: UNLOCKED OR LOCKED)
            // ----------------------------------------------------------------
            GameObject w5Card = CreateImagePanel(_worldSelectPanel.transform, "World5Card", new Vector2(0f, 10f), new Vector2(510f, 630f), _worldCardLockedSprite);
            _world5CardRect = w5Card.GetComponent<RectTransform>();
            _world5CardBgImage = w5Card.GetComponent<Image>();

            GameObject w5Outline = CreateImagePanel(w5Card.transform, "W5SelectionOutline", Vector2.zero, new Vector2(526f, 646f), _btnSelected);
            _world5CardSelectionOutline = w5Outline.GetComponent<Image>();
            _world5CardSelectionOutline.color = new Color(0.85f, 0.45f, 0.95f, 0f);
            _world5CardSelectionOutline.raycastTarget = false;
            w5Outline.transform.SetAsFirstSibling();

            _world5WorldTagText = CreateText(w5Card.transform, "W5WorldTag", "WORLD 5   •   LOCKED", 19, new Vector2(0f, 265f), new Vector2(460f, 32f), new Color(0.68f, 0.62f, 0.86f, 1f), TextAnchor.MiddleCenter);
            _world5NameText = CreateStyledTitleText(w5Card.transform, "W5Name", "THE ABYSS OF THE LAST FLAME", 24, new Vector2(0f, 226f), new Vector2(480f, 38f), new Color(0.95f, 0.85f, 0.70f, 1f));

            GameObject w5Art = CreateSpriteElement(w5Card.transform, "W5Artwork", _world5ArtSprite, new Vector2(0f, 62f), new Vector2(440f, 250f));
            Button w5ArtBtn = w5Art.AddComponent<Button>();
            w5ArtBtn.transition = Selectable.Transition.None;
            w5ArtBtn.onClick.AddListener(() => SelectWorldCard(4, openIfUnlocked: true));

            _world5LockSealObj = CreateSpriteElement(w5Art.transform, "W5LockSeal", _lockSealSprite, new Vector2(0f, 0f), new Vector2(80f, 80f));

            _world5ProgressText = CreateText(
                w5Card.transform,
                "W5ProgressLabel",
                "10 LEVELS   |   COMPLETED: 0 / 10",
                18,
                new Vector2(0f, -105f),
                new Vector2(460f, 32f),
                new Color(0.95f, 0.85f, 0.70f, 1f),
                TextAnchor.MiddleCenter
            );

            GameObject w5BarBg = CreateImagePanel(w5Card.transform, "W5ProgressBarBg", new Vector2(0f, -145f), new Vector2(400f, 20f));
            w5BarBg.GetComponent<Image>().color = new Color(0.08f, 0.04f, 0.12f, 0.95f);

            GameObject w5BarFill = new GameObject("W5ProgressBarFill", typeof(RectTransform), typeof(Image));
            w5BarFill.transform.SetParent(w5BarBg.transform, false);
            _world5ProgressBarFill = w5BarFill.GetComponent<RectTransform>();
            _world5ProgressBarFill.anchorMin = new Vector2(0f, 0f);
            _world5ProgressBarFill.anchorMax = new Vector2(0f, 1f);
            _world5ProgressBarFill.pivot = new Vector2(0f, 0.5f);
            _world5ProgressBarFill.anchoredPosition = new Vector2(3f, 0f);
            _world5ProgressBarFill.sizeDelta = new Vector2(18f, -6f);
            w5BarFill.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.35f, 1f);

            _world5ActionButton = CreateMenuButton(w5Card.transform, "WORLD 5 — SELECT LEVEL", new Vector2(0f, -225f), () =>
            {
                SelectWorldCard(4, openIfUnlocked: true);
            }, new Vector2(390f, 58f));
            _world5ActionBtnText = _world5ActionButton.GetComponentInChildren<Text>();

            // Carousel Navigation Arrows
            CreateMenuButton(_worldSelectPanel.transform, "◄ PREV", new Vector2(-680f, -425f), () =>
            {
                int prev = (_selectedWorldCardIndex - 1 + 5) % 5;
                SelectWorldCard(prev, openIfUnlocked: false);
            }, new Vector2(180f, 62f));

            CreateMenuButton(_worldSelectPanel.transform, "NEXT ►", new Vector2(680f, -425f), () =>
            {
                int next = (_selectedWorldCardIndex + 1) % 5;
                SelectWorldCard(next, openIfUnlocked: false);
            }, new Vector2(180f, 62f));

            // Bottom Navigation Bar
            CreateMenuButton(_worldSelectPanel.transform, "BACK", new Vector2(-240f, -425f), () =>
            {
                PlayMenuBackSound();
                TransitionToPanel(ShowMainMenu);
            }, new Vector2(340f, 62f));

            CreateMenuButton(_worldSelectPanel.transform, "OPEN LEVEL MAP", new Vector2(240f, -425f), () =>
            {
                SelectWorldCard(_selectedWorldCardIndex, openIfUnlocked: true);
            }, new Vector2(340f, 62f));

            _worldSelectPanel.SetActive(false);
        }

        private void SelectWorldCard(int worldIdx, bool openIfUnlocked)
        {
            _selectedWorldCardIndex = Mathf.Clamp(worldIdx, 0, 4);
            UpdateWorldSelectHighlight();

            if (!openIfUnlocked) return;

            WorldDefinition w = WorldCatalog.GetWorld(_selectedWorldCardIndex);
            bool unlocked = w != null && w.IsUnlocked;
            if (!unlocked)
            {
                PlayMenuLockedSound();
                if (_selectedWorldCardIndex == 1)
                {
                    TransitionToPanel(ShowWorld2ComingSoonScreen);
                }
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.55f, 1.05f);
            }
            SetSelectedMapWorld(_selectedWorldCardIndex);
            TransitionToAction(() =>
            {
                OnLevelSelectRequested?.Invoke();
                ShowLevelSelectScreen();
            });
        }

        private void UpdateWorldSelectHighlight()
        {
            // Cards horizontal positions: selected card is centered at x=0; others offset by (i - selected) * 560
            RectTransform[] cards = { _world1CardRect, _world2CardRect, _world3CardRect, _world4CardRect, _world5CardRect };
            Image[] outlines = { _world1CardSelectionOutline, _world2CardSelectionOutline, _world3CardSelectionOutline, _world4CardSelectionOutline, _world5CardSelectionOutline };

            Color[] themeColors =
            {
                new Color(1f, 0.86f, 0.42f, 0.55f),
                new Color(0.45f, 0.95f, 0.75f, 0.45f),
                new Color(0.95f, 0.78f, 0.38f, 0.50f),
                new Color(0.35f, 0.75f, 0.95f, 0.50f),
                new Color(0.85f, 0.45f, 0.95f, 0.55f)
            };

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                bool isSel = (i == _selectedWorldCardIndex);
                float targetX = (i - _selectedWorldCardIndex) * 560f;
                cards[i].anchoredPosition = new Vector2(targetX, 10f);
                cards[i].localScale = isSel ? new Vector3(1.02f, 1.02f, 1f) : new Vector3(0.94f, 0.94f, 1f);

                // Show only cards near center
                cards[i].gameObject.SetActive(Mathf.Abs(i - _selectedWorldCardIndex) <= 1);

                if (outlines[i] != null)
                {
                    Color c = themeColors[i];
                    c.a = isSel ? 0.55f : 0f;
                    outlines[i].color = c;
                }
            }

            if (_worldSelectAmbientGlow != null)
            {
                _worldSelectAmbientGlow.rectTransform.anchoredPosition = new Vector2(0f, 20f);
                Color glowC = themeColors[_selectedWorldCardIndex];
                glowC.a = 0.28f;
                _worldSelectAmbientGlow.color = glowC;
            }
        }

        public void RefreshWorldSelectState()
        {
            // World 1 Status
            WorldDefinition w1 = WorldCatalog.GetWorld(0);
            int total1 = w1 != null ? w1.LevelCount : 10;
            int completedCount1 = WorldProgressSave.GetCompletedLevelsCount("world_1", total1);
            bool world1Completed = WorldProgressSave.IsWorldCompleted("world_1");
            if (world1Completed) completedCount1 = total1;

            if (_world1ProgressText != null)
            {
                string statusTag = world1Completed ? "COMPLETED" : "AVAILABLE";
                _world1ProgressText.text = $"10 LEVELS   |   COMPLETED: {completedCount1} / {total1}   |   {statusTag}";
            }

            if (_world1ProgressBarFill != null)
            {
                float ratio = Mathf.Clamp01(completedCount1 / (float)Mathf.Max(1, total1));
                float barWidth = Mathf.Max(18f, 394f * ratio);
                _world1ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }

            // World 2 Status
            WorldDefinition w2 = WorldCatalog.GetWorld(1);
            int total2 = w2 != null ? w2.LevelCount : 10;
            bool isW2Unlocked = w2 != null && w2.IsUnlocked;
            int completedCount2 = WorldProgressSave.GetCompletedLevelsCount("world_2", total2);
            bool world2Completed = WorldProgressSave.IsWorldCompleted("world_2");
            if (world2Completed) completedCount2 = total2;

            if (_world2CardBgImage != null)
            {
                _world2CardBgImage.sprite = isW2Unlocked ? _worldCardActiveSprite : _worldCardLockedSprite;
            }
            if (_world2LockSealObj != null)
            {
                _world2LockSealObj.SetActive(!isW2Unlocked);
            }
            if (_world2WorldTagText != null)
            {
                _world2WorldTagText.text = isW2Unlocked
                    ? (world2Completed ? "WORLD 2   •   COMPLETED" : "WORLD 2   •   AVAILABLE")
                    : "WORLD 2   •   LOCKED (COMPLETE WORLD 1)";
                _world2WorldTagText.color = isW2Unlocked ? new Color(0.45f, 0.95f, 0.75f, 1f) : new Color(0.68f, 0.62f, 0.86f, 1f);
            }
            if (_world2NameText != null)
            {
                _world2NameText.text = "THE INVERTED FOREST";
            }
            if (_world2ProgressText != null)
            {
                _world2ProgressText.text = isW2Unlocked
                    ? $"10 LEVELS   |   COMPLETED: {completedCount2} / {total2}"
                    : "COMPLETE WORLD 1 TO ENTER";
            }
            if (_world2ProgressBarFill != null)
            {
                float ratio = isW2Unlocked ? Mathf.Clamp01(completedCount2 / (float)Mathf.Max(1, total2)) : 0f;
                float barWidth = Mathf.Max(18f, 394f * ratio);
                _world2ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }
            if (_world2ActionButton != null)
            {
                SetButtonVisualDisabled(_world2ActionButton, !isW2Unlocked);
                if (_world2ActionBtnText != null)
                {
                    _world2ActionBtnText.text = isW2Unlocked ? "WORLD 2 — SELECT LEVEL" : "WORLD 2 (LOCKED)";
                }
            }

            // World 3 Status
            WorldDefinition w3 = WorldCatalog.GetWorld(2);
            int total3 = w3 != null ? w3.LevelCount : 10;
            bool isW3Unlocked = w3 != null && w3.IsUnlocked;
            int completedCount3 = WorldProgressSave.GetCompletedLevelsCount("world_3", total3);
            bool world3Completed = WorldProgressSave.IsWorldCompleted("world_3");
            if (world3Completed) completedCount3 = total3;

            if (_world3CardBgImage != null)
            {
                _world3CardBgImage.sprite = isW3Unlocked ? _worldCardActiveSprite : _worldCardLockedSprite;
            }
            if (_world3LockSealObj != null)
            {
                _world3LockSealObj.SetActive(!isW3Unlocked);
            }
            if (_world3WorldTagText != null)
            {
                _world3WorldTagText.text = isW3Unlocked
                    ? (world3Completed ? "WORLD 3   •   COMPLETED" : "WORLD 3   •   AVAILABLE")
                    : "WORLD 3   •   LOCKED (COMPLETE WORLD 2)";
                _world3WorldTagText.color = isW3Unlocked ? new Color(0.95f, 0.85f, 0.55f, 1f) : new Color(0.68f, 0.62f, 0.86f, 1f);
            }
            if (_world3NameText != null)
            {
                _world3NameText.text = "THE HOLLOW CLOCKTOWER";
            }
            if (_world3ProgressText != null)
            {
                _world3ProgressText.text = isW3Unlocked
                    ? $"10 LEVELS   |   COMPLETED: {completedCount3} / {total3}"
                    : "COMPLETE WORLD 2 TO ENTER";
            }
            if (_world3ProgressBarFill != null)
            {
                float ratio = isW3Unlocked ? Mathf.Clamp01(completedCount3 / (float)Mathf.Max(1, total3)) : 0f;
                float barWidth = Mathf.Max(18f, 394f * ratio);
                _world3ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }
            if (_world3ActionButton != null)
            {
                SetButtonVisualDisabled(_world3ActionButton, !isW3Unlocked);
                if (_world3ActionBtnText != null)
                {
                    _world3ActionBtnText.text = isW3Unlocked ? "WORLD 3 — SELECT LEVEL" : "WORLD 3 (LOCKED)";
                }
            }

            // World 4 Status
            WorldDefinition w4 = WorldCatalog.GetWorld(3);
            int total4 = w4 != null ? w4.LevelCount : 10;
            bool isW4Unlocked = w4 != null && w4.IsUnlocked;
            int completedCount4 = WorldProgressSave.GetCompletedLevelsCount("world_4", total4);
            bool world4Completed = WorldProgressSave.IsWorldCompleted("world_4");
            if (world4Completed) completedCount4 = total4;

            if (_world4CardBgImage != null)
            {
                _world4CardBgImage.sprite = isW4Unlocked ? _worldCardActiveSprite : _worldCardLockedSprite;
            }
            if (_world4LockSealObj != null)
            {
                _world4LockSealObj.SetActive(!isW4Unlocked);
            }
            if (_world4WorldTagText != null)
            {
                _world4WorldTagText.text = isW4Unlocked
                    ? (world4Completed ? "WORLD 4   •   COMPLETED" : "WORLD 4   •   AVAILABLE")
                    : "WORLD 4   •   LOCKED (COMPLETE WORLD 3)";
                _world4WorldTagText.color = isW4Unlocked ? new Color(0.35f, 0.85f, 0.95f, 1f) : new Color(0.68f, 0.62f, 0.86f, 1f);
            }
            if (_world4NameText != null)
            {
                _world4NameText.text = "THE DROWNED CATHEDRAL";
            }
            if (_world4ProgressText != null)
            {
                _world4ProgressText.text = isW4Unlocked
                    ? $"10 LEVELS   |   COMPLETED: {completedCount4} / {total4}"
                    : "COMPLETE WORLD 3 TO ENTER";
            }
            if (_world4ProgressBarFill != null)
            {
                float ratio = isW4Unlocked ? Mathf.Clamp01(completedCount4 / (float)Mathf.Max(1, total4)) : 0f;
                float barWidth = Mathf.Max(18f, 394f * ratio);
                _world4ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }
            if (_world4ActionButton != null)
            {
                SetButtonVisualDisabled(_world4ActionButton, !isW4Unlocked);
                if (_world4ActionBtnText != null)
                {
                    _world4ActionBtnText.text = isW4Unlocked ? "WORLD 4 — SELECT LEVEL" : "WORLD 4 (LOCKED)";
                }
            }

            // World 5 Status
            WorldDefinition w5 = WorldCatalog.GetWorld(4);
            int total5 = w5 != null ? w5.LevelCount : 10;
            bool isW5Unlocked = w5 != null && w5.IsUnlocked;
            int completedCount5 = WorldProgressSave.GetCompletedLevelsCount("world_5", total5);
            bool world5Completed = WorldProgressSave.IsWorldCompleted("world_5");
            if (world5Completed) completedCount5 = total5;

            if (_world5CardBgImage != null)
            {
                _world5CardBgImage.sprite = isW5Unlocked ? _worldCardActiveSprite : _worldCardLockedSprite;
            }
            if (_world5LockSealObj != null)
            {
                _world5LockSealObj.SetActive(!isW5Unlocked);
            }
            if (_world5WorldTagText != null)
            {
                _world5WorldTagText.text = isW5Unlocked
                    ? (world5Completed ? "WORLD 5   •   COMPLETED" : "WORLD 5   •   AVAILABLE")
                    : "WORLD 5   •   LOCKED (COMPLETE WORLD 4)";
                _world5WorldTagText.color = isW5Unlocked ? new Color(0.95f, 0.70f, 0.95f, 1f) : new Color(0.68f, 0.62f, 0.86f, 1f);
            }
            if (_world5NameText != null)
            {
                _world5NameText.text = "THE ABYSS OF THE LAST FLAME";
            }
            if (_world5ProgressText != null)
            {
                _world5ProgressText.text = isW5Unlocked
                    ? $"10 LEVELS   |   COMPLETED: {completedCount5} / {total5}"
                    : "COMPLETE WORLD 4 TO ENTER";
            }
            if (_world5ProgressBarFill != null)
            {
                float ratio = isW5Unlocked ? Mathf.Clamp01(completedCount5 / (float)Mathf.Max(1, total5)) : 0f;
                float barWidth = Mathf.Max(18f, 394f * ratio);
                _world5ProgressBarFill.sizeDelta = new Vector2(barWidth, -6f);
            }
            if (_world5ActionButton != null)
            {
                SetButtonVisualDisabled(_world5ActionButton, !isW5Unlocked);
                if (_world5ActionBtnText != null)
                {
                    _world5ActionBtnText.text = isW5Unlocked ? "WORLD 5 — SELECT LEVEL" : "WORLD 5 (LOCKED)";
                }
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

            _mapTitleBannerText = CreateStyledTitleText(topBanner.transform, "MapTitle", "WORLD 1 — THE RUINS OF THE LAST FLAME", 28, new Vector2(-460f, 0f), new Vector2(740f, 54f), new Color(1f, 0.85f, 0.38f, 1f));

            // World Switcher Tabs on Map Banner
            CreateMenuButton(topBanner.transform, "W1", new Vector2(-120f, 0f), () => SetSelectedMapWorld(0), new Vector2(70f, 46f));
            CreateMenuButton(topBanner.transform, "W2", new Vector2(-40f, 0f), () => SetSelectedMapWorld(1), new Vector2(70f, 46f));
            CreateMenuButton(topBanner.transform, "W3", new Vector2(40f, 0f), () => SetSelectedMapWorld(2), new Vector2(70f, 46f));
            CreateMenuButton(topBanner.transform, "W4", new Vector2(120f, 0f), () => SetSelectedMapWorld(3), new Vector2(70f, 46f));
            CreateMenuButton(topBanner.transform, "W5", new Vector2(200f, 0f), () => SetSelectedMapWorld(4), new Vector2(70f, 46f));

            _mapHeaderProgressText = CreateText(topBanner.transform, "MapProgressSummary", "SANCTUARY PROGRESS: 0 / 10 COMPLETED", 20, new Vector2(610f, 0f), new Vector2(460f, 48f), new Color(0.86f, 0.78f, 1f, 1f), TextAnchor.MiddleRight);

            // ----------------------------------------------------------------
            // LEFT/CENTER: 16:9 ILLUSTRATED PIXEL-ART MAP (1360x765)
            // ----------------------------------------------------------------
            const float mapWidth = 1360f;
            const float mapHeight = 765f;
            GameObject mapContainer = CreateSpriteElement(_levelSelectPanel.transform, "WorldMapCanvas", _ruinsMapSprite, new Vector2(-215f, 8f), new Vector2(mapWidth, mapHeight));
            _mapContainerImage = mapContainer.GetComponent<Image>();
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
            _inspectorLevelNumberText = CreateStyledTitleText(inspector.transform, "InspectorLevelNumber", "LEVEL 1", 40, new Vector2(0f, 276f), new Vector2(370f, 52f), new Color(1f, 0.86f, 0.38f, 1f));
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

        public void SetSelectedMapWorld(int worldIdx)
        {
            if (worldIdx >= 1)
            {
                WorldDefinition w = WorldCatalog.GetWorld(worldIdx);
                if (w == null || !w.IsUnlocked)
                {
                    PlayMenuLockedSound();
                    return;
                }
            }

            _selectedMapWorldIndex = Mathf.Clamp(worldIdx, 0, 4);
            if (_mapContainerImage != null)
            {
                switch (_selectedMapWorldIndex)
                {
                    case 4:
                        _mapContainerImage.sprite = _abyssMapSprite != null ? _abyssMapSprite : _ruinsMapSprite;
                        break;
                    case 3:
                        _mapContainerImage.sprite = _cathedralMapSprite != null ? _cathedralMapSprite : _ruinsMapSprite;
                        break;
                    case 2:
                        _mapContainerImage.sprite = _clocktowerMapSprite != null ? _clocktowerMapSprite : _ruinsMapSprite;
                        break;
                    case 1:
                        _mapContainerImage.sprite = _forestMapSprite != null ? _forestMapSprite : _ruinsMapSprite;
                        break;
                    default:
                        _mapContainerImage.sprite = _ruinsMapSprite;
                        break;
                }
            }

            if (_mapTitleBannerText != null)
            {
                string[] titles =
                {
                    "WORLD 1 — THE RUINS OF THE LAST FLAME",
                    "WORLD 2 — THE INVERTED FOREST",
                    "WORLD 3 — THE HOLLOW CLOCKTOWER",
                    "WORLD 4 — THE DROWNED CATHEDRAL",
                    "WORLD 5 — THE ABYSS OF THE LAST FLAME"
                };
                Color[] titleColors =
                {
                    new Color(1f, 0.85f, 0.38f, 1f),
                    new Color(0.42f, 0.95f, 0.75f, 1f),
                    new Color(0.95f, 0.82f, 0.45f, 1f),
                    new Color(0.35f, 0.85f, 0.95f, 1f),
                    new Color(0.95f, 0.70f, 0.95f, 1f)
                };
                _mapTitleBannerText.text = titles[_selectedMapWorldIndex];
                _mapTitleBannerText.color = titleColors[_selectedMapWorldIndex];
            }

            const float mapWidth = 1360f;
            const float mapHeight = 765f;
            float scaleX = mapWidth / 480f;
            float scaleY = mapHeight / 270f;

            Vector2[][] allCoords =
            {
                MapNodePixelCoords,
                World2MapNodePixelCoords,
                World3MapNodePixelCoords,
                World4MapNodePixelCoords,
                World5MapNodePixelCoords
            };
            Vector2[] coords = allCoords[_selectedMapWorldIndex];
            for (int i = 0; i < _mapNodes.Count && i < coords.Length; i++)
            {
                float ux = (coords[i].x - 240f) * scaleX;
                float uy = (135f - coords[i].y) * scaleY;
                Vector2 pos = new Vector2(ux, uy);
                _mapNodes[i].MapAnchoredPos = pos;
                _mapNodes[i].NodeRect.anchoredPosition = pos;
                int baseNum = (_selectedMapWorldIndex * 10) + i + 1;
                _mapNodes[i].NodeNumberLabel.text = $"{baseNum:D2}";
            }

            // Update path segment positions
            for (int p = 0; p < _mapPathSegments.Count; p++)
            {
                int targetIdx = _mapPathTargetNodeIndex[p];
                int fromIdx = targetIdx - 1;
                if (fromIdx >= 0 && targetIdx < _mapNodes.Count)
                {
                    Vector2 fromPos = _mapNodes[fromIdx].MapAnchoredPos;
                    Vector2 toPos = _mapNodes[targetIdx].MapAnchoredPos;
                    Vector2 delta = toPos - fromPos;
                    RectTransform rt = _mapPathSegments[p].rectTransform;
                    rt.anchoredPosition = fromPos + delta * 0.5f;
                    rt.sizeDelta = new Vector2(delta.magnitude, 10f);
                    float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    rt.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
            }

            RefreshLevelSelectMap();
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

            int displayNum = (_selectedMapWorldIndex * 10) + clamped + 1;

            if (_inspectorLevelNumberText != null)
            {
                _inspectorLevelNumberText.text = $"LEVEL {displayNum}";
            }
            if (_inspectorLevelTitleText != null)
            {
                string[][] worldTitles =
                {
                    CanonicalLevelShortNames,
                    World2CanonicalLevelShortNames,
                    World3CanonicalLevelShortNames,
                    World4CanonicalLevelShortNames,
                    World5CanonicalLevelShortNames
                };
                string[] titles = worldTitles[_selectedMapWorldIndex];
                _inspectorLevelTitleText.text = (clamped < titles.Length) ? titles[clamped] : $"CHAMBER {displayNum}";
            }
            if (_inspectorHintText != null)
            {
                int globalIdx = (_selectedMapWorldIndex * 10) + clamped;
                _inspectorHintText.text = globalIdx < LevelManager.LevelHints.Length
                    ? LevelManager.LevelHints[globalIdx]
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
                    _inspectorStatusBadgeText.text = $"LOCKED (COMPLETE LEVEL {displayNum - 1})";
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
            int targetWorld = _selectedMapWorldIndex;
            TransitionToAction(() =>
            {
                OnStartWorldRequested?.Invoke(targetWorld, false);
                OnLevelSelectedFromMap?.Invoke(targetLevel);
            });
            return true;
        }

        public void RefreshLevelSelectMap()
        {
            string worldId = "world_" + (_selectedMapWorldIndex + 1);
            WorldDefinition w = WorldCatalog.GetWorld(_selectedMapWorldIndex);
            int total = w != null ? w.LevelCount : 10;
            int completedCount = WorldProgressSave.GetCompletedLevelsCount(worldId, total);

            if (_mapHeaderProgressText != null)
            {
                string[] tags = { "SANCTUARY PROGRESS", "FOREST PROGRESS", "CLOCKTOWER PROGRESS", "CATHEDRAL PROGRESS", "ABYSS PROGRESS" };
                string tag = (_selectedMapWorldIndex < tags.Length) ? tags[_selectedMapWorldIndex] : "WORLD PROGRESS";
                _mapHeaderProgressText.text = $"{tag}: {completedCount} / {total} COMPLETED";
            }

            Color[] glowColors =
            {
                new Color(1f, 0.84f, 0.32f, 0.42f),
                new Color(0.35f, 0.95f, 0.72f, 0.45f),
                new Color(0.95f, 0.78f, 0.38f, 0.45f),
                new Color(0.35f, 0.85f, 0.95f, 0.45f),
                new Color(0.95f, 0.70f, 0.95f, 0.50f)
            };

            for (int i = 0; i < _mapNodes.Count; i++)
            {
                MapNodeUI node = _mapNodes[i];
                node.IsUnlocked = WorldProgressSave.IsLevelUnlocked(worldId, i, total);
                node.IsCompleted = WorldProgressSave.IsLevelCompleted(worldId, i, total);

                if (node.IsCompleted)
                {
                    node.NodeIconImage.sprite = _nodeCompletedSprite;
                    node.NodeIconImage.color = Color.white;
                    node.NodeGlowImage.gameObject.SetActive(true);
                    node.NodeGlowImage.color = glowColors[_selectedMapWorldIndex];
                    node.NodeNumberLabel.color = new Color(1f, 0.90f, 0.48f, 1f);
                }
                else if (node.IsUnlocked)
                {
                    node.NodeIconImage.sprite = _nodeAvailableSprite;
                    node.NodeIconImage.color = Color.white;
                    node.NodeGlowImage.gameObject.SetActive(true);
                    node.NodeGlowImage.color = glowColors[_selectedMapWorldIndex];
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
                    ? glowColors[_selectedMapWorldIndex]
                    : new Color(0.55f, 0.58f, 0.75f, 0.75f);
            }

            int defaultSelection = WorldProgressSave.GetContinueLevelIndex(worldId, total);
            SelectMapNode(defaultSelection);
        }

        // ====================================================================
        // 5. SHOP — CHAMBER OF RELICS (1920x1080 Landscape)
        // ====================================================================
        private void BuildShopMenu(Transform parent)
        {
            _shopPanel = CreateFullScreenContainer(parent, "ShopPanel");

            // Background
            GameObject bgObj = CreateSpriteElement(_shopPanel.transform, "ShopRuinsBackground", _homeRuinsBgSprite, Vector2.zero, new Vector2(1980f, 1114f));
            bgObj.GetComponent<Image>().color = new Color(0.42f, 0.42f, 0.52f, 1f);

            // Floating decorative embers
            BuildFloatingEmbers(_shopPanel.transform, 14);

            // Main frame container
            GameObject mainBox = CreateImagePanel(_shopPanel.transform, "ShopMainBox", Vector2.zero, new Vector2(1740f, 960f), _modalFrameSprite);
            mainBox.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.14f, 0.96f);

            // Header Section
            CreateStyledTitleText(mainBox.transform, "ShopTitle", "CHAMBER OF RELICS", 44, new Vector2(0f, 415f), new Vector2(900f, 54f), new Color(1f, 0.85f, 0.35f, 1f));
            CreateStyledTitleText(mainBox.transform, "ShopSubtitle", "SANCTUARY FORGE & ANCIENT EMBERS", 22, new Vector2(0f, 375f), new Vector2(900f, 32f), new Color(0.78f, 0.65f, 0.98f, 1f));
            CreateSpriteElement(mainBox.transform, "ShopHeaderDivider", _stoneDividerSprite, new Vector2(0f, 350f), new Vector2(1200f, 20f));

            // Return / Back Button (Top Left)
            CreateMenuButton(mainBox.transform, "RETURN", new Vector2(-700f, 415f), () =>
            {
                PlayMenuBackSound();
                TransitionToPanel(ShowMainMenu);
            }, new Vector2(220f, 50f));

            // Soul Coins Balance Widget (Top Right)
            GameObject coinWidget = CreateImagePanel(mainBox.transform, "CoinBalanceWidget", new Vector2(660f, 415f), new Vector2(300f, 52f), _btnNormal);
            coinWidget.GetComponent<Image>().color = new Color(0.12f, 0.11f, 0.22f, 0.95f);
            if (_coinSoulIconSprite != null)
            {
                CreateSpriteElement(coinWidget.transform, "SoulCoinIcon", _coinSoulIconSprite, new Vector2(-115f, 0f), new Vector2(32f, 32f));
            }
            _shopCoinBalanceText = CreateText(coinWidget.transform, "SoulCoinAmount", "0 COINS", 22, new Vector2(20f, 0f), new Vector2(230f, 40f), new Color(1f, 0.86f, 0.35f, 1f), TextAnchor.MiddleCenter);

            // Category Tabs: [ PROTAGONIST ] (0), [ SHADOW ] (1), [ TORCH ] (2)
            string[] tabNames = { "PROTAGONIST", "SHADOW", "TORCH" };
            float[] tabX = { -360f, 0f, 360f };
            for (int i = 0; i < 3; i++)
            {
                int tabIdx = i;
                Button tabBtn = CreateMenuButton(mainBox.transform, tabNames[i], new Vector2(tabX[i], 305f), () =>
                {
                    SwitchShopCategory(tabIdx);
                }, new Vector2(300f, 46f));

                _shopCategoryTabButtons[i] = tabBtn;
                _shopCategoryTabImages[i] = tabBtn.GetComponent<Image>();
                _shopCategoryTabTexts[i] = tabBtn.GetComponentInChildren<Text>();
            }

            // Left Column: 5 Skin Cards List Container
            GameObject cardsContainer = new GameObject("SkinCardsContainer", typeof(RectTransform));
            cardsContainer.transform.SetParent(mainBox.transform, false);
            RectTransform cardsRt = cardsContainer.GetComponent<RectTransform>();
            cardsRt.anchoredPosition = new Vector2(-410f, -65f);
            cardsRt.sizeDelta = new Vector2(780f, 600f);

            _shopCards.Clear();
            float cardSpacingY = 114f;
            for (int i = 0; i < 5; i++)
            {
                int skinIdx = i;
                float cardY = 228f - (i * cardSpacingY);

                GameObject cardObj = CreateImagePanel(cardsContainer.transform, $"SkinCard_{i}", new Vector2(0f, cardY), new Vector2(740f, 102f), _shopCardFrameNormal);
                Image cardBg = cardObj.GetComponent<Image>();
                Button cardBtn = cardObj.AddComponent<Button>();

                // Icon preview (64x64) in Left Zone
                GameObject iconObj = CreateSpriteElement(cardObj.transform, "SkinIcon", null, new Vector2(-300f, 0f), new Vector2(64f, 64f));
                Image iconImg = iconObj.GetComponent<Image>();

                // Center Zone: Skin Title & Rarity Badge
                Text titleTxt = CreateText(cardObj.transform, "SkinTitle", "SKIN NAME", 20, new Vector2(-75f, 16f), new Vector2(360f, 30f), Color.white, TextAnchor.MiddleLeft);
                titleTxt.fontStyle = FontStyle.Bold;

                Text rarityTxt = CreateText(cardObj.transform, "SkinRarity", "COMMON", 15, new Vector2(-75f, -16f), new Vector2(360f, 24f), new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleLeft);

                // Right Zone: Price (Coin Icon + Number) or Status Badge (OWNED / EQUIPPED)
                GameObject cardCoinObj = CreateSpriteElement(cardObj.transform, "CardCoinIcon", _coinSoulIconSprite, new Vector2(195f, 0f), new Vector2(24f, 24f));
                Image cardCoinImg = cardCoinObj.GetComponent<Image>();

                Text statusTxt = CreateText(cardObj.transform, "SkinStatus", "EQUIPPED", 20, new Vector2(245f, 0f), new Vector2(210f, 36f), new Color(1f, 0.85f, 0.35f), TextAnchor.MiddleCenter);
                statusTxt.fontStyle = FontStyle.Bold;

                cardBtn.onClick.AddListener(() =>
                {
                    SelectShopSkin(skinIdx);
                });

                ShopSkinCardUI cardUI = new ShopSkinCardUI
                {
                    CardButton = cardBtn,
                    CardBgImage = cardBg,
                    CardIconImage = iconImg,
                    CardTitleText = titleTxt,
                    CardRarityText = rarityTxt,
                    CardStatusText = statusTxt,
                    CardCoinIcon = cardCoinImg
                };
                _shopCards.Add(cardUI);
            }

            // Right Column: Preview Stage & Inspector
            GameObject stagePanel = CreateImagePanel(mainBox.transform, "PreviewStagePanel", new Vector2(440f, -65f), new Vector2(740f, 600f), _panelSprite);
            stagePanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.10f, 0.90f);

            // Halo glow on the stage
            GameObject stageHalo = CreateSpriteElement(stagePanel.transform, "StageHalo", _lightHaloSprite, new Vector2(0f, 60f), new Vector2(420f, 420f));
            _shopPreviewHaloImage = stageHalo.GetComponent<Image>();
            _shopPreviewHaloImage.color = new Color(1f, 0.7f, 0.25f, 0.35f);
            _shopPreviewHaloImage.raycastTarget = false;

            // Pedestal on the stage
            CreateSpriteElement(stagePanel.transform, "StagePedestal", _shopPedestalSprite, new Vector2(0f, -30f), new Vector2(200f, 80f));

            // Soft contact shadow on pedestal surface under character
            GameObject shadowObj = CreateSpriteElement(stagePanel.transform, "StageContactShadow", _lightHaloSprite, new Vector2(0f, -8f), new Vector2(100f, 24f));
            _shopPreviewContactShadow = shadowObj.GetComponent<Image>();
            _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, 0.45f);
            _shopPreviewContactShadow.raycastTarget = false;

            // Animated character / flame preview sprite
            GameObject previewSpr = CreateSpriteElement(stagePanel.transform, "StagePreviewSprite", null, new Vector2(0f, 65f), new Vector2(160f, 160f));
            _shopPreviewImage = previewSpr.GetComponent<Image>();
            _shopPreviewImage.preserveAspect = true;

            // Preview Animation Mode Buttons: [ IDLE ] [ MOVE ] [ JUMP ]
            string[] modeLabels = { "IDLE", "MOVE", "JUMP" };
            float[] modeX = { -150f, 0f, 150f };
            for (int m = 0; m < 3; m++)
            {
                int modeIdx = m;
                Button mBtn = CreateMenuButton(stagePanel.transform, modeLabels[m], new Vector2(modeX[m], -95f), () =>
                {
                    SetShopPreviewMode(modeIdx);
                }, new Vector2(130f, 38f));

                _shopPreviewModeButtons[m] = mBtn;
                _shopPreviewModeTexts[m] = mBtn.GetComponentInChildren<Text>();
                if (_shopPreviewModeTexts[m] != null) _shopPreviewModeTexts[m].fontSize = 16;
            }

            // Skin Details below stage
            _shopPreviewTitleText = CreateStyledTitleText(stagePanel.transform, "PreviewTitle", "ASHEN KNIGHT", 26, new Vector2(0f, -135f), new Vector2(680f, 32f), new Color(1f, 0.88f, 0.40f));
            _shopPreviewRarityText = CreateText(stagePanel.transform, "PreviewRarity", "RARE RELIC", 17, new Vector2(0f, -165f), new Vector2(680f, 24f), new Color(0.35f, 0.75f, 1f), TextAnchor.MiddleCenter);
            _shopPreviewLoreText = CreateText(stagePanel.transform, "PreviewLore", "Forged in the embers of the sanctuary.", 15, new Vector2(0f, -198f), new Vector2(660f, 36f), new Color(0.85f, 0.85f, 0.95f), TextAnchor.MiddleCenter);

            // Toast feedback message (placed safely above action button)
            _shopToastText = CreateText(stagePanel.transform, "ShopToastText", "", 17, new Vector2(0f, -225f), new Vector2(600f, 24f), new Color(0.4f, 1f, 0.5f), TextAnchor.MiddleCenter);
            _shopToastText.gameObject.SetActive(false);

            // Big Action Button: BUY / EQUIP / EQUIPPED
            _shopActionButton = CreateMenuButton(stagePanel.transform, "EQUIP", new Vector2(0f, -262f), () =>
            {
                ExecuteShopActionButton();
            }, new Vector2(360f, 50f));
            _shopActionBtnImage = _shopActionButton.GetComponent<Image>();
            _shopActionBtnText = _shopActionButton.GetComponentInChildren<Text>();
            if (_shopActionBtnText != null) _shopActionBtnText.fontSize = 22;

            _shopPanel.SetActive(false);
        }

        public void SwitchShopCategory(int tabIndex)
        {
            _shopSelectedTab = Mathf.Clamp(tabIndex, 0, 2);
            _shopSelectedSkinIndex = 0;
            _shopPreviewMode = 0;
            _shopJumpTimer = 0f;
            ResetShopPreviewTransform();

            RefreshShopTabSelection();
            RefreshShopSkinList();
            UpdateShopSelectedPreview();
            PlayMenuHoverSound();
        }

        private void RefreshShopTabSelection()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_shopCategoryTabImages[i] != null)
                {
                    _shopCategoryTabImages[i].sprite = (i == _shopSelectedTab) ? _btnSelected : _btnNormal;
                }
                if (_shopCategoryTabTexts[i] != null)
                {
                    _shopCategoryTabTexts[i].color = (i == _shopSelectedTab)
                        ? new Color(1f, 0.88f, 0.40f, 1f)
                        : new Color(0.80f, 0.80f, 0.90f, 0.85f);
                }
            }
        }

        private void RefreshShopSkinList()
        {
            SkinType currentType = (_shopSelectedTab == 0) ? SkinType.Player :
                                   (_shopSelectedTab == 1) ? SkinType.Shadow : SkinType.Torch;

            List<SkinDefinition> skins = SkinCatalog.GetSkinsByType(currentType);

            string equippedId = "";
            if (SkinEquipmentService.Instance != null)
            {
                equippedId = (_shopSelectedTab == 0) ? SkinEquipmentService.Instance.EquippedPlayerSkinId :
                             (_shopSelectedTab == 1) ? SkinEquipmentService.Instance.EquippedShadowSkinId :
                             SkinEquipmentService.Instance.EquippedTorchSkinId;
            }

            for (int i = 0; i < _shopCards.Count; i++)
            {
                if (i >= skins.Count)
                {
                    _shopCards[i].CardButton.gameObject.SetActive(false);
                    continue;
                }

                _shopCards[i].CardButton.gameObject.SetActive(true);
                SkinDefinition def = skins[i];

                bool isEquipped = (def.Id == equippedId);
                bool isOwned = InventoryService.Instance != null && InventoryService.Instance.IsSkinOwned(def.Id);
                bool isSelected = (i == _shopSelectedSkinIndex);

                _shopCards[i].CardBgImage.sprite = isEquipped ? _shopCardFrameEquipped :
                                                   isSelected ? _shopCardFrameSelected : _shopCardFrameNormal;

                _shopCards[i].CardIconImage.sprite = AssetRegistry.GetSprite("Skins/Icons/" + def.Id + "_icon", 32f);

                _shopCards[i].CardTitleText.text = def.DisplayName.ToUpperInvariant();
                _shopCards[i].CardRarityText.text = def.Rarity.ToString().ToUpperInvariant();
                _shopCards[i].CardRarityText.color = GetRarityColor(def.Rarity);

                if (isEquipped)
                {
                    _shopCards[i].CardCoinIcon.gameObject.SetActive(false);
                    _shopCards[i].CardStatusText.rectTransform.anchoredPosition = new Vector2(245f, 0f);
                    _shopCards[i].CardStatusText.rectTransform.sizeDelta = new Vector2(210f, 36f);
                    _shopCards[i].CardStatusText.alignment = TextAnchor.MiddleCenter;
                    _shopCards[i].CardStatusText.text = "EQUIPPED";
                    _shopCards[i].CardStatusText.color = new Color(0.4f, 1f, 0.5f);
                }
                else if (isOwned)
                {
                    _shopCards[i].CardCoinIcon.gameObject.SetActive(false);
                    _shopCards[i].CardStatusText.rectTransform.anchoredPosition = new Vector2(245f, 0f);
                    _shopCards[i].CardStatusText.rectTransform.sizeDelta = new Vector2(210f, 36f);
                    _shopCards[i].CardStatusText.alignment = TextAnchor.MiddleCenter;
                    _shopCards[i].CardStatusText.text = "OWNED";
                    _shopCards[i].CardStatusText.color = new Color(0.7f, 0.85f, 1f);
                }
                else
                {
                    _shopCards[i].CardCoinIcon.gameObject.SetActive(true);
                    _shopCards[i].CardCoinIcon.rectTransform.anchoredPosition = new Vector2(195f, 0f);
                    _shopCards[i].CardCoinIcon.rectTransform.sizeDelta = new Vector2(24f, 24f);
                    _shopCards[i].CardStatusText.rectTransform.anchoredPosition = new Vector2(275f, 0f);
                    _shopCards[i].CardStatusText.rectTransform.sizeDelta = new Vector2(130f, 36f);
                    _shopCards[i].CardStatusText.alignment = TextAnchor.MiddleLeft;
                    _shopCards[i].CardStatusText.text = $"{def.Price}";
                    _shopCards[i].CardStatusText.color = new Color(1f, 0.85f, 0.35f);
                }
            }
        }

        private Color GetRarityColor(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare: return new Color(0.35f, 0.75f, 1f);
                case SkinRarity.Epic: return new Color(0.80f, 0.45f, 1f);
                case SkinRarity.Legendary: return new Color(1f, 0.85f, 0.30f);
                default: return new Color(0.80f, 0.85f, 0.90f);
            }
        }

        public void SelectShopSkin(int index)
        {
            _shopSelectedSkinIndex = Mathf.Clamp(index, 0, 4);
            _shopJumpTimer = 0f;
            ResetShopPreviewTransform();
            RefreshShopSkinList();
            UpdateShopSelectedPreview();
            PlayMenuHoverSound();
        }

        public void SetShopPreviewMode(int mode)
        {
            _shopPreviewMode = Mathf.Clamp(mode, 0, 2);
            _shopJumpTimer = 0f;
            ResetShopPreviewTransform();
            UpdateShopPreviewModeButtons();
            LoadCachedPreviewFrames();
        }

        private void ResetShopPreviewTransform()
        {
            if (_shopPreviewImage != null)
            {
                _shopPreviewImage.rectTransform.anchoredPosition = new Vector2(0f, 65f);
                _shopPreviewImage.rectTransform.localScale = Vector3.one;
            }
            if (_shopPreviewContactShadow != null)
            {
                _shopPreviewContactShadow.rectTransform.localScale = Vector3.one;
                _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, 0.45f);
            }
        }

        private void UpdateShopPreviewModeButtons()
        {
            bool isTorch = (_shopSelectedTab == 2);
            for (int i = 0; i < 3; i++)
            {
                if (isTorch)
                {
                    _shopPreviewModeButtons[i].gameObject.SetActive(i == 0);
                    if (i == 0 && _shopPreviewModeTexts[0] != null)
                    {
                        _shopPreviewModeTexts[0].text = "BURNING FLAME";
                    }
                }
                else
                {
                    _shopPreviewModeButtons[i].gameObject.SetActive(true);
                    if (_shopPreviewModeTexts[i] != null)
                    {
                        _shopPreviewModeTexts[i].text = (i == 0) ? "IDLE" : (i == 1) ? "MOVE" : "JUMP";
                    }
                }

                if (_shopPreviewModeButtons[i] != null)
                {
                    _shopPreviewModeButtons[i].GetComponent<Image>().sprite = (i == _shopPreviewMode) ? _btnSelected : _btnNormal;
                }
            }
        }

        private void LoadCachedPreviewFrames()
        {
            SkinType currentType = (_shopSelectedTab == 0) ? SkinType.Player :
                                   (_shopSelectedTab == 1) ? SkinType.Shadow : SkinType.Torch;
            List<SkinDefinition> skins = SkinCatalog.GetSkinsByType(currentType);
            if (_shopSelectedSkinIndex < 0 || _shopSelectedSkinIndex >= skins.Count) return;

            SkinDefinition def = skins[_shopSelectedSkinIndex];
            string prefix = def.SpritePathPrefix;

            if (currentType == SkinType.Player)
            {
                if (_shopPreviewMode == 0)
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/idle", 4);
                else if (_shopPreviewMode == 1)
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/run", 6);
                else
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/jump", 2);
            }
            else if (currentType == SkinType.Shadow)
            {
                if (_shopPreviewMode == 0)
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/idle", 4);
                else if (_shopPreviewMode == 1)
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/move", 6);
                else
                    _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/jump", 2);
            }
            else
            {
                _shopCachedPreviewFrames = AssetRegistry.GetAnimationFrames(prefix + "/flame", 4);
            }

            if (_shopCachedPreviewFrames != null && _shopCachedPreviewFrames.Length > 0 && _shopPreviewImage != null)
            {
                _shopPreviewImage.sprite = _shopCachedPreviewFrames[0];
            }
        }

        private void UpdateShopSelectedPreview()
        {
            SkinType currentType = (_shopSelectedTab == 0) ? SkinType.Player :
                                   (_shopSelectedTab == 1) ? SkinType.Shadow : SkinType.Torch;
            List<SkinDefinition> skins = SkinCatalog.GetSkinsByType(currentType);
            if (_shopSelectedSkinIndex < 0 || _shopSelectedSkinIndex >= skins.Count) return;

            SkinDefinition def = skins[_shopSelectedSkinIndex];

            string equippedId = "";
            if (SkinEquipmentService.Instance != null)
            {
                equippedId = (_shopSelectedTab == 0) ? SkinEquipmentService.Instance.EquippedPlayerSkinId :
                             (_shopSelectedTab == 1) ? SkinEquipmentService.Instance.EquippedShadowSkinId :
                             SkinEquipmentService.Instance.EquippedTorchSkinId;
            }

            bool isEquipped = (def.Id == equippedId);
            bool isOwned = InventoryService.Instance != null && InventoryService.Instance.IsSkinOwned(def.Id);
            int balance = WalletService.Instance != null ? WalletService.Instance.Balance : 0;

            if (_shopPreviewTitleText != null) _shopPreviewTitleText.text = def.DisplayName.ToUpperInvariant();
            if (_shopPreviewRarityText != null)
            {
                _shopPreviewRarityText.text = $"{def.Rarity.ToString().ToUpperInvariant()} RELIC";
                _shopPreviewRarityText.color = GetRarityColor(def.Rarity);
            }
            if (_shopPreviewLoreText != null) _shopPreviewLoreText.text = def.LoreDescription;

            if (_shopPreviewHaloImage != null)
            {
                _shopPreviewHaloImage.color = new Color(def.HaloColor.r, def.HaloColor.g, def.HaloColor.b, 0.40f);
            }

            UpdateShopPreviewModeButtons();
            LoadCachedPreviewFrames();

            if (_shopActionButton != null)
            {
                if (isEquipped)
                {
                    SetButtonVisualDisabled(_shopActionButton, true);
                    if (_shopActionBtnText != null) _shopActionBtnText.text = "EQUIPPED";
                }
                else if (isOwned)
                {
                    SetButtonVisualDisabled(_shopActionButton, false);
                    if (_shopActionBtnText != null) _shopActionBtnText.text = "EQUIP";
                }
                else
                {
                    bool canAfford = balance >= def.Price;
                    SetButtonVisualDisabled(_shopActionButton, false);
                    if (_shopActionBtnText != null)
                    {
                        _shopActionBtnText.text = canAfford ? $"UNLOCK ({def.Price} COINS)" : $"NEED {def.Price} COINS";
                        if (!canAfford)
                        {
                            _shopActionBtnText.color = new Color(1f, 0.45f, 0.45f);
                        }
                    }
                }
            }
        }

        public void ExecuteShopActionButton()
        {
            SkinType currentType = (_shopSelectedTab == 0) ? SkinType.Player :
                                   (_shopSelectedTab == 1) ? SkinType.Shadow : SkinType.Torch;
            List<SkinDefinition> skins = SkinCatalog.GetSkinsByType(currentType);
            if (_shopSelectedSkinIndex < 0 || _shopSelectedSkinIndex >= skins.Count) return;

            SkinDefinition def = skins[_shopSelectedSkinIndex];

            string equippedId = "";
            if (SkinEquipmentService.Instance != null)
            {
                equippedId = (_shopSelectedTab == 0) ? SkinEquipmentService.Instance.EquippedPlayerSkinId :
                             (_shopSelectedTab == 1) ? SkinEquipmentService.Instance.EquippedShadowSkinId :
                             SkinEquipmentService.Instance.EquippedTorchSkinId;
            }

            if (def.Id == equippedId)
            {
                ShowShopToast("ALREADY EQUIPPED", new Color(1f, 0.85f, 0.35f));
                return;
            }

            bool isOwned = InventoryService.Instance != null && InventoryService.Instance.IsSkinOwned(def.Id);
            if (isOwned)
            {
                bool success = SkinEquipmentService.Instance != null && SkinEquipmentService.Instance.EquipSkin(def.Id);
                if (success)
                {
                    ShowShopToast($"EQUIPPED {def.DisplayName.ToUpperInvariant()}!", new Color(0.4f, 1f, 0.5f));
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.25f);
                    }
                }
            }
            else
            {
                if (ShopTransactionService.Instance != null)
                {
                    bool purchased = ShopTransactionService.Instance.PurchaseSkin(def.Id);
                    if (purchased)
                    {
                        ShowShopToast($"UNLOCKED {def.DisplayName.ToUpperInvariant()}!", new Color(0.4f, 1f, 0.5f));
                    }
                    else
                    {
                        ShowShopToast("NOT ENOUGH SOUL COINS", new Color(1f, 0.4f, 0.4f));
                    }
                }
            }

            RefreshShopCoinBalance();
            RefreshShopSkinList();
            UpdateShopSelectedPreview();
            RefreshHomeEquippedSkins();
        }

        public void RefreshShopCoinBalance()
        {
            if (_shopCoinBalanceText != null)
            {
                int balance = WalletService.Instance != null ? WalletService.Instance.Balance : 0;
                _shopCoinBalanceText.text = $"{balance:N0} COINS";
            }
        }

        private void ShowShopToast(string message, Color color)
        {
            if (_shopToastText == null) return;
            if (_shopToastCoroutine != null) StopCoroutine(_shopToastCoroutine);
            _shopToastCoroutine = StartCoroutine(ShopToastCoroutine(message, color));
        }

        private IEnumerator ShopToastCoroutine(string message, Color color)
        {
            _shopToastText.text = message;
            _shopToastText.color = color;
            _shopToastText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2.0f);
            _shopToastText.gameObject.SetActive(false);
            _shopToastCoroutine = null;
        }

        // ====================================================================
        // 6. PAUSE, SETTINGS, CREDITS, WORLD 1 COMPLETE & WORLD 2 COMING SOON
        // ====================================================================
        private void BuildPauseMenu(Transform parent)
        {
            _pauseMenuContainer = CreateFullScreenContainer(parent, "PauseMenuContainer");
            Image backdrop = _pauseMenuContainer.AddComponent<Image>();
            backdrop.color = new Color(0.02f, 0.03f, 0.06f, 0.72f); // partially dim gameplay behind pause

            _pauseMenuPanel = CreateImagePanel(_pauseMenuContainer.transform, "PauseMenuPanel", Vector2.zero, new Vector2(720f, 740f), _modalFrameSprite);
            CreateStyledTitleText(_pauseMenuPanel.transform, "PauseHeader", "PAUSED", 46, new Vector2(0f, 275f), new Vector2(560f, 60f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateStyledTitleText(_pauseMenuPanel.transform, "PauseSubtitle", "THE FLAME RESTS", 24, new Vector2(0f, 225f), new Vector2(560f, 38f), new Color(0.78f, 0.62f, 0.98f, 1f));
            CreateSpriteElement(_pauseMenuPanel.transform, "PauseDivider", _stoneDividerSprite, new Vector2(0f, 185f), new Vector2(460f, 24f));

            _pauseMenuButtons.Clear();

            Button resumeBtn = CreateMenuButton(_pauseMenuPanel.transform, "RESUME", new Vector2(0f, 115f), () => OnResumeRequested?.Invoke(), new Vector2(360f, 54f));
            _pauseMenuButtons.Add(resumeBtn);

            Button restartBtn = CreateMenuButton(_pauseMenuPanel.transform, "RESTART LEVEL", new Vector2(0f, 50f), () => OnRestartRequested?.Invoke(), new Vector2(360f, 54f));
            _pauseMenuButtons.Add(restartBtn);

            Button journalBtn = CreateMenuButton(_pauseMenuPanel.transform, "LORE JOURNAL", new Vector2(0f, -15f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Pause;
                ShowSubPanel(_loreJournalPanel);
                RefreshLoreJournalUI();
            }, new Vector2(360f, 54f));
            _pauseMenuButtons.Add(journalBtn);

            Button levelSelectBtn = CreateMenuButton(_pauseMenuPanel.transform, "LEVEL SELECT", new Vector2(0f, -80f), () =>
            {
                TransitionToAction(() =>
                {
                    OnLevelSelectRequested?.Invoke();
                    ShowLevelSelectScreen();
                });
            }, new Vector2(360f, 54f));
            _pauseMenuButtons.Add(levelSelectBtn);

            Button settingsBtn = CreateMenuButton(_pauseMenuPanel.transform, "SETTINGS", new Vector2(0f, -145f), () =>
            {
                _screenBeforeSubPanel = ActiveMenuScreen.Pause;
                ShowSubPanel(_settingsPanel);
            }, new Vector2(360f, 54f));
            _pauseMenuButtons.Add(settingsBtn);

            Button mainMenuBtn = CreateMenuButton(_pauseMenuPanel.transform, "MAIN MENU", new Vector2(0f, -210f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            }, new Vector2(360f, 54f));
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

        private void BuildLoreJournalMenu(Transform parent)
        {
            _loreJournalPanel = CreateImagePanel(parent, "LoreJournalPanel", Vector2.zero, new Vector2(1040f, 720f), _modalFrameSprite);
            CreateStyledTitleText(_loreJournalPanel.transform, "LoreHeader", "LORE JOURNAL", 38, new Vector2(0f, 295f), new Vector2(620f, 48f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateSpriteElement(_loreJournalPanel.transform, "LoreDivider", _stoneDividerSprite, new Vector2(0f, 255f), new Vector2(720f, 22f));

            // Left side list
            _loreJournalListText = CreateText(_loreJournalPanel.transform, "LoreList", "ARCHIVED MEMORIES:", 20, new Vector2(-280f, 40f), new Vector2(400f, 360f), new Color(0.85f, 0.85f, 0.95f, 1f), TextAnchor.UpperLeft);

            // Right side detailed content
            _loreJournalContentText = CreateText(_loreJournalPanel.transform, "LoreContent", "Select a fragment to read.", 19, new Vector2(180f, 40f), new Vector2(480f, 360f), new Color(1f, 0.92f, 0.72f, 1f), TextAnchor.UpperLeft);

            CreateMenuButton(_loreJournalPanel.transform, "BACK", new Vector2(0f, -270f), () =>
            {
                CloseSubPanelAndReturn();
            }, new Vector2(260f, 50f));

            _loreJournalPanel.SetActive(false);
        }

        public void RefreshLoreJournalUI()
        {
            if (_loreJournalListText == null || _loreJournalContentText == null) return;

            var entries = Narrative.JournalService.GetAllUnlockedEntries();
            if (entries.Count == 0)
            {
                _loreJournalListText.text = "CHRONICLES OF ASH\n\n(No memories unlocked yet.\nDiscover NPCs across the 5 worlds to gather fragments.)";
                _loreJournalContentText.text = "";
                return;
            }

            System.Text.StringBuilder listSb = new System.Text.StringBuilder();
            System.Text.StringBuilder contSb = new System.Text.StringBuilder();

            listSb.AppendLine($"CHRONICLES ({entries.Count}/{Narrative.JournalService.TotalAvailableCount} UNLOCKED):\n");
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                listSb.AppendLine($"• {e.title}");
                contSb.AppendLine($"<b><color=#FFD566>{e.title}</color></b>");
                contSb.AppendLine($"<i><color=#C8A8F8>{e.worldOrigin}</color></i>");
                contSb.AppendLine(e.content);
                contSb.AppendLine("----------------------------------------\n");
            }

            _loreJournalListText.text = listSb.ToString();
            _loreJournalContentText.text = contSb.ToString();
        }

        private void BuildSettingsMenu(Transform parent)
        {
            _settingsPanel = CreateImagePanel(parent, "SettingsPanel", Vector2.zero, new Vector2(860f, 660f), _modalFrameSprite);
            CreateStyledTitleText(_settingsPanel.transform, "SettingsHeader", "AUDIO & CONTROLS", 36, new Vector2(0f, 265f), new Vector2(620f, 48f), new Color(1f, 0.84f, 0.35f, 1f));
            CreateSpriteElement(_settingsPanel.transform, "SettingsDivider", _stoneDividerSprite, new Vector2(0f, 225f), new Vector2(520f, 22f));

            float initMusic = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1.0f;
            float initSfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f;

            // Audio Controls
            _musicVolumeText = CreateText(_settingsPanel.transform, "MusicVolumeText", $"MUSIC VOLUME: {Mathf.RoundToInt(initMusic * 100f)}%", 22, new Vector2(-160f, 175f), new Vector2(340f, 42f), new Color(1f, 0.90f, 0.55f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "MUSIC + / -", new Vector2(175f, 175f), () =>
            {
                if (AudioManager.Instance != null)
                {
                    float next = AudioManager.Instance.MusicVolume + 0.2f;
                    if (next > 1.05f) next = 0.0f;
                    AudioManager.Instance.MusicVolume = next;
                    RefreshAudioSettingsLabels();
                    AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.05f);
                }
            }, new Vector2(250f, 46f));

            _sfxVolumeText = CreateText(_settingsPanel.transform, "SfxVolumeText", $"SFX VOLUME: {Mathf.RoundToInt(initSfx * 100f)}%", 22, new Vector2(-160f, 120f), new Vector2(340f, 42f), new Color(0.85f, 0.80f, 1f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "SFX + / -", new Vector2(175f, 120f), () =>
            {
                if (AudioManager.Instance != null)
                {
                    float next = AudioManager.Instance.SfxVolume + 0.2f;
                    if (next > 1.05f) next = 0.0f;
                    AudioManager.Instance.SfxVolume = next;
                    RefreshAudioSettingsLabels();
                    AudioManager.Instance.PlaySFX("switch_activate", 0.75f, 1.0f);
                }
            }, new Vector2(250f, 46f));

            // Mobile Controls Section Divider
            CreateSpriteElement(_settingsPanel.transform, "SettingsMobileDivider", _stoneDividerSprite, new Vector2(0f, 65f), new Vector2(460f, 18f));
            CreateText(_settingsPanel.transform, "MobileSectionHeader", "MOBILE TOUCH CONTROLS", 20, new Vector2(0f, 85f), new Vector2(400f, 32f), new Color(0.92f, 0.78f, 1f, 1f), TextAnchor.MiddleCenter);

            // Row 1: Size & Opacity
            _mobileSizeText = CreateText(_settingsPanel.transform, "MobileSizeText", "SIZE: 100%", 21, new Vector2(-160f, 15f), new Vector2(340f, 40f), new Color(1f, 0.92f, 0.65f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "CYCLE SIZE", new Vector2(175f, 15f), () =>
            {
                if (_mobileControlsUI != null) _mobileControlsUI.CycleSize();
                else
                {
                    float cur = PlayerPrefs.GetFloat(MobileControlsUI.PrefKeySize, 1.0f);
                    float next = (cur < 0.9f) ? 1.0f : ((cur < 1.1f) ? 1.2f : 0.8f);
                    PlayerPrefs.SetFloat(MobileControlsUI.PrefKeySize, next);
                    PlayerPrefs.Save();
                }
                RefreshMobileSettingsLabels();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.1f);
            }, new Vector2(250f, 44f));

            _mobileOpacityText = CreateText(_settingsPanel.transform, "MobileOpacityText", "OPACITY: 75%", 21, new Vector2(-160f, -38f), new Vector2(340f, 40f), new Color(1f, 0.92f, 0.65f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "CYCLE OPACITY", new Vector2(175f, -38f), () =>
            {
                if (_mobileControlsUI != null) _mobileControlsUI.CycleOpacity();
                else
                {
                    float cur = PlayerPrefs.GetFloat(MobileControlsUI.PrefKeyOpacity, 0.75f);
                    float next = (cur < 0.6f) ? 0.75f : ((cur < 0.9f) ? 1.0f : 0.5f);
                    PlayerPrefs.SetFloat(MobileControlsUI.PrefKeyOpacity, next);
                    PlayerPrefs.Save();
                }
                RefreshMobileSettingsLabels();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.1f);
            }, new Vector2(250f, 44f));

            // Row 2: Margins / Notches & Touch UI Mode
            _mobilePositionText = CreateText(_settingsPanel.transform, "MobilePositionText", "MARGINS: DEFAULT", 21, new Vector2(-160f, -90f), new Vector2(340f, 40f), new Color(0.85f, 0.88f, 1f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "CYCLE MARGINS", new Vector2(175f, -90f), () =>
            {
                if (_mobileControlsUI != null) _mobileControlsUI.CyclePosition();
                else
                {
                    int cur = PlayerPrefs.GetInt(MobileControlsUI.PrefKeyPosition, 0);
                    PlayerPrefs.SetInt(MobileControlsUI.PrefKeyPosition, (cur == 0) ? 1 : 0);
                    PlayerPrefs.Save();
                }
                RefreshMobileSettingsLabels();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.1f);
            }, new Vector2(250f, 44f));

            _mobileToggleText = CreateText(_settingsPanel.transform, "MobileToggleText", "TOUCH UI: AUTO", 21, new Vector2(-160f, -142f), new Vector2(340f, 40f), new Color(0.85f, 0.88f, 1f, 1f), TextAnchor.MiddleLeft);
            CreateMenuButton(_settingsPanel.transform, "TOGGLE TOUCH", new Vector2(175f, -142f), () =>
            {
                int cur = PlayerPrefs.GetInt(MobileControlsUI.PrefKeyEnabled, 0);
                int next = (cur == 0) ? 1 : ((cur == 1) ? -1 : 0);
                PlayerPrefs.SetInt(MobileControlsUI.PrefKeyEnabled, next);
                PlayerPrefs.Save();
                _mobileControlsUI?.UpdateVisibility();
                RefreshMobileSettingsLabels();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.1f);
            }, new Vector2(250f, 44f));

            CreateText(
                _settingsPanel.transform,
                "ControlsGuide",
                "KEYBOARD: A/D (Move)  SPACE (Jump)  Q (Torch)  E (Interact)  J (Attack)  F (Weapon)  TAB (Switch)  ESC (Pause)",
                17,
                new Vector2(0f, -195f),
                new Vector2(880f, 40f),
                new Color(0.75f, 0.82f, 0.95f, 0.85f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_settingsPanel.transform, "RETURN", new Vector2(0f, -255f), () =>
            {
                CloseSubPanelAndReturn();
            }, new Vector2(240f, 48f));
            _settingsPanel.SetActive(false);
        }

        public void RefreshAudioSettingsLabels()
        {
            if (AudioManager.Instance != null)
            {
                if (_musicVolumeText != null)
                {
                    _musicVolumeText.text = $"MUSIC VOLUME: {Mathf.RoundToInt(AudioManager.Instance.MusicVolume * 100f)}%";
                }
                if (_sfxVolumeText != null)
                {
                    _sfxVolumeText.text = $"SFX VOLUME: {Mathf.RoundToInt(AudioManager.Instance.SfxVolume * 100f)}%";
                }
            }
            RefreshMobileSettingsLabels();
        }

        public void RefreshMobileSettingsLabels()
        {
            float size = PlayerPrefs.GetFloat(MobileControlsUI.PrefKeySize, 1.0f);
            float opacity = PlayerPrefs.GetFloat(MobileControlsUI.PrefKeyOpacity, 0.75f);
            int pos = PlayerPrefs.GetInt(MobileControlsUI.PrefKeyPosition, 0);
            int enabledPref = PlayerPrefs.GetInt(MobileControlsUI.PrefKeyEnabled, 0);

            if (_mobileSizeText != null) _mobileSizeText.text = $"SIZE: {Mathf.RoundToInt(size * 100f)}%";
            if (_mobileOpacityText != null) _mobileOpacityText.text = $"OPACITY: {Mathf.RoundToInt(opacity * 100f)}%";
            if (_mobilePositionText != null) _mobilePositionText.text = (pos == 1) ? "MARGINS: WIDE" : "MARGINS: DEFAULT";
            if (_mobileToggleText != null)
            {
                string modeStr = "AUTO";
                if (enabledPref == 1) modeStr = "ALWAYS ON";
                else if (enabledPref == -1) modeStr = "OFF";
                _mobileToggleText.text = $"TOUCH UI: {modeStr}";
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
            Image panelImg = _worldCompletePanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.12f, 0.10f, 0.16f, 0.95f);
            }

            // Warm amber flame halo behind the torch
            GameObject haloDeco = CreateSpriteElement(_worldCompletePanel.transform, "WorldCompleteHalo", _lightHaloSprite, new Vector2(0f, 268f), new Vector2(160f, 160f));
            Image haloImg = haloDeco.GetComponent<Image>();
            if (haloImg != null)
            {
                haloImg.color = new Color(1f, 0.72f, 0.28f, 0.35f);
                haloImg.raycastTarget = false;
            }

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

            CreateMenuButton(_worldCompletePanel.transform, "CONTINUE TO WORLD 2", new Vector2(0f, -32f), () =>
            {
                SetSelectedMapWorld(1);
                TransitionToAction(() => OnWorldCompleteContinueRequested?.Invoke());
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

        private void BuildWorld2CompleteScreen(Transform parent)
        {
            _world2CompletePanel = CreateImagePanel(parent, "World2CompletePanel", Vector2.zero, new Vector2(880f, 700f), _modalFrameSprite);
            Image panelImg = _world2CompletePanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.06f, 0.18f, 0.14f, 0.95f);
            }

            // Bioluminescent emerald/jade halo behind the torch
            GameObject haloDeco = CreateSpriteElement(_world2CompletePanel.transform, "World2CompleteHalo", _lightHaloSprite, new Vector2(0f, 268f), new Vector2(160f, 160f));
            Image haloImg = haloDeco.GetComponent<Image>();
            if (haloImg != null)
            {
                haloImg.color = new Color(0.24f, 0.92f, 0.70f, 0.40f);
                haloImg.raycastTarget = false;
            }

            GameObject torchDeco = CreateSpriteElement(_world2CompletePanel.transform, "World2CompleteTorchIcon", AssetRegistry.GetSprite("Torch/torch_item_0", 32f), new Vector2(0f, 268f), new Vector2(68f, 88f));
            torchDeco.GetComponent<Image>().raycastTarget = false;

            CreateStyledTitleText(_world2CompletePanel.transform, "World2CompleteTitle", "WORLD 2 CONQUERED", 46, new Vector2(0f, 192f), new Vector2(760f, 60f), new Color(0.35f, 0.98f, 0.75f, 1f));
            CreateStyledTitleText(_world2CompletePanel.transform, "World2CompleteSub", "THE INVERTED FOREST", 28, new Vector2(0f, 140f), new Vector2(740f, 44f), new Color(0.55f, 0.88f, 1f, 1f));
            CreateSpriteElement(_world2CompletePanel.transform, "World2CompleteDivider", _stoneDividerSprite, new Vector2(0f, 102f), new Vector2(520f, 24f));

            CreateText(
                _world2CompletePanel.transform,
                "World2CompleteQuote",
                World2CompleteQuote,
                24,
                new Vector2(0f, 48f),
                new Vector2(760f, 64f),
                new Color(0.85f, 1f, 0.92f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_world2CompletePanel.transform, "CONTINUE TO WORLD 3", new Vector2(0f, -32f), () =>
            {
                SetSelectedMapWorld(2);
                TransitionToAction(() => OnWorldCompleteContinueRequested?.Invoke());
            });
            CreateMenuButton(_world2CompletePanel.transform, "LEVEL SELECT", new Vector2(0f, -106f), () =>
            {
                OnLevelSelectRequested?.Invoke();
                TransitionToPanel(ShowLevelSelectScreen);
            });
            CreateMenuButton(_world2CompletePanel.transform, "WORLD SELECT", new Vector2(0f, -180f), () =>
            {
                OnWorldSelectRequested?.Invoke();
                TransitionToPanel(ShowWorldSelectScreen);
            });
            CreateMenuButton(_world2CompletePanel.transform, "MAIN MENU", new Vector2(0f, -254f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            });

            _world2CompletePanel.SetActive(false);
        }

        private void BuildWorld3CompleteScreen(Transform parent)
        {
            _world3CompletePanel = CreateImagePanel(parent, "World3CompletePanel", Vector2.zero, new Vector2(880f, 700f), _modalFrameSprite);
            Image panelImg = _world3CompletePanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.12f, 0.11f, 0.16f, 0.95f);
            }

            // Warm aged bronze and moonlight halo behind the torch
            GameObject haloDeco = CreateSpriteElement(_world3CompletePanel.transform, "World3CompleteHalo", _lightHaloSprite, new Vector2(0f, 268f), new Vector2(160f, 160f));
            Image haloImg = haloDeco.GetComponent<Image>();
            if (haloImg != null)
            {
                haloImg.color = new Color(0.95f, 0.76f, 0.38f, 0.40f);
                haloImg.raycastTarget = false;
            }

            GameObject torchDeco = CreateSpriteElement(_world3CompletePanel.transform, "World3CompleteTorchIcon", AssetRegistry.GetSprite("Torch/torch_item_0", 32f), new Vector2(0f, 268f), new Vector2(68f, 88f));
            torchDeco.GetComponent<Image>().raycastTarget = false;

            CreateStyledTitleText(_world3CompletePanel.transform, "World3CompleteTitle", "WORLD 3 CONQUERED", 46, new Vector2(0f, 192f), new Vector2(760f, 60f), new Color(0.96f, 0.82f, 0.44f, 1f));
            CreateStyledTitleText(_world3CompletePanel.transform, "World3CompleteSub", "THE HOLLOW CLOCKTOWER", 28, new Vector2(0f, 140f), new Vector2(740f, 44f), new Color(0.68f, 0.84f, 1f, 1f));
            CreateSpriteElement(_world3CompletePanel.transform, "World3CompleteDivider", _stoneDividerSprite, new Vector2(0f, 102f), new Vector2(520f, 24f));

            CreateText(
                _world3CompletePanel.transform,
                "World3CompleteQuote",
                World3CompleteQuote,
                24,
                new Vector2(0f, 48f),
                new Vector2(760f, 64f),
                new Color(0.92f, 0.90f, 0.85f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_world3CompletePanel.transform, "CONTINUE TO WORLD 4", new Vector2(0f, -32f), () =>
            {
                SetSelectedMapWorld(3);
                TransitionToAction(() => OnWorldCompleteContinueRequested?.Invoke());
            });
            CreateMenuButton(_world3CompletePanel.transform, "LEVEL SELECT", new Vector2(0f, -106f), () =>
            {
                OnLevelSelectRequested?.Invoke();
                TransitionToPanel(ShowLevelSelectScreen);
            });
            CreateMenuButton(_world3CompletePanel.transform, "WORLD SELECT", new Vector2(0f, -180f), () =>
            {
                OnWorldSelectRequested?.Invoke();
                TransitionToPanel(ShowWorldSelectScreen);
            });
            CreateMenuButton(_world3CompletePanel.transform, "MAIN MENU", new Vector2(0f, -254f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            });

            _world3CompletePanel.SetActive(false);
        }

        private void BuildWorld4CompleteScreen(Transform parent)
        {
            _world4CompletePanel = CreateImagePanel(parent, "World4CompletePanel", Vector2.zero, new Vector2(880f, 700f), _modalFrameSprite);
            Image panelImg = _world4CompletePanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.08f, 0.14f, 0.20f, 0.95f);
            }

            GameObject haloDeco = CreateSpriteElement(_world4CompletePanel.transform, "World4CompleteHalo", _lightHaloSprite, new Vector2(0f, 268f), new Vector2(160f, 160f));
            Image haloImg = haloDeco.GetComponent<Image>();
            if (haloImg != null)
            {
                haloImg.color = new Color(0.35f, 0.85f, 0.95f, 0.40f);
                haloImg.raycastTarget = false;
            }

            GameObject torchDeco = CreateSpriteElement(_world4CompletePanel.transform, "World4CompleteTorchIcon", AssetRegistry.GetSprite("Torch/torch_item_0", 32f), new Vector2(0f, 268f), new Vector2(68f, 88f));
            torchDeco.GetComponent<Image>().raycastTarget = false;

            CreateStyledTitleText(_world4CompletePanel.transform, "World4CompleteTitle", "WORLD 4 CONQUERED", 46, new Vector2(0f, 192f), new Vector2(760f, 60f), new Color(0.35f, 0.95f, 0.98f, 1f));
            CreateStyledTitleText(_world4CompletePanel.transform, "World4CompleteSub", "THE DROWNED CATHEDRAL", 28, new Vector2(0f, 140f), new Vector2(740f, 44f), new Color(0.65f, 0.85f, 1f, 1f));
            CreateSpriteElement(_world4CompletePanel.transform, "World4CompleteDivider", _stoneDividerSprite, new Vector2(0f, 102f), new Vector2(520f, 24f));

            CreateText(
                _world4CompletePanel.transform,
                "World4CompleteQuote",
                World4CompleteQuote,
                24,
                new Vector2(0f, 48f),
                new Vector2(760f, 64f),
                new Color(0.85f, 0.95f, 1f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_world4CompletePanel.transform, "CONTINUE TO WORLD 5", new Vector2(0f, -32f), () =>
            {
                SetSelectedMapWorld(4);
                HideAllMenuPanels();
                TransitionToAction(() => OnWorldCompleteContinueRequested?.Invoke());
            });
            CreateMenuButton(_world4CompletePanel.transform, "LEVEL SELECT", new Vector2(0f, -106f), () =>
            {
                OnLevelSelectRequested?.Invoke();
                TransitionToPanel(ShowLevelSelectScreen);
            });
            CreateMenuButton(_world4CompletePanel.transform, "WORLD SELECT", new Vector2(0f, -180f), () =>
            {
                OnWorldSelectRequested?.Invoke();
                TransitionToPanel(ShowWorldSelectScreen);
            });
            CreateMenuButton(_world4CompletePanel.transform, "MAIN MENU", new Vector2(0f, -254f), () =>
            {
                TransitionToAction(() => OnMainMenuRequested?.Invoke());
            });

            _world4CompletePanel.SetActive(false);
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

            if (IsShopScreenVisible)
            {
                UpdateShopPreviewAnimations(t);
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
                float haloAlpha = 0.30f + 0.05f * Mathf.Sin(t * 6.5f);
                _homeTorchHaloOuter.color = new Color(_homeTorchHaloBaseColor.r, _homeTorchHaloBaseColor.g, _homeTorchHaloBaseColor.b, haloAlpha * _homeTorchHaloBaseColor.a);
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

        private void UpdateShopPreviewAnimations(float t)
        {
            if (_shopPreviewImage == null) return;

            bool isTorch = (_shopSelectedTab == 2);
            bool isShadow = (_shopSelectedTab == 1);

            if (isTorch)
            {
                // Torch: Flame flicker animation in place
                if (_shopCachedPreviewFrames != null && _shopCachedPreviewFrames.Length > 0)
                {
                    int frame = Mathf.FloorToInt(t * 10f) % _shopCachedPreviewFrames.Length;
                    if (_shopCachedPreviewFrames[frame] != null) _shopPreviewImage.sprite = _shopCachedPreviewFrames[frame];
                }
                _shopPreviewImage.rectTransform.anchoredPosition = new Vector2(0f, 65f);
                _shopPreviewImage.rectTransform.localScale = Vector3.one;
                if (_shopPreviewContactShadow != null)
                {
                    _shopPreviewContactShadow.rectTransform.localScale = Vector3.one;
                    _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, 0.45f);
                }
            }
            else if (_shopPreviewMode == 0)
            {
                // IDLE: Soft gentle breathing oscillation
                if (_shopCachedPreviewFrames != null && _shopCachedPreviewFrames.Length > 0)
                {
                    int frame = Mathf.FloorToInt(t * 5.5f) % _shopCachedPreviewFrames.Length;
                    if (_shopCachedPreviewFrames[frame] != null) _shopPreviewImage.sprite = _shopCachedPreviewFrames[frame];
                }
                float breathe = Mathf.Sin(t * 3.5f) * 1.5f;
                _shopPreviewImage.rectTransform.anchoredPosition = new Vector2(0f, 65f + breathe);
                _shopPreviewImage.rectTransform.localScale = new Vector3(1f + breathe * 0.015f, 1f - breathe * 0.015f, 1f);
                if (_shopPreviewContactShadow != null)
                {
                    _shopPreviewContactShadow.rectTransform.localScale = new Vector3(1f - breathe * 0.02f, 1f, 1f);
                    _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, 0.45f);
                }
            }
            else if (_shopPreviewMode == 1)
            {
                // MOVE: Step rhythm bobbing and running frames
                if (_shopCachedPreviewFrames != null && _shopCachedPreviewFrames.Length > 0)
                {
                    int frame = Mathf.FloorToInt(t * 8f) % _shopCachedPreviewFrames.Length;
                    if (_shopCachedPreviewFrames[frame] != null) _shopPreviewImage.sprite = _shopCachedPreviewFrames[frame];
                }
                float runBob = Mathf.Abs(Mathf.Sin(t * 10f)) * 4.5f;
                _shopPreviewImage.rectTransform.anchoredPosition = new Vector2(0f, 65f + runBob);
                _shopPreviewImage.rectTransform.localScale = Vector3.one;
                if (_shopPreviewContactShadow != null)
                {
                    float shadowScale = 1f - (runBob / 4.5f) * 0.12f;
                    _shopPreviewContactShadow.rectTransform.localScale = new Vector3(shadowScale, shadowScale, 1f);
                    _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, 0.45f);
                }
            }
            else // _shopPreviewMode == 2: JUMP
            {
                _shopJumpTimer += Time.unscaledDeltaTime;

                float height = 0f;
                Vector3 scale = Vector3.one;
                int frameIdx = 0;

                if (!isShadow)
                {
                    // Protagonist Jump trajectory (1.4s cycle)
                    const float cycle = 1.4f;
                    float prog = _shopJumpTimer % cycle;

                    if (prog < 0.12f)
                    {
                        // Anticipation crouch
                        height = 0f;
                        scale = new Vector3(1.10f, 0.88f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 0.62f)
                    {
                        // Upward leap
                        float p = (prog - 0.12f) / 0.50f;
                        height = Mathf.Sin(p * Mathf.PI * 0.5f) * 82f;
                        scale = new Vector3(0.92f, 1.08f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 0.74f)
                    {
                        // Apex hang
                        height = 82f;
                        scale = Vector3.one;
                        frameIdx = 1;
                    }
                    else if (prog < 1.18f)
                    {
                        // Descent
                        float p = (prog - 0.74f) / 0.44f;
                        height = (1f - (p * p)) * 82f;
                        scale = new Vector3(0.95f, 1.05f, 1f);
                        frameIdx = 1;
                    }
                    else if (prog < 1.30f)
                    {
                        // Landing impact
                        height = 0f;
                        scale = new Vector3(1.14f, 0.86f, 1f);
                        frameIdx = 0;
                    }
                    else
                    {
                        // Recovery
                        height = 0f;
                        scale = Vector3.one;
                        frameIdx = 0;
                    }
                }
                else
                {
                    // Shadow Double Jump trajectory (2.0s cycle)
                    const float cycle = 2.0f;
                    float prog = _shopJumpTimer % cycle;

                    if (prog < 0.10f)
                    {
                        // Anticipation
                        height = 0f;
                        scale = new Vector3(1.10f, 0.88f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 0.50f)
                    {
                        // First jump ascent
                        float p = (prog - 0.10f) / 0.40f;
                        height = Mathf.Sin(p * Mathf.PI * 0.5f) * 50f;
                        scale = new Vector3(0.92f, 1.08f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 0.62f)
                    {
                        // First apex
                        height = 50f;
                        scale = Vector3.one;
                        frameIdx = 1;
                    }
                    else if (prog < 0.72f)
                    {
                        // Mid-air double jump impulse pop!
                        height = 50f;
                        scale = new Vector3(1.20f, 0.82f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 1.18f)
                    {
                        // Second jump ascent to high peak
                        float p = (prog - 0.72f) / 0.46f;
                        height = 50f + Mathf.Sin(p * Mathf.PI * 0.5f) * 54f; // 104f peak
                        scale = new Vector3(0.90f, 1.10f, 1f);
                        frameIdx = 0;
                    }
                    else if (prog < 1.35f)
                    {
                        // High floaty apex
                        height = 104f;
                        scale = Vector3.one;
                        frameIdx = 1;
                    }
                    else if (prog < 1.80f)
                    {
                        // Floaty low-gravity descent
                        float p = (prog - 1.35f) / 0.45f;
                        height = (1f - (p * p)) * 104f;
                        scale = new Vector3(0.95f, 1.05f, 1f);
                        frameIdx = 1;
                    }
                    else if (prog < 1.92f)
                    {
                        // Soft landing
                        height = 0f;
                        scale = new Vector3(1.10f, 0.90f, 1f);
                        frameIdx = 0;
                    }
                    else
                    {
                        height = 0f;
                        scale = Vector3.one;
                        frameIdx = 0;
                    }
                }

                _shopPreviewImage.rectTransform.anchoredPosition = new Vector2(0f, 65f + height);
                _shopPreviewImage.rectTransform.localScale = scale;

                if (_shopCachedPreviewFrames != null && _shopCachedPreviewFrames.Length > 0)
                {
                    int clampedFrame = Mathf.Clamp(frameIdx, 0, _shopCachedPreviewFrames.Length - 1);
                    if (_shopCachedPreviewFrames[clampedFrame] != null)
                    {
                        _shopPreviewImage.sprite = _shopCachedPreviewFrames[clampedFrame];
                    }
                }

                if (_shopPreviewContactShadow != null)
                {
                    float shadowFactor = Mathf.Clamp01(height / 100f);
                    float shadowScale = Mathf.Lerp(1.0f, 0.52f, shadowFactor);
                    _shopPreviewContactShadow.rectTransform.localScale = new Vector3(shadowScale, shadowScale, 1f);
                    _shopPreviewContactShadow.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.48f, 0.12f, shadowFactor));
                }
            }

            if (_shopPreviewHaloImage != null)
            {
                float haloFlicker = 1f + 0.08f * Mathf.Sin(t * 6.8f);
                _shopPreviewHaloImage.rectTransform.localScale = new Vector3(haloFlicker, haloFlicker, 1f);
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

            if (_currentScreen == ActiveMenuScreen.Settings || _currentScreen == ActiveMenuScreen.Credits || _currentScreen == ActiveMenuScreen.LoreJournal)
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
            else if (_currentScreen == ActiveMenuScreen.Shop)
            {
                if (GameInput.GetMenuLeftDown())
                {
                    int prevTab = (_shopSelectedTab - 1 + 3) % 3;
                    SwitchShopCategory(prevTab);
                }
                else if (GameInput.GetMenuRightDown())
                {
                    int nextTab = (_shopSelectedTab + 1) % 3;
                    SwitchShopCategory(nextTab);
                }
                else if (GameInput.GetMenuUpDown())
                {
                    int prevSkin = Mathf.Max(0, _shopSelectedSkinIndex - 1);
                    if (prevSkin != _shopSelectedSkinIndex)
                    {
                        SelectShopSkin(prevSkin);
                    }
                }
                else if (GameInput.GetMenuDownDown())
                {
                    int nextSkin = Mathf.Min(4, _shopSelectedSkinIndex + 1);
                    if (nextSkin != _shopSelectedSkinIndex)
                    {
                        SelectShopSkin(nextSkin);
                    }
                }
                else if (GameInput.GetConfirmDown())
                {
                    ExecuteShopActionButton();
                }
                else if (GameInput.GetCancelDown())
                {
                    PlayMenuBackSound();
                    TransitionToPanel(ShowMainMenu);
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
            bool hasSave = WorldProgressSave.HasAnySavedProgress();
            WorldProgressSave.GetGlobalContinuePoint(out string worldId, out int worldIndex, out int levelIndex, out int displayLevelNumber);

            if (_homeContinueButton != null)
            {
                SetButtonVisualDisabled(_homeContinueButton, !hasSave);
                if (_homeContinueBtnLabel != null)
                {
                    _homeContinueBtnLabel.resizeTextForBestFit = true;
                    _homeContinueBtnLabel.resizeTextMinSize = 14;
                    _homeContinueBtnLabel.resizeTextMaxSize = 22;
                    _homeContinueBtnLabel.text = hasSave
                        ? $"CONTINUE (W{worldIndex + 1} · LVL {displayLevelNumber:D2})"
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
            if (_loreJournalPanel != null) _loreJournalPanel.SetActive(false);
            if (_worldCompletePanel != null) _worldCompletePanel.SetActive(false);
            if (_world2CompletePanel != null) _world2CompletePanel.SetActive(false);
            if (_world3CompletePanel != null) _world3CompletePanel.SetActive(false);
            if (_world4CompletePanel != null) _world4CompletePanel.SetActive(false);
            if (_world2ComingSoonPanel != null) _world2ComingSoonPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_hudPanel != null) _hudPanel.SetActive(false);
            if (_shopPanel != null) _shopPanel.SetActive(false);

            if (_mobileControlsUI != null)
            {
                _mobileControlsUI.SetGameplayActive(false);
                _mobileControlsUI.ReleaseAll();
            }
        }

        public void RefreshHomeEquippedSkins()
        {
            string playerSkinId = "player_original";
            string shadowSkinId = "shadow_original";
            string torchSkinId = "torch_original_flame";

            if (SkinEquipmentService.Instance != null)
            {
                playerSkinId = SkinEquipmentService.Instance.EquippedPlayerSkinId;
                shadowSkinId = SkinEquipmentService.Instance.EquippedShadowSkinId;
                torchSkinId = SkinEquipmentService.Instance.EquippedTorchSkinId;
            }
            else
            {
                playerSkinId = PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedPlayer, "player_original");
                shadowSkinId = PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedShadow, "shadow_original");
                torchSkinId = PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedTorch, "torch_original_flame");
            }

            // 1. Protagonist idle frames
            bool isOrigPlayer = (playerSkinId == "player_original");
            string pPrefix = isOrigPlayer ? "Characters/protagonist" : $"Skins/Player/{playerSkinId}";
            string pSep = isOrigPlayer ? "_" : "/";
            _protagonistIdleFrames = AssetRegistry.GetAnimationFrames(pPrefix + pSep + "idle", 4);
            if (_homeProtagonistImage != null && _protagonistIdleFrames != null && _protagonistIdleFrames.Length > 0)
            {
                _homeProtagonistImage.sprite = _protagonistIdleFrames[0];
            }

            // 2. Shadow idle frames
            bool isOrigShadow = (shadowSkinId == "shadow_original");
            string sPrefix = isOrigShadow ? "Characters/shadow" : $"Skins/Shadow/{shadowSkinId}";
            string sSep = isOrigShadow ? "_" : "/";
            _shadowIdleFrames = AssetRegistry.GetAnimationFrames(sPrefix + sSep + "idle", 4);
            if (_homeShadowImage != null && _shadowIdleFrames != null && _shadowIdleFrames.Length > 0)
            {
                _homeShadowImage.sprite = _shadowIdleFrames[0];
            }

            // 3. Torch flame frames & halo colors
            bool isOrigTorch = (torchSkinId == "torch_original_flame");
            string tPrefix = isOrigTorch ? "Torch/torch_item" : $"Skins/Torch/{torchSkinId}/flame";
            _torchFlameFrames = AssetRegistry.GetAnimationFrames(tPrefix, 4);
            if (_homeTorchFlameImage != null && _torchFlameFrames != null && _torchFlameFrames.Length > 0)
            {
                _homeTorchFlameImage.sprite = _torchFlameFrames[0];
            }

            SkinDefinition torchDef = SkinCatalog.GetSkin(torchSkinId);
            if (torchDef != null)
            {
                _homeTorchHaloBaseColor = torchDef.HaloColor;
                if (_homeTorchHaloInner != null)
                {
                    _homeTorchHaloInner.color = new Color(
                        Mathf.Clamp01(torchDef.HaloColor.r * 1.2f),
                        Mathf.Clamp01(torchDef.HaloColor.g * 1.2f),
                        Mathf.Clamp01(torchDef.HaloColor.b * 1.2f),
                        0.55f);
                }
            }
        }

        public void ShowMainMenu()
        {
            _targetFadeAlpha = 0f;
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            RefreshHomeEquippedSkins();
            _mainMenuPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.Home;
            _homeSelectedBtnIndex = WorldProgressSave.HasAnySavedProgress() ? 0 : 1;
            RefreshHomeButtonSelectionVisuals();
            if (Application.isPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        public void ShowWorldSelectScreen()
        {
            _selectedWorldCardIndex = 0;
            RefreshWorldSelectState();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _worldSelectPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.WorldSelect;
            if (Application.isPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        public void ShowLevelSelectScreen()
        {
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _levelSelectPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.LevelSelect;
            if (Application.isPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
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

        public void ShowWorld2CompleteScreen()
        {
            RefreshWorldSelectState();
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _world2CompletePanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World2Complete;
        }

        public void ShowWorld3CompleteScreen()
        {
            RefreshWorldSelectState();
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _world3CompletePanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World3Complete;
        }

        public void ShowWorld4CompleteScreen()
        {
            RefreshWorldSelectState();
            RefreshLevelSelectMap();
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _world4CompletePanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World4Complete;
        }

        public void ShowWorld2ComingSoonScreen()
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _world2ComingSoonPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.World2ComingSoon;
            if (Application.isPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        public void ShowShopScreen()
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _shopPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.Shop;
            if (Application.isPlaying && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }

            RefreshShopCoinBalance();
            SwitchShopCategory(_shopSelectedTab);
        }

        public void ShowTemporaryRewardNotification(string message)
        {
            if (_rewardToastBanner == null || _rewardToastText == null) return;
            if (_rewardToastCoroutine != null) StopCoroutine(_rewardToastCoroutine);
            _rewardToastCoroutine = StartCoroutine(RewardToastCoroutine(message));
        }

        private IEnumerator RewardToastCoroutine(string message)
        {
            _rewardToastText.text = message;
            _rewardToastBanner.SetActive(true);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.8f, 1.4f);
            }

            yield return new WaitForSecondsRealtime(2.8f);

            _rewardToastBanner.SetActive(false);
            _rewardToastCoroutine = null;
        }

        public void ShowGameplayHUD(string levelTitle, string hint, bool isControllingShadow)
        {
            SetEndingQuoteAlpha(0f);
            HideAllMenuPanels();
            _hudPanel.SetActive(true);
            _currentScreen = ActiveMenuScreen.None;

            UpdateHUDText(levelTitle, hint, isControllingShadow);
            if (_mobileControlsUI != null)
            {
                _mobileControlsUI.SetGameplayActive(true);
            }
        }

        public void UpdateHUDText(string levelTitle, string hint, bool isControllingShadow)
        {
            if (_levelTitleText != null) _levelTitleText.text = levelTitle;
            if (_hintText != null) _hintText.text = hint;
            if (_activeControlText != null)
            {
                _activeControlText.text = isControllingShadow
                    ? "ACTIVE: SHADOW [TAB / SWITCH]"
                    : "ACTIVE: PROTAGONIST [TAB / SWITCH]";
                _activeControlText.color = isControllingShadow
                    ? new Color(0.76f, 0.56f, 1f, 1f)
                    : new Color(1f, 0.82f, 0.38f, 1f);
            }

            // Update Hearts
            RefreshHeartsHUD();

            // Update Weapon Icon
            RefreshWeaponHUD();
        }

        public void RefreshHeartsHUD()
        {
            if (_heartIcons != null && Combat.HealthSystem.Instance != null)
            {
                int maxHp = Combat.HealthSystem.Instance.MaxHealth;
                int curHp = Combat.HealthSystem.Instance.CurrentHealth;
                Sprite hFull = Resources.Load<Sprite>("Sprites/UI/heart_full");
                Sprite hEmpty = Resources.Load<Sprite>("Sprites/UI/heart_empty");

                for (int i = 0; i < _heartIcons.Length; i++)
                {
                    if (i < maxHp)
                    {
                        _heartIcons[i].gameObject.SetActive(true);
                        _heartIcons[i].sprite = (i < curHp) ? hFull : hEmpty;
                    }
                    else
                    {
                        _heartIcons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void RefreshWeaponHUD()
        {
            // Weapon sprites completely removed from UI per specification
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
                if (_hudPanel != null && _hudPanel.activeSelf && _mobileControlsUI != null)
                {
                    _mobileControlsUI.SetGameplayActive(true);
                }
            }
            else
            {
                _currentScreen = ActiveMenuScreen.Pause;
                _pauseSelectedBtnIndex = 0;
                RefreshPauseButtonSelectionVisuals();
                if (_mobileControlsUI != null)
                {
                    _mobileControlsUI.SetGameplayActive(false);
                    _mobileControlsUI.ReleaseAll();
                }
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
            if (_loreJournalPanel != null) _loreJournalPanel.SetActive(panel == _loreJournalPanel);

            if (panel == _settingsPanel) _currentScreen = ActiveMenuScreen.Settings;
            else if (panel == _creditsPanel) _currentScreen = ActiveMenuScreen.Credits;
            else if (panel == _loreJournalPanel) _currentScreen = ActiveMenuScreen.LoreJournal;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.60f, 1.1f);
            }
        }

        private void CloseSubPanelAndReturn()
        {
            PlayMenuBackSound();
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
            if (_loreJournalPanel != null) _loreJournalPanel.SetActive(false);
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
                RefreshHomeEquippedSkins();
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
