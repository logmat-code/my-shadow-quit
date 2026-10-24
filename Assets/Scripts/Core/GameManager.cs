using System.Collections;
using UnityEngine;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.UI;
using MyShadowQuit.Cosmetics;
using MyShadowQuit.Economy;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Central orchestrator for MY SHADOW QUIT: THE LAST FLAME.
    /// Auto-bootstraps when any scene loads so the game is immediately playable in Editor or WebGL build.
    /// Manages Main Menu, World Select, World 1 ("The Ruins of the Last Flame", Levels 1–10),
    /// TutorialDirector contextual progression, character switching (TAB), level restart (R),
    /// pause menu (ESC), camera target tracking, the World 1 Complete transition,
    /// and the preserved reusable Final Game Ending sequence for future worlds.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private bool startInMainMenu = true;

        private LevelManager _levelManager;
        private UIManager _uiManager;
        private CameraController _cameraController;
        private AudioManager _audioManager;
        private TutorialDirector _tutorialDirector;

        private int _currentLevelIndex;
        private bool _isControllingShadow;
        private bool _isPaused;
        private bool _isInMainMenu;
        private bool _isEndingSequenceActive;
        private bool _isWorld1Completed;
        private bool _isGameCompleted;
        private bool _hasRestartedCurrentLevel;
        private Coroutine _endingCoroutine;

        public int CurrentLevelIndex => _currentLevelIndex;
        public bool IsControllingShadow => _isControllingShadow;
        public bool IsPaused => _isPaused;
        public bool IsInMainMenu => _isInMainMenu;
        public bool IsEndingSequenceActive => _isEndingSequenceActive;
        public bool IsWorld1Completed => _isWorld1Completed;
        public bool IsGameCompleted => _isGameCompleted;
        public LevelManager Levels => _levelManager;
        public TutorialDirector Tutorial => _tutorialDirector;
        public UIManager UI => _uiManager;
        public string ActiveWorldId => _levelManager != null && _levelManager.ActiveWorld != null
            ? _levelManager.ActiveWorld.WorldId
            : "world_1";
        public int CurrentWorldIndex => _levelManager != null && _levelManager.ActiveWorld != null
            ? _levelManager.ActiveWorld.WorldIndex
            : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (Instance == null && FindFirstObjectByType<GameManager>() == null)
            {
                GameObject root = new GameObject("GameManager_Root");
                root.AddComponent<GameManager>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupSystems();
        }

        private void SetupSystems()
        {
            // Clean up any leftover EditMode VerificationRunner objects if present in the scene
            LevelManager[] allLevelManagers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
            foreach (LevelManager lm in allLevelManagers)
            {
                if (lm != null && lm.gameObject != gameObject)
                {
                    lm.ClearCurrentLevel();
                    lm.gameObject.SetActive(false);
                    Destroy(lm.gameObject);
                }
            }

            _audioManager = GetComponent<AudioManager>();
            if (_audioManager == null)
            {
                _audioManager = FindFirstObjectByType<AudioManager>();
            }
            if (_audioManager == null)
            {
                _audioManager = gameObject.AddComponent<AudioManager>();
            }

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }
            _cameraController = mainCam.GetComponent<CameraController>();
            if (_cameraController == null)
            {
                _cameraController = mainCam.gameObject.AddComponent<CameraController>();
            }

            _levelManager = GetComponent<LevelManager>();
            if (_levelManager == null)
            {
                _levelManager = gameObject.AddComponent<LevelManager>();
            }

            _uiManager = GetComponent<UIManager>();
            if (_uiManager == null)
            {
                _uiManager = FindFirstObjectByType<UIManager>();
            }
            if (_uiManager == null)
            {
                _uiManager = gameObject.AddComponent<UIManager>();
            }

            _tutorialDirector = GetComponent<TutorialDirector>();
            if (_tutorialDirector == null)
            {
                _tutorialDirector = gameObject.AddComponent<TutorialDirector>();
            }

            _uiManager.OnPlayRequested -= StartNewGame;
            _uiManager.OnPlayRequested += StartNewGame;
            _uiManager.OnContinueRequested -= ContinueSavedGame;
            _uiManager.OnContinueRequested += ContinueSavedGame;
            _uiManager.OnNewGameRequested -= StartNewGameWithReset;
            _uiManager.OnNewGameRequested += StartNewGameWithReset;
            _uiManager.OnWorldSelectRequested -= OpenWorldSelect;
            _uiManager.OnWorldSelectRequested += OpenWorldSelect;
            _uiManager.OnLevelSelectRequested -= OpenLevelSelect;
            _uiManager.OnLevelSelectRequested += OpenLevelSelect;
            _uiManager.OnStartWorldRequested -= SelectWorldAndStart;
            _uiManager.OnStartWorldRequested += SelectWorldAndStart;
            _uiManager.OnLevelSelectedFromMap -= StartLevelFromMap;
            _uiManager.OnLevelSelectedFromMap += StartLevelFromMap;
            _uiManager.OnWorldCompleteContinueRequested -= ContinueToWorld2FromWorldComplete;
            _uiManager.OnWorldCompleteContinueRequested += ContinueToWorld2FromWorldComplete;
            _uiManager.OnPauseRequested -= TogglePause;
            _uiManager.OnPauseRequested += TogglePause;
            _uiManager.OnResumeRequested -= TogglePause;
            _uiManager.OnResumeRequested += TogglePause;
            _uiManager.OnRestartRequested -= RestartCurrentLevel;
            _uiManager.OnRestartRequested += RestartCurrentLevel;
            _uiManager.OnMainMenuRequested -= OpenMainMenu;
            _uiManager.OnMainMenuRequested += OpenMainMenu;
            _uiManager.OnShopRequested -= OpenShop;
            _uiManager.OnShopRequested += OpenShop;

            if (GetComponent<WalletService>() == null) gameObject.AddComponent<WalletService>();
            if (GetComponent<InventoryService>() == null) gameObject.AddComponent<InventoryService>();
            if (GetComponent<RewardService>() == null) gameObject.AddComponent<RewardService>();
            if (GetComponent<SkinEquipmentService>() == null) gameObject.AddComponent<SkinEquipmentService>();
            if (GetComponent<ShopTransactionService>() == null) gameObject.AddComponent<ShopTransactionService>();

            LevelExitPortal.OnLevelExitReached -= HandleLevelCompleted;
            LevelExitPortal.OnLevelExitReached += HandleLevelCompleted;
        }

        private void Start()
        {
            if (startInMainMenu)
            {
                OpenMainMenu();
            }
            else
            {
                LoadLevel(0);
            }
        }

        private void OnDestroy()
        {
            LevelExitPortal.OnLevelExitReached -= HandleLevelCompleted;
            if (_uiManager != null)
            {
                _uiManager.OnShopRequested -= OpenShop;
            }
        }

        private void Update()
        {
            if (_isInMainMenu || _isEndingSequenceActive) return;

            if (GameInput.GetPauseDown())
            {
                TogglePause();
                return;
            }

            if (_isPaused) return;

            if (GameInput.GetRestartDown())
            {
                RestartCurrentLevel();
                return;
            }

            if (GameInput.GetSwitchCharacterDown())
            {
                SwitchActiveCharacter();
            }
        }

        private void StopEndingSequenceIfNeeded()
        {
            if (_endingCoroutine != null)
            {
                StopCoroutine(_endingCoroutine);
                _endingCoroutine = null;
            }
            _isEndingSequenceActive = false;
            if (_cameraController != null)
            {
                _cameraController.ResetZoom();
            }
            if (_audioManager != null)
            {
                _audioManager.RestoreNormalMix();
            }
        }

        public void OpenMainMenu()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = true;
            _isPaused = false;
            _isWorld1Completed = false;
            _isGameCompleted = false;
            Time.timeScale = 1f;
            if (_audioManager != null)
            {
                _audioManager.SetPauseDucking(false);
                _audioManager.PlayMenuMusic();
            }
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
            LoadLevel(0);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            _uiManager.ShowMainMenu();
        }

        public void OpenWorldSelect()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_audioManager != null)
            {
                _audioManager.SetPauseDucking(false);
                _audioManager.PlayMenuMusic();
            }
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            if (_uiManager != null)
            {
                _uiManager.ShowWorldSelectScreen();
            }
        }

        public void OpenLevelSelect()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_audioManager != null)
            {
                _audioManager.SetPauseDucking(false);
                _audioManager.PlayMenuMusic();
            }
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            if (_uiManager != null)
            {
                _uiManager.ShowLevelSelectScreen();
            }
        }

        public void OpenWorld2ComingSoon()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_audioManager != null) _audioManager.PlayMenuMusic();
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            if (_uiManager != null)
            {
                _uiManager.ShowWorld2ComingSoonScreen();
            }
        }

        public void OpenShop()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_audioManager != null)
            {
                _audioManager.SetPauseDucking(false);
                _audioManager.PlayMenuMusic();
            }
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            if (_uiManager != null)
            {
                _uiManager.ShowShopScreen();
            }
        }

        public void ContinueSavedGame()
        {
            WorldProgressSave.GetGlobalContinuePoint(out string worldId, out int worldIndex, out int levelIndex, out int displayLevelNumber);
            SelectWorldAndStart(worldIndex, continueSavedProgress: true);
        }

        public void ContinueToWorld2FromWorldComplete()
        {
            StopEndingSequenceIfNeeded();
            int nextWorldIdx = 1;
            if (ActiveWorldId == "world_2")
            {
                nextWorldIdx = 2;
            }
            SelectWorldAndStart(nextWorldIdx, continueSavedProgress: true);
        }

        public void StartNewGameWithReset()
        {
            WorldProgressSave.ResetWorldProgress("world_1");
            SelectWorldAndStart(0, continueSavedProgress: false);
        }

        public void StartLevelFromMap(int levelIndex)
        {
            int worldIdx = _levelManager != null && _levelManager.ActiveWorld != null
                ? _levelManager.ActiveWorld.WorldIndex
                : 0;
            StartLevelFromMap(worldIdx, levelIndex);
        }

        public void StartLevelFromMap(int worldIndex, int levelIndex)
        {
            WorldDefinition world = WorldCatalog.GetWorld(worldIndex);
            if (world == null || !world.IsImplemented || !world.IsUnlocked)
            {
                return;
            }
            int total = world.LevelCount;
            if (!WorldProgressSave.IsLevelUnlocked(world.WorldId, levelIndex, total))
            {
                return;
            }

            WorldProgressSave.SetLastPlayedWorld(world.WorldId);

            StopEndingSequenceIfNeeded();
            _levelManager.SetActiveWorld(world);
            _isInMainMenu = false;
            _isPaused = false;
            _isWorld1Completed = false;
            _isGameCompleted = false;
            Time.timeScale = 1f;

            LoadLevel(levelIndex);
        }

        public void SelectWorldAndStart(int worldIndex, bool continueSavedProgress)
        {
            WorldDefinition world = WorldCatalog.GetWorld(worldIndex);
            if (world == null || !world.IsImplemented || !world.IsUnlocked)
            {
                OpenWorld2ComingSoon();
                return;
            }

            WorldProgressSave.SetLastPlayedWorld(world.WorldId);

            StopEndingSequenceIfNeeded();
            _levelManager.SetActiveWorld(world);
            _isInMainMenu = false;
            _isPaused = false;
            _isWorld1Completed = false;
            _isGameCompleted = false;
            Time.timeScale = 1f;

            int startLevel = 0;
            if (continueSavedProgress && WorldProgressSave.HasSavedProgress(world.WorldId))
            {
                startLevel = WorldProgressSave.GetContinueLevelIndex(world.WorldId, world.LevelCount);
            }

            LoadLevel(startLevel);
        }

        public void StartNewGame()
        {
            SelectWorldAndStart(0, false);
        }

        public void LoadLevel(int index, bool isRestart = false)
        {
            StopEndingSequenceIfNeeded();
            _currentLevelIndex = Mathf.Clamp(index, 0, _levelManager.TotalLevels - 1);
            _isControllingShadow = false;
            _isPaused = false;
            Time.timeScale = 1f;
            GameInput.ResetTouchInputs();

            if (!isRestart)
            {
                _hasRestartedCurrentLevel = false;
            }

            if (!_isInMainMenu)
            {
                WorldProgressSave.SaveLevelProgress(ActiveWorldId, _currentLevelIndex, _levelManager.TotalLevels);
            }

            _levelManager.BuildLevel(_currentLevelIndex, _cameraController);
            if (_cameraController != null)
            {
                _cameraController.ResetZoom();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.ResetEndingGlow();
            }
            ApplyActiveCharacterControl();

            if (_audioManager != null)
            {
                if (_isInMainMenu)
                {
                    _audioManager.PlayMenuMusic();
                }
                else
                {
                    int wIdx = (_levelManager != null && _levelManager.ActiveWorld != null) ? _levelManager.ActiveWorld.WorldIndex : 0;
                    _audioManager.PlayGameplayMusic(wIdx);
                }
            }

            if (!_isInMainMenu && _uiManager != null)
            {
                _uiManager.TriggerFlashFade();
                _uiManager.ShowGameplayHUD(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    _levelManager.GetLevelHint(_currentLevelIndex),
                    _isControllingShadow
                );
            }

            if (_tutorialDirector != null)
            {
                _tutorialDirector.InitializeForLevel(_currentLevelIndex, _levelManager, _uiManager);
            }
        }

        public void RestartCurrentLevel()
        {
            _hasRestartedCurrentLevel = true;
            _isPaused = false;
            Time.timeScale = 1f;
            if (_audioManager != null) _audioManager.SetPauseDucking(false);
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
            LoadLevel(_currentLevelIndex, isRestart: true);
        }

        public void SwitchActiveCharacter()
        {
            if (_isEndingSequenceActive || _levelManager.Player == null || _levelManager.Shadow == null) return;

            _isControllingShadow = !_isControllingShadow;
            ApplyActiveCharacterControl();

            if (_cameraController != null)
            {
                _cameraController.TriggerShake(0.12f, 0.05f);
            }

            if (_audioManager != null)
            {
                _audioManager.PlaySFX("shadow_shift", 0.8f, _isControllingShadow ? 0.95f : 1.12f);
            }

            if (_tutorialDirector != null && _currentLevelIndex == 0)
            {
                _tutorialDirector.EvaluateAwakeningProgression(forceUpdate: true);
            }
            else if (_uiManager != null)
            {
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    _levelManager.GetLevelHint(_currentLevelIndex),
                    _isControllingShadow
                );
            }
        }

        private void ApplyActiveCharacterControl()
        {
            if (_levelManager.Player != null)
            {
                _levelManager.Player.SetControlActive(!_isControllingShadow);
            }
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.SetControlActive(_isControllingShadow);
            }

            if (_cameraController != null)
            {
                Transform target = _isControllingShadow && _levelManager.Shadow != null
                    ? _levelManager.Shadow.transform
                    : (_levelManager.Player != null ? _levelManager.Player.transform : null);
                _cameraController.SetTarget(target);
            }
        }

        public void TogglePause()
        {
            if (_isInMainMenu || _isEndingSequenceActive) return;
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            if (_isInMainMenu || _isEndingSequenceActive || _isPaused) return;
            _isPaused = true;
            Time.timeScale = 0f;
            GameInput.ResetTouchInputs();
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            if (_audioManager != null) _audioManager.SetPauseDucking(true);
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(true);
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = 1f;
            GameInput.ResetTouchInputs();
            ApplyActiveCharacterControl();
            if (_audioManager != null) _audioManager.SetPauseDucking(false);
            if (_uiManager != null) _uiManager.SetPauseMenuVisible(false);
        }

        private void HandleLevelCompleted()
        {
            WorldProgressSave.MarkLevelCompleted(ActiveWorldId, _currentLevelIndex, _levelManager.TotalLevels);

            if (RewardService.Instance != null)
            {
                int earned = RewardService.Instance.EvaluateAndClaimLevelRewards(
                    ActiveWorldId,
                    _currentLevelIndex,
                    !_hasRestartedCurrentLevel,
                    out string summary
                );

                if (earned > 0 && _uiManager != null)
                {
                    _uiManager.ShowTemporaryRewardNotification(summary);
                }
            }

            if (_currentLevelIndex + 1 < _levelManager.TotalLevels)
            {
                LoadLevel(_currentLevelIndex + 1);
            }
            else
            {
                if (ActiveWorldId == "world_5")
                {
                    BeginGameEndingSequence();
                }
                else if (ActiveWorldId == "world_4")
                {
                    BeginWorld4CompleteSequence();
                }
                else if (ActiveWorldId == "world_3")
                {
                    BeginWorld3CompleteSequence();
                }
                else if (ActiveWorldId == "world_2")
                {
                    BeginWorld2CompleteSequence();
                }
                else
                {
                    BeginWorld1CompleteSequence();
                }
            }
        }

        /// <summary>
        /// Triggers the World 1 Complete narrative sequence at the end of Level 10 ("The First Dawn").
        /// Camera zooms in smoothly, Torch illuminates the Dawn Portal, ruin mechanisms resonate,
        /// an unknown light expands beyond the Ceremonial Gate, both characters step toward the light,
        /// and the World 1 Complete screen opens.
        /// </summary>
        public void BeginWorld1CompleteSequence()
        {
            if (_isEndingSequenceActive || _isWorld1Completed) return;

            _isEndingSequenceActive = true;
            _isWorld1Completed = true;
            _isControllingShadow = false;

            WorldProgressSave.MarkWorldCompleted(ActiveWorldId, _levelManager.TotalLevels);

            if (RewardService.Instance != null)
            {
                int worldEarned = RewardService.Instance.EvaluateAndClaimWorldClearReward(ActiveWorldId, out string worldSummary);
                if (worldEarned > 0 && _uiManager != null)
                {
                    _uiManager.ShowTemporaryRewardNotification(worldSummary);
                }
            }

            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);

            LevelExitPortal exitPortal = FindFirstObjectByType<LevelExitPortal>();
            if (_cameraController != null)
            {
                Transform focus = exitPortal != null
                    ? exitPortal.transform
                    : (_levelManager.Player != null ? _levelManager.Player.transform : null);
                _cameraController.SetTarget(focus);
                _cameraController.SetTargetZoom(3.35f, 0.95f);
                _cameraController.TriggerShake(0.35f, 0.07f);
            }

            if (_audioManager != null)
            {
                _audioManager.PlaySFX("stone_door", 0.72f, 0.88f);
            }

            if (_levelManager.Torch != null && _levelManager.Player != null)
            {
                if (!_levelManager.Player.HasTorch)
                {
                    _levelManager.Player.ForceEquipTorch(_levelManager.Torch);
                }
                _levelManager.Torch.SetEndingGlow(1.45f, 1.35f);
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World1CompleteQuote);
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    UIManager.World1CompleteQuote,
                    false
                );
            }

            if (Application.isPlaying)
            {
                _endingCoroutine = StartCoroutine(PlayWorld1CompleteSequenceCoroutine(exitPortal));
            }
            else
            {
                CompleteWorld1SequenceImmediate();
            }
        }

        public void CompleteWorld1SequenceImmediate()
        {
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.ReconnectToProtagonist();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.SetEndingGlow(1.35f, 1.25f);
            }
            if (_audioManager != null)
            {
                _audioManager.SetEndingMixDucking(0.72f, 0f);
            }
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World1CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 2.5f);
                _uiManager.ShowWorld1CompleteScreen();
            }
            _isInMainMenu = true;
            _isEndingSequenceActive = false;
        }

        private IEnumerator PlayWorld1CompleteSequenceCoroutine(LevelExitPortal exitPortal)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            Transform dawnHalo = exitPortal != null ? exitPortal.transform.Find("DawnThresholdLight") : null;
            SpriteRenderer dawnSr = dawnHalo != null ? dawnHalo.GetComponent<SpriteRenderer>() : null;

            Vector2 portalPos = exitPortal != null
                ? (Vector2)exitPortal.transform.position
                : (player != null ? (Vector2)player.transform.position : Vector2.zero);

            // Phase 1: Ruin mechanisms resonate, unknown dawn light expands at the threshold,
            // and both Protagonist and Shadow walk forward toward the new light (~1.3s)
            float stepDuration = 1.3f;
            float elapsed = 0f;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (player != null)
                {
                    Vector2 targetPlayerPos = new Vector2(portalPos.x + 0.15f, player.transform.position.y);
                    player.transform.position = Vector2.MoveTowards(player.transform.position, targetPlayerPos, 2.2f * Time.deltaTime);
                }
                if (shadow != null)
                {
                    Vector2 targetShadowPos = new Vector2(portalPos.x - 0.32f, shadow.transform.position.y);
                    shadow.StepEndingApproach(targetShadowPos, 2.6f);
                }
                if (torch != null)
                {
                    torch.SetEndingGlow(Mathf.Lerp(1.25f, 1.60f, smoothT), Mathf.Lerp(1.15f, 1.45f, smoothT));
                }
                if (dawnHalo != null)
                {
                    float scale = Mathf.Lerp(1.85f, 3.60f, smoothT);
                    dawnHalo.localScale = new Vector3(scale, scale, 1f);
                }
                if (dawnSr != null)
                {
                    dawnSr.color = new Color(1f, 0.94f, 0.72f, Mathf.Lerp(0.32f, 0.78f, smoothT));
                }
                yield return null;
            }

            if (shadow != null)
            {
                shadow.ReconnectToProtagonist();
            }

            // Phase 2: Screen transitions smoothly with "Beyond the darkness, another light awaits." (~1.8s)
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World1CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 0.65f);
            }

            float fadeDuration = 1.8f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (_audioManager != null)
                {
                    _audioManager.SetEndingMixDucking(Mathf.Lerp(1.0f, 0.72f, smoothT), Mathf.Lerp(0.85f, 0.0f, smoothT));
                }
                if (_uiManager != null)
                {
                    _uiManager.SetEndingQuoteAlpha(smoothT);
                }
                yield return null;
            }

            yield return new WaitForSeconds(1.15f);

            CompleteWorld1SequenceImmediate();
            _endingCoroutine = null;
        }

        /// <summary>
        /// Triggers the World 2 Complete narrative sequence at the end of Level 10 of World 2 ("Heart of the Inverted Forest").
        /// Forest canopy energy resonates, emerald and twilight dawn lights expand at the sanctuary arch,
        /// and the World 2 Complete screen opens.
        /// </summary>
        public void BeginWorld2CompleteSequence()
        {
            if (_isEndingSequenceActive) return;

            _isEndingSequenceActive = true;
            _isControllingShadow = false;

            WorldProgressSave.MarkWorldCompleted(ActiveWorldId, _levelManager.TotalLevels);

            if (RewardService.Instance != null)
            {
                int worldEarned = RewardService.Instance.EvaluateAndClaimWorldClearReward(ActiveWorldId, out string worldSummary);
                if (worldEarned > 0 && _uiManager != null)
                {
                    _uiManager.ShowTemporaryRewardNotification(worldSummary);
                }
            }

            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);

            LevelExitPortal exitPortal = FindFirstObjectByType<LevelExitPortal>();
            if (_cameraController != null)
            {
                Transform focus = exitPortal != null
                    ? exitPortal.transform
                    : (_levelManager.Player != null ? _levelManager.Player.transform : null);
                _cameraController.SetTarget(focus);
                _cameraController.SetTargetZoom(3.35f, 0.95f);
                _cameraController.TriggerShake(0.35f, 0.07f);
            }

            if (_audioManager != null)
            {
                _audioManager.PlaySFX("stone_door", 0.72f, 0.88f);
            }

            if (_levelManager.Torch != null && _levelManager.Player != null)
            {
                if (!_levelManager.Player.HasTorch)
                {
                    _levelManager.Player.ForceEquipTorch(_levelManager.Torch);
                }
                _levelManager.Torch.SetEndingGlow(1.45f, 1.35f);
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World2CompleteQuote);
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    UIManager.World2CompleteQuote,
                    false
                );
            }

            if (Application.isPlaying)
            {
                _endingCoroutine = StartCoroutine(PlayWorld2CompleteSequenceCoroutine(exitPortal));
            }
            else
            {
                CompleteWorld2SequenceImmediate();
            }
        }

        public void CompleteWorld2SequenceImmediate()
        {
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.ReconnectToProtagonist();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.SetEndingGlow(1.35f, 1.25f);
            }
            if (_audioManager != null)
            {
                _audioManager.SetEndingMixDucking(0.72f, 0f);
            }
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World2CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 2.5f);
                _uiManager.ShowWorld2CompleteScreen();
            }
            _isInMainMenu = true;
            _isEndingSequenceActive = false;
        }

        private IEnumerator PlayWorld2CompleteSequenceCoroutine(LevelExitPortal exitPortal)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            Transform dawnHalo = exitPortal != null ? exitPortal.transform.Find("DawnThresholdLight") : null;
            SpriteRenderer dawnSr = dawnHalo != null ? dawnHalo.GetComponent<SpriteRenderer>() : null;

            Vector2 portalPos = exitPortal != null
                ? (Vector2)exitPortal.transform.position
                : (player != null ? (Vector2)player.transform.position : Vector2.zero);

            float stepDuration = 1.3f;
            float elapsed = 0f;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (player != null)
                {
                    Vector2 targetPlayerPos = new Vector2(portalPos.x + 0.15f, player.transform.position.y);
                    player.transform.position = Vector2.MoveTowards(player.transform.position, targetPlayerPos, 2.2f * Time.deltaTime);
                }
                if (shadow != null)
                {
                    Vector2 targetShadowPos = new Vector2(portalPos.x - 0.32f, shadow.transform.position.y);
                    shadow.StepEndingApproach(targetShadowPos, 2.6f);
                }
                if (torch != null)
                {
                    torch.SetEndingGlow(Mathf.Lerp(1.25f, 1.60f, smoothT), Mathf.Lerp(1.15f, 1.45f, smoothT));
                }
                if (dawnHalo != null)
                {
                    float scale = Mathf.Lerp(1.85f, 3.60f, smoothT);
                    dawnHalo.localScale = new Vector3(scale, scale, 1f);
                }
                if (dawnSr != null)
                {
                    dawnSr.color = new Color(0.42f, 0.95f, 0.72f, Mathf.Lerp(0.32f, 0.78f, smoothT));
                }
                yield return null;
            }

            if (shadow != null)
            {
                shadow.ReconnectToProtagonist();
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World2CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 0.65f);
            }

            float fadeDuration = 1.8f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (_audioManager != null)
                {
                    _audioManager.SetEndingMixDucking(Mathf.Lerp(1.0f, 0.72f, smoothT), Mathf.Lerp(0.85f, 0.0f, smoothT));
                }
                if (_uiManager != null)
                {
                    _uiManager.SetEndingQuoteAlpha(smoothT);
                }
                yield return null;
            }

            yield return new WaitForSeconds(1.15f);

            CompleteWorld2SequenceImmediate();
            _endingCoroutine = null;
        }

        /// <summary>
        /// Triggers the World 3 Complete narrative sequence at the end of Level 10 of World 3 ("The Eternal Spire").
        /// Clocktower resonance pulses, bronze gear halo awakens at the exit portal,
        /// and the World 3 Complete screen opens.
        /// </summary>
        public void BeginWorld3CompleteSequence()
        {
            if (_isEndingSequenceActive) return;

            _isEndingSequenceActive = true;
            _isControllingShadow = false;

            WorldProgressSave.MarkWorldCompleted(ActiveWorldId, _levelManager.TotalLevels);

            if (RewardService.Instance != null)
            {
                int worldEarned = RewardService.Instance.EvaluateAndClaimWorldClearReward(ActiveWorldId, out string worldSummary);
                if (worldEarned > 0 && _uiManager != null)
                {
                    _uiManager.ShowTemporaryRewardNotification(worldSummary);
                }
            }

            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);

            LevelExitPortal exitPortal = FindFirstObjectByType<LevelExitPortal>();
            if (_cameraController != null)
            {
                Transform focus = exitPortal != null
                    ? exitPortal.transform
                    : (_levelManager.Player != null ? _levelManager.Player.transform : null);
                _cameraController.SetTarget(focus);
                _cameraController.SetTargetZoom(3.35f, 0.95f);
                _cameraController.TriggerShake(0.35f, 0.07f);
            }

            if (_audioManager != null)
            {
                _audioManager.PlaySFX("stone_door", 0.75f, 0.92f);
            }

            if (_levelManager.Torch != null && _levelManager.Player != null)
            {
                if (!_levelManager.Player.HasTorch)
                {
                    _levelManager.Player.ForceEquipTorch(_levelManager.Torch);
                }
                _levelManager.Torch.SetEndingGlow(1.50f, 1.40f);
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World3CompleteQuote);
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    UIManager.World3CompleteQuote,
                    false
                );
            }

            if (Application.isPlaying)
            {
                _endingCoroutine = StartCoroutine(PlayWorld3CompleteSequenceCoroutine(exitPortal));
            }
            else
            {
                CompleteWorld3SequenceImmediate();
            }
        }

        public void CompleteWorld3SequenceImmediate()
        {
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.ReconnectToProtagonist();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.SetEndingGlow(1.35f, 1.25f);
            }
            if (_audioManager != null)
            {
                _audioManager.SetEndingMixDucking(0.72f, 0f);
            }
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World3CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 2.5f);
                _uiManager.ShowWorld3CompleteScreen();
            }
            _isInMainMenu = true;
            _isEndingSequenceActive = false;
        }

        private IEnumerator PlayWorld3CompleteSequenceCoroutine(LevelExitPortal exitPortal)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            Transform dawnHalo = exitPortal != null ? exitPortal.transform.Find("DawnThresholdLight") : null;
            SpriteRenderer dawnSr = dawnHalo != null ? dawnHalo.GetComponent<SpriteRenderer>() : null;

            Vector2 portalPos = exitPortal != null
                ? (Vector2)exitPortal.transform.position
                : (player != null ? (Vector2)player.transform.position : Vector2.zero);

            float stepDuration = 1.3f;
            float elapsed = 0f;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (player != null)
                {
                    Vector2 targetPlayerPos = new Vector2(portalPos.x + 0.15f, player.transform.position.y);
                    player.transform.position = Vector2.MoveTowards(player.transform.position, targetPlayerPos, 2.2f * Time.deltaTime);
                }
                if (shadow != null)
                {
                    Vector2 targetShadowPos = new Vector2(portalPos.x - 0.32f, shadow.transform.position.y);
                    shadow.StepEndingApproach(targetShadowPos, 2.6f);
                }
                if (torch != null)
                {
                    torch.SetEndingGlow(Mathf.Lerp(1.25f, 1.65f, smoothT), Mathf.Lerp(1.15f, 1.50f, smoothT));
                }
                if (dawnHalo != null)
                {
                    float scale = Mathf.Lerp(1.85f, 3.60f, smoothT);
                    dawnHalo.localScale = new Vector3(scale, scale, 1f);
                }
                if (dawnSr != null)
                {
                    dawnSr.color = new Color(0.96f, 0.82f, 0.44f, Mathf.Lerp(0.32f, 0.82f, smoothT));
                }
                yield return null;
            }

            if (shadow != null)
            {
                shadow.ReconnectToProtagonist();
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World3CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 0.65f);
            }

            float fadeDuration = 1.8f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (_audioManager != null)
                {
                    _audioManager.SetEndingMixDucking(Mathf.Lerp(1.0f, 0.72f, smoothT), Mathf.Lerp(0.85f, 0.0f, smoothT));
                }
                if (_uiManager != null)
                {
                    _uiManager.SetEndingQuoteAlpha(smoothT);
                }
                yield return null;
            }

            yield return new WaitForSeconds(1.15f);

            CompleteWorld3SequenceImmediate();
            _endingCoroutine = null;
        }

        /// <summary>
        /// Triggers the World 4 Complete narrative sequence at the end of Level 40 ("The Last Cathedral Light").
        /// </summary>
        public void BeginWorld4CompleteSequence()
        {
            if (_isEndingSequenceActive) return;

            _isEndingSequenceActive = true;
            _isControllingShadow = false;

            WorldProgressSave.MarkWorldCompleted(ActiveWorldId, _levelManager.TotalLevels);

            if (RewardService.Instance != null)
            {
                int worldEarned = RewardService.Instance.EvaluateAndClaimWorldClearReward(ActiveWorldId, out string worldSummary);
                if (worldEarned > 0 && _uiManager != null)
                {
                    _uiManager.ShowTemporaryRewardNotification(worldSummary);
                }
            }

            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);

            LevelExitPortal exitPortal = FindFirstObjectByType<LevelExitPortal>();
            if (_cameraController != null)
            {
                Transform focus = exitPortal != null
                    ? exitPortal.transform
                    : (_levelManager.Player != null ? _levelManager.Player.transform : null);
                _cameraController.SetTarget(focus);
                _cameraController.SetTargetZoom(3.35f, 0.95f);
                _cameraController.TriggerShake(0.35f, 0.07f);
            }

            if (_audioManager != null)
            {
                _audioManager.PlaySFX("stone_door", 0.75f, 0.92f);
            }

            if (_levelManager.Torch != null && _levelManager.Player != null)
            {
                if (!_levelManager.Player.HasTorch)
                {
                    _levelManager.Player.ForceEquipTorch(_levelManager.Torch);
                }
                _levelManager.Torch.SetEndingGlow(1.50f, 1.40f);
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World4CompleteQuote);
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    UIManager.World4CompleteQuote,
                    false
                );
            }

            if (Application.isPlaying)
            {
                _endingCoroutine = StartCoroutine(PlayWorld4CompleteSequenceCoroutine(exitPortal));
            }
            else
            {
                CompleteWorld4SequenceImmediate();
            }
        }

        public void CompleteWorld4SequenceImmediate()
        {
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.ReconnectToProtagonist();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.SetEndingGlow(1.35f, 1.25f);
            }
            if (_audioManager != null)
            {
                _audioManager.SetEndingMixDucking(0.72f, 0f);
            }
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World4CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 2.5f);
                _uiManager.ShowWorld4CompleteScreen();
            }
            _isInMainMenu = true;
            _isEndingSequenceActive = false;
        }

        private IEnumerator PlayWorld4CompleteSequenceCoroutine(LevelExitPortal exitPortal)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            Transform dawnHalo = exitPortal != null ? exitPortal.transform.Find("DawnThresholdLight") : null;
            SpriteRenderer dawnSr = dawnHalo != null ? dawnHalo.GetComponent<SpriteRenderer>() : null;

            Vector2 portalPos = exitPortal != null
                ? (Vector2)exitPortal.transform.position
                : (player != null ? (Vector2)player.transform.position : Vector2.zero);

            float stepDuration = 1.3f;
            float elapsed = 0f;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (player != null)
                {
                    Vector2 targetPlayerPos = new Vector2(portalPos.x + 0.15f, player.transform.position.y);
                    player.transform.position = Vector2.MoveTowards(player.transform.position, targetPlayerPos, 2.2f * Time.deltaTime);
                }
                if (shadow != null)
                {
                    Vector2 targetShadowPos = new Vector2(portalPos.x - 0.32f, shadow.transform.position.y);
                    shadow.StepEndingApproach(targetShadowPos, 2.6f);
                }
                if (torch != null)
                {
                    torch.SetEndingGlow(Mathf.Lerp(1.25f, 1.65f, smoothT), Mathf.Lerp(1.15f, 1.50f, smoothT));
                }
                if (dawnHalo != null)
                {
                    float scale = Mathf.Lerp(1.85f, 3.60f, smoothT);
                    dawnHalo.localScale = new Vector3(scale, scale, 1f);
                }
                if (dawnSr != null)
                {
                    dawnSr.color = new Color(0.35f, 0.85f, 0.95f, Mathf.Lerp(0.32f, 0.82f, smoothT));
                }
                yield return null;
            }

            if (shadow != null)
            {
                shadow.ReconnectToProtagonist();
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.World4CompleteQuote);
                _uiManager.SetEndingFade(0.45f, 0.65f);
            }

            float fadeDuration = 1.8f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (_audioManager != null)
                {
                    _audioManager.SetEndingMixDucking(Mathf.Lerp(1.0f, 0.72f, smoothT), Mathf.Lerp(0.85f, 0.0f, smoothT));
                }
                if (_uiManager != null)
                {
                    _uiManager.SetEndingQuoteAlpha(smoothT);
                }
                yield return null;
            }

            yield return new WaitForSeconds(1.15f);

            CompleteWorld4SequenceImmediate();
            _endingCoroutine = null;
        }

        /// <summary>
        /// Reusable Final Game Ending Sequence preserved for the future final world of the game.
        /// Not called upon completing World 1 (which calls BeginWorld1CompleteSequence instead).
        /// </summary>
        public void BeginGameEndingSequence()
        {
            if (_isEndingSequenceActive || _isGameCompleted) return;

            _isEndingSequenceActive = true;
            _isGameCompleted = true;
            _isControllingShadow = false;

            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);

            if (_cameraController != null && _levelManager.Player != null)
            {
                _cameraController.SetTarget(_levelManager.Player.transform);
            }

            // Ensure the Torch illuminates both characters at the Sanctuary Arch
            if (_levelManager.Torch != null && _levelManager.Player != null)
            {
                if (!_levelManager.Player.HasTorch)
                {
                    _levelManager.Player.ForceEquipTorch(_levelManager.Torch);
                }
                _levelManager.Torch.SetEndingGlow(1.35f, 1.25f);
            }

            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.FinalNarrativeQuote);
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(_currentLevelIndex),
                    UIManager.FinalNarrativeQuote,
                    false
                );
            }

            if (Application.isPlaying)
            {
                _endingCoroutine = StartCoroutine(PlayFinalEndingSequenceCoroutine());
            }
            else
            {
                CompleteEndingSequenceImmediate();
            }
        }

        public void CompleteEndingSequenceImmediate()
        {
            if (_levelManager.Shadow != null)
            {
                _levelManager.Shadow.ReconnectToProtagonist();
            }
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.SetEndingGlow(0.25f, 0.08f);
            }
            if (_audioManager != null)
            {
                _audioManager.SetEndingMixDucking(0.58f, 0f);
            }
            if (_uiManager != null)
            {
                _uiManager.SetEndingQuoteText(UIManager.FinalNarrativeQuote);
                _uiManager.SetEndingFade(0.88f, 2.5f);
                _uiManager.ShowVictoryScreen();
            }
            _isInMainMenu = true;
            _isEndingSequenceActive = false;
        }

        private IEnumerator PlayFinalEndingSequenceCoroutine()
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            // Phase 1: Shadow slowly walks toward the Protagonist while Torch illuminates both (~1.1s)
            float approachDuration = 1.1f;
            float elapsed = 0f;
            while (elapsed < approachDuration)
            {
                elapsed += Time.deltaTime;
                if (player != null && shadow != null)
                {
                    Vector2 reunionPos = (Vector2)player.transform.position + new Vector2(-player.FacingDirection * 0.48f, 0f);
                    shadow.StepEndingApproach(reunionPos, 2.8f);
                }
                if (torch != null)
                {
                    float glowT = Mathf.Clamp01(elapsed / approachDuration);
                    torch.SetEndingGlow(Mathf.Lerp(1.15f, 1.45f, glowT), Mathf.Lerp(1.05f, 1.30f, glowT));
                }
                yield return null;
            }

            // Phase 2: Shadow stops moving independently and reconnects with Protagonist
            if (shadow != null)
            {
                shadow.ReconnectToProtagonist();
            }

            // Phase 3: Flame gradually dims, scene darkens, SFX fade out smoothly, narrative quote appears (~2.0s)
            if (_uiManager != null)
            {
                _uiManager.SetEndingFade(0.88f, 0.55f);
            }

            float dimDuration = 2.0f;
            elapsed = 0f;
            while (elapsed < dimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dimDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                if (torch != null)
                {
                    torch.SetEndingGlow(Mathf.Lerp(1.45f, 0.25f, smoothT), Mathf.Lerp(1.30f, 0.08f, smoothT));
                }
                if (_audioManager != null)
                {
                    _audioManager.SetEndingMixDucking(Mathf.Lerp(1.0f, 0.58f, smoothT), Mathf.Lerp(0.85f, 0.0f, smoothT));
                }
                if (_uiManager != null)
                {
                    _uiManager.SetEndingQuoteAlpha(smoothT);
                }
                yield return null;
            }

            // Hold the narrative closing message briefly before revealing the Victory Screen
            yield return new WaitForSeconds(1.25f);

            CompleteEndingSequenceImmediate();
            _endingCoroutine = null;
        }
    }
}
