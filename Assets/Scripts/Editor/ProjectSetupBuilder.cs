#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.UI;

namespace MyShadowQuit.EditorTools
{
    /// <summary>
    /// Batchmode & Editor Menu setup builder for MY SHADOW QUIT: THE LAST FLAME.
    /// Configures 16:9 Landscape Game View & PlayerSettings, builds reusable Prefabs and Scenes,
    /// and runs comprehensive automated verification on Level 1 contextual progression,
    /// dB-calibrated audio mixing, and 16:9 landscape aspect enforcement.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetupBuilder
    {
        static ProjectSetupBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                EnforceEditorGameViewLandscape16x9();
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    try
                    {
                        EditorSettings.enterPlayModeOptionsEnabled = false;
                        CleanupEditModeSceneArtifacts(saveIfCleaned: true);
                        RunAutomatedVerification();
                    }
                    catch (Exception ex)
                    {
                        CleanupEditModeSceneArtifacts(saveIfCleaned: true);
                        Debug.LogError(ex);
                    }
                }
            };
        }

        [MenuItem("MyShadowQuit/Build Prefabs, Scenes & Verify All Systems")]
        public static void BuildAndVerifyAll()
        {
            Debug.Log("[ProjectSetupBuilder] Starting asset import refresh, prefab generation, world asset creation, scene building, and automated verification...");
            AssetDatabase.Refresh();

            ConfigureLandscapePlayerSettings();
            ConfigureTextureImporters();
            BuildWorldAssets();
            BuildReusablePrefabs();
            BuildScenes();
            RunAutomatedVerification();

            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetupBuilder] SUCCESS: All prefabs, world definitions, scenes, and automated verifications completed with zero errors!");
        }

        private static void BuildWorldAssets()
        {
            Directory.CreateDirectory("Assets/Resources/Worlds");

            WorldDefinition w1 = WorldCatalog.CreateDefaultWorld1();
            SaveOrUpdateWorldAsset(w1, "Assets/Resources/Worlds/World1_TheRuinsOfTheLastFlame.asset");

            WorldDefinition w2 = WorldCatalog.CreateDefaultWorld2();
            SaveOrUpdateWorldAsset(w2, "Assets/Resources/Worlds/World2_ComingSoon.asset");

            Debug.Log("[ProjectSetupBuilder] Created/Updated WorldDefinition ScriptableObjects in Assets/Resources/Worlds/.");
        }

        private static void SaveOrUpdateWorldAsset(WorldDefinition source, string assetPath)
        {
            WorldDefinition existing = AssetDatabase.LoadAssetAtPath<WorldDefinition>(assetPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(source, assetPath);
            }
            else
            {
                existing.Configure(
                    source.WorldIndex,
                    source.WorldId,
                    source.WorldTitle,
                    source.WorldSubtitle,
                    source.CompletionQuote,
                    source.IsUnlocked,
                    source.IsImplemented,
                    new List<LevelEntry>(source.Levels)
                );
                EditorUtility.SetDirty(existing);
            }
        }

        public static void CleanupEditModeSceneArtifacts(bool saveIfCleaned = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded) return;

            int cleanedCount = 0;
            bool seenMainCam = false;
            bool seenGameManager = false;

            GameObject[] roots = activeScene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root == null) continue;

                if (root.name == "Main Camera" && !seenMainCam)
                {
                    seenMainCam = true;
                    CameraController cc = root.GetComponent<CameraController>();
                    if (cc != null) cc.ClearParallaxLayers();
                    for (int i = root.transform.childCount - 1; i >= 0; i--)
                    {
                        UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                        cleanedCount++;
                    }
                    continue;
                }

                if (root.name == "GameManager" && !seenGameManager)
                {
                    seenGameManager = true;
                    for (int i = root.transform.childCount - 1; i >= 0; i--)
                    {
                        UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                        cleanedCount++;
                    }
                    continue;
                }

                // Destroy any leaked EditMode object (VerificationRunner, Level_1..8, ParallaxFar/Mid/Near, etc.)
                UnityEngine.Object.DestroyImmediate(root);
                cleanedCount++;
            }

            if (cleanedCount > 0 && saveIfCleaned && !string.IsNullOrEmpty(activeScene.path))
            {
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[ProjectSetupBuilder] Cleaned {cleanedCount} leaked EditMode object(s) from active scene '{activeScene.name}' and saved clean state.");
            }
        }

        private static void ConfigureLandscapePlayerSettings()
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.WebGL.template = "PROJECT:Landscape16x9";
        }

        public static void EnforceEditorGameViewLandscape16x9()
        {
            try
            {
                Type gvType = Type.GetType("UnityEditor.GameView,UnityEditor");
                if (gvType == null) return;
                EditorWindow[] windows = Resources.FindObjectsOfTypeAll(gvType) as EditorWindow[];
                if (windows == null || windows.Length == 0) return;

                PropertyInfo selectedSizeIndexProp = gvType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (selectedSizeIndexProp != null)
                {
                    // Index 1 in Standalone/WebGL GameView sizes is 16:9 Aspect
                    foreach (EditorWindow win in windows)
                    {
                        selectedSizeIndexProp.SetValue(win, 1, null);
                        win.Repaint();
                    }
                }
            }
            catch
            {
                // Ignore if running in headless -batchmode without GUI windows
            }
        }

        private static void ConfigureTextureImporters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites", "Assets/Sprites" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    bool changed = false;
                    if (ti.textureType != TextureImporterType.Sprite)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    FilterMode targetFilter = path.Contains("light_halo") ? FilterMode.Bilinear : FilterMode.Point;
                    if (ti.filterMode != targetFilter)
                    {
                        ti.filterMode = targetFilter;
                        changed = true;
                    }
                    float targetPpu = path.Contains("parallax_") ? 16f : 32f;
                    if (Mathf.Abs(ti.spritePixelsPerUnit - targetPpu) > 0.1f)
                    {
                        ti.spritePixelsPerUnit = targetPpu;
                        changed = true;
                    }
                    if (ti.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        ti.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }
                    if (changed)
                    {
                        ti.SaveAndReimport();
                    }
                }
            }
        }

        private static void BuildReusablePrefabs()
        {
            Directory.CreateDirectory("Assets/Prefabs");

            CreatePrefabWithComponent<PlayerController>("Assets/Prefabs/Protagonist.prefab");
            CreatePrefabWithComponent<ShadowController>("Assets/Prefabs/Shadow.prefab");
            CreatePrefabWithComponent<TorchController>("Assets/Prefabs/Torch.prefab");
            CreatePrefabWithComponent<TorchPedestal>("Assets/Prefabs/TorchPedestal.prefab");
            CreatePrefabWithComponent<PressurePlate>("Assets/Prefabs/PressurePlate.prefab");
            CreatePrefabWithComponent<LightSwitch>("Assets/Prefabs/LightSwitch.prefab");
            CreatePrefabWithComponent<ShadowSwitch>("Assets/Prefabs/ShadowSwitch.prefab");
            CreatePrefabWithComponent<ShadowPassage>("Assets/Prefabs/ShadowPassage.prefab");
            CreatePrefabWithComponent<DarknessBarrier>("Assets/Prefabs/DarknessBarrier.prefab");
            CreatePrefabWithComponent<MovableBlock>("Assets/Prefabs/MovableBlock.prefab");
            CreatePrefabWithComponent<Checkpoint>("Assets/Prefabs/Checkpoint.prefab");
            CreatePrefabWithComponent<AncientDoor>("Assets/Prefabs/AncientDoor.prefab");
            CreatePrefabWithComponent<LevelExitPortal>("Assets/Prefabs/ExitSanctuary.prefab");

            Debug.Log("[ProjectSetupBuilder] Created 13 reusable Prefabs in Assets/Prefabs/.");
        }

        private static void CreatePrefabWithComponent<T>(string prefabPath) where T : Component
        {
            GameObject temp = new GameObject(typeof(T).Name);
            temp.AddComponent<T>();
            PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            UnityEngine.Object.DestroyImmediate(temp);
        }

        private static void BuildScenes()
        {
            Directory.CreateDirectory("Assets/Scenes");

            Scene levelScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.backgroundColor = new Color(0.031f, 0.039f, 0.071f, 1f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<CameraController>();

            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
            gmObj.AddComponent<LevelManager>();
            gmObj.AddComponent<AudioManager>();
            gmObj.AddComponent<UIManager>();
            gmObj.AddComponent<TutorialDirector>();

            string level1Path = "Assets/Scenes/Level1_Awakening.unity";
            EditorSceneManager.SaveScene(levelScene, level1Path);
            EditorSceneManager.SaveScene(levelScene, "Assets/Scenes/SampleScene.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(level1Path, true)
            };

            Debug.Log("[ProjectSetupBuilder] Saved Level1_Awakening.unity and configured EditorBuildSettings.");
        }

        private static void RunAutomatedVerification()
        {
            // 1. Verify Landscape 16:9 Display & PlayerSettings
            if (PlayerSettings.defaultScreenWidth != 1920 || PlayerSettings.defaultScreenHeight != 1080)
            {
                throw new Exception("[Verification Failed] PlayerSettings base resolution is not 1920x1080!");
            }
            if (PlayerSettings.allowedAutorotateToPortrait || PlayerSettings.allowedAutorotateToPortraitUpsideDown)
            {
                throw new Exception("[Verification Failed] Portrait autorotation must be disabled!");
            }
            if (!File.Exists("Assets/WebGLTemplates/Landscape16x9/index.html"))
            {
                throw new Exception("[Verification Failed] Custom 16:9 WebGL template is missing!");
            }

            // 2. Verify Audio Mixing Hierarchy (Music 0 dB > SFX -8 dB > Ambience -12 dB > Fire -18 dB)
            float musicGain = AudioManager.DbToLinear(AudioManager.MusicBusDb);
            float sfxGain = AudioManager.DbToLinear(AudioManager.SfxBusDb);
            float ambienceGain = AudioManager.DbToLinear(AudioManager.AmbienceBusDb);
            float fireGain = AudioManager.DbToLinear(AudioManager.FireRelativeToMusicDb);

            if (!(musicGain > sfxGain && sfxGain > ambienceGain && ambienceGain > fireGain))
            {
                throw new Exception($"[Verification Failed] Audio bus gain hierarchy invalid: Music={musicGain}, SFX={sfxGain}, Ambience={ambienceGain}, Fire={fireGain}");
            }

            // 3. Verify Level 1 ("Awakening"), Level 2 ("The Separation"), & Idempotent Single-Instance Lifecycle
            GameObject testObj = new GameObject("VerificationRunner");
            LevelManager lm = testObj.AddComponent<LevelManager>();
            TutorialDirector td = testObj.AddComponent<TutorialDirector>();

            try
            {
                for (int lvl = 0; lvl < lm.TotalLevels; lvl++)
                {
                    lm.BuildLevel(lvl, null);
                    if (lm.Player == null || lm.Shadow == null || lm.Torch == null)
                    {
                        throw new Exception($"[Verification Failed] Level {lvl + 1} missing Player, Shadow, or Torch!");
                    }

                    // Verify exactly 1 PlayerController, 1 ShadowController, and 1 TorchController exist per level build
                    int playerCount = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
                    int shadowCount = UnityEngine.Object.FindObjectsByType<ShadowController>(FindObjectsSortMode.None).Length;
                    int torchCount = UnityEngine.Object.FindObjectsByType<TorchController>(FindObjectsSortMode.None).Length;
                    if (playerCount != 1 || shadowCount != 1 || torchCount != 1)
                    {
                        throw new Exception($"[Verification Failed] Duplicated entities in Level {lvl + 1}: Players={playerCount}, Shadows={shadowCount}, Torches={torchCount}!");
                    }
                }

                // Test repeated Level 1 rebuild (idempotency check) + step-by-step tutorial & puzzle flow
                lm.BuildLevel(0, null);
                lm.BuildLevel(0, null);
                td.InitializeForLevel(0, lm, null);

                if (td.CurrentStep != TutorialDirector.AwakeningStep.Move)
                {
                    throw new Exception($"[Verification Failed] Expected initial tutorial step Move, got {td.CurrentStep}");
                }

                // Move player near torch -> step transitions to PickUpTorch
                lm.Player.transform.position = lm.Torch.transform.position + new Vector3(-0.5f, 0f, 0f);
                td.EvaluateAwakeningProgression(false);
                if (td.CurrentStep != TutorialDirector.AwakeningStep.PickUpTorch)
                {
                    throw new Exception($"[Verification Failed] Expected PickUpTorch step, got {td.CurrentStep}");
                }

                // Player picks up Torch -> step transitions to PlaceTorchOnPedestal
                lm.Player.ForceEquipTorch(lm.Torch);
                td.EvaluateAwakeningProgression(false);
                if (td.CurrentStep != TutorialDirector.AwakeningStep.PlaceTorchOnPedestal)
                {
                    throw new Exception($"[Verification Failed] Expected PlaceTorchOnPedestal step, got {td.CurrentStep}");
                }

                // Player places Torch on Pedestal -> step transitions to SwitchToShadow
                TorchPedestal pedestal = UnityEngine.Object.FindFirstObjectByType<TorchPedestal>();
                ShadowSwitch shadowSw = UnityEngine.Object.FindFirstObjectByType<ShadowSwitch>();
                ShadowPassage passage = UnityEngine.Object.FindFirstObjectByType<ShadowPassage>();
                AncientDoor door = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
                LevelExitPortal exitPortal = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();

                if (pedestal == null || shadowSw == null || passage == null || door == null || exitPortal == null)
                {
                    throw new Exception("[Verification Failed] Level 1 Awakening puzzle components not found!");
                }

                // Verify ShadowPassage starts closed and physically blocking the Protagonist
                if (!passage.IsBlockingProtagonist || passage.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 1 ShadowPassage should start closed and blocking!");
                }

                // Verify AncientDoor created visual RuneConduit lines for both required nodes
                if (door.RequiredNodes.Count != 2)
                {
                    throw new Exception("[Verification Failed] Level 1 AncientDoor should have 2 required nodes connected via Rune Conduits!");
                }

                lm.Torch.PlaceInPedestal(pedestal);
                td.EvaluateAwakeningProgression(false);
                if (td.CurrentStep != TutorialDirector.AwakeningStep.SwitchToShadow)
                {
                    throw new Exception($"[Verification Failed] Expected SwitchToShadow step after placing torch, got {td.CurrentStep}");
                }

                // Verify TutorialDirector never shows EnterSanctuaryExit if ShadowPassage is still blocking even if door were forced open
                door.SetOpen(true);
                td.EvaluateAwakeningProgression(false);
                if (td.CurrentStep == TutorialDirector.AwakeningStep.EnterSanctuaryExit)
                {
                    throw new Exception("[Verification Failed] TutorialDirector showed EnterSanctuaryExit while ShadowPassage was still physically blocking!");
                }
                door.SetOpen(false);

                // Shadow activates ShadowSwitch -> Both ShadowPassage AND AncientDoor open!
                shadowSw.ActivateByShadow(lm.Shadow);
                if (!door.IsOpen)
                {
                    throw new Exception("[Verification Failed] AncientDoor failed to open after Pedestal + ShadowSwitch activation!");
                }
                if (!passage.IsOpen || passage.IsBlockingProtagonist)
                {
                    throw new Exception("[Verification Failed] ShadowPassage remained closed/blocking after ShadowSwitch activation!");
                }

                // Physical Linecast check along the ground corridor (y = -2.0) from x = 7.5 (before ShadowPassage) to x = 18.8 (ExitSanctuary)
                Physics2D.SyncTransforms();
                RaycastHit2D[] pathHits = Physics2D.LinecastAll(new Vector2(7.5f, -2.0f), exitPortal.transform.position);
                foreach (RaycastHit2D hit in pathHits)
                {
                    if (hit.collider != null && !hit.collider.isTrigger)
                    {
                        if (hit.collider.GetComponent<PlayerController>() != null ||
                            hit.collider.GetComponent<ShadowController>() != null)
                        {
                            continue;
                        }
                        throw new Exception($"[Verification Failed] Protagonist path to ExitSanctuary is still physically blocked by non-trigger collider: {hit.collider.name} at {hit.point}!");
                    }
                }

                // Now that both ShadowPassage and AncientDoor are physically open, TutorialDirector advances to EnterSanctuaryExit
                td.EvaluateAwakeningProgression(false);
                if (td.CurrentStep != TutorialDirector.AwakeningStep.EnterSanctuaryExit)
                {
                    throw new Exception($"[Verification Failed] Expected EnterSanctuaryExit step after path is clear, got {td.CurrentStep}");
                }

                // Verify Protagonist entering LevelExitPortal triggers level completion event cleanly
                bool exitEventFired = false;
                Action onExit = () => { exitEventFired = true; };
                LevelExitPortal.OnLevelExitReached += onExit;
                try
                {
                    lm.Player.transform.position = exitPortal.transform.position;
                    bool completed = exitPortal.TryCompleteLevel(lm.Player);
                    if (!completed || !exitEventFired)
                    {
                        throw new Exception("[Verification Failed] LevelExitPortal did not trigger completion when Protagonist reached exit!");
                    }
                }
                finally
                {
                    LevelExitPortal.OnLevelExitReached -= onExit;
                }

                // 4. Verify Level 2 ("The Separation") + Shadow Double Jump + Reduced Gravity + Air Control
                lm.BuildLevel(1, null);
                ShadowController shadow2 = lm.Shadow;
                if (!Mathf.Approximately(shadow2.GravityScale, 1.3f))
                {
                    throw new Exception($"[Verification Failed] Expected Shadow gravity scale 1.3f (50% of Player 2.6f), got {shadow2.GravityScale}");
                }

                float gEff = Mathf.Abs(Physics2D.gravity.y) * shadow2.GravityScale;
                float h1 = (shadow2.FirstJumpForce * shadow2.FirstJumpForce) / (2f * gEff);
                float h2 = (shadow2.SecondJumpForce * shadow2.SecondJumpForce) / (2f * gEff);
                float totalJumpApex = h1 + h2;
                if (totalJumpApex < 4.0f || totalJumpApex > 5.2f)
                {
                    throw new Exception($"[Verification Failed] Shadow double jump apex ({totalJumpApex:F2}u) out of balanced target range [4.0u..5.2u] for 3.47u upper platform!");
                }

                // Verify 2-jump state machine: 1st jump succeeds, 2nd mid-air jump succeeds, 3rd jump rejected until grounded
                shadow2.SetGroundedStateForTest(true);
                if (!shadow2.TryPerformJumpForTest() || shadow2.JumpsRemaining != 1)
                {
                    throw new Exception("[Verification Failed] Shadow 1st jump from ground failed or did not leave 1 jump remaining!");
                }
                if (!shadow2.TryPerformJumpForTest() || shadow2.JumpsRemaining != 0)
                {
                    throw new Exception("[Verification Failed] Shadow 2nd jump in mid-air failed or did not leave 0 jumps remaining!");
                }
                if (shadow2.TryPerformJumpForTest())
                {
                    throw new Exception("[Verification Failed] Shadow allowed a 3rd jump in mid-air (infinite jump bug)!");
                }
                shadow2.SetGroundedStateForTest(true);
                if (shadow2.JumpsRemaining != ShadowController.MaxJumps)
                {
                    throw new Exception("[Verification Failed] Shadow jumps did not reset upon landing on valid ground!");
                }

                // Verify Level 2 Dual Pressure Plate cooperation & physical path to Level 3
                PressurePlate[] plates = UnityEngine.Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
                AncientDoor door2 = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
                ShadowPassage passage2 = UnityEngine.Object.FindFirstObjectByType<ShadowPassage>();
                LevelExitPortal exitPortal2 = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();

                if (plates.Length < 2 || door2 == null || passage2 == null || exitPortal2 == null)
                {
                    throw new Exception("[Verification Failed] Level 2 puzzle components missing!");
                }

                PressurePlate lowerPlate = plates[0].transform.position.y < 0f ? plates[0] : plates[1];
                PressurePlate upperPlate = plates[0].transform.position.y >= 0f ? plates[0] : plates[1];

                // Verify Shadow ignores collision with upper ShadowPassage while Protagonist is blocked by it
                passage2.EnsureInitialized();
                shadow2.EnsureInitialized();
                Collider2D shadowCol = shadow2.Collider;
                Collider2D passageCol = passage2.GetComponent<Collider2D>();
                if (!Physics2D.GetIgnoreCollision(shadowCol, passageCol))
                {
                    throw new Exception("[Verification Failed] Level 2 ShadowPassage is not ignoring collision with Shadow!");
                }

                // Position Protagonist on lower plate and Shadow (after double jump + passing ShadowPassage) on upper plate
                lm.Player.transform.position = lowerPlate.transform.position + new Vector3(0f, 0.5f, 0f);
                shadow2.transform.position = upperPlate.transform.position + new Vector3(0f, 0.5f, 0f);
                Physics2D.SyncTransforms();

                lowerPlate.EvaluateOccupancy();
                upperPlate.EvaluateOccupancy();

                if (!lowerPlate.IsActivated || !upperPlate.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 2 lower or upper PressurePlate failed to activate when occupied!");
                }
                if (!door2.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 2 AncientDoor failed to open when both PressurePlates were activated!");
                }

                // Verify AncientDoor latches open when Protagonist steps off lowerPlate to walk to ExitSanctuary
                lm.Player.transform.position = new Vector3(6.0f, -2.2f, 0f);
                Physics2D.SyncTransforms();
                lowerPlate.EvaluateOccupancy();
                if (!door2.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 2 AncientDoor did not stay latched open when Protagonist walked toward exit!");
                }

                // Physical Linecast check along the ground floor (y = -2.0) from x = 1.5 to ExitSanctuary (x = 19.0)
                RaycastHit2D[] pathHits2 = Physics2D.LinecastAll(new Vector2(1.5f, -2.0f), exitPortal2.transform.position);
                foreach (RaycastHit2D hit in pathHits2)
                {
                    if (hit.collider != null && !hit.collider.isTrigger)
                    {
                        if (hit.collider.GetComponent<PlayerController>() != null ||
                            hit.collider.GetComponent<ShadowController>() != null)
                        {
                            continue;
                        }
                        throw new Exception($"[Verification Failed] Level 2 Protagonist path to ExitSanctuary is blocked by: {hit.collider.name} at {hit.point}!");
                    }
                }

                bool level2ExitFired = false;
                Action onExit2 = () => { level2ExitFired = true; };
                LevelExitPortal.OnLevelExitReached += onExit2;
                try
                {
                    lm.Player.transform.position = exitPortal2.transform.position;
                    if (!exitPortal2.TryCompleteLevel(lm.Player) || !level2ExitFired)
                    {
                        throw new Exception("[Verification Failed] Level 2 ExitSanctuary did not trigger transition to Level 3!");
                    }
                }
                finally
                {
                    LevelExitPortal.OnLevelExitReached -= onExit2;
                }

                // =====================================================================
                // 5. Verify Level 8 ("MY SHADOW QUIT") Grand Finale & Reunion Completion
                // =====================================================================
                lm.BuildLevel(7, null);
                PressurePlate plate8 = UnityEngine.Object.FindFirstObjectByType<PressurePlate>();
                LightSwitch crystal8 = UnityEngine.Object.FindFirstObjectByType<LightSwitch>();
                TorchPedestal pedestal8 = UnityEngine.Object.FindFirstObjectByType<TorchPedestal>();
                ShadowPassage passage8 = UnityEngine.Object.FindFirstObjectByType<ShadowPassage>();
                ShadowSwitch shadowSw8 = UnityEngine.Object.FindFirstObjectByType<ShadowSwitch>();
                AncientDoor finalDoor8 = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
                LevelExitPortal exitPortal8 = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();

                if (plate8 == null || crystal8 == null || pedestal8 == null || passage8 == null || shadowSw8 == null || finalDoor8 == null || exitPortal8 == null)
                {
                    throw new Exception("[Verification Failed] Level 8 puzzle components missing!");
                }
                if (!plate8.StayPressedOnceActivated)
                {
                    throw new Exception("[Verification Failed] Level 8 PressurePlate must latch once activated!");
                }
                if (!exitPortal8.RequireReconnectedShadow)
                {
                    throw new Exception("[Verification Failed] Level 8 ExitSanctuary must require reconnected Shadow!");
                }

                // Step 1: Activate PressurePlate via physical occupancy & verify latch
                lm.Player.transform.position = plate8.transform.position + new Vector3(0f, 0.35f, 0f);
                Physics2D.SyncTransforms();
                plate8.EvaluateOccupancy();
                if (!plate8.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 8 PressurePlate did not activate when occupied!");
                }
                // Step 2: Place Torch on Pedestal & illuminate Solar Crystal
                lm.Torch.PlaceInPedestal(pedestal8);
                crystal8.EvaluateIllumination();
                if (!crystal8.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 8 Solar Crystal did not activate when Torch placed on altar Pedestal!");
                }
                // Step 3: Shadow passes ShadowPassage and activates ShadowSwitch
                lm.Shadow.transform.position = shadowSw8.transform.position;
                shadowSw8.ActivateByShadow(lm.Shadow);

                if (!passage8.IsOpen || passage8.IsBlockingProtagonist)
                {
                    throw new Exception("[Verification Failed] Level 8 ShadowPassage did not unlock after ShadowSwitch activation!");
                }
                if (!finalDoor8.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 8 Final AncientDoor did not open after all 3 seals were activated!");
                }

                // Step 4: Protagonist arrives at Sanctuary Arch first while Shadow is still at ShadowSwitch (x = 13.0)
                lm.Player.transform.position = exitPortal8.transform.position;
                exitPortal8.EvaluatePortalProximity();
                if (exitPortal8.IsTriggered)
                {
                    throw new Exception("[Verification Failed] Level 8 ExitSanctuary triggered before Shadow reunited with Protagonist!");
                }

                // Step 5: Shadow walks into Sanctuary Arch beside stationary Protagonist -> Portal triggers!
                bool level8ExitFired = false;
                Action onExit8 = () => { level8ExitFired = true; };
                LevelExitPortal.OnLevelExitReached += onExit8;
                try
                {
                    lm.Shadow.transform.position = exitPortal8.transform.position + new Vector3(-0.6f, 0f, 0f);
                    Physics2D.SyncTransforms();
                    exitPortal8.EvaluatePortalProximity();
                    if (!exitPortal8.IsTriggered || !level8ExitFired)
                    {
                        throw new Exception("[Verification Failed] Level 8 ExitSanctuary did not trigger when Shadow reunited with Protagonist at the Arch!");
                    }
                }
                finally
                {
                    LevelExitPortal.OnLevelExitReached -= onExit8;
                }
                // =====================================================================
                // 6. Verify World System (World 1: 10 Levels, World 2: Coming Soon)
                // =====================================================================
                if (lm.TotalLevels != 10)
                {
                    throw new Exception($"[Verification Failed] Expected World 1 to have 10 levels, got {lm.TotalLevels}!");
                }
                WorldDefinition world1Def = WorldCatalog.GetWorld(0);
                WorldDefinition world2Def = WorldCatalog.GetWorld(1);
                if (world1Def == null || world1Def.LevelCount != 10 || !world1Def.IsUnlocked || !world1Def.IsImplemented)
                {
                    throw new Exception("[Verification Failed] World 1 definition invalid or not 10 levels!");
                }
                if (world2Def == null || world2Def.IsUnlocked || world2Def.IsImplemented)
                {
                    throw new Exception("[Verification Failed] World 2 definition should be present as Locked / Coming Soon!");
                }

                // =====================================================================
                // 7. Verify Level 9 ("IX. ECHOES IN THE DARK") Complete Puzzle Flow
                // =====================================================================
                lm.BuildLevel(8, null);
                TorchPedestal pedestal9 = UnityEngine.Object.FindFirstObjectByType<TorchPedestal>();
                LightSwitch crystal9 = UnityEngine.Object.FindFirstObjectByType<LightSwitch>();
                DarknessBarrier barrier9 = UnityEngine.Object.FindFirstObjectByType<DarknessBarrier>();
                ShadowSwitch shadowSw9 = UnityEngine.Object.FindFirstObjectByType<ShadowSwitch>();
                MovableBlock block9 = UnityEngine.Object.FindFirstObjectByType<MovableBlock>();
                PressurePlate plate9 = UnityEngine.Object.FindFirstObjectByType<PressurePlate>();
                AncientDoor[] doors9 = UnityEngine.Object.FindObjectsByType<AncientDoor>(FindObjectsSortMode.None);
                LevelExitPortal exitPortal9 = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();

                if (pedestal9 == null || crystal9 == null || barrier9 == null || shadowSw9 == null || block9 == null || plate9 == null || doors9.Length < 2 || exitPortal9 == null)
                {
                    throw new Exception("[Verification Failed] Level 9 (Echoes in the Dark) puzzle components missing!");
                }

                AncientDoor lowerGate9 = doors9[0].transform.position.x < 10f ? doors9[0] : doors9[1];
                AncientDoor exitDoor9 = doors9[0].transform.position.x >= 10f ? doors9[0] : doors9[1];

                // Step 1: Place Torch on West Altar Pedestal -> illuminates Solar Crystal & dissolves upper DarknessBarrier
                lm.Torch.PlaceInPedestal(pedestal9);
                crystal9.EvaluateIllumination();
                if (!crystal9.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 9 Solar Crystal did not activate when Torch placed on West Altar Pedestal!");
                }

                // Step 2: Shadow double jumps to Upper Route and activates ShadowSwitch -> unlocks lowerGate9
                lm.Shadow.transform.position = shadowSw9.transform.position;
                shadowSw9.ActivateByShadow(lm.Shadow);
                if (!lowerGate9.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 9 lower AncientDoor did not unlock when ShadowSwitch was activated!");
                }

                // Step 3: Protagonist pushes MovableBlock onto lower PressurePlate -> exitDoor9 opens!
                block9.transform.position = plate9.transform.position + new Vector3(0f, 0.45f, 0f);
                Physics2D.SyncTransforms();
                plate9.EvaluateOccupancy();
                if (!plate9.IsActivated || !exitDoor9.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 9 exit AncientDoor did not open after Crystal + ShadowSwitch + MovableBlock on PressurePlate!");
                }

                // Step 4: Protagonist reaches ExitSanctuary -> transitions to Level 10
                bool level9ExitFired = false;
                Action onExit9 = () => { level9ExitFired = true; };
                LevelExitPortal.OnLevelExitReached += onExit9;
                try
                {
                    lm.Player.transform.position = exitPortal9.transform.position;
                    if (!exitPortal9.TryCompleteLevel(lm.Player) || !level9ExitFired)
                    {
                        throw new Exception("[Verification Failed] Level 9 ExitSanctuary did not trigger transition to Level 10!");
                    }
                }
                finally
                {
                    LevelExitPortal.OnLevelExitReached -= onExit9;
                }

                // =====================================================================
                // 8. Verify Level 10 ("X. THE FIRST DAWN") Multi-Phase World 1 Finale
                // =====================================================================
                lm.BuildLevel(9, null);
                PressurePlate plate10 = UnityEngine.Object.FindFirstObjectByType<PressurePlate>();
                MovableBlock block10 = UnityEngine.Object.FindFirstObjectByType<MovableBlock>();
                TorchPedestal[] pedestals10 = UnityEngine.Object.FindObjectsByType<TorchPedestal>(FindObjectsSortMode.None);
                LightSwitch[] crystals10 = UnityEngine.Object.FindObjectsByType<LightSwitch>(FindObjectsSortMode.None);
                ShadowSwitch[] shadowSwitches10 = UnityEngine.Object.FindObjectsByType<ShadowSwitch>(FindObjectsSortMode.None);
                ShadowPassage[] passages10 = UnityEngine.Object.FindObjectsByType<ShadowPassage>(FindObjectsSortMode.None);
                AncientDoor ceremonialDoor10 = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
                LevelExitPortal dawnPortal10 = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();

                if (plate10 == null || block10 == null || pedestals10.Length < 2 || crystals10.Length < 2 || shadowSwitches10.Length < 2 || passages10.Length < 2 || ceremonialDoor10 == null || dawnPortal10 == null)
                {
                    throw new Exception("[Verification Failed] Level 10 (The First Dawn) puzzle components missing!");
                }
                if (ceremonialDoor10.RequiredNodes.Count != 5)
                {
                    throw new Exception($"[Verification Failed] Level 10 Ceremonial Gate must require all 5 seals, got {ceremonialDoor10.RequiredNodes.Count}!");
                }
                if (!dawnPortal10.RequireReconnectedShadow)
                {
                    throw new Exception("[Verification Failed] Level 10 Dawn Portal must require both Protagonist and Shadow to reunite!");
                }

                TorchPedestal phase2Ped = pedestals10[0].transform.position.x < 10f ? pedestals10[0] : pedestals10[1];
                TorchPedestal phase4Ped = pedestals10[0].transform.position.x >= 10f ? pedestals10[0] : pedestals10[1];
                LightSwitch phase2Crys = crystals10[0].transform.position.x < 10f ? crystals10[0] : crystals10[1];
                LightSwitch phase4Crys = crystals10[0].transform.position.x >= 10f ? crystals10[0] : crystals10[1];
                ShadowSwitch phase3Sw = shadowSwitches10[0].transform.position.x < 16f ? shadowSwitches10[0] : shadowSwitches10[1];
                ShadowSwitch phase4Sw = shadowSwitches10[0].transform.position.x >= 16f ? shadowSwitches10[0] : shadowSwitches10[1];

                // Phase 1: Push MovableBlock onto phase1Plate -> Seal 1 active
                block10.transform.position = plate10.transform.position + new Vector3(0f, 0.45f, 0f);
                Physics2D.SyncTransforms();
                plate10.EvaluateOccupancy();
                if (!plate10.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 10 Phase 1 PressurePlate failed to activate!");
                }

                // Phase 2: Place Torch on Central Altar Pedestal -> Seal 2 (phase2Crystal) active & latch
                lm.Torch.PlaceInPedestal(phase2Ped);
                phase2Crys.EvaluateIllumination();
                if (!phase2Crys.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 10 Phase 2 Solar Crystal failed to activate!");
                }

                // Phase 3: Shadow double jumps to High Balcony and activates phase3Sw -> Seal 3 active & lower passage opens
                lm.Shadow.transform.position = phase3Sw.transform.position;
                phase3Sw.ActivateByShadow(lm.Shadow);
                if (!phase3Sw.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 10 Phase 3 ShadowSwitch failed to activate!");
                }

                // Phase 4: Protagonist carries Torch to Eastern Wing phase4Ped -> Seal 4 (phase4Crystal) active; Shadow activates phase4Sw -> Seal 5 active
                lm.Player.ForceEquipTorch(lm.Torch);
                lm.Torch.PlaceInPedestal(phase4Ped);
                phase4Crys.EvaluateIllumination();
                if (!phase4Crys.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 10 Phase 4 Eastern Solar Crystal failed to activate!");
                }

                lm.Shadow.transform.position = phase4Sw.transform.position;
                phase4Sw.ActivateByShadow(lm.Shadow);
                if (!phase4Sw.IsActivated)
                {
                    throw new Exception("[Verification Failed] Level 10 Phase 4 Eastern ShadowSwitch failed to activate!");
                }

                // Phase 5: Ceremonial Gate opens when all 5 seals are active!
                if (!ceremonialDoor10.IsOpen)
                {
                    throw new Exception("[Verification Failed] Level 10 Ceremonial Gate did not open when all 5 seals were activated!");
                }

                // Phase 6: Protagonist arrives at Dawn Portal first -> waits for Shadow; Shadow reunites -> triggers World 1 Complete!
                lm.Player.transform.position = dawnPortal10.transform.position;
                dawnPortal10.EvaluatePortalProximity();
                if (dawnPortal10.IsTriggered)
                {
                    throw new Exception("[Verification Failed] Level 10 Dawn Portal triggered before Shadow reunited with Protagonist!");
                }

                bool level10ExitFired = false;
                Action onExit10 = () => { level10ExitFired = true; };
                LevelExitPortal.OnLevelExitReached += onExit10;
                try
                {
                    lm.Shadow.transform.position = dawnPortal10.transform.position + new Vector3(-0.6f, 0f, 0f);
                    Physics2D.SyncTransforms();
                    dawnPortal10.EvaluatePortalProximity();
                    if (!dawnPortal10.IsTriggered || !level10ExitFired)
                    {
                        throw new Exception("[Verification Failed] Level 10 Dawn Portal did not trigger when Flame and Shadow reunited!");
                    }
                }
                finally
                {
                    LevelExitPortal.OnLevelExitReached -= onExit10;
                }
            }
            finally
            {
                if (lm != null) lm.ClearCurrentLevel();
                if (testObj != null) UnityEngine.Object.DestroyImmediate(testObj);
                CleanupEditModeSceneArtifacts(saveIfCleaned: false);
            }

            // 9. Verify EditMode scene is 100% clean (0 leaked Protagonists, Shadows, Torches; 1 of each core Manager)
            int leakedPlayers = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
            int leakedShadows = UnityEngine.Object.FindObjectsByType<ShadowController>(FindObjectsSortMode.None).Length;
            int leakedTorches = UnityEngine.Object.FindObjectsByType<TorchController>(FindObjectsSortMode.None).Length;
            if (leakedPlayers != 0 || leakedShadows != 0 || leakedTorches != 0)
            {
                throw new Exception($"[Verification Failed] Leaked entities remain in EditMode scene: Players={leakedPlayers}, Shadows={leakedShadows}, Torches={leakedTorches}!");
            }

            Debug.Log("[Verification Passed] World 1 (10 Levels: I. Awakening to X. The First Dawn), Level 9 & Level 10 Solvability, World 1 Complete Transition, World System + World 2 Coming Soon, Zero Duplicated Objects, Shadow Double Jump, Audio dB Mix, and 16:9 Landscape verified!");
        }

        [MenuItem("MyShadowQuit/Build WebGL Release")]
        public static void BuildWebGL()
        {
            BuildAndVerifyAll();
            string buildPath = "Builds/WebGL";
            Directory.CreateDirectory(buildPath);

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Level1_Awakening.unity" },
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[ProjectSetupBuilder] WebGL Build Result: {report.summary.result}, Total Size: {report.summary.totalSize} bytes");
        }
    }
}
#endif
