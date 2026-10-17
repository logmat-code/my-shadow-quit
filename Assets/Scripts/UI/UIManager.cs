using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyShadowQuit.Core;

namespace MyShadowQuit.UI
{
    /// <summary>
    /// Custom pixel-art UI system for MY SHADOW QUIT: THE LAST FLAME.
    /// Renders Main Menu (PLAY, SETTINGS, CREDITS, QUIT/RETURN), Pause Menu (RESUME, RESTART, SETTINGS, MAIN MENU),
    /// Level HUD (level title, active character badge, contextual hint), and Victory Screen.
    /// Uses original generated pixel-art button and panel sprites (`ui_button_normal`, `ui_button_hover`, `ui_panel`).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public event Action OnPlayRequested;
        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnMainMenuRequested;

        private Canvas _canvas;
        private GameObject _mainMenuPanel;
        private GameObject _pauseMenuPanel;
        private GameObject _settingsPanel;
        private GameObject _creditsPanel;
        private GameObject _hudPanel;
        private GameObject _victoryPanel;

        private Text _levelTitleText;
        private Text _hintText;
        private Text _activeControlText;
        private Text _volumeValueText;
        private Image _fadeOverlay;

        private Font _uiFont;
        private Sprite _btnNormal;
        private Sprite _btnHover;
        private Sprite _panelSprite;

        private float _targetFadeAlpha;

        private void Awake()
        {
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_uiFont == null)
            {
                _uiFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            _btnNormal = AssetRegistry.GetSprite("UI/ui_button_normal", 32f);
            _btnHover = AssetRegistry.GetSprite("UI/ui_button_hover", 32f);
            _panelSprite = AssetRegistry.GetSprite("UI/ui_panel", 32f);

            EnsureEventSystem();
            BuildCanvasAndPanels();
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
            GameObject canvasObj = new GameObject("UICanvas");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            BuildHUD(canvasObj.transform);
            BuildMainMenu(canvasObj.transform);
            BuildPauseMenu(canvasObj.transform);
            BuildSettingsMenu(canvasObj.transform);
            BuildCreditsMenu(canvasObj.transform);
            BuildVictoryScreen(canvasObj.transform);
            BuildFadeOverlay(canvasObj.transform);
        }

        private void BuildHUD(Transform parent)
        {
            _hudPanel = new GameObject("HUDPanel", typeof(RectTransform));
            _hudPanel.transform.SetParent(parent, false);
            RectTransform rt = _hudPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Top bar container
            GameObject topBar = CreateImagePanel(_hudPanel.transform, "TopHUD", new Vector2(0f, 315f), new Vector2(1220f, 64f));
            topBar.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.12f, 0.85f);

            _levelTitleText = CreateText(topBar.transform, "LevelTitle", "I. AWAKENING", 22, new Vector2(-380f, 0f), new Vector2(420f, 48f), new Color(1f, 0.82f, 0.35f, 1f), TextAnchor.MiddleLeft);
            _activeControlText = CreateText(topBar.transform, "ActiveControl", "ACTIVE: PROTAGONIST (FLAME)", 18, new Vector2(340f, 0f), new Vector2(480f, 48f), new Color(0.85f, 0.78f, 1f, 1f), TextAnchor.MiddleRight);

            // Bottom hint banner
            GameObject bottomBar = CreateImagePanel(_hudPanel.transform, "BottomHUD", new Vector2(0f, -320f), new Vector2(1060f, 46f));
            bottomBar.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.10f, 0.82f);

            _hintText = CreateText(bottomBar.transform, "HintText", "", 16, Vector2.zero, new Vector2(1020f, 40f), new Color(0.82f, 0.86f, 0.94f, 1f), TextAnchor.MiddleCenter);
        }

        private void BuildMainMenu(Transform parent)
        {
            _mainMenuPanel = CreateImagePanel(parent, "MainMenuPanel", Vector2.zero, new Vector2(620f, 500f));

            // Decorative torch icon on Main Menu
            GameObject torchDeco = new GameObject("MenuTorchIcon", typeof(RectTransform), typeof(Image));
            torchDeco.transform.SetParent(_mainMenuPanel.transform, false);
            RectTransform trt = torchDeco.GetComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(0f, 185f);
            trt.sizeDelta = new Vector2(56f, 72f);
            Image tImg = torchDeco.GetComponent<Image>();
            tImg.sprite = AssetRegistry.GetSprite("Torch/torch_item_0", 32f);

            CreateText(_mainMenuPanel.transform, "TitleMain", "MY SHADOW QUIT", 38, new Vector2(0f, 120f), new Vector2(560f, 54f), new Color(1f, 0.84f, 0.35f, 1f), TextAnchor.MiddleCenter);
            CreateText(_mainMenuPanel.transform, "TitleSub", "THE LAST FLAME", 22, new Vector2(0f, 78f), new Vector2(500f, 36f), new Color(0.75f, 0.58f, 0.96f, 1f), TextAnchor.MiddleCenter);

            CreateMenuButton(_mainMenuPanel.transform, "PLAY", new Vector2(0f, 8f), () => OnPlayRequested?.Invoke());
            CreateMenuButton(_mainMenuPanel.transform, "SETTINGS", new Vector2(0f, -54f), () => ShowSubPanel(_settingsPanel));
            CreateMenuButton(_mainMenuPanel.transform, "CREDITS", new Vector2(0f, -116f), () => ShowSubPanel(_creditsPanel));
            CreateMenuButton(_mainMenuPanel.transform, "QUIT / RETURN", new Vector2(0f, -178f), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void BuildPauseMenu(Transform parent)
        {
            _pauseMenuPanel = CreateImagePanel(parent, "PauseMenuPanel", Vector2.zero, new Vector2(520f, 440f));
            CreateText(_pauseMenuPanel.transform, "PauseHeader", "PAUSED", 32, new Vector2(0f, 140f), new Vector2(400f, 48f), new Color(1f, 0.84f, 0.35f, 1f), TextAnchor.MiddleCenter);

            CreateMenuButton(_pauseMenuPanel.transform, "RESUME", new Vector2(0f, 55f), () => OnResumeRequested?.Invoke());
            CreateMenuButton(_pauseMenuPanel.transform, "RESTART", new Vector2(0f, -10f), () => OnRestartRequested?.Invoke());
            CreateMenuButton(_pauseMenuPanel.transform, "SETTINGS", new Vector2(0f, -75f), () => ShowSubPanel(_settingsPanel));
            CreateMenuButton(_pauseMenuPanel.transform, "MAIN MENU", new Vector2(0f, -140f), () => OnMainMenuRequested?.Invoke());
        }

        private void BuildSettingsMenu(Transform parent)
        {
            _settingsPanel = CreateImagePanel(parent, "SettingsPanel", Vector2.zero, new Vector2(540f, 380f));
            CreateText(_settingsPanel.transform, "SettingsHeader", "AUDIO & CONTROLS", 28, new Vector2(0f, 130f), new Vector2(460f, 44f), new Color(1f, 0.84f, 0.35f, 1f), TextAnchor.MiddleCenter);

            _volumeValueText = CreateText(_settingsPanel.transform, "VolumeText", "MASTER VOLUME: 85%", 20, new Vector2(0f, 65f), new Vector2(420f, 36f), Color.white, TextAnchor.MiddleCenter);

            CreateMenuButton(_settingsPanel.transform, "VOLUME + / -", new Vector2(0f, 10f), () =>
            {
                if (AudioManager.Instance != null)
                {
                    float next = AudioManager.Instance.MasterVolume + 0.2f;
                    if (next > 1.05f) next = 0.0f;
                    AudioManager.Instance.MasterVolume = next;
                    _volumeValueText.text = $"MASTER VOLUME: {Mathf.RoundToInt(AudioManager.Instance.MasterVolume * 100f)}%";
                    AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.0f);
                }
            });

            CreateText(
                _settingsPanel.transform,
                "ControlsGuide",
                "A / D : Move     SPACE : Jump     E : Torch / Switch\nTAB : Switch Protagonist & Shadow     R : Restart Level",
                16,
                new Vector2(0f, -60f),
                new Vector2(490f, 64f),
                new Color(0.8f, 0.84f, 0.95f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_settingsPanel.transform, "RETURN", new Vector2(0f, -135f), () =>
            {
                _settingsPanel.SetActive(false);
            });
            _settingsPanel.SetActive(false);
        }

        private void BuildCreditsMenu(Transform parent)
        {
            _creditsPanel = CreateImagePanel(parent, "CreditsPanel", Vector2.zero, new Vector2(560f, 380f));
            CreateText(_creditsPanel.transform, "CreditsHeader", "CREDITS", 28, new Vector2(0f, 130f), new Vector2(440f, 44f), new Color(1f, 0.84f, 0.35f, 1f), TextAnchor.MiddleCenter);
            CreateText(
                _creditsPanel.transform,
                "CreditsBody",
                "MY SHADOW QUIT: THE LAST FLAME\n\nCreated for AI Browser Game Jam 5\nOfficial Theme: TORCH\n\nOriginal Pixel Art, Procedural Dark Ambient Audio,\nand Dual-Character Shadow Puzzle Design.",
                18,
                new Vector2(0f, 10f),
                new Vector2(480f, 180f),
                new Color(0.88f, 0.88f, 0.96f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_creditsPanel.transform, "RETURN", new Vector2(0f, -135f), () =>
            {
                _creditsPanel.SetActive(false);
            });
            _creditsPanel.SetActive(false);
        }

        private void BuildVictoryScreen(Transform parent)
        {
            _victoryPanel = CreateImagePanel(parent, "VictoryPanel", Vector2.zero, new Vector2(600f, 420f));
            CreateText(_victoryPanel.transform, "VictoryHeader", "THE FLAME ENDURES", 34, new Vector2(0f, 120f), new Vector2(520f, 50f), new Color(1f, 0.86f, 0.38f, 1f), TextAnchor.MiddleCenter);
            CreateText(
                _victoryPanel.transform,
                "VictoryBody",
                "Together in light and shadow, you have escaped the ancient ruins.\nYour shadow no longer walks behind you—it walks beside you.",
                18,
                new Vector2(0f, 25f),
                new Vector2(520f, 110f),
                new Color(0.85f, 0.82f, 0.98f, 1f),
                TextAnchor.MiddleCenter
            );

            CreateMenuButton(_victoryPanel.transform, "PLAY AGAIN", new Vector2(0f, -65f), () => OnPlayRequested?.Invoke());
            CreateMenuButton(_victoryPanel.transform, "MAIN MENU", new Vector2(0f, -130f), () => OnMainMenuRequested?.Invoke());
            _victoryPanel.SetActive(false);
        }

        private void BuildFadeOverlay(Transform parent)
        {
            GameObject fadeObj = new GameObject("FadeOverlay", typeof(RectTransform), typeof(Image));
            fadeObj.transform.SetParent(parent, false);
            RectTransform rt = fadeObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _fadeOverlay = fadeObj.GetComponent<Image>();
            _fadeOverlay.color = new Color(0.03f, 0.04f, 0.08f, 0f);
            _fadeOverlay.raycastTarget = false;
        }

        private void Update()
        {
            if (_fadeOverlay != null)
            {
                Color c = _fadeOverlay.color;
                c.a = Mathf.MoveTowards(c.a, _targetFadeAlpha, Time.unscaledDeltaTime * 2.5f);
                _fadeOverlay.color = c;
            }
        }

        public void TriggerFlashFade()
        {
            if (_fadeOverlay != null)
            {
                _fadeOverlay.color = new Color(0.03f, 0.04f, 0.08f, 0.85f);
                _targetFadeAlpha = 0f;
            }
        }

        public void ShowMainMenu()
        {
            _mainMenuPanel.SetActive(true);
            _pauseMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _creditsPanel.SetActive(false);
            _victoryPanel.SetActive(false);
            _hudPanel.SetActive(false);
        }

        public void ShowGameplayHUD(string levelTitle, string hint, bool isControllingShadow)
        {
            _mainMenuPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _creditsPanel.SetActive(false);
            _victoryPanel.SetActive(false);
            _hudPanel.SetActive(true);

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
            _pauseMenuPanel.SetActive(visible);
            if (!visible)
            {
                _settingsPanel.SetActive(false);
            }
        }

        public void ShowVictoryScreen()
        {
            _hudPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _victoryPanel.SetActive(true);
        }

        private void ShowSubPanel(GameObject panel)
        {
            _settingsPanel.SetActive(panel == _settingsPanel);
            _creditsPanel.SetActive(panel == _creditsPanel);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.65f, 1.1f);
            }
        }

        private GameObject CreateImagePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.sprite = _panelSprite;
            img.type = Image.Type.Simple;
            return go;
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

        private Button CreateMenuButton(Transform parent, string label, Vector2 pos, Action onClick)
        {
            GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280f, 50f);

            Image img = go.GetComponent<Image>();
            img.sprite = _btnNormal;

            Button btn = go.GetComponent<Button>();
            SpriteState st = new SpriteState
            {
                highlightedSprite = _btnHover,
                pressedSprite = _btnHover,
                selectedSprite = _btnHover
            };
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = st;

            CreateText(go.transform, "Label", label, 20, Vector2.zero, new Vector2(260f, 44f), new Color(1f, 0.94f, 0.80f, 1f), TextAnchor.MiddleCenter);

            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.05f);
                }
                onClick?.Invoke();
            });

            return btn;
        }
    }
}
