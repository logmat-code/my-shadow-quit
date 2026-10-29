using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.UI;
using MyShadowQuit.Narrative;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Tracks real-time player progression in Level 1 (Awakening) and subsequent levels.
    /// Displays ONE relevant contextual instruction at a time, drives world-space interaction prompts
    /// and subtle mechanism beacon pulses, and surfaces progressive guidance if the player hesitates.
    /// </summary>
    public class TutorialDirector : MonoBehaviour
    {
        public enum AwakeningStep
        {
            Move,
            PickUpTorch,
            PlaceTorchOnPedestal,
            SwitchToShadow,
            ActivateShadowSwitch,
            EnterSanctuaryExit,
            Completed
        }

        private AwakeningStep _currentStep = AwakeningStep.Move;
        private float _stepIdleTimer;
        private Vector2 _initialPlayerPos;
        private bool _hasMovedEnough;

        private LevelManager _levelManager;
        private UIManager _uiManager;

        private TorchPedestal _level1Pedestal;
        private ShadowSwitch _level1ShadowSwitch;
        private ShadowPassage _level1Passage;
        private AncientDoor _level1Door;
        private LevelExitPortal _level1Exit;

        // World-space floating prompt badge above the active interactive object
        private GameObject _worldPromptObj;
        private TextMesh _worldPromptText;
        private SpriteRenderer _beaconHalo;

        public AwakeningStep CurrentStep => _currentStep;
        public string ActiveInstruction { get; private set; } = "";

        public void InitializeForLevel(int levelIndex, LevelManager lm, UIManager ui)
        {
            _levelManager = lm;
            _uiManager = ui;
            _stepIdleTimer = 0f;
            _hasMovedEnough = false;

            EnsureWorldPromptObjects();

            int activeWorldIdx = (_levelManager != null && _levelManager.ActiveWorld != null) ? _levelManager.ActiveWorld.WorldIndex : 0;
            if (activeWorldIdx == 0 && levelIndex == 0 && _levelManager != null && _levelManager.Player != null)
            {
                _initialPlayerPos = _levelManager.Player.transform.position;
                _currentStep = AwakeningStep.Move;

                _level1Pedestal = FindFirstObjectByType<TorchPedestal>();
                _level1ShadowSwitch = FindFirstObjectByType<ShadowSwitch>();
                _level1Passage = FindFirstObjectByType<ShadowPassage>();
                _level1Door = FindFirstObjectByType<AncientDoor>();
                _level1Exit = FindFirstObjectByType<LevelExitPortal>();

                // Subscribe to mechanism activations for visual & audio celebration feedback
                if (_level1Pedestal != null)
                {
                    _level1Pedestal.OnStateChanged -= OnMechanismStateChanged;
                    _level1Pedestal.OnStateChanged += OnMechanismStateChanged;
                }
                if (_level1ShadowSwitch != null)
                {
                    _level1ShadowSwitch.OnStateChanged -= OnMechanismStateChanged;
                    _level1ShadowSwitch.OnStateChanged += OnMechanismStateChanged;
                }

                EvaluateAwakeningProgression(forceUpdate: true);
            }
            else
            {
                _currentStep = AwakeningStep.Completed;
                if (_worldPromptObj != null) _worldPromptObj.SetActive(false);
                if (_beaconHalo != null) _beaconHalo.gameObject.SetActive(false);
            }
        }

        public bool IsPathToExitPhysicallyClear()
        {
            if (_level1Door == null || !_level1Door.IsOpen) return false;
            if (_level1Passage != null && _level1Passage.IsBlockingProtagonist) return false;
            return true;
        }

        private void EnsureWorldPromptObjects()
        {
            if (_worldPromptObj == null)
            {
                _worldPromptObj = new GameObject("WorldContextualPrompt");
                _worldPromptObj.transform.SetParent(transform, false);
                _worldPromptText = _worldPromptObj.AddComponent<TextMesh>();
                _worldPromptText.fontSize = 32;
                _worldPromptText.characterSize = 0.085f;
                _worldPromptText.anchor = TextAnchor.LowerCenter;
                _worldPromptText.alignment = TextAlignment.Center;
                _worldPromptText.color = new Color(1f, 0.90f, 0.48f, 0.95f);

                MeshRenderer mr = _worldPromptObj.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 40;
            }

            if (_beaconHalo == null)
            {
                GameObject haloGo = new GameObject("ObjectiveBeaconHalo");
                haloGo.transform.SetParent(transform, false);
                _beaconHalo = haloGo.AddComponent<SpriteRenderer>();
                _beaconHalo.sprite = AssetRegistry.GetSprite("Torch/light_halo", 32f, FilterMode.Bilinear);
                _beaconHalo.sortingOrder = 3;
            }
        }

        private void OnMechanismStateChanged(PuzzleNode node, bool active)
        {
            if (!active) return;

            // Subtle camera shake and rune glow feedback when any puzzle node is solved
            CameraController cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            if (cam != null)
            {
                cam.TriggerShake(0.16f, 0.06f);
            }
        }

        private string GetInputPrompt(string desktopKey, string touchAction)
        {
#if UNITY_ANDROID || UNITY_IOS
            return touchAction;
#else
            return desktopKey;
#endif
        }

        private void Update()
        {
            if (_levelManager == null || _levelManager.Player == null) return;

            // Only run tutorial and floating torch/mechanism prompts in World 1 (Levels 1 to 10)
            int currentLevel = (GameManager.Instance != null) ? GameManager.Instance.CurrentLevelIndex : 0;
            int activeWorldIdx = (_levelManager != null && _levelManager.ActiveWorld != null) ? _levelManager.ActiveWorld.WorldIndex : 0;
            if (activeWorldIdx > 0 || currentLevel >= 10)
            {
                if (_worldPromptObj != null) _worldPromptObj.SetActive(false);
                if (_beaconHalo != null) _beaconHalo.gameObject.SetActive(false);
                return;
            }

            // Do not show prompts if a dialogue, pause menu, or ending sequence is active
            if (DialogueController.Instance != null && DialogueController.Instance.IsDialogueActive)
            {
                if (_worldPromptObj != null) _worldPromptObj.SetActive(false);
                if (_beaconHalo != null) _beaconHalo.gameObject.SetActive(false);
                return;
            }

            if (UIManager.Instance != null && UIManager.Instance.CurrentScreen != UIManager.ActiveMenuScreen.None)
            {
                if (_worldPromptObj != null) _worldPromptObj.SetActive(false);
                if (_beaconHalo != null) _beaconHalo.gameObject.SetActive(false);
                return;
            }

            // Update dynamic torch crackle proximity so fire stays subtle
            if (AudioManager.Instance != null && _levelManager.Torch != null)
            {
                Transform listenerTarget = (GameManager.Instance != null && GameManager.Instance.IsControllingShadow && _levelManager.Shadow != null)
                    ? _levelManager.Shadow.transform
                    : _levelManager.Player.transform;
                float dist = Vector2.Distance(listenerTarget.position, _levelManager.Torch.transform.position);
                float proximity = Mathf.Clamp01(1f - (dist / 12f));
                AudioManager.Instance.SetTorchCrackleIntensity(0.25f + 0.55f * proximity);
            }

            if (currentLevel == 0)
            {
                EvaluateAwakeningProgression(forceUpdate: false);
            }
            else
            {
                UpdateGeneralInteractivePrompts();
            }
        }

        public void EvaluateAwakeningProgression(bool forceUpdate)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;
            if (player == null || shadow == null || torch == null) return;

            if (!_hasMovedEnough && Vector2.Distance(player.transform.position, _initialPlayerPos) > 0.75f)
            {
                _hasMovedEnough = true;
            }

            AwakeningStep nextStep;

            if (IsPathToExitPhysicallyClear())
            {
                nextStep = AwakeningStep.EnterSanctuaryExit;
            }
            else if (_level1Pedestal != null && _level1Pedestal.HasTorch)
            {
                bool controllingShadow = GameManager.Instance != null && GameManager.Instance.IsControllingShadow;
                nextStep = controllingShadow ? AwakeningStep.ActivateShadowSwitch : AwakeningStep.SwitchToShadow;
            }
            else if (player.HasTorch)
            {
                nextStep = AwakeningStep.PlaceTorchOnPedestal;
            }
            else if (_hasMovedEnough || Vector2.Distance(player.transform.position, torch.transform.position) <= 1.8f)
            {
                nextStep = AwakeningStep.PickUpTorch;
            }
            else
            {
                nextStep = AwakeningStep.Move;
            }

            if (nextStep != _currentStep || forceUpdate)
            {
                _currentStep = nextStep;
                _stepIdleTimer = 0f;
            }
            else
            {
                _stepIdleTimer += Time.deltaTime;
            }

            bool showProgressiveHint = _stepIdleTimer >= 9.0f;
            UpdateStepVisualsAndHUD(showProgressiveHint);
        }

        private void UpdateStepVisualsAndHUD(bool progressiveHint)
        {
            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;

            Transform beaconTarget = null;
            Color beaconColor = new Color(1f, 0.82f, 0.35f, 0.32f);
            string worldTag = "";

            switch (_currentStep)
            {
                case AwakeningStep.Move:
                    ActiveInstruction = progressiveHint
                        ? "Press A / D (or Left / Right Arrow) to walk toward the burning torch."
                        : "Press A/D to move";
                    beaconTarget = torch != null ? torch.transform : null;
                    break;

                case AwakeningStep.PickUpTorch:
                    ActiveInstruction = progressiveHint
                        ? $"Stand next to the burning torch and {GetInputPrompt("press Q", "tap Torch Button")} to pick it up."
                        : $"{GetInputPrompt("PRESS Q TO PICK UP TORCH", "TAP TORCH BUTTON TO PICK UP")}";
                    beaconTarget = torch != null ? torch.transform : null;
                    if (torch != null && Vector2.Distance(player.transform.position, torch.transform.position) <= 1.8f)
                    {
                        worldTag = GetInputPrompt("[Q] Pick Up Torch", "[TAP] Pick Up Torch");
                    }
                    break;

                case AwakeningStep.PlaceTorchOnPedestal:
                    ActiveInstruction = progressiveHint
                        ? $"Jump up the stone ledges (SPACE) and {GetInputPrompt("press Q", "tap Torch Button")} at the carved pedestal to mount the torch."
                        : $"{GetInputPrompt("PRESS Q TO PLACE TORCH", "TAP TORCH BUTTON TO PLACE TORCH")}";
                    beaconTarget = _level1Pedestal != null ? _level1Pedestal.transform : null;
                    if (_level1Pedestal != null)
                    {
                        worldTag = Vector2.Distance(player.transform.position, _level1Pedestal.transform.position) <= 1.8f
                            ? GetInputPrompt("[Q] Place Torch", "[TAP] Place Torch")
                            : "Pedestal";
                    }
                    break;

                case AwakeningStep.SwitchToShadow:
                    ActiveInstruction = progressiveHint
                        ? "The violet lattice blocks your body, but your Shadow can pass. Press TAB to control your Shadow."
                        : "Press TAB to control your shadow";
                    beaconTarget = shadow != null ? shadow.transform : null;
                    beaconColor = new Color(0.72f, 0.45f, 1f, 0.38f);
                    worldTag = "[TAB] Control Shadow";
                    break;

                case AwakeningStep.ActivateShadowSwitch:
                    ActiveInstruction = progressiveHint
                        ? "Walk your Shadow right through the violet lattice barrier and press E at the Shadow Obelisk."
                        : "Pass the shadow barrier and activate the shadow switch [E]";
                    beaconTarget = _level1ShadowSwitch != null ? _level1ShadowSwitch.transform : null;
                    beaconColor = new Color(0.75f, 0.48f, 1f, 0.42f);
                    if (_level1ShadowSwitch != null)
                    {
                        worldTag = (shadow != null && Vector2.Distance(shadow.transform.position, _level1ShadowSwitch.transform.position) <= 1.8f)
                            ? "[E] Activate Switch"
                            : "Shadow Switch";
                    }
                    break;

                case AwakeningStep.EnterSanctuaryExit:
                    bool isShadow = GameManager.Instance != null && GameManager.Instance.IsControllingShadow;
                    if (isShadow)
                    {
                        ActiveInstruction = "The stone door is open! Press TAB to return to the Protagonist and enter the exit.";
                        beaconTarget = player != null ? player.transform : null;
                        worldTag = "[TAB] Switch to Protagonist";
                    }
                    else
                    {
                        ActiveInstruction = progressiveHint
                            ? "Walk right through the opened stone door into the glowing Sanctuary Arch."
                            : "Proceed through the open door to the Sanctuary Exit";
                        beaconTarget = _level1Exit != null ? _level1Exit.transform : null;
                        worldTag = "Sanctuary Exit";
                    }
                    break;
            }

            if (_uiManager != null && GameManager.Instance != null && _levelManager != null)
            {
                _uiManager.UpdateHUDText(
                    _levelManager.GetLevelTitle(0),
                    ActiveInstruction,
                    GameManager.Instance.IsControllingShadow
                );
            }

            UpdateBeaconAndWorldTag(beaconTarget, beaconColor, worldTag);
        }

        private void UpdateGeneralInteractivePrompts()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsEndingSequenceActive)
            {
                UpdateBeaconAndWorldTag(null, Color.clear, "");
                return;
            }

            PlayerController player = _levelManager.Player;
            ShadowController shadow = _levelManager.Shadow;
            TorchController torch = _levelManager.Torch;
            bool isShadow = GameManager.Instance != null && GameManager.Instance.IsControllingShadow;

            Transform target = null;
            string tagText = "";
            Color col = new Color(1f, 0.82f, 0.35f, 0.3f);

            // Dynamic guidance for any level requiring Flame & Shadow reunion at the exit (Level 8 and Level 10)
            if (GameManager.Instance != null && player != null && shadow != null)
            {
                LevelExitPortal exit = FindFirstObjectByType<LevelExitPortal>();
                if (exit != null && exit.RequireReconnectedShadow && exit.GuardingDoor != null && exit.GuardingDoor.IsOpen)
                {
                    bool playerAtArch = exit.IsCharacterAtArch(player.transform.position);
                    bool shadowAtArch = exit.IsCharacterAtArch(shadow.transform.position);
                    string finaleHint;
                    bool isWorldFinale = GameManager.Instance.CurrentLevelIndex >= _levelManager.TotalLevels - 1;
                    string archName = isWorldFinale ? "Dawn Portal" : "Sanctuary Arch";

                    if (playerAtArch && !shadowAtArch)
                    {
                        if (!isShadow)
                        {
                            finaleHint = $"The {archName} awaits you both! Press TAB to bring your Shadow to the threshold.";
                            target = shadow.transform;
                            tagText = "[TAB] Control Shadow";
                            col = new Color(0.75f, 0.48f, 1f, 0.42f);
                        }
                        else
                        {
                            finaleHint = $"Guide your Shadow into the {archName} beside the Protagonist.";
                            target = exit.transform;
                            tagText = "Reunite Here";
                            col = new Color(0.75f, 0.48f, 1f, 0.42f);
                        }
                    }
                    else if (shadowAtArch && !playerAtArch)
                    {
                        if (isShadow)
                        {
                            finaleHint = $"Your Shadow is at the {archName}! Press TAB to bring the Protagonist to the threshold.";
                            target = player.transform;
                            tagText = "[TAB] Control Protagonist";
                        }
                        else
                        {
                            finaleHint = $"Walk the Protagonist into the {archName} beside your Shadow.";
                            target = exit.transform;
                            tagText = "Reunite Here";
                        }
                    }
                    else
                    {
                        finaleHint = $"All seals are open! Bring Flame and Shadow together at the {archName}.";
                        target = exit.transform;
                        tagText = archName;
                    }

                    ActiveInstruction = finaleHint;
                    if (_uiManager != null)
                    {
                        _uiManager.UpdateHUDText(_levelManager.GetLevelTitle(GameManager.Instance.CurrentLevelIndex), finaleHint, isShadow);
                    }
                    UpdateBeaconAndWorldTag(target, col, tagText);
                    return;
                }
            }

            if (!isShadow && player != null)
            {
                if (!player.HasTorch && torch != null && Vector2.Distance(player.transform.position, torch.transform.position) <= 1.6f)
                {
                    target = torch.transform;
                    tagText = GetInputPrompt("[Q] Pick Up Torch", "[TAP] Pick Up Torch");
                }
                else
                {
                    TorchPedestal ped = FindFirstObjectByType<TorchPedestal>();
                    if (ped != null && Vector2.Distance(player.transform.position, ped.transform.position) <= 1.6f)
                    {
                        target = ped.transform;
                        tagText = player.HasTorch 
                            ? GetInputPrompt("[Q] Place Torch", "[TAP] Place Torch") 
                            : (ped.HasTorch ? GetInputPrompt("[Q] Take Torch", "[TAP] Take Torch") : "");
                    }
                }
            }
            else if (isShadow && shadow != null)
            {
                ShadowSwitch sw = FindFirstObjectByType<ShadowSwitch>();
                if (sw != null && !sw.IsActivated && Vector2.Distance(shadow.transform.position, sw.transform.position) <= 1.6f)
                {
                    target = sw.transform;
                    tagText = "[E] Activate";
                    col = new Color(0.75f, 0.48f, 1f, 0.38f);
                }
            }

            UpdateBeaconAndWorldTag(target, col, tagText);
        }

        private void UpdateBeaconAndWorldTag(Transform target, Color color, string tagText)
        {
            if (target != null)
            {
                float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 4.5f);
                if (_beaconHalo != null)
                {
                    _beaconHalo.gameObject.SetActive(true);
                    _beaconHalo.transform.position = target.position + new Vector3(0f, 0.15f, 0f);
                    _beaconHalo.transform.localScale = new Vector3(0.55f * pulse, 0.55f * pulse, 1f);
                    _beaconHalo.color = color;
                }

                if (_worldPromptObj != null && !string.IsNullOrEmpty(tagText))
                {
                    _worldPromptObj.SetActive(true);
                    _worldPromptObj.transform.position = target.position + new Vector3(0f, 0.85f + 0.05f * Mathf.Sin(Time.time * 5f), 0f);
                    _worldPromptText.text = tagText;
                }
                else if (_worldPromptObj != null)
                {
                    _worldPromptObj.SetActive(false);
                }
            }
            else
            {
                if (_beaconHalo != null) _beaconHalo.gameObject.SetActive(false);
                if (_worldPromptObj != null) _worldPromptObj.SetActive(false);
            }
        }
    }
}
