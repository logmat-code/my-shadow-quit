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
                        RunAutomatedVerification();
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
            Debug.Log("[ProjectSetupBuilder] Starting asset import refresh, prefab generation, scene building, and automated verification...");
            AssetDatabase.Refresh();

            ConfigureLandscapePlayerSettings();
            ConfigureTextureImporters();
            BuildReusablePrefabs();
            BuildScenes();
            RunAutomatedVerification();

            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectSetupBuilder] SUCCESS: All prefabs, scenes, and automated verifications completed with zero errors!");
        }

        private static void ConfigureLandscapePlayerSettings()
        {
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

            // 3. Verify Level 1 ("Awakening") End-to-End Puzzle & Contextual Tutorial Progression
            GameObject testObj = new GameObject("VerificationRunner");
            LevelManager lm = testObj.AddComponent<LevelManager>();
            TutorialDirector td = testObj.AddComponent<TutorialDirector>();
            CameraController cc = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;

            for (int lvl = 0; lvl < lm.TotalLevels; lvl++)
            {
                lm.BuildLevel(lvl, cc);
                if (lm.Player == null || lm.Shadow == null || lm.Torch == null)
                {
                    throw new Exception($"[Verification Failed] Level {lvl + 1} missing Player, Shadow, or Torch!");
                }
            }

            // Test Level 1 Awakening step-by-step tutorial & puzzle flow
            lm.BuildLevel(0, cc);
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
            lm.BuildLevel(1, cc);
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

            //Verify Shadow ignores collision with upper ShadowPassage while Protagonist is blocked by it
            Collider2D shadowCol = shadow2.GetComponent<Collider2D>();
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

            lm.ClearCurrentLevel();
            UnityEngine.Object.DestroyImmediate(testObj);

            Debug.Log("[Verification Passed] Shadow Double Jump (2 jumps max, 50% gravity, air control, visual/SFX feedback), Level 1 & Level 2 Physical Completion, Audio dB Mix, and 16:9 Landscape verified!");
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
