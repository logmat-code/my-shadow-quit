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
    /// pause menu (ESC), and camera target tracking.
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

        public int CurrentLevelIndex => _currentLevelIndex;
        public bool IsControllingShadow => _isControllingShadow;
        public bool IsPaused => _isPaused;
        public LevelManager Levels => _levelManager;
        public TutorialDirector Tutorial => _tutorialDirector;

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
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupSystems();
        }

        private void SetupSystems()
        {
            _audioManager = FindFirstObjectByType<AudioManager>();
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

            _uiManager = FindFirstObjectByType<UIManager>();
            if (_uiManager == null)
            {
                _uiManager = gameObject.AddComponent<UIManager>();
            }

            _tutorialDirector = GetComponent<TutorialDirector>();
            if (_tutorialDirector == null)
            {
                _tutorialDirector = gameObject.AddComponent<TutorialDirector>();
            }

            _uiManager.OnPlayRequested += StartNewGame;
            _uiManager.OnResumeRequested += TogglePause;
            _uiManager.OnRestartRequested += RestartCurrentLevel;
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
            if (_isInMainMenu) return;

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

        public void OpenMainMenu()
        {
            _isInMainMenu = true;
            _isPaused = false;
            Time.timeScale = 1f;
            LoadLevel(0);
            if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
            if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
            _uiManager.ShowMainMenu();
        }

        public void StartNewGame()
        {
            _isInMainMenu = false;
            _isPaused = false;
            Time.timeScale = 1f;
            LoadLevel(0);
        }

        public void LoadLevel(int index)
        {
            _currentLevelIndex = Mathf.Clamp(index, 0, _levelManager.TotalLevels - 1);
            _isControllingShadow = false;
            _isPaused = false;
            Time.timeScale = 1f;

            _levelManager.BuildLevel(_currentLevelIndex, _cameraController);
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
            if (_levelManager.Player == null || _levelManager.Shadow == null) return;

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
                _isInMainMenu = true;
                if (_levelManager.Player != null) _levelManager.Player.SetControlActive(false);
                if (_levelManager.Shadow != null) _levelManager.Shadow.SetControlActive(false);
                if (_uiManager != null) _uiManager.ShowVictoryScreen();
            }
        }
    }
}
