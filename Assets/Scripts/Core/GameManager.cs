using System.Collections;
using UnityEngine;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.UI;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Central orchestrator for MY SHADOW QUIT: THE LAST FLAME.
    /// Auto-bootstraps when any scene loads so the game is immediately playable in Editor or WebGL build.
    /// Manages Main Menu, Level 1 ("Awakening") through Level 8 ("My Shadow Quit"),
    /// TutorialDirector contextual progression, character switching (TAB), level restart (R),
    /// pause menu (ESC), camera target tracking, and the narrative Game Ending sequence.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private bool startInMainMenu = false;

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
        private bool _isGameCompleted;
        private Coroutine _endingCoroutine;

        public int CurrentLevelIndex => _currentLevelIndex;
        public bool IsControllingShadow => _isControllingShadow;
        public bool IsPaused => _isPaused;
        public bool IsEndingSequenceActive => _isEndingSequenceActive;
        public bool IsGameCompleted => _isGameCompleted;
        public LevelManager Levels => _levelManager;
        public TutorialDirector Tutorial => _tutorialDirector;
        public UIManager UI => _uiManager;

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
            _uiManager.OnResumeRequested -= TogglePause;
            _uiManager.OnResumeRequested += TogglePause;
            _uiManager.OnRestartRequested -= RestartCurrentLevel;
            _uiManager.OnRestartRequested += RestartCurrentLevel;
            _uiManager.OnMainMenuRequested -= OpenMainMenu;
            _uiManager.OnMainMenuRequested += OpenMainMenu;

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
            _isGameCompleted = false;
            Time.timeScale = 1f;
            LoadLevel(0);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            _uiManager.ShowMainMenu();
        }

        public void StartNewGame()
        {
            StopEndingSequenceIfNeeded();
            _isInMainMenu = false;
            _isPaused = false;
            _isGameCompleted = false;
            Time.timeScale = 1f;
            LoadLevel(0);
        }

        public void LoadLevel(int index)
        {
            StopEndingSequenceIfNeeded();
            _currentLevelIndex = Mathf.Clamp(index, 0, _levelManager.TotalLevels - 1);
            _isControllingShadow = false;
            _isPaused = false;
            Time.timeScale = 1f;

            _levelManager.BuildLevel(_currentLevelIndex, _cameraController);
            if (_levelManager.Torch != null)
            {
                _levelManager.Torch.ResetEndingGlow();
            }
            ApplyActiveCharacterControl();

            if (!_isInMainMenu && _uiManager != null)
            {
                _uiManager.TriggerFlashFade();
                _uiManager.ShowGameplayHUD(
                    LevelManager.LevelTitles[_currentLevelIndex],
                    LevelManager.LevelHints[_currentLevelIndex],
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
            _isPaused = false;
            Time.timeScale = 1f;
            LoadLevel(_currentLevelIndex);
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
                    LevelManager.LevelTitles[_currentLevelIndex],
                    LevelManager.LevelHints[_currentLevelIndex],
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
            if (_isEndingSequenceActive) return;
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            if (_uiManager != null)
            {
                _uiManager.SetPauseMenuVisible(_isPaused);
            }
        }

        private void HandleLevelCompleted()
        {
            if (_currentLevelIndex + 1 < _levelManager.TotalLevels)
            {
                LoadLevel(_currentLevelIndex + 1);
            }
            else
            {
                BeginGameEndingSequence();
            }
        }

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
                _uiManager.UpdateHUDText(
                    LevelManager.LevelTitles[_currentLevelIndex],
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
