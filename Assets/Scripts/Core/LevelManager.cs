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
            "I. AWAKENING",
            "II. THE SEPARATION",
            "III. FOLLOW THE LIGHT",
            "IV. THE FORGOTTEN FLAME",
            "V. DARK PASSAGE",
            "VI. TWO SIDES",
            "VII. THE LAST LIGHT",
            "VIII. MY SHADOW QUIT",
            "IX. ECHOES IN THE DARK",
            "X. THE FIRST DAWN"
        };

        public static readonly string[] LevelHints =
        {
            "A/D: Move   SPACE: Jump   E: Pick up / Place Torch   TAB: Control Shadow",
            "Stand on the lower stone plate, switch to Shadow [TAB], and Double Jump [SPACE x2 in mid-air] to the upper ledge.",
            "Darkness webs dissolve in Torchlight. Guide your Shadow across the illuminated path.",
            "Ancient solar crystals awaken when bathed in the Torch's golden flame.",
            "Push the carved stone block onto the plate while your Shadow unlocks the inner sanctum.",
            "Two parallel tiers: the Flame walks above, the Shadow walks below.",
            "Transfer the Torch between pedestals to clear Darkness Webs and open the final seal.",
            "Unite the three ancient seals and reconnect Flame and Shadow at the Sanctuary Arch.",
            "Mount the Torch to awaken the upper path, Double Jump with your Shadow across the high vault, and weight the lower seal.",
            "Awaken all five ceremonial seals of light and shadow to open the Great Gate toward the First Dawn."
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
        public int TotalLevels => ActiveWorld != null && ActiveWorld.LevelCount > 0 ? ActiveWorld.LevelCount : LevelTitles.Length;

        public void SetActiveWorld(WorldDefinition world)
        {
            _activeWorld = world;
        }

        public string GetLevelTitle(int levelIndex)
        {
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(levelIndex) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.title)) return entry.title;
            return LevelTitles[Mathf.Clamp(levelIndex, 0, LevelTitles.Length - 1)];
        }

        public string GetLevelHint(int levelIndex)
        {
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(levelIndex) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.hint)) return entry.hint;
            return LevelHints[Mathf.Clamp(levelIndex, 0, LevelHints.Length - 1)];
        }

        public void BuildLevel(int levelIndex, CameraController cam)
        {
            ClearCurrentLevel();

            _levelRoot = new GameObject($"Level_{levelIndex + 1}");

            bool isExtendedCeremonialChamber = (levelIndex == 9);
            int maxChamberX = isExtendedCeremonialChamber ? 29 : 23;

            if (cam != null)
            {
                cam.ResetZoom();
                cam.BuildParallaxLayers();
                // 16:9 orthographic size 4.5 = 16 units wide x 9 units tall.
                // Clamp camera so outer walls and floor/ceiling fill the 16:9 frame cleanly.
                float maxCamX = isExtendedCeremonialChamber ? 20.8f : 14.8f;
                cam.SetBounds(new Vector2(-1.2f, 0.2f), new Vector2(maxCamX, 2.8f));
            }

            // Base outer chamber walls & floor common framing
            BuildChamberFrame(-9, maxChamberX, -4, 8);

            switch (levelIndex)
            {
                case 0:
                    BuildLevel1_Awakening();
                    break;
                case 1:
                    BuildLevel2_TheSeparation();
                    break;
                case 2:
                    BuildLevel3_FollowTheLight();
                    break;
                case 3:
                    BuildLevel4_TheForgottenFlame();
                    break;
                case 4:
                    BuildLevel5_DarkPassage();
                    break;
                case 5:
                    BuildLevel6_TwoSides();
                    break;
                case 6:
                    BuildLevel7_TheLastLight();
                    break;
                case 7:
                    BuildLevel8_MyShadowQuit();
                    break;
                case 8:
                    BuildLevel9_EchoesInTheDark();
                    break;
                default:
                    BuildLevel10_TheFirstDawn();
                    break;
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
    }
}
