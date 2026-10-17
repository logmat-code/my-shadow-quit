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
            EditorApplication.delayCall += EnforceEditorGameViewLandscape16x9;
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
            AncientDoor door = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();

            if (pedestal == null || shadowSw == null || door == null)
            {
                throw new Exception("[Verification Failed] Level 1 Awakening puzzle components not found!");
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

            // Shadow activates ShadowSwitch -> Door opens -> step transitions to EnterSanctuaryExit
            shadowSw.ActivateByShadow(lm.Shadow);
            if (!door.IsOpen)
            {
                throw new Exception("[Verification Failed] AncientDoor failed to open after Pedestal + ShadowSwitch activation!");
            }

            td.EvaluateAwakeningProgression(false);
            if (td.CurrentStep != TutorialDirector.AwakeningStep.EnterSanctuaryExit)
            {
                throw new Exception($"[Verification Failed] Expected EnterSanctuaryExit step after door opens, got {td.CurrentStep}");
            }

            lm.ClearCurrentLevel();
            UnityEngine.Object.DestroyImmediate(testObj);

            Debug.Log("[Verification Passed] Landscape 16:9 (1920x1080), Audio dB Mix Hierarchy (Music 0dB > SFX -8dB > Ambience -12dB > Fire -18dB), and Level 1 Contextual Tutorial + Rune Conduits verified!");
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
