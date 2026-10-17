using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Constructs and manages all 10 levels of World 1 ("The Ruins of the Last Flame")
    /// in MY SHADOW QUIT: THE LAST FLAME using original pixel-art assets:
    /// 1. Awakening
    /// 2. The Separation
    /// 3. Follow the Light
    /// 4. The Forgotten Flame
    /// 5. Dark Passage
    /// 6. Two Sides
    /// 7. The Last Light
    /// 8. My Shadow Quit
    /// 9. Echoes in the Dark
    /// 10. The First Dawn
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static readonly string[] LevelTitles =
        {
            // World 1: The Ruins of the Last Flame (Levels 1–10)
            "I. AWAKENING",
            "II. THE SEPARATION",
            "III. FOLLOW THE LIGHT",
            "IV. THE FORGOTTEN FLAME",
            "V. DARK PASSAGE",
            "VI. TWO SIDES",
            "VII. THE LAST LIGHT",
            "VIII. MY SHADOW QUIT",
            "IX. ECHOES IN THE DARK",
            "X. THE FIRST DAWN",

            // World 2: The Inverted Forest (Levels 11–20)
            "XI. WHISPERS IN THE ROOTS",
            "XII. THE RISING PATH",
            "XIII. ROOTS OF LIGHT",
            "XIV. FADING FOOTSTEPS",
            "XV. THE HOLLOW CANOPY",
            "XVI. A BRIDGE OF EMBERS",
            "XVII. THE SILENT THORNS",
            "XVIII. THE FRACTURED GROVE",
            "XIX. TWO PATHS, ONE FLAME",
            "XX. HEART OF THE INVERTED FOREST"
        };

        public static readonly string[] LevelHints =
        {
            // World 1 Hints
            "A/D: Move   SPACE: Jump   E: Pick up / Place Torch   TAB: Control Shadow",
            "Stand on the lower stone plate, switch to Shadow [TAB], and Double Jump [SPACE x2 in mid-air] to the upper ledge.",
            "Darkness webs dissolve in Torchlight. Guide your Shadow across the illuminated path.",
            "Ancient solar crystals awaken when bathed in the Torch's golden flame.",
            "Push the carved stone block onto the plate while your Shadow unlocks the inner sanctum.",
            "Two parallel tiers: the Flame walks above, the Shadow walks below.",
            "Transfer the Torch between pedestals to clear Darkness Webs and open the final seal.",
            "Unite the three ancient seals and reconnect Flame and Shadow at the Sanctuary Arch.",
            "Mount the Torch to awaken the upper path, Double Jump with your Shadow across the high vault, and weight the lower seal.",
            "Awaken all five ceremonial seals of light and shadow to open the Great Gate toward the First Dawn.",

            // World 2 Hints
            "Moving platforms carry the Flame across deep abysses. Switch between characters [TAB] to guide each other.",
            "The vertical elevator ascends through the ancient tree canopy. Step aboard and ride upward.",
            "Dormant roots bloom into solid bridges under the Torch's golden warmth. Light the way for your Shadow.",
            "Bioluminescent fungal platforms crumble shortly after being touched. Move swiftly with double jumps.",
            "Dense shadow vines bar the Flame's passage, but your Shadow slips through freely to activate the lower mechanism.",
            "Mount the Torch on the ancient pedestal to awaken the shimmering Light Bridge across the chasm.",
            "Beware the toxic forest briars. Ride moving platforms carefully above the thorn beds.",
            "Synchronize temporary platforms, light roots, and shadow passages to unlock the triad seals.",
            "Two separate chambers: the Flame activates the light roots while the Shadow coordinates the dual switches.",
            "Ascend to the core of the Great Tree. Overcome all forest mechanisms and reunite Flame and Shadow at the Ancient Arch."
        };

        private GameObject _levelRoot;
        private PlayerController _player;
        private ShadowController _shadow;
        private TorchController _torch;
        private WorldDefinition _activeWorld;

        public PlayerController Player => _player;
        public ShadowController Shadow => _shadow;
        public TorchController Torch => _torch;
        public WorldDefinition ActiveWorld => _activeWorld != null ? _activeWorld : (_activeWorld = WorldCatalog.GetWorld(0));
        public int TotalLevels => ActiveWorld != null && ActiveWorld.LevelCount > 0 ? ActiveWorld.LevelCount : 10;

        public void SetActiveWorld(WorldDefinition world)
        {
            _activeWorld = world;
        }

        public string GetLevelTitle(int levelIndex)
        {
            int worldIdx = (ActiveWorld != null) ? ActiveWorld.WorldIndex : 0;
            int globalIndex = (levelIndex >= 10) ? levelIndex : (worldIdx * 10) + levelIndex;
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(levelIndex % 10) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.title)) return entry.title;
            return LevelTitles[Mathf.Clamp(globalIndex, 0, LevelTitles.Length - 1)];
        }

        public string GetLevelHint(int levelIndex)
        {
            int worldIdx = (ActiveWorld != null) ? ActiveWorld.WorldIndex : 0;
            int globalIndex = (levelIndex >= 10) ? levelIndex : (worldIdx * 10) + levelIndex;
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(levelIndex % 10) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.hint)) return entry.hint;
            return LevelHints[Mathf.Clamp(globalIndex, 0, LevelHints.Length - 1)];
        }

        public void BuildLevel(int levelIndex, CameraController cam)
        {
            ClearCurrentLevel();

            int worldIdx = (ActiveWorld != null) ? ActiveWorld.WorldIndex : 0;
            int localIndex = levelIndex;

            if (levelIndex >= 10)
            {
                worldIdx = 1;
                localIndex = levelIndex - 10;
                if (_activeWorld == null || _activeWorld.WorldIndex != 1)
                {
                    _activeWorld = WorldCatalog.GetWorld(1);
                }
            }
            else if (worldIdx == 1)
            {
                localIndex = levelIndex;
            }

            int displayLevelNumber = (worldIdx * 10) + localIndex + 1;
            _levelRoot = new GameObject($"Level_{displayLevelNumber}");

            bool isExtendedChamber = (worldIdx == 0 && localIndex == 9) || (worldIdx == 1 && localIndex == 9);
            int maxChamberX = isExtendedChamber ? 29 : 23;

            if (cam != null)
            {
                cam.ResetZoom();
                cam.BuildParallaxLayers();
                float maxCamX = isExtendedChamber ? 20.8f : 14.8f;
                cam.SetBounds(new Vector2(-1.2f, 0.2f), new Vector2(maxCamX, 2.8f));
            }

            BuildChamberFrame(-9, maxChamberX, -4, 8);

            if (worldIdx == 0)
            {
                switch (localIndex)
                {
                    case 0: BuildLevel1_Awakening(); break;
                    case 1: BuildLevel2_TheSeparation(); break;
                    case 2: BuildLevel3_FollowTheLight(); break;
                    case 3: BuildLevel4_TheForgottenFlame(); break;
                    case 4: BuildLevel5_DarkPassage(); break;
                    case 5: BuildLevel6_TwoSides(); break;
                    case 6: BuildLevel7_TheLastLight(); break;
                    case 7: BuildLevel8_MyShadowQuit(); break;
                    case 8: BuildLevel9_EchoesInTheDark(); break;
                    default: BuildLevel10_TheFirstDawn(); break;
                }
            }
            else
            {
                switch (localIndex)
                {
                    case 0: BuildLevel11_WhispersInTheRoots(); break;
                    case 1: BuildLevel12_TheRisingPath(); break;
                    case 2: BuildLevel13_RootsOfLight(); break;
                    case 3: BuildLevel14_FadingFootsteps(); break;
                    case 4: BuildLevel15_TheHollowCanopy(); break;
                    case 5: BuildLevel16_ABridgeOfEmbers(); break;
                    case 6: BuildLevel17_TheSilentThorns(); break;
                    case 7: BuildLevel18_TheFracturedGrove(); break;
                    case 8: BuildLevel19_TwoPathsOneFlame(); break;
                    default: BuildLevel20_HeartOfTheInvertedForest(); break;
                }
            }
        }

        public void ClearCurrentLevel()
        {
            if (_levelRoot != null)
            {
                DestroyObjectImmediateOrSafe(_levelRoot);
                _levelRoot = null;
            }

            // Clean up any orphaned Level_* containers or stray Protagonist/Shadow/Torch instances
            // (e.g., if left behind by an interrupted EditMode verification or scene reload)
            PlayerController[] strayPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController p in strayPlayers)
            {
                if (p != null)
                {
                    GameObject rootToDestroy = (p.transform.parent != null && p.transform.parent.name.StartsWith("Level_"))
                        ? p.transform.parent.gameObject
                        : p.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            ShadowController[] strayShadows = FindObjectsByType<ShadowController>(FindObjectsSortMode.None);
            foreach (ShadowController s in strayShadows)
            {
                if (s != null)
                {
                    GameObject rootToDestroy = (s.transform.parent != null && s.transform.parent.name.StartsWith("Level_"))
                        ? s.transform.parent.gameObject
                        : s.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            TorchController[] strayTorches = FindObjectsByType<TorchController>(FindObjectsSortMode.None);
            foreach (TorchController t in strayTorches)
            {
                if (t != null)
                {
                    GameObject rootToDestroy = (t.transform.parent != null && t.transform.parent.name.StartsWith("Level_"))
                        ? t.transform.parent.gameObject
                        : t.gameObject;
                    DestroyObjectImmediateOrSafe(rootToDestroy);
                }
            }

            _player = null;
            _shadow = null;
            _torch = null;
        }

        private static void DestroyObjectImmediateOrSafe(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying)
            {
                go.SetActive(false);
                Destroy(go);
            }
            else
            {
                DestroyImmediate(go);
            }
        }

        // =========================================================================
        // LEVEL 1: AWAKENING (Vertical Slice)
        // Wake inside ancient ruins -> pick up burning torch -> shadow separates ->
        // place torch in pedestal -> switch to shadow [TAB] -> pass shadow lattice ->
        // activate shadow obelisk -> stone door opens -> exit sanctuary.
        // =========================================================================
        private void BuildLevel1_Awakening()
        {
            // Accessible stepped stone ledges teaching jump & elevation without blocking the Shadow on the main floor
            CreatePlatformRow(-2, 1, -2, true);
            CreatePlatformRow(3, 7, -1, true);

            // Decorative ancient architecture framed for 16:9 landscape
            CreateProp("Environment/ancient_column", new Vector2(-5.5f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(0.5f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(-2.5f, -1.0f), 0);
            CreateProp("Environment/hanging_chain", new Vector2(-3.5f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(4.5f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(12.5f, 4.0f), 2);

            // Spawn Protagonist & Shadow & Nearby Burning Torch
            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.2f, -2.2f),
                shadowPos: new Vector2(-7.1f, -2.2f),
                torchPos: new Vector2(-3.8f, -2.4f),
                startWithTorch: false
            );

            // Torch Pedestal perched on the central altar platform right before the Shadow Passage
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(5.5f, -0.25f));

            // Shadow Passage dividing the upper & lower chamber at x = 9.0
            CreateWallColumn(9, 0, 7);
            ShadowPassage passage1 = CreateShadowPassage(new Vector2(9.0f, -2.0f));

            // Shadow-only Obelisk Switch inside the shadow alcove (x = 12.2)
            ShadowSwitch shadowSwitch = CreateShadowSwitch(new Vector2(12.2f, -2.25f));

            // Activating the ShadowSwitch dissolves the ShadowPassage barrier so the Protagonist can walk through!
            passage1.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSwitch }, requireAll: true, latchOpen: true);

            // Ancient Stone Door guarding the Exit Sanctuary (requires Pedestal Lit + Shadow Switch Active)
            // Automatically draws rune conduit lines to both pedestal and shadowSwitch!
            AncientDoor exitDoor = CreateAncientDoor(
                new Vector2(15.5f, -1.5f),
                new List<PuzzleNode> { pedestal, shadowSwitch },
                requireAll: true
            );

            CreateExitPortal(new Vector2(18.8f, -1.75f), exitDoor, false);
        }

        // =========================================================================
        // LEVEL 2: THE SEPARATION
        // Requires simultaneous cooperation on dual pressure plates.
        // =========================================================================
        private void BuildLevel2_TheSeparation()
        {
            CreateProp("Environment/ancient_column", new Vector2(-4f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(3f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.2f, -2.4f),
                startWithTorch: true
            );

            // First plate on lower floor
            PressurePlate plateA = CreatePressurePlate(new Vector2(1.5f, -2.85f), false);

            // Upper ledge reachable by jumping or passing a shadow barrier
            CreatePlatformRow(5, 11, 0, true);
            CreateShadowPassage(new Vector2(7.0f, 1.5f));
            PressurePlate plateB = CreatePressurePlate(new Vector2(9.5f, 0.65f), false);

            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(3.5f, -2.25f));
            _ = pedestal;

            AncientDoor door = CreateAncientDoor(
                new Vector2(15.5f, -1.5f),
                new List<PuzzleNode> { plateA, plateB },
                requireAll: true,
                latchOpen: true
            );

            CreateExitPortal(new Vector2(19.0f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 3: FOLLOW THE LIGHT
        // Darkness Barriers block the path unless illuminated by the protagonist's Torch.
        // =========================================================================
        private void BuildLevel3_FollowTheLight()
        {
            CreateProp("Environment/hanging_chain", new Vector2(-1f, 4.0f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(8f, 4.0f), 2);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.2f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Upper walkway for Protagonist carrying the Torch
            CreatePlatformRow(-2, 12, 1, true);
            CreatePlatformRow(-4, -2, -1, true);

            // Darkness Webs on the lower path blocking the Shadow unless Player stands above with the Torch
            CreateDarknessBarrier(new Vector2(2.0f, -2.0f), dissolvesWhenLit: true);
            CreateDarknessBarrier(new Vector2(7.5f, -2.0f), dissolvesWhenLit: true);

            // Shadow Passage at the end of the lower tunnel
            ShadowPassage passage3 = CreateShadowPassage(new Vector2(11.5f, -2.0f));
            ShadowSwitch sw = CreateShadowSwitch(new Vector2(13.5f, -2.25f));
            passage3.ConfigureUnlockNodes(new List<PuzzleNode> { sw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { sw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 4: THE FORGOTTEN FLAME
        // Light-sensitive solar crystals + Torch Pedestal placement.
        // =========================================================================
        private void BuildLevel4_TheForgottenFlame()
        {
            CreateProp("Environment/ruined_arch", new Vector2(-1f, -1.0f), 0);
            CreateProp("Environment/ancient_column", new Vector2(10f, -1.5f), 1);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Pedestal near high LightSwitch
            CreatePlatformRow(0, 6, 0, true);
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(3.0f, 0.75f));
            LightSwitch lightCrystal = CreateLightSwitch(new Vector2(4.5f, 0.75f), latch: false);
            _ = pedestal;

            // Shadow passage leading to second switch
            ShadowPassage passage4 = CreateShadowPassage(new Vector2(9.0f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(12.0f, -2.25f));
            passage4.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { lightCrystal, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 5: DARK PASSAGE
        // Introduces Movable Stone Block + Pressure Plate + Shadow Switch.
        // =========================================================================
        private void BuildLevel5_DarkPassage()
        {
            CreateProp("Environment/ancient_column", new Vector2(-3f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(6f, -1.5f), 1);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            CreateMovableBlock(new Vector2(-1.5f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(2.5f, -2.85f), false);

            CreateCheckpoint(new Vector2(5.0f, -2.25f));

            CreatePlatformRow(7, 13, 0, true);
            CreateShadowPassage(new Vector2(9.5f, 1.5f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(12.0f, 0.75f));

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { plate, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 6: TWO SIDES
        // Divided upper and lower sanctums requiring synchronized switches.
        // =========================================================================
        private void BuildLevel6_TwoSides()
        {
            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-6.5f, 1.8f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Dividing stone deck separating Upper Shadow Realm and Lower Torch Realm
            CreatePlatformRow(-8, 12, 0, false);

            // Lower path: Torch dissolves darkness barrier & lights solar crystal
            CreateDarknessBarrier(new Vector2(1.0f, -2.0f), dissolvesWhenLit: true);
            TorchPedestal lowerPedestal = CreateTorchPedestal(new Vector2(6.5f, -2.25f));
            LightSwitch lowerCrystal = CreateLightSwitch(new Vector2(8.0f, -2.25f), latch: false);
            _ = lowerPedestal;

            // Upper path: Shadow navigates shadow passage and activates obelisk
            CreateShadowPassage(new Vector2(3.5f, 2.0f));
            ShadowSwitch upperShadowSw = CreateShadowSwitch(new Vector2(9.5f, 1.75f));

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { lowerCrystal, upperShadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 7: THE LAST LIGHT
        // Multi-stage torch relay with latching solar crystal, block, and shadow gate.
        // =========================================================================
        private void BuildLevel7_TheLastLight()
        {
            CreateProp("Environment/ruined_arch", new Vector2(0f, -1.0f), 0);
            CreateProp("Environment/ruined_arch", new Vector2(10f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Stage 1: Latching solar crystal on high ledge
            CreatePlatformRow(-3, 2, 0, true);
            LightSwitch crystal1 = CreateLightSwitch(new Vector2(-0.5f, 0.75f), latch: true);

            // Stage 2: Darkness barrier & movable block
            CreateDarknessBarrier(new Vector2(4.0f, -2.0f), dissolvesWhenLit: true);
            CreateMovableBlock(new Vector2(6.0f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(8.5f, -2.85f), false);

            // Stage 3: Shadow passage & obelisk
            ShadowPassage passage7 = CreateShadowPassage(new Vector2(11.5f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(13.8f, -2.25f));
            passage7.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor door = CreateAncientDoor(
                new Vector2(16.5f, -1.5f),
                new List<PuzzleNode> { crystal1, plate, shadowSw },
                requireAll: true
            );

            CreateExitPortal(new Vector2(19.5f, -1.75f), door, false);
        }

        // =========================================================================
        // LEVEL 8: MY SHADOW QUIT (Grand Finale)
        // Full synthesis of Torch, Pedestal, Light Crystal, Shadow Passage, Pressure Plate,
        // and final Reconnection at the Sanctuary Arch.
        // =========================================================================
        private void BuildLevel8_MyShadowQuit()
        {
            CreateProp("Environment/ancient_column", new Vector2(-4f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(4f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(12f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(8f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.5f, -2.2f),
                shadowPos: new Vector2(-7.3f, -2.2f),
                torchPos: new Vector2(-5.5f, -2.4f),
                startWithTorch: true
            );

            // Tier 1: Movable block onto ancient latching pressure plate
            CreateMovableBlock(new Vector2(-2.5f, -2.1f));
            PressurePlate plate = CreatePressurePlate(new Vector2(0.5f, -2.85f), true);

            // Tier 2: Stepped ledge + Upper altar where Torch illuminates Solar Crystal & Pedestal
            CreatePlatformRow(1, 2, -2, true);
            CreatePlatformRow(4, 8, -1, true);
            TorchPedestal pedestal = CreateTorchPedestal(new Vector2(5.2f, -0.25f));
            LightSwitch crystal = CreateLightSwitch(new Vector2(6.8f, -0.25f), latch: true);
            _ = pedestal;

            // Tier 3: Shadow passes barrier to trigger final Obelisk, unlocking the barrier for Protagonist
            ShadowPassage passage8 = CreateShadowPassage(new Vector2(10.5f, -2.0f));
            ShadowSwitch shadowSw = CreateShadowSwitch(new Vector2(13.0f, -2.25f));
            passage8.ConfigureUnlockNodes(new List<PuzzleNode> { shadowSw }, requireAll: true, latchOpen: true);

            AncientDoor finalDoor = CreateAncientDoor(
                new Vector2(16.0f, -1.5f),
                new List<PuzzleNode> { plate, crystal, shadowSw },
                requireAll: true
            );

            // Final Sanctuary of Level 8 requires both Protagonist and Shadow to reunite at the exit!
            CreateExitPortal(new Vector2(19.5f, -1.75f), finalDoor, requireShadowNear: true);
        }

        // =========================================================================
        // LEVEL 9: ECHOES IN THE DARK
        // Advanced vertical cooperation:
        // 1. Protagonist carries Torch and places it in West Altar Pedestal (-1.0, -0.25).
        // 2. Illumination activates Solar Crystal (0.8, -0.25) and dissolves upper Darkness Barrier (1.2, 2.2).
        // 3. Shadow uses Double Jump + 50% Low Gravity to reach Upper Vault Platform (x=2..12, y=2, deltaY=3.0u).
        // 4. Shadow passes upper Violet Barrier (6.5, 3.4) and activates Upper Shadow Switch (10.0, 3.22).
        // 5. Upper Shadow Switch unlocks lower Shadow Passage (4.5, -2.0) for Protagonist.
        // 6. Protagonist pushes Movable Block (6.8, -2.1) onto Lower Pressure Plate (9.8, -2.85).
        // 7. All three seals unlock the Ancient Door (15.5, -1.5) and Protagonist reaches Exit (18.8, -1.75).
        // =========================================================================
        private void BuildLevel9_EchoesInTheDark()
        {
            CreateProp("Environment/echo_shrine_pillar", new Vector2(-4.5f, -1.8f), 1);
            CreateProp("Environment/echo_shrine_pillar", new Vector2(8.0f, 3.2f), 1);
            CreateProp("Environment/ancient_column", new Vector2(2.5f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(12.5f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(8.2f, -1.0f), 0);
            CreateProp("Environment/hanging_chain", new Vector2(-1.5f, 4.2f), 2);
            CreateProp("Environment/hanging_chain", new Vector2(14.0f, 4.2f), 2);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-6.8f, -2.2f),
                shadowPos: new Vector2(-7.6f, -2.2f),
                torchPos: new Vector2(-5.4f, -2.4f),
                startWithTorch: true
            );

            // Step 1-3: Stepped access (-5..-4, y=-2) -> West Altar (-3..0, y=-1) with Pedestal & Solar Crystal
            CreatePlatformRow(-5, -4, -2, true);
            CreatePlatformRow(-3, 0, -1, true);
            TorchPedestal pedestal9 = CreateTorchPedestal(new Vector2(-1.0f, -0.25f));
            LightSwitch crystal9 = CreateLightSwitch(new Vector2(0.8f, -0.25f), latch: true);
            _ = pedestal9;

            // Darkness Barrier guarding the aerial ascent to the Upper Shadow Route (dissolves when Torch is in pedestal9)
            CreateDarknessBarrier(new Vector2(1.2f, 2.2f), dissolvesWhenLit: true);

            // Step 4-6: Upper Shadow Route (x=2..12, y=2 -> top at y=+2.47, 3.00u above West Altar at y=-0.53)
            // Reachable ONLY via Shadow's Double Jump + 50% Reduced Gravity!
            CreatePlatformRow(2, 12, 2, true);
            CreateShadowPassage(new Vector2(6.5f, 3.4f));
            ShadowSwitch upperShadowSw = CreateShadowSwitch(new Vector2(10.0f, 3.22f));

            // Lower route gate at x=4.5 unlocks when Shadow activates upperShadowSw
            ShadowPassage lowerGate9 = CreateShadowPassage(new Vector2(4.5f, -2.0f));
            lowerGate9.ConfigureUnlockNodes(new List<PuzzleNode> { upperShadowSw }, requireAll: true, latchOpen: true);

            // Step 7: Protagonist pushes MovableBlock (6.8, -2.1) onto PressurePlate (9.8, -2.85) on flat lower route
            CreateMovableBlock(new Vector2(6.8f, -2.1f));
            PressurePlate lowerPlate9 = CreatePressurePlate(new Vector2(9.8f, -2.85f), true);

            // Step 8-9: Final Door requires Crystal + Upper Shadow Switch + Lower Pressure Plate
            AncientDoor exitDoor9 = CreateAncientDoor(
                new Vector2(15.5f, -1.5f),
                new List<PuzzleNode> { crystal9, upperShadowSw, lowerPlate9 },
                requireAll: true
            );

            CreateExitPortal(new Vector2(18.8f, -1.75f), exitDoor9, requireShadowNear: false);
        }

        // =========================================================================
        // LEVEL 10: THE FIRST DAWN (World 1 Grand Finale — 6-Phase Ceremonial Chamber)
        // Phase 1: Activate first seal via MovableBlock (-4.2) onto PressurePlate (-1.5).
        // Phase 2: Carry Torch up stepped ledges to Elevated Pedestal (5.2, -0.25) & Solar Crystal (6.8, -0.25).
        // Phase 3: Double Jump with Shadow to High Balcony (x=8..15, y=2) and activate ShadowSwitch (13.5, 3.22).
        // Phase 4: Solve final Light & Shadow combination in eastern wing (Pedestal 14.5, Crystal 16.8, ShadowSwitch 20.5).
        // Phase 5: Open the Great Ceremonial Door (23.0, -1.5).
        // Phase 6: Both Protagonist and Shadow reunite at the Dawn Portal (26.2, -1.75) to complete World 1.
        // =========================================================================
        private void BuildLevel10_TheFirstDawn()
        {
            CreateProp("Environment/ceremonial_banner", new Vector2(-5.5f, 1.2f), 1);
            CreateProp("Environment/ceremonial_banner", new Vector2(5.5f, 2.2f), 1);
            CreateProp("Environment/ceremonial_banner", new Vector2(16.5f, 1.2f), 1);
            CreateProp("Environment/ceremonial_banner", new Vector2(21.5f, 1.2f), 1);
            CreateProp("Environment/echo_shrine_pillar", new Vector2(-2.8f, -1.8f), 1);
            CreateProp("Environment/echo_shrine_pillar", new Vector2(12.0f, 3.2f), 1);
            CreateProp("Environment/ancient_column", new Vector2(3.5f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(14.0f, -1.5f), 1);
            CreateProp("Environment/ancient_column", new Vector2(21.8f, -1.5f), 1);
            CreateProp("Environment/ruined_arch", new Vector2(9.0f, -1.0f), 0);

            SpawnCharactersAndTorch(
                playerPos: new Vector2(-7.0f, -2.2f),
                shadowPos: new Vector2(-7.8f, -2.2f),
                torchPos: new Vector2(-5.8f, -2.4f),
                startWithTorch: true
            );

            // PHASE 1: First seal via MovableBlock & Latching PressurePlate
            CreateMovableBlock(new Vector2(-4.2f, -2.1f));
            PressurePlate phase1Plate = CreatePressurePlate(new Vector2(-1.5f, -2.85f), true);

            // PHASE 2: Transport Torch to Elevated Sanctuary Pedestal & Solar Crystal
            CreatePlatformRow(0, 1, -2, true);
            CreatePlatformRow(3, 8, -1, true);
            TorchPedestal phase2Pedestal = CreateTorchPedestal(new Vector2(5.2f, -0.25f));
            LightSwitch phase2Crystal = CreateLightSwitch(new Vector2(6.8f, -0.25f), latch: true);
            _ = phase2Pedestal;

            // Darkness barrier above the altar dissolves when Torch is placed in phase2Pedestal
            CreateDarknessBarrier(new Vector2(7.5f, 2.2f), dissolvesWhenLit: true);

            // PHASE 3: High Shadow Balcony (x=8..15, y=2 -> 3.00u above Elevated Altar, requiring Shadow Double Jump)
            CreatePlatformRow(8, 15, 2, true);
            CreateShadowPassage(new Vector2(10.5f, 3.4f));
            ShadowSwitch phase3ShadowSw = CreateShadowSwitch(new Vector2(13.5f, 3.22f));

            // Lower passage at x=11.5 unlocks once phase3ShadowSw is activated on the high balcony
            ShadowPassage lowerPassage10 = CreateShadowPassage(new Vector2(11.5f, -2.0f));
            lowerPassage10.ConfigureUnlockNodes(new List<PuzzleNode> { phase3ShadowSw }, requireAll: true, latchOpen: true);

            // PHASE 4: Final Light & Shadow Combination in Eastern Ceremonial Wing
            TorchPedestal phase4Pedestal = CreateTorchPedestal(new Vector2(14.5f, -2.25f));
            CreateDarknessBarrier(new Vector2(15.6f, -2.0f), dissolvesWhenLit: true);
            LightSwitch phase4Crystal = CreateLightSwitch(new Vector2(16.8f, -2.25f), latch: true);
            _ = phase4Pedestal;

            ShadowPassage finalShadowGate = CreateShadowPassage(new Vector2(18.5f, -2.0f));
            ShadowSwitch phase4ShadowSw = CreateShadowSwitch(new Vector2(20.5f, -2.25f));
            finalShadowGate.ConfigureUnlockNodes(new List<PuzzleNode> { phase4ShadowSw }, requireAll: true, latchOpen: true);

            // PHASE 5: Great Ceremonial Door guarding the threshold to World 2
            AncientDoor ceremonialDoor = CreateAncientDoor(
                new Vector2(23.0f, -1.5f),
                new List<PuzzleNode> { phase1Plate, phase2Crystal, phase3ShadowSw, phase4Crystal, phase4ShadowSw },
                requireAll: true
            );
            ceremonialDoor.ConfigureCustomSprites("Environment/ceremonial_door_closed", "Environment/ceremonial_door_open");

            // PHASE 6: Dawn Portal Archway where both Protagonist & Shadow reunite to complete World 1
            CreateProp("Environment/dawn_portal_arch", new Vector2(26.2f, -1.55f), 1);
            CreateExitPortal(new Vector2(26.2f, -1.75f), ceremonialDoor, requireShadowNear: true);
        }

        // =========================================================================
        // BUILDER HELPERS
        // =========================================================================
        private void BuildChamberFrame(int minX, int maxX, int minY, int maxY)
        {
            Sprite floorSp = AssetRegistry.GetSprite("Environment/stone_floor");
            Sprite wallSp = AssetRegistry.GetSprite("Environment/stone_wall");

            // Solid Floor (2 tiles thick)
            for (int x = minX; x <= maxX; x++)
            {
                CreateTileSprite("Floor_" + x, floorSp, new Vector2(x, minY + 0.5f), 2);
                CreateTileSprite("SubFloor_" + x, wallSp, new Vector2(x, minY - 0.5f), 2);
            }
            CreateBoxCollider("FloorCollider", new Vector2((minX + maxX) * 0.5f, minY), new Vector2(maxX - minX + 1f, 2f));

            // Ceiling
            for (int x = minX; x <= maxX; x++)
            {
                CreateTileSprite("Ceiling_" + x, wallSp, new Vector2(x, maxY - 0.5f), 2);
            }
            CreateBoxCollider("CeilingCollider", new Vector2((minX + maxX) * 0.5f, maxY - 0.5f), new Vector2(maxX - minX + 1f, 1f));

            // Left & Right Outer Walls
            for (int y = minY + 1; y < maxY; y++)
            {
                CreateTileSprite("WallL_" + y, wallSp, new Vector2(minX, y + 0.5f), 2);
                CreateTileSprite("WallR_" + y, wallSp, new Vector2(maxX, y + 0.5f), 2);
            }
            CreateBoxCollider("LeftWallCollider", new Vector2(minX, (minY + maxY) * 0.5f), new Vector2(1f, maxY - minY));
            CreateBoxCollider("RightWallCollider", new Vector2(maxX, (minY + maxY) * 0.5f), new Vector2(1f, maxY - minY));
        }

        private void CreatePlatformRow(int startX, int endX, int y, bool useBevelEdges)
        {
            Sprite midSp = AssetRegistry.GetSprite("Environment/stone_floor");
            Sprite leftSp = AssetRegistry.GetSprite("Environment/platform_edge_left");
            Sprite rightSp = AssetRegistry.GetSprite("Environment/platform_edge_right");

            for (int x = startX; x <= endX; x++)
            {
                Sprite sp = midSp;
                if (useBevelEdges && x == startX) sp = leftSp;
                else if (useBevelEdges && x == endX) sp = rightSp;
                CreateTileSprite($"Plat_{startX}_{y}_{x}", sp, new Vector2(x, y), 2);
            }

            float width = (endX - startX + 1);
            float centerX = (startX + endX) * 0.5f;
            CreateBoxCollider($"PlatCol_{startX}_{y}", new Vector2(centerX, y + 0.08f), new Vector2(width, 0.78f));
        }

        private void CreateWallColumn(int x, int startY, int endY)
        {
            Sprite wallSp = AssetRegistry.GetSprite("Environment/stone_wall");
            for (int y = startY; y <= endY; y++)
            {
                CreateTileSprite($"WallCol_{x}_{y}", wallSp, new Vector2(x, y), 2);
            }
            float height = (endY - startY + 1);
            float centerY = (startY + endY) * 0.5f;
            CreateBoxCollider($"WallColCollider_{x}", new Vector2(x, centerY), new Vector2(0.95f, height));
        }

        private void CreateTileSprite(string name, Sprite sp, Vector2 pos, int sortOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortOrder;
        }

        private void CreateBoxCollider(string name, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private void CreateProp(string spritePath, Vector2 pos, int sortOrder)
        {
            GameObject go = new GameObject("Prop_" + spritePath);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite(spritePath);
            sr.sortingOrder = sortOrder;
        }

        private void SpawnCharactersAndTorch(Vector2 playerPos, Vector2 shadowPos, Vector2 torchPos, bool startWithTorch)
        {
            GameObject torchObj = new GameObject("Torch");
            torchObj.transform.SetParent(_levelRoot.transform, false);
            torchObj.transform.position = torchPos;
            _torch = torchObj.AddComponent<TorchController>();

            GameObject playerObj = new GameObject("Protagonist");
            playerObj.transform.SetParent(_levelRoot.transform, false);
            playerObj.transform.position = playerPos;
            _player = playerObj.AddComponent<PlayerController>();
            _player.EnsureInitialized();

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(_levelRoot.transform, false);
            shadowObj.transform.position = shadowPos;
            _shadow = shadowObj.AddComponent<ShadowController>();
            _shadow.Initialize(_player, _torch, startSeparated: true);

            if (startWithTorch)
            {
                _player.ForceEquipTorch(_torch);
            }
        }

        private TorchPedestal CreateTorchPedestal(Vector2 pos)
        {
            GameObject go = new GameObject("TorchPedestal");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<TorchPedestal>();
        }

        private PressurePlate CreatePressurePlate(Vector2 pos, bool latch)
        {
            GameObject go = new GameObject("PressurePlate");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            PressurePlate pp = go.AddComponent<PressurePlate>();
            pp.Configure(latch);
            return pp;
        }

        private LightSwitch CreateLightSwitch(Vector2 pos, bool latch)
        {
            GameObject go = new GameObject("LightSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LightSwitch ls = go.AddComponent<LightSwitch>();
            ls.Configure(latch);
            return ls;
        }

        private ShadowSwitch CreateShadowSwitch(Vector2 pos)
        {
            GameObject go = new GameObject("ShadowSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<ShadowSwitch>();
        }

        private ShadowPassage CreateShadowPassage(Vector2 pos)
        {
            GameObject go = new GameObject("ShadowPassage");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ShadowPassage sp = go.AddComponent<ShadowPassage>();
            sp.EnsureInitialized();
            return sp;
        }

        private DarknessBarrier CreateDarknessBarrier(Vector2 pos, bool dissolvesWhenLit)
        {
            GameObject go = new GameObject("DarknessBarrier");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            DarknessBarrier db = go.AddComponent<DarknessBarrier>();
            db.Configure(dissolvesWhenLit, new Vector2(0.9f, 1.95f));
            return db;
        }

        private MovableBlock CreateMovableBlock(Vector2 pos)
        {
            GameObject go = new GameObject("MovableBlock");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<MovableBlock>();
        }

        private Checkpoint CreateCheckpoint(Vector2 pos)
        {
            GameObject go = new GameObject("Checkpoint");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            return go.AddComponent<Checkpoint>();
        }

        private AncientDoor CreateAncientDoor(Vector2 pos, List<PuzzleNode> nodes, bool requireAll = true, bool latchOpen = true)
        {
            GameObject go = new GameObject("AncientDoor");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            AncientDoor door = go.AddComponent<AncientDoor>();
            door.Configure(nodes, requireAll, latchOpen);
            return door;
        }

        private LevelExitPortal CreateExitPortal(Vector2 pos, AncientDoor door, bool requireShadowNear)
        {
            GameObject go = new GameObject("ExitSanctuary");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LevelExitPortal portal = go.AddComponent<LevelExitPortal>();
            portal.Configure(door, requireShadowNear);
            return portal;
        }

        // =========================================================================
        // WORLD 2: THE INVERTED FOREST (Levels 11–20)
        // Dark petroleum green, deep cyan/jade, ancient roots, glowing flora
        // =========================================================================

        // Level 11: Whispers in the Roots (Horizontal Moving Platform)
        private void BuildLevel11_WhispersInTheRoots()
        {
            CreatePlatformRow(-8, -2, -2, true);
            CreatePlatformRow(5, 12, -2, true);
            CreatePlatformRow(1, 4, 1.5f, true);

            CreateProp("World2/forest_thorns", new Vector2(0.5f, -3.2f), 1);
            CreateProp("World2/forest_thorns", new Vector2(3.5f, -3.2f), 1);

            CreateMovingPlatform(new[] { new Vector2(-0.5f, -2.0f), new Vector2(4.5f, -2.0f) }, speed: 2.4f, wait: 0.6f);

            PressurePlate plate = CreatePressurePlate(new Vector2(2.5f, 2.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.0f), new List<PuzzleNode> { plate });
            CreateExitPortal(new Vector2(10.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-4.0f, -1.0f), new Vector2(-5.5f, -1.0f), new Vector2(-4.0f, -1.0f), true);
        }

        // Level 12: The Rising Path (Vertical Elevator Moving Platform)
        private void BuildLevel12_TheRisingPath()
        {
            CreatePlatformRow(-8, -2, -2, true);
            CreatePlatformRow(1, 12, -2, true);
            CreatePlatformRow(1, 8, 3.2f, true);

            CreateMovingPlatform(new[] { new Vector2(-0.5f, -2.0f), new Vector2(-0.5f, 3.2f) }, speed: 2.2f, wait: 0.8f);

            LightSwitch ls = CreateLightSwitch(new Vector2(4.5f, 4.0f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.0f), new List<PuzzleNode> { ls });
            CreateExitPortal(new Vector2(10.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-4.5f, -1.0f), new Vector2(-5.5f, -1.0f), new Vector2(-4.5f, -1.0f), true);
        }

        // Level 13: Roots of Light (Torch proximity blooms solid bridges)
        private void BuildLevel13_RootsOfLight()
        {
            CreatePlatformRow(-8, -3, -2, true);
            CreatePlatformRow(5, 12, -2, true);
            CreatePlatformRow(6, 8, 1.5f, true);

            CreateLightRoot(new Vector2(-1.0f, -2.0f), radius: 4.2f);
            CreateLightRoot(new Vector2(2.5f, -2.0f), radius: 4.2f);

            ShadowSwitch ss = CreateShadowSwitch(new Vector2(7.0f, 2.3f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.0f), new List<PuzzleNode> { ss });
            CreateExitPortal(new Vector2(11.0f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-5.0f, -1.0f), new Vector2(-6.0f, -1.0f), new Vector2(-5.0f, -1.0f), true);
        }

        // Level 14: Fading Footsteps (Temporary crumbling platforms)
        private void BuildLevel14_FadingFootsteps()
        {
            CreatePlatformRow(-8, -4, -2, true);
            CreatePlatformRow(6, 12, 1.5f, true);
            CreatePlatformRow(6, 12, -2, true);

            CreateForestHazard(new Vector2(1.0f, -3.2f), new Vector2(9.0f, 0.8f));

            CreateTemporaryPlatform(new Vector2(-2.0f, -1.2f), collapseTime: 1.2f, respawnTime: 2.4f);
            CreateTemporaryPlatform(new Vector2(0.8f, -0.2f), collapseTime: 1.2f, respawnTime: 2.4f);
            CreateTemporaryPlatform(new Vector2(3.6f, 0.8f), collapseTime: 1.2f, respawnTime: 2.4f);

            MovableBlock block = CreateMovableBlock(new Vector2(7.5f, 2.5f));
            PressurePlate plate = CreatePressurePlate(new Vector2(8.0f, -1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(10.5f, 2.5f), new List<PuzzleNode> { plate });
            CreateExitPortal(new Vector2(11.8f, 2.5f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 15: The Hollow Canopy (Shadow Vine barrier + Lower Moving Platform)
        private void BuildLevel15_TheHollowCanopy()
        {
            CreatePlatformRow(-8, -3, -2, true);
            CreatePlatformRow(-2, 8, 2.8f, true);
            CreatePlatformRow(6, 12, -2, true);

            CreateWallColumn(0, -1, 2);
            CreateShadowVine(new Vector2(0.0f, 3.8f));

            ShadowSwitch ss = CreateShadowSwitch(new Vector2(5.5f, 3.6f));
            CreateMovingPlatform(new[] { new Vector2(-1.0f, -2.0f), new Vector2(4.5f, -2.0f) }, speed: 2.3f);

            AncientDoor door = CreateAncientDoor(new Vector2(9.0f, -1.0f), new List<PuzzleNode> { ss });
            CreateExitPortal(new Vector2(11.0f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-5.5f, -1.0f), new Vector2(-6.5f, -1.0f), new Vector2(-5.5f, -1.0f), true);
        }

        // Level 16: A Bridge of Embers (Torch Pedestal activates Light Bridge)
        private void BuildLevel16_ABridgeOfEmbers()
        {
            CreatePlatformRow(-8, -2, -2, true);
            CreatePlatformRow(7, 13, -2, true);

            CreateForestHazard(new Vector2(2.5f, -3.2f), new Vector2(8.0f, 0.8f));

            TorchPedestal ped1 = CreateTorchPedestal(new Vector2(-3.5f, -1.0f));
            LightBridgeMechanism bridge = CreateLightBridge(new Vector2(2.5f, -2.0f), new List<PuzzleNode> { ped1 });

            PressurePlate farPlate = CreatePressurePlate(new Vector2(9.0f, -1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(11.0f, -1.0f), new List<PuzzleNode> { farPlate });
            CreateExitPortal(new Vector2(12.2f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-5.5f, -1.0f), new Vector2(-6.5f, -1.0f), new Vector2(-5.5f, -1.0f), true);
        }

        // Level 17: The Silent Thorns (Thorn beds + synchronized moving platforms)
        private void BuildLevel17_TheSilentThorns()
        {
            CreatePlatformRow(-8, -4, -1, true);
            CreatePlatformRow(3, 4, -0.5f, true);
            CreatePlatformRow(10, 13, 0.5f, true);

            CreateForestHazard(new Vector2(3.0f, -3.2f), new Vector2(14.0f, 0.8f));

            CreateMovingPlatform(new[] { new Vector2(-2.0f, -0.5f), new Vector2(1.5f, -0.5f) }, speed: 2.2f);
            CreateMovingPlatform(new[] { new Vector2(5.5f, 0.0f), new Vector2(8.5f, 0.0f) }, speed: 2.2f);

            LightSwitch ls = CreateLightSwitch(new Vector2(3.5f, 1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(11.0f, 1.5f), new List<PuzzleNode> { ls });
            CreateExitPortal(new Vector2(12.2f, 1.5f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, 0.0f), new Vector2(-7.0f, 0.0f), new Vector2(-6.0f, 0.0f), true);
        }

        // Level 18: The Fractured Grove (Triad: Light Root, Temp Platform, Shadow Vine)
        private void BuildLevel18_TheFracturedGrove()
        {
            CreatePlatformRow(-8, -4, -2, true);
            CreatePlatformRow(-1, 2, 0.5f, true);
            CreatePlatformRow(5, 8, 2.5f, true);
            CreatePlatformRow(7, 13, -2, true);

            // Triad 1: Light Root blooming under Torch
            CreateLightRoot(new Vector2(-2.5f, -1.0f), radius: 3.8f);
            LightSwitch s1 = CreateLightSwitch(new Vector2(0.5f, 1.3f), latch: true);

            // Triad 2: Temporary Platform
            CreateTemporaryPlatform(new Vector2(3.5f, 1.5f), collapseTime: 1.15f, respawnTime: 2.5f);

            // Triad 3: Shadow Vine & Switch
            CreateShadowVine(new Vector2(5.0f, 3.5f));
            ShadowSwitch s2 = CreateShadowSwitch(new Vector2(7.0f, 3.3f));

            PressurePlate s3 = CreatePressurePlate(new Vector2(9.0f, -1.2f), latch: true);

            AncientDoor door = CreateAncientDoor(new Vector2(11.0f, -1.0f), new List<PuzzleNode> { s1, s2, s3 }, requireAll: true);
            CreateExitPortal(new Vector2(12.2f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 19: Two Paths, One Flame (Parallel paths + Cooperative Switch)
        private void BuildLevel19_TwoPathsOneFlame()
        {
            // Upper path
            CreatePlatformRow(-8, 9, 2.5f, true);
            // Lower path
            CreatePlatformRow(-8, 9, -2.5f, true);

            // Dividing barrier
            CreateWallColumn(0, -2, 2);

            // Lower path has shadow vines
            CreateShadowVine(new Vector2(-3.0f, -1.5f));

            // Upper path has light root
            CreateLightRoot(new Vector2(3.0f, 2.5f), radius: 4.0f);

            // Cooperative Switches at the end of both paths
            CooperativeSwitch upperCoop = CreateCooperativeSwitch(new Vector2(6.5f, 3.3f));
            CooperativeSwitch lowerCoop = CreateCooperativeSwitch(new Vector2(6.5f, -1.7f));
            upperCoop.Configure(lowerCoop, simultaneous: true);
            lowerCoop.Configure(upperCoop, simultaneous: true);

            CreatePlatformRow(10, 13, 0.0f, true);
            AncientDoor door = CreateAncientDoor(new Vector2(10.5f, 1.0f), new List<PuzzleNode> { upperCoop });
            CreateExitPortal(new Vector2(12.0f, 1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, 3.5f), new Vector2(-6.0f, -1.5f), new Vector2(-6.0f, 3.5f), true);
        }

        // Level 20: Heart of the Inverted Forest (5-Phase Grand Climax)
        private void BuildLevel20_HeartOfTheInvertedForest()
        {
            // Phase 1: Base Platform & Elevator
            CreatePlatformRow(-8, -4, -2, true);
            CreateMovingPlatform(new[] { new Vector2(-2.5f, -2.0f), new Vector2(-2.5f, 2.5f) }, speed: 2.4f);

            // Phase 2: High Canopy with Light Roots
            CreatePlatformRow(-1, 3, 2.5f, true);
            CreateLightRoot(new Vector2(4.5f, 2.5f), radius: 4.2f);
            CreatePlatformRow(6, 10, 2.5f, true);

            // Phase 3: Shadow Vine high chamber & temporary bridge
            CreateShadowVine(new Vector2(10.5f, 3.5f));
            CreateTemporaryPlatform(new Vector2(12.5f, 3.5f), collapseTime: 1.2f, respawnTime: 2.5f);
            CreatePlatformRow(14, 17, 3.5f, true);
            ShadowSwitch highSwitch = CreateShadowSwitch(new Vector2(16.0f, 4.3f));

            // Phase 4: Descent to Dual Cooperative Switches
            CreatePlatformRow(14, 21, -1.5f, true);
            CreatePlatformRow(14, 21, 1.0f, true);

            CooperativeSwitch coopUpper = CreateCooperativeSwitch(new Vector2(19.0f, 1.8f));
            CooperativeSwitch coopLower = CreateCooperativeSwitch(new Vector2(19.0f, -0.7f));
            coopUpper.Configure(coopLower, simultaneous: true);
            coopLower.Configure(coopUpper, simultaneous: true);

            // Phase 5: Sacred Grove Arch (requires both Flame and Shadow united!)
            CreatePlatformRow(22, 27, 0.5f, true);
            AncientDoor finalDoor = CreateAncientDoor(new Vector2(23.0f, 1.5f), new List<PuzzleNode> { highSwitch, coopUpper }, requireAll: true);
            CreateExitPortal(new Vector2(25.5f, 1.5f), finalDoor, requireShadowNear: true);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // =========================================================================
        // WORLD 2 FACTORY HELPERS
        // =========================================================================

        private MovingPlatform2D CreateMovingPlatform(Vector2[] waypoints, float speed = 2.2f, float wait = 0.6f, bool pingPong = true)
        {
            GameObject go = new GameObject("MovingPlatform");
            go.transform.SetParent(_levelRoot.transform, false);
            MovingPlatform2D mp = go.AddComponent<MovingPlatform2D>();
            mp.Configure(waypoints, speed, wait, pingPong);
            return mp;
        }

        private TemporaryPlatform2D CreateTemporaryPlatform(Vector2 pos, float collapseTime = 1.15f, float respawnTime = 2.5f)
        {
            GameObject go = new GameObject("TemporaryPlatform");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            TemporaryPlatform2D tp = go.AddComponent<TemporaryPlatform2D>();
            tp.Configure(collapseTime, respawnTime);
            return tp;
        }

        private LightRootMechanism CreateLightRoot(Vector2 pos, float radius = 3.8f, bool stayBloomed = false)
        {
            GameObject go = new GameObject("LightRoot");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LightRootMechanism lr = go.AddComponent<LightRootMechanism>();
            lr.Configure(radius, stayBloomed);
            return lr;
        }

        private ShadowVinePassage CreateShadowVine(Vector2 pos, List<PuzzleNode> unlockNodes = null, bool stayOpen = true)
        {
            GameObject go = new GameObject("ShadowVine");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ShadowVinePassage sv = go.AddComponent<ShadowVinePassage>();
            sv.Configure(unlockNodes, stayOpen);
            return sv;
        }

        private LightBridgeMechanism CreateLightBridge(Vector2 pos, List<PuzzleNode> nodes, bool requireAll = true, bool stayActive = false)
        {
            GameObject go = new GameObject("LightBridge");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            LightBridgeMechanism lb = go.AddComponent<LightBridgeMechanism>();
            lb.Configure(nodes, requireAll, stayActive);
            return lb;
        }

        private CooperativeSwitch CreateCooperativeSwitch(Vector2 pos, CooperativeSwitch partner = null, bool simultaneous = true)
        {
            GameObject go = new GameObject("CooperativeSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            CooperativeSwitch cs = go.AddComponent<CooperativeSwitch>();
            cs.Configure(partner, simultaneous);
            return cs;
        }

        private ForestHazard CreateForestHazard(Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject("ForestHazard");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ForestHazard fh = go.AddComponent<ForestHazard>();
            fh.Configure(size);
            return fh;
        }
    }
}
