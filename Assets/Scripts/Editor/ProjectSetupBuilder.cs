#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.UI;
using MyShadowQuit.Cosmetics;
using MyShadowQuit.Economy;
using MyShadowQuit.Monetization.IAP;

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
                    }
                    catch (Exception ex)
                    {
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
            SaveOrUpdateWorldAsset(w2, "Assets/Resources/Worlds/World2_TheInvertedForest.asset");

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
            PlayerSettings.productName = "MY SHADOW QUIT.";
            PlayerSettings.companyName = "Logmat";
            PlayerSettings.applicationIdentifier = "com.logmat.myshadowquit";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.logmat.myshadowquit");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.WebGL, "com.logmat.myshadowquit");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.logmat.myshadowquit");
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
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            Texture2D masterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Branding/master_app_icon_1024.png");
            if (masterIcon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { masterIcon }, IconKind.Application);
                PlayerSettings.SetIcons(NamedBuildTarget.Android, new[] { masterIcon }, IconKind.Application);
            }
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
                if (world2Def == null || !world2Def.IsImplemented || world2Def.LevelCount != 10)
                {
                    throw new Exception("[Verification Failed] World 2 definition should be present and implemented with 10 levels!");
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

                // =====================================================================
                // 9. Verify HOME, WORLD SELECT, LEVEL SELECT Ruins Map & Persistence
                // =====================================================================
                string[] requiredUiSprites =
                {
                    "Assets/Resources/Sprites/UI/home_ruins_bg.png",
                    "Assets/Resources/Sprites/UI/home_foreground_arch.png",
                    "Assets/Resources/Sprites/UI/logo_emblem_crest.png",
                    "Assets/Resources/Sprites/UI/world1_card_art.png",
                    "Assets/Resources/Sprites/UI/world2_card_art.png",
                    "Assets/Resources/Sprites/UI/world_card_frame_active.png",
                    "Assets/Resources/Sprites/UI/world_card_frame_locked.png",
                    "Assets/Resources/Sprites/UI/world1_ruins_map.png",
                    "Assets/Resources/Sprites/UI/world2_forest_map.png",
                    "Assets/Resources/Sprites/UI/map_node_completed.png",
                    "Assets/Resources/Sprites/UI/map_node_available.png",
                    "Assets/Resources/Sprites/UI/map_node_locked.png",
                    "Assets/Resources/Sprites/UI/map_node_selected_ring.png",
                    "Assets/Resources/Sprites/UI/map_path_segment_lit.png",
                    "Assets/Resources/Sprites/UI/map_path_segment_dark.png"
                };
                foreach (string spritePath in requiredUiSprites)
                {
                    if (!File.Exists(spritePath))
                    {
                        throw new Exception($"[Verification Failed] Missing custom UI/Map sprite asset: {spritePath}");
                    }
                }

                string[] requiredBrandingFiles =
                {
                    "Assets/Art/Branding/master_app_icon_1024.png",
                    "Assets/Art/Branding/android_adaptive_bg.png",
                    "Assets/Art/Branding/android_adaptive_fg.png",
                    "Assets/Art/Branding/app_icon_512.png",
                    "Assets/Art/Branding/favicon.png",
                    "Assets/Art/Branding/favicon.ico",
                    "Assets/WebGLTemplates/Landscape16x9/favicon.png",
                    "Assets/WebGLTemplates/Landscape16x9/favicon.ico"
                };
                foreach (string bPath in requiredBrandingFiles)
                {
                    if (!File.Exists(bPath))
                    {
                        throw new Exception($"[Verification Failed] Missing branding asset: {bPath}");
                    }
                }

                string[] requiredWorld2Sprites =
                {
                    "Assets/Resources/Sprites/World2/moving_platform.png",
                    "Assets/Resources/Sprites/World2/temp_platform.png",
                    "Assets/Resources/Sprites/World2/temp_platform_fading.png",
                    "Assets/Resources/Sprites/World2/light_root_inactive.png",
                    "Assets/Resources/Sprites/World2/light_root_active.png",
                    "Assets/Resources/Sprites/World2/shadow_vine.png",
                    "Assets/Resources/Sprites/World2/light_bridge.png",
                    "Assets/Resources/Sprites/World2/forest_thorns.png",
                    "Assets/Resources/Sprites/World2/forest_platform.png",
                    "Assets/Resources/Sprites/World2/world2_forest_bg.png"
                };
                foreach (string w2Sprite in requiredWorld2Sprites)
                {
                    if (!File.Exists(w2Sprite))
                    {
                        throw new Exception($"[Verification Failed] Missing World 2 sprite asset: {w2Sprite}");
                    }
                }

                // Verify WorldProgressSave sequential unlock logic (Levels 1..10)
                WorldProgressSave.ClearWorldProgress("world_1");
                if (WorldProgressSave.HasSavedProgress("world_1"))
                {
                    throw new Exception("[Verification Failed] HasSavedProgress(\"world_1\") should be false after ClearWorldProgress!");
                }
                if (!WorldProgressSave.IsLevelUnlocked("world_1", 0, 10) || WorldProgressSave.IsLevelUnlocked("world_1", 1, 10))
                {
                    throw new Exception("[Verification Failed] Initially only Level 1 (index 0) should be unlocked!");
                }

                WorldProgressSave.ResetWorldProgress("world_1");
                if (!WorldProgressSave.HasSavedProgress("world_1") || WorldProgressSave.GetContinueLevelIndex("world_1", 10) != 0)
                {
                    throw new Exception("[Verification Failed] ResetWorldProgress(\"world_1\") should initialize save at Level 1 (index 0)!");
                }

                for (int i = 0; i < 10; i++)
                {
                    WorldProgressSave.MarkLevelCompleted("world_1", i, 10);
                    if (!WorldProgressSave.IsLevelCompleted("world_1", i, 10))
                    {
                        throw new Exception($"[Verification Failed] Level {i + 1} was not marked completed!");
                    }
                    if (i + 1 < 10 && !WorldProgressSave.IsLevelUnlocked("world_1", i + 1, 10))
                    {
                        throw new Exception($"[Verification Failed] Completing Level {i + 1} did not unlock Level {i + 2}!");
                    }
                }
                if (WorldProgressSave.GetCompletedLevelsCount("world_1", 10) != 10 || !WorldProgressSave.IsWorldCompleted("world_1"))
                {
                    throw new Exception("[Verification Failed] World 1 should report 10/10 completed levels and IsWorldCompleted == true!");
                }

                // Reset back to Level 1 completed, Level 2 available, Levels 3..10 locked for UIManager map test
                WorldProgressSave.ClearWorldProgress("world_1");
                WorldProgressSave.MarkLevelCompleted("world_1", 0, 10);

                // Verify UIManager HOME, WORLD SELECT, and LEVEL SELECT (10-node Map)
                UIManager uiTest = testObj.AddComponent<UIManager>();
                uiTest.EnsureInitialized();

                if (uiTest.MapNodeCount != 10)
                {
                    throw new Exception($"[Verification Failed] Expected 10 nodes in Level Select Ruins Map, got {uiTest.MapNodeCount}!");
                }

                uiTest.ShowMainMenu();
                if (!uiTest.IsHomeScreenVisible)
                {
                    throw new Exception("[Verification Failed] UIManager failed to activate HOME / Main Menu screen!");
                }

                uiTest.ShowWorldSelectScreen();
                if (!uiTest.IsWorldSelectScreenVisible)
                {
                    throw new Exception("[Verification Failed] UIManager failed to activate WORLD SELECT screen!");
                }

                uiTest.ShowLevelSelectScreen();
                if (!uiTest.IsLevelSelectScreenVisible)
                {
                    throw new Exception("[Verification Failed] UIManager failed to activate LEVEL SELECT Map screen!");
                }

                // Node 0 (Completed) & Node 1 (Available) must launch; Node 2 (Locked) must be blocked
                uiTest.SelectMapNode(0);
                if (!uiTest.TryLaunchSelectedMapLevel())
                {
                    throw new Exception("[Verification Failed] Completed Level 1 node could not be replayed from Level Select Map!");
                }
                uiTest.SelectMapNode(1);
                if (!uiTest.TryLaunchSelectedMapLevel())
                {
                    throw new Exception("[Verification Failed] Unlocked Level 2 node could not be launched from Level Select Map!");
                }
                uiTest.SelectMapNode(2);
                if (uiTest.TryLaunchSelectedMapLevel())
                {
                    throw new Exception("[Verification Failed] Locked Level 3 node was allowed to launch from Level Select Map!");
                }

                // Verify World 2 Map Switching & World 2 Complete Screen
                uiTest.SetSelectedMapWorld(1);
                if (uiTest.SelectedMapWorldIndex != 1)
                {
                    throw new Exception("[Verification Failed] SetSelectedMapWorld(1) failed to switch map world index to 1!");
                }
                uiTest.ShowWorld2CompleteScreen();
                if (!uiTest.IsWorld2CompleteScreenVisible)
                {
                    throw new Exception("[Verification Failed] ShowWorld2CompleteScreen failed to display World 2 Complete panel!");
                }
                uiTest.SetSelectedMapWorld(0);

                // =====================================================================
                // Verify World 2 (Levels 11 to 20) and 7 New Mechanics
                // =====================================================================
                lm.SetActiveWorld(world2Def);
                if (lm.TotalLevels != 10)
                {
                    throw new Exception($"[Verification Failed] Expected World 2 to have 10 levels, got {lm.TotalLevels}!");
                }

                for (int lvlIdx = 0; lvlIdx < 10; lvlIdx++)
                {
                    lm.BuildLevel(lvlIdx, null);
                    if (lm.Player == null || lm.Shadow == null || lm.Torch == null)
                    {
                        throw new Exception($"[Verification Failed] World 2 Level {lvlIdx + 11} missing core player/shadow/torch entities!");
                    }
                    LevelExitPortal exit = UnityEngine.Object.FindFirstObjectByType<LevelExitPortal>();
                    if (exit == null)
                    {
                        throw new Exception($"[Verification Failed] World 2 Level {lvlIdx + 11} missing LevelExitPortal!");
                    }
                }

                // Verify specific mechanic types exist across World 2 levels
                lm.BuildLevel(1, null); // Level 12: MovingPlatform2D
                if (UnityEngine.Object.FindFirstObjectByType<MovingPlatform2D>() == null)
                {
                    throw new Exception("[Verification Failed] Level 12 missing MovingPlatform2D component!");
                }

                lm.BuildLevel(0, null); // Level 11: TemporaryPlatform2D
                if (UnityEngine.Object.FindFirstObjectByType<TemporaryPlatform2D>() == null)
                {
                    throw new Exception("[Verification Failed] Level 11 missing TemporaryPlatform2D component!");
                }

                lm.BuildLevel(2, null); // Level 13: LightRootMechanism
                if (UnityEngine.Object.FindFirstObjectByType<LightRootMechanism>() == null)
                {
                    throw new Exception("[Verification Failed] Level 13 missing LightRootMechanism component!");
                }

                lm.BuildLevel(4, null); // Level 15: ShadowVinePassage
                if (UnityEngine.Object.FindFirstObjectByType<ShadowVinePassage>() == null)
                {
                    throw new Exception("[Verification Failed] Level 15 missing ShadowVinePassage component!");
                }

                lm.BuildLevel(5, null); // Level 16: LightBridgeMechanism
                if (UnityEngine.Object.FindFirstObjectByType<LightBridgeMechanism>() == null)
                {
                    throw new Exception("[Verification Failed] Level 16 missing LightBridgeMechanism component!");
                }

                lm.BuildLevel(6, null); // Level 17: ForestHazard
                if (UnityEngine.Object.FindFirstObjectByType<ForestHazard>() == null)
                {
                    throw new Exception("[Verification Failed] Level 17 missing ForestHazard component!");
                }

                lm.BuildLevel(7, null); // Level 18: CooperativeSwitch
                if (UnityEngine.Object.FindFirstObjectByType<CooperativeSwitch>() == null)
                {
                    throw new Exception("[Verification Failed] Level 18 missing CooperativeSwitch component!");
                }

                // Switch back to World 1 for subsequent tests
                lm.SetActiveWorld(world1Def);

                // =====================================================================
                // 10. Verify In-Game PAUSE Button & PAUSE Menu Architecture
                // =====================================================================
                string[] pauseSprites =
                {
                    "Assets/Resources/Sprites/UI/btn_pause_hud_normal.png",
                    "Assets/Resources/Sprites/UI/btn_pause_hud_hover.png",
                    "Assets/Resources/Sprites/UI/btn_pause_hud_pressed.png"
                };
                foreach (string pSprite in pauseSprites)
                {
                    if (!File.Exists(pSprite))
                    {
                        throw new Exception($"[Verification Failed] Missing pause button sprite asset: {pSprite}");
                    }
                }

                if (uiTest.InGamePauseButton == null)
                {
                    throw new Exception("[Verification Failed] InGamePauseButton was not created in HUD!");
                }
                RectTransform pauseBtnRect = uiTest.InGamePauseButton.GetComponent<RectTransform>();
                if (pauseBtnRect.sizeDelta != new Vector2(48f, 48f) || pauseBtnRect.anchoredPosition.x < 800f)
                {
                    throw new Exception($"[Verification Failed] InGamePauseButton rect invalid: size={pauseBtnRect.sizeDelta}, pos={pauseBtnRect.anchoredPosition}!");
                }

                if (uiTest.PauseButtonCount != 5)
                {
                    throw new Exception($"[Verification Failed] Expected 5 buttons in Pause Menu, got {uiTest.PauseButtonCount}!");
                }

                uiTest.SetPauseMenuVisible(true);
                if (!uiTest.IsPauseMenuVisible)
                {
                    throw new Exception("[Verification Failed] SetPauseMenuVisible(true) failed to show Pause Menu!");
                }
                uiTest.SetPauseMenuVisible(false);
                if (uiTest.IsPauseMenuVisible)
                {
                    throw new Exception("[Verification Failed] SetPauseMenuVisible(false) failed to hide Pause Menu!");
                }

                // Verify AudioManager Pause Ducking (-6 dB / 50% music ducking, silence fire crackle)
                AudioManager audioTest = testObj.AddComponent<AudioManager>();
                audioTest.SetPauseDucking(true);
                if (Mathf.Abs(audioTest.PauseMusicScale - 0.50f) > 0.01f || Mathf.Abs(audioTest.PauseSfxScale - 0f) > 0.01f)
                {
                    throw new Exception($"[Verification Failed] AudioManager pause ducking scales invalid: Music={audioTest.PauseMusicScale}, SFX/Fire={audioTest.PauseSfxScale}");
                }
                audioTest.SetPauseDucking(false);
                if (Mathf.Abs(audioTest.PauseMusicScale - 1.0f) > 0.01f || Mathf.Abs(audioTest.PauseSfxScale - 1.0f) > 0.01f)
                {
                    throw new Exception($"[Verification Failed] AudioManager pause ducking restore invalid: Music={audioTest.PauseMusicScale}, SFX/Fire={audioTest.PauseSfxScale}");
                }
                // =====================================================================
                // 11. Verify SHOP, 15 SKINS CATALOG, WALLET, INVENTORY, REWARDS & MONETIZATION
                // =====================================================================
                string[] shopRequiredSprites =
                {
                    "Assets/Resources/Sprites/UI/coin_soul_icon.png",
                    "Assets/Resources/Sprites/UI/shop_pedestal.png",
                    "Assets/Resources/Sprites/UI/shop_card_frame_normal.png",
                    "Assets/Resources/Sprites/UI/shop_card_frame_selected.png",
                    "Assets/Resources/Sprites/UI/shop_card_frame_equipped.png"
                };
                foreach (string sPath in shopRequiredSprites)
                {
                    if (!File.Exists(sPath))
                    {
                        throw new Exception($"[Verification Failed] Missing Shop UI sprite asset: {sPath}");
                    }
                }

                // Verify 15 skin preview icon sprites exist on disk
                string[] skinIconNames =
                {
                    "player_original", "player_ashen_knight", "player_forgotten_guardian", "player_ember_wanderer", "player_void_traveler",
                    "shadow_original", "shadow_violet_phantom", "shadow_crimson_wraith", "shadow_spectral_echo", "shadow_abyssal_shade",
                    "torch_original_flame", "torch_blue_soul", "torch_violet_fire", "torch_emerald_ember", "torch_crimson_inferno"
                };
                foreach (string skinId in skinIconNames)
                {
                    string iconPath = $"Assets/Resources/Sprites/Skins/Icons/{skinId}_icon.png";
                    if (!File.Exists(iconPath))
                    {
                        throw new Exception($"[Verification Failed] Missing skin preview icon: {iconPath}");
                    }
                }

                // Verify SkinCatalog has exactly 15 skins (5 per category)
                if (SkinCatalog.GetAllSkins().Count != 15)
                {
                    throw new Exception($"[Verification Failed] Expected 15 skins in SkinCatalog, got {SkinCatalog.GetAllSkins().Count}!");
                }
                if (SkinCatalog.GetSkins(SkinType.Player).Count != 5 ||
                    SkinCatalog.GetSkins(SkinType.Shadow).Count != 5 ||
                    SkinCatalog.GetSkins(SkinType.Torch).Count != 5)
                {
                    throw new Exception("[Verification Failed] SkinCatalog must have exactly 5 Player, 5 Shadow, and 5 Torch skins!");
                }

                // Verify default skins are free and flagged as default
                SkinDefinition defPlayer = SkinCatalog.GetSkin("player_original");
                SkinDefinition defShadow = SkinCatalog.GetSkin("shadow_original");
                SkinDefinition defTorch = SkinCatalog.GetSkin("torch_original_flame");
                if (defPlayer == null || defPlayer.Price != 0 || !defPlayer.IsDefault ||
                    defShadow == null || defShadow.Price != 0 || !defShadow.IsDefault ||
                    defTorch == null || defTorch.Price != 0 || !defTorch.IsDefault)
                {
                    throw new Exception("[Verification Failed] Default original skins must be free (Price 0) and flagged IsDefault = true!");
                }

                // Verify WalletService economy logic
                WalletService walletTest = testObj.AddComponent<WalletService>();
                PlayerPrefs.DeleteKey(WalletService.KeySoulCoinsBalance);
                walletTest.LoadBalance();
                if (walletTest.Balance != 0)
                {
                    throw new Exception("[Verification Failed] Initial wallet balance must be 0!");
                }
                walletTest.AddCoins(500, "Test Coins");
                if (walletTest.Balance != 500 || !walletTest.CanAfford(300) || walletTest.CanAfford(600))
                {
                    throw new Exception($"[Verification Failed] Wallet balance calculation error: balance={walletTest.Balance}!");
                }
                if (!walletTest.SpendCoins(300, "Test Spend") || walletTest.Balance != 200)
                {
                    throw new Exception($"[Verification Failed] Wallet spend error: balance={walletTest.Balance}!");
                }
                if (walletTest.SpendCoins(500, "Should Fail") || walletTest.Balance != 200)
                {
                    throw new Exception("[Verification Failed] Wallet allowed overspending! Non-negative balance guarantee violated.");
                }

                // Verify InventoryService ownership and auto-unlocked defaults
                InventoryService invTest = testObj.AddComponent<InventoryService>();
                PlayerPrefs.DeleteKey(InventoryService.KeyOwnedSkins);
                invTest.LoadInventory();
                if (!invTest.IsSkinOwned("player_original") || !invTest.IsSkinOwned("shadow_original") || !invTest.IsSkinOwned("torch_original_flame"))
                {
                    throw new Exception("[Verification Failed] Default original skins must always be owned in Inventory!");
                }
                if (invTest.IsSkinOwned("player_ashen_knight"))
                {
                    throw new Exception("[Verification Failed] Non-default skin should not be owned initially!");
                }
                invTest.UnlockSkin("player_ashen_knight");
                if (!invTest.IsSkinOwned("player_ashen_knight"))
                {
                    throw new Exception("[Verification Failed] UnlockSkin failed to register ownership!");
                }

                // Verify RewardService non-repeatable rewards & exploit protection
                RewardService rewTest = testObj.AddComponent<RewardService>();
                rewTest.ResetClaimedRewards();
                int r1 = rewTest.EvaluateAndClaimLevelRewards("world_1", 0, noRestartUsed: true, out string rMsg1);
                if (r1 != 125 || walletTest.Balance != 325) // 200 + 100 + 25
                {
                    throw new Exception($"[Verification Failed] Level 1 first-clear reward mismatch: expected 125, got {r1}!");
                }
                // Duplicate replay must give 0
                int r2 = rewTest.EvaluateAndClaimLevelRewards("world_1", 0, noRestartUsed: true, out string rMsg2);
                if (r2 != 0)
                {
                    throw new Exception("[Verification Failed] Replaying completed level awarded duplicate coins! Anti-farming exploit protection failed.");
                }
                // World clear reward
                int rW = rewTest.EvaluateAndClaimWorldClearReward("world_1", out string rwMsg);
                if (rW != 300)
                {
                    throw new Exception($"[Verification Failed] World clear reward mismatch: expected 300, got {rW}!");
                }
                int rW2 = rewTest.EvaluateAndClaimWorldClearReward("world_1", out string rwMsg2);
                if (rW2 != 0)
                {
                    throw new Exception("[Verification Failed] Re-triggering world clear awarded duplicate coins!");
                }

                // Verify SkinEquipmentService persistence & entity update
                SkinEquipmentService equipTest = testObj.AddComponent<SkinEquipmentService>();
                equipTest.LoadEquippedSkins();
                if (equipTest.EquippedPlayerSkinId != "player_original")
                {
                    throw new Exception($"[Verification Failed] Default equipped player skin should be player_original, got {equipTest.EquippedPlayerSkinId}!");
                }
                equipTest.EquipSkin("player_ashen_knight");
                if (equipTest.EquippedPlayerSkinId != "player_ashen_knight")
                {
                    throw new Exception("[Verification Failed] EquipSkin failed to update equipped player skin!");
                }

                // Verify Shop UI screen, tabs, and skin cards
                uiTest.ShowShopScreen();
                if (!uiTest.IsShopScreenVisible)
                {
                    throw new Exception("[Verification Failed] ShowShopScreen failed to display Shop screen!");
                }
                if (uiTest.ShopCardCount != 5)
                {
                    throw new Exception($"[Verification Failed] Expected 5 skin cards in Shop, got {uiTest.ShopCardCount}!");
                }
                uiTest.SwitchShopCategory(1); // Shadow
                if (uiTest.ShopSelectedTab != 1)
                {
                    throw new Exception("[Verification Failed] SwitchShopCategory failed to switch to Shadow tab!");
                }
                uiTest.SwitchShopCategory(2); // Torch
                if (uiTest.ShopSelectedTab != 2)
                {
                    throw new Exception("[Verification Failed] SwitchShopCategory failed to switch to Torch tab!");
                }
                uiTest.SelectShopSkin(3);
                if (uiTest.ShopSelectedSkinIndex != 3)
                {
                    throw new Exception("[Verification Failed] SelectShopSkin failed to select skin card 3!");
                }

                // Verify Monetization Adapters
                WebGLMonetizationAdapter webglAdapter = new WebGLMonetizationAdapter();
                if (webglAdapter.IsPlatformPurchasingSupported)
                {
                    throw new Exception("[Verification Failed] WebGLMonetizationAdapter must report IsPlatformPurchasingSupported == false!");
                }
                GooglePlayBillingAdapter playAdapter = new GooglePlayBillingAdapter();
                if (!playAdapter.IsPlatformPurchasingSupported)
                {
                    throw new Exception("[Verification Failed] GooglePlayBillingAdapter must report IsPlatformPurchasingSupported == true!");
                }

                // Clean up test services
                UnityEngine.Object.DestroyImmediate(walletTest);
                UnityEngine.Object.DestroyImmediate(invTest);
                UnityEngine.Object.DestroyImmediate(rewTest);
                UnityEngine.Object.DestroyImmediate(equipTest);

                // 10. Verify GameInput Touch Abstraction Layer
                GameInput.ResetTouchInputs();
                GameInput.SetTouchHorizontal(1.0f);
                if (Mathf.Abs(GameInput.GetHorizontal() - 1.0f) > 0.01f)
                {
                    throw new Exception("[Verification Failed] GameInput.GetHorizontal did not reflect touch right horizontal!");
                }
                GameInput.SetTouchHorizontal(-1.0f);
                if (Mathf.Abs(GameInput.GetHorizontal() - (-1.0f)) > 0.01f)
                {
                    throw new Exception("[Verification Failed] GameInput.GetHorizontal did not reflect touch left horizontal!");
                }
                GameInput.TriggerTouchJump();
                if (!GameInput.GetJumpDown() || !GameInput.GetJumpHeld())
                {
                    throw new Exception("[Verification Failed] GameInput.TriggerTouchJump did not set JumpDown and JumpHeld!");
                }
                GameInput.ClearFrameTriggers();
                if (GameInput.GetJumpDown() || !GameInput.GetJumpHeld())
                {
                    throw new Exception("[Verification Failed] GameInput.ClearFrameTriggers should clear JumpDown while preserving JumpHeld!");
                }
                GameInput.ReleaseTouchJump();
                if (GameInput.GetJumpHeld())
                {
                    throw new Exception("[Verification Failed] GameInput.ReleaseTouchJump did not clear JumpHeld!");
                }
                GameInput.TriggerTouchInteract();
                if (!GameInput.GetInteractDown())
                {
                    throw new Exception("[Verification Failed] GameInput.TriggerTouchInteract did not set InteractDown!");
                }
                GameInput.TriggerTouchSwitchCharacter();
                if (!GameInput.GetSwitchCharacterDown())
                {
                    throw new Exception("[Verification Failed] GameInput.TriggerTouchSwitchCharacter did not set SwitchCharacterDown!");
                }
                GameInput.ResetTouchInputs();

                // 11. Verify MobileControlsUI and Multitouch Virtual Buttons
                MobileControlsUI mobileUI = uiTest.MobileControls;
                if (mobileUI == null)
                {
                    throw new Exception("[Verification Failed] UIManager does not contain MobileControlsUI instance!");
                }
                if (mobileUI.LeftButton == null || mobileUI.RightButton == null ||
                    mobileUI.JumpButton == null || mobileUI.InteractButton == null ||
                    mobileUI.SwitchButton == null)
                {
                    throw new Exception("[Verification Failed] MobileControlsUI is missing one or more required virtual buttons!");
                }

                // Simulate Multitouch: Right Button (finger 0) + Jump Button (finger 1)
                mobileUI.ReleaseAll();
                mobileUI.RightButton.SimulatePointerDown(0);
                if (Mathf.Abs(GameInput.GetHorizontal() - 1.0f) > 0.01f)
                {
                    throw new Exception("[Verification Failed] Mobile RightButton press failed to update horizontal input!");
                }
                mobileUI.JumpButton.SimulatePointerDown(1);
                if (!GameInput.GetJumpDown() || Mathf.Abs(GameInput.GetHorizontal() - 1.0f) > 0.01f)
                {
                    throw new Exception("[Verification Failed] Multitouch failed: Jump did not fire or cancelled RightButton horizontal movement!");
                }
                mobileUI.JumpButton.SimulatePointerUp(1);
                if (Mathf.Abs(GameInput.GetHorizontal() - 1.0f) > 0.01f || !mobileUI.RightButton.IsPressed)
                {
                    throw new Exception("[Verification Failed] Releasing Jump cancelled concurrent RightButton press!");
                }
                mobileUI.RightButton.SimulatePointerUp(0);
                if (Mathf.Abs(GameInput.GetHorizontal()) > 0.01f)
                {
                    throw new Exception("[Verification Failed] Releasing RightButton did not return horizontal to zero!");
                }
                mobileUI.ReleaseAll();

                // Verify Mobile Settings cycling
                float initScale = mobileUI.CurrentScale;
                mobileUI.CycleSize();
                if (Mathf.Abs(mobileUI.CurrentScale - initScale) < 0.01f)
                {
                    throw new Exception("[Verification Failed] MobileControlsUI.CycleSize did not change scale!");
                }
                mobileUI.SetScale(1.0f);
                mobileUI.SetOpacity(0.75f);
                mobileUI.SetPositionMode(0);

                // 12. Verify CooperativeSwitch Modes (BothPresent, Latched, Timed)
                GameObject coopObjA = new GameObject("CoopA_Test");
                GameObject coopObjB = new GameObject("CoopB_Test");
                CooperativeSwitch csA = coopObjA.AddComponent<CooperativeSwitch>();
                CooperativeSwitch csB = coopObjB.AddComponent<CooperativeSwitch>();
                csA.Configure(csB, CooperativeMode.TimedActivation, timer: 4.5f, stayActivated: true);
                csB.Configure(csA, CooperativeMode.TimedActivation, timer: 4.5f, stayActivated: true);
                if (csA.Mode != CooperativeMode.TimedActivation || csA.RemainingTimer > 0.01f)
                {
                    throw new Exception("[Verification Failed] CooperativeSwitch TimedActivation initialization error!");
                }
                UnityEngine.Object.DestroyImmediate(coopObjA);
                UnityEngine.Object.DestroyImmediate(coopObjB);

                // 13. Verify all 20 Levels Build Cleanly (Levels 1 to 20)
                for (int lvlIdx = 0; lvlIdx < 20; lvlIdx++)
                {
                    lm.BuildLevel(lvlIdx, null);
                    if (lm.Player == null || lm.Shadow == null || lm.Torch == null)
                    {
                        throw new Exception($"[Verification Failed] Level {lvlIdx + 1} did not spawn core entities properly!");
                    }
                }

                WorldProgressSave.ClearWorldProgress("world_1");
                WorldProgressSave.ClearWorldProgress("world_2");
            }
            finally
            {
                if (lm != null) lm.ClearCurrentLevel();
                if (testObj != null) UnityEngine.Object.DestroyImmediate(testObj);
                CleanupEditModeSceneArtifacts(saveIfCleaned: false);
            }

            // 10. Verify EditMode scene is 100% clean (0 leaked Protagonists, Shadows, Torches; 1 of each core Manager)
            int leakedPlayers = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
            int leakedShadows = UnityEngine.Object.FindObjectsByType<ShadowController>(FindObjectsSortMode.None).Length;
            int leakedTorches = UnityEngine.Object.FindObjectsByType<TorchController>(FindObjectsSortMode.None).Length;
            if (leakedPlayers != 0 || leakedShadows != 0 || leakedTorches != 0)
            {
                throw new Exception($"[Verification Failed] Leaked entities remain in EditMode scene: Players={leakedPlayers}, Shadows={leakedShadows}, Torches={leakedTorches}!");
            }

            Debug.Log("[Verification Passed] HOME + WORLD SELECT + LEVEL SELECT Maps (World 1 & World 2, 20 Levels: I. Awakening to XX. Heart of the Inverted Forest), 7 New Physical Mechanics, Shop 3-Zone Cards, Branding Icons, Zero Duplicated Objects, Shadow Double Jump, Audio dB Mix, and 16:9 Landscape verified!");
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
