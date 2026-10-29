using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Entities;
using MyShadowQuit.Puzzle;
using MyShadowQuit.Combat;
using MyShadowQuit.Narrative;

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
            "LEVEL 1: AWAKENING",
            "LEVEL 2: THE SEPARATION",
            "LEVEL 3: FOLLOW THE LIGHT",
            "LEVEL 4: THE FORGOTTEN FLAME",
            "LEVEL 5: DARK PASSAGE",
            "LEVEL 6: TWO SIDES",
            "LEVEL 7: THE LAST LIGHT",
            "LEVEL 8: MY SHADOW QUIT",
            "LEVEL 9: ECHOES IN THE DARK",
            "LEVEL 10: THE FIRST DAWN",

            // World 2: The Inverted Forest (Levels 11–20)
            "LEVEL 11: WHISPERS IN THE ROOTS",
            "LEVEL 12: THE RISING PATH",
            "LEVEL 13: ROOTS OF LIGHT",
            "LEVEL 14: FADING FOOTSTEPS",
            "LEVEL 15: THE HOLLOW CANOPY",
            "LEVEL 16: A BRIDGE OF EMBERS",
            "LEVEL 17: THE SILENT THORNS",
            "LEVEL 18: THE FRACTURED GROVE",
            "LEVEL 19: TWO PATHS, ONE FLAME",
            "LEVEL 20: HEART OF THE INVERTED FOREST",

            // World 3: The Hollow Clocktower (Levels 21–30)
            "LEVEL 21: THE SILENT HANDS",
            "LEVEL 22: THE FIRST TICK",
            "LEVEL 23: BROKEN GEARS",
            "LEVEL 24: THE SWINGING HALL",
            "LEVEL 25: THE FROZEN MINUTE",
            "LEVEL 26: THE LENS OF EMBERS",
            "LEVEL 27: BETWEEN TWO TICKS",
            "LEVEL 28: THE FALLEN BELLS",
            "LEVEL 29: THE LAST HOUR",
            "LEVEL 30: HEART OF THE CLOCKTOWER",

            // World 4: The Drowned Cathedral (Levels 31–40)
            "LEVEL 31: BENEATH THE BLACK TIDE",
            "LEVEL 32: THE FLOODED NAVE",
            "LEVEL 33: DROWNED STEPS",
            "LEVEL 34: GLASS BENEATH THE SEA",
            "LEVEL 35: THE SILENT CHOIR",
            "LEVEL 36: RISING WATERS",
            "LEVEL 37: THE BROKEN BAPTISTERY",
            "LEVEL 38: BENEATH THE BELLS",
            "LEVEL 39: THE SUNKEN RELIQUARY",
            "LEVEL 40: THE LAST CATHEDRAL LIGHT",

            // World 5: The Abyss of the Last Flame (Levels 41–50)
            "LEVEL 41: EDGE OF OBLIVION",
            "LEVEL 42: WHERE LIGHT BREAKS",
            "LEVEL 43: ECHOES WITHOUT FORM",
            "LEVEL 44: THE FLAME ANCHORS",
            "LEVEL 45: BRIDGES OF NOTHING",
            "LEVEL 46: FRACTURED REFLECTIONS",
            "LEVEL 47: THE FORGOTTEN DIVIDE",
            "LEVEL 48: A WORLD WITHOUT FIRE",
            "LEVEL 49: BEFORE THE LAST DAWN",
            "LEVEL 50: THE LAST FLAME"
        };

        public static string FormatLevelTitle(int globalLevelNumber, string shortTitle)
        {
            return $"LEVEL {globalLevelNumber}: {shortTitle}";
        }

        public static string FormatLevelNumber(int globalLevelNumber)
        {
            return $"LEVEL {globalLevelNumber}";
        }

        public static readonly string[] LevelHints =
        {
            // World 1 Hints
            "A/D: Move   SPACE: Jump   Q: Pick up / Place Torch   E: Interact   TAB: Control Shadow",
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
            "Ascend to the core of the Great Tree. Overcome all forest mechanisms and reunite Flame and Shadow at the Ancient Arch.",

            // World 3 Hints
            "Step aboard the slow-turning gear platform to traverse the clocktower vestibule.",
            "The clockwork switch winds the ancient spring, opening the timed gate for a brief interval.",
            "Gear platforms rotate at differing cadences. Use your Shadow's double jump [SPACE x2] to bridge the high gap.",
            "Observe the pendulum's rhythmic swing. Wait for the apex before dashing through the hazard zone.",
            "Hit the clockwork switch to temporarily lock the swinging pendulums in place.",
            "Focus the Torch's golden flame upon the ancient lens to activate the dormant clockwork bridge.",
            "Your Shadow slips through the spectral iron grate while the Flame operates the mechanical lever.",
            "Ascend the vertical belfry. Coordinate moving gear platforms and evade the resonant pendulum weights.",
            "Synchronize gear elevators, clockwork levers, and the solar lens to unseal the high sanctuary.",
            "The Heart of the Clocktower. Unwind all five ancient celestial clockworks to open the Grand Chrono Portal.",

            // World 4 Hints
            "The cathedral lies drowned. Cross the flooded threshold and find the exit.",
            "Pull the tide lever to lower the water and reach the far side.",
            "Buoyant platforms rise and fall with the tide. Ride them across.",
            "Aim the Torch at the crystal; its refracted beam wakes the sealed gate.",
            "Two plates, two voices. Player and Shadow must each hold one.",
            "Raise the tide to float the platform, then lower it to open the passage.",
            "Shadow slips through the submerged grate while the Flame works the lever.",
            "Mind the sunken bell blades and the cutting currents.",
            "Combine tide, crystal and plates to unseal the reliquary.",
            "Player, Shadow and Torch must work together to kindle the final light.",

            // World 5 Hints
            "The ground ends here. Step across the fractured stones.",
            "Some stones exist only in light, others only in shadow. Watch the pulse.",
            "Only your Shadow can wake the echo switch.",
            "Set the Torch on the anchor to keep the mechanism alive.",
            "Torch and Shadow together weave a bridge. Cross before it fades.",
            "Light and shadow routes must meet at the far gate.",
            "A gate of light and a gate of shadow. Open both.",
            "Only the Torch's glow reveals the path. Carry it carefully.",
            "Every lesson of the descent, once more.",
            "Reach the heart of the abyss and kindle the Last Flame."
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
            int localIdx = (levelIndex >= 10) ? (levelIndex % 10) : levelIndex;
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(localIdx) : null;
            if (entry != null && !string.IsNullOrEmpty(entry.title)) return entry.title;
            return LevelTitles[Mathf.Clamp(globalIndex, 0, LevelTitles.Length - 1)];
        }

        public string GetLevelHint(int levelIndex)
        {
            int worldIdx = (ActiveWorld != null) ? ActiveWorld.WorldIndex : 0;
            int globalIndex = (levelIndex >= 10) ? levelIndex : (worldIdx * 10) + levelIndex;
            int localIdx = (levelIndex >= 10) ? (levelIndex % 10) : levelIndex;
            LevelEntry entry = ActiveWorld != null ? ActiveWorld.GetLevel(localIdx) : null;
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
                worldIdx = levelIndex / 10;
                localIndex = levelIndex % 10;
                if (_activeWorld == null || _activeWorld.WorldIndex != worldIdx)
                {
                    _activeWorld = WorldCatalog.GetWorld(worldIdx);
                }
            }
            else
            {
                localIndex = levelIndex;
            }

            int displayLevelNumber = (worldIdx * 10) + localIndex + 1;
            _levelRoot = new GameObject($"Level_{displayLevelNumber}");

            bool isExtendedChamber = localIndex == 9;
            int maxChamberX = isExtendedChamber ? 29 : 23;

            WorldEnvironmentProfile env = GetActiveEnvironmentProfile();

            if (cam != null)
            {
                cam.ResetZoom();
                cam.BuildParallaxLayers(env);
                float maxCamX = isExtendedChamber ? 20.8f : 14.8f;
                cam.SetBounds(new Vector2(-1.2f, 0.2f), new Vector2(maxCamX, 2.8f));
            }

            BuildChamberFrame(-9, maxChamberX, -4, 8);

            if (env.HasSporeParticles)
            {
                CreateSporeParticles(env.SporeParticleColor);
            }

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
            else if (worldIdx == 1)
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
            else if (worldIdx == 2)
            {
                switch (localIndex)
                {
                    case 0: BuildLevel21_TheSilentHands(); break;
                    case 1: BuildLevel22_TheFirstTick(); break;
                    case 2: BuildLevel23_BrokenGears(); break;
                    case 3: BuildLevel24_TheSwingingHall(); break;
                    case 4: BuildLevel25_TheFrozenMinute(); break;
                    case 5: BuildLevel26_TheLensOfEmbers(); break;
                    case 6: BuildLevel27_BetweenTwoTicks(); break;
                    case 7: BuildLevel28_TheFallenBells(); break;
                    case 8: BuildLevel29_TheLastHour(); break;
                    default: BuildLevel30_HeartOfTheClocktower(); break;
                }
            }
            else if (worldIdx == 3)
            {
                switch (localIndex)
                {
                    case 0: BuildLevel31_BeneathTheBlackTide(); break;
                    case 1: BuildLevel32_TheFloodedNave(); break;
                    case 2: BuildLevel33_DrownedSteps(); break;
                    case 3: BuildLevel34_GlassBeneathTheSea(); break;
                    case 4: BuildLevel35_TheSilentChoir(); break;
                    case 5: BuildLevel36_RisingWaters(); break;
                    case 6: BuildLevel37_TheBrokenBaptistery(); break;
                    case 7: BuildLevel38_BeneathTheBells(); break;
                    case 8: BuildLevel39_TheSunkenReliquary(); break;
                    default: BuildLevel40_TheLastCathedralLight(); break;
                }
            }
            else
            {
                switch (localIndex)
                {
                    case 0: BuildLevel41_EdgeOfOblivion(); break;
                    case 1: BuildLevel42_WhereLightBreaks(); break;
                    case 2: BuildLevel43_EchoesWithoutForm(); break;
                    case 3: BuildLevel44_TheFlameAnchors(); break;
                    case 4: BuildLevel45_BridgesOfNothing(); break;
                    case 5: BuildLevel46_FracturedReflections(); break;
                    case 6: BuildLevel47_TheForgottenDivide(); break;
                    case 7: BuildLevel48_AWorldWithoutFire(); break;
                    case 8: BuildLevel49_BeforeTheLastDawn(); break;
                    default: BuildLevel50_TheLastFlame(); break;
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

            // NPC: The Archivist
            CreateNPC(
                name: "The Archivist",
                sprite: "npc_archivist",
                pos: new Vector2(-1.5f, -1.15f),
                sequence: new DialogueSequence
                {
                    dialogueId = "archivist_intro",
                    unlockedLoreEntryId = "lore_archivist_intro",
                    lines = new[]
                    {
                        new DialogueLine { speakerName = "The Archivist", portraitSpriteName = "portrait_archivist", text = "Bearer of the spark... you awaken at last. But your shadow has stepped away from you." },
                        new DialogueLine { speakerName = "The Archivist", portraitSpriteName = "portrait_archivist", text = "Do not fear the dark. Coexist with it, solve the seals of the five sanctums, and rekindle what was lost." }
                    }
                }
            );

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

            // World 1 Enemy: Ash Crawler patrolling upper walkway
            CreateEnemy<AshCrawler>(new Vector2(4.0f, 1.6f));

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
            AncientDoor lowerGate9 = CreateAncientDoor(new Vector2(4.5f, -1.5f), new List<PuzzleNode> { upperShadowSw }, requireAll: true);
            _ = lowerGate9;

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
            WorldEnvironmentProfile env = GetActiveEnvironmentProfile();
            Sprite floorSp = AssetRegistry.GetSprite(env.FloorSpritePath);
            Sprite wallSp = AssetRegistry.GetSprite(env.WallSpritePath);

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
            CreatePlatformRow(startX, endX, (float)y, useBevelEdges);
        }

        private void CreatePlatformRow(int startX, int endX, float y, bool useBevelEdges)
        {
            WorldEnvironmentProfile env = GetActiveEnvironmentProfile();
            Sprite midSp = AssetRegistry.GetSprite(env.FloorSpritePath);
            Sprite leftSp = AssetRegistry.GetSprite(env.PlatformLeftSpritePath) ?? AssetRegistry.GetSprite("Environment/platform_edge_left");
            Sprite rightSp = AssetRegistry.GetSprite(env.PlatformRightSpritePath) ?? AssetRegistry.GetSprite("Environment/platform_edge_right");

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

        private void CreatePlatformRow(float startX, float endX, float y, bool useBevelEdges)
        {
            CreatePlatformRow(Mathf.RoundToInt(startX), Mathf.RoundToInt(endX), y, useBevelEdges);
        }

        private void CreateWallColumn(int x, int startY, int endY)
        {
            CreateWallColumn((float)x, startY, endY);
        }

        private void CreateWallColumn(float x, int startY, int endY)
        {
            WorldEnvironmentProfile env = GetActiveEnvironmentProfile();
            Sprite wallSp = AssetRegistry.GetSprite(env.WallSpritePath);
            for (int y = startY; y <= endY; y++)
            {
                CreateTileSprite($"WallCol_{x}_{y}", wallSp, new Vector2(x, y), 2);
            }
            float height = (endY - startY + 1);
            float centerY = (startY + endY) * 0.5f;
            CreateBoxCollider($"WallColCollider_{x}", new Vector2(x, centerY), new Vector2(0.95f, height));
        }

        private void CreateWallColumn(float x, float startY, float endY)
        {
            CreateWallColumn(x, Mathf.RoundToInt(startY), Mathf.RoundToInt(endY));
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
            playerObj.tag = "Player";
            playerObj.transform.SetParent(_levelRoot.transform, false);
            playerObj.transform.position = playerPos;
            _player = playerObj.AddComponent<PlayerController>();
            _player.EnsureInitialized();

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.tag = "Shadow";
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

        private PressurePlate CreatePressurePlate(Vector2 pos, bool latch, PressurePlateAllowedActivator activator = PressurePlateAllowedActivator.Any)
        {
            GameObject go = new GameObject("PressurePlate");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            PressurePlate pp = go.AddComponent<PressurePlate>();
            pp.Configure(latch, activator);
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
            MovableBlock mb = go.AddComponent<MovableBlock>();
            mb.EnsureInitialized();
            return mb;
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

            // NPC: The Rootkeeper
            CreateNPC(
                name: "The Rootkeeper",
                sprite: "npc_rootkeeper",
                pos: new Vector2(-6.2f, -1.15f),
                sequence: new DialogueSequence
                {
                    dialogueId = "rootkeeper_dialogue",
                    unlockedLoreEntryId = "lore_rootkeeper_secrets",
                    lines = new[]
                    {
                        new DialogueLine { speakerName = "The Rootkeeper", portraitSpriteName = "portrait_rootkeeper", text = "You tread upon the inverted canopy, mortal. The trees here weep shadows instead of dew." },
                        new DialogueLine { speakerName = "The Rootkeeper", portraitSpriteName = "portrait_rootkeeper", text = "Be wary of the floating wisps. They feed upon the unresolved sorrow of separated souls." }
                    }
                }
            );

            // World 2 Enemy: Forest Wisp
            CreateEnemy<ForestWisp>(new Vector2(2.5f, 0.5f));

            CreateForestHazard(new Vector2(1.5f, -3.2f), new Vector2(7.0f, 0.8f));

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

            // Hazard pit underneath the light root platforms: falling causes damage and respawns safely
            CreateForestHazard(new Vector2(1.0f, -3.4f), new Vector2(8.5f, 0.8f));

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

        // Level 15: The Hollow Canopy (Natural, clean 2-tier puzzle)
        // Design:
        // 1. Lower Level: Left bank (-8..-3, Y=-2.0) and Right bank (5..12, Y=-2.0) separated by a deep thorn chasm.
        //    A kinematic moving platform ferries the Protagonist across the chasm at Y=-2.0.
        // 2. Upper Canopy: Stepped natural boughs (-4..-2, Y=0.0) leading up to the Canopy Bridge (-1..8, Y=2.0).
        // 3. Obstacle: Permeable ShadowVinePassage at (2.0, 2.0) blocks the physical Protagonist while allowing Shadow to pass.
        // 4. Puzzle Sequence:
        //    - Protagonist with Torch waits at lower bank or platform.
        //    - Shadow ascends the natural bough (-4..-2, Y=0.0) into the canopy (-1..1, Y=2.0).
        //    - Shadow slips effortlessly through the Shadow Vine at (2.0, 2.0) to the inner canopy (3..8, Y=2.0).
        //    - Shadow activates the Shadow Switch at (6.0, 2.8), which dissolves the vine gate and opens the lower sanctum door at (8.5, -1.0).
        //    - Protagonist steps onto the moving ferry platform (-2.0 -> 4.0, Y=-2.0) and crosses the chasm.
        //    - Both reunite at the sanctuary exit portal (10.5, -1.0).
        private void BuildLevel15_TheHollowCanopy()
        {
            // Lower Level: Left entry bank and Right exit bank
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(5, 12, -2.0f, true);

            // Natural thorn hazard spanning the lower chasm bottom
            CreateForestHazard(new Vector2(1.0f, -3.4f), new Vector2(7.0f, 0.8f));

            // Lower ferry platform to carry Protagonist (and Torch) across the chasm
            CreateMovingPlatform(new[] { new Vector2(-2.0f, -2.0f), new Vector2(4.0f, -2.0f) }, speed: 2.2f);

            // Upper Canopy: Natural single ascending bough ledge
            CreatePlatformRow(-4, -2, 0.0f, true);

            // Continuous canopy bough (Shadow Vine provides the ethereal barrier)
            CreatePlatformRow(-1, 8, 2.0f, true);

            // Permeable Shadow Vine passage (no opaque solid wall column behind it)
            ShadowVinePassage vine = CreateShadowVine(new Vector2(2.0f, 2.0f));

            // Shadow switch in the inner canopy
            ShadowSwitch ss = CreateShadowSwitch(new Vector2(6.0f, 2.8f));
            vine.Configure(new List<PuzzleNode> { ss }, stayOpen: true);

            // Ancient Door guarding the sanctuary exit on the right bank
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.0f), new List<PuzzleNode> { ss });
            CreateExitPortal(new Vector2(10.5f, -1.0f), door, false);

            // Spawn Protagonist with Torch and Shadow at left bank
            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
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

        // Level 19: Two Paths, One Flame (Parallel paths + Moving Platform + Cooperative Switch)
        private void BuildLevel19_TwoPathsOneFlame()
        {
            // === UPPER PATH (The Flame's Route) ===
            // Upper Spawn ledge
            CreatePlatformRow(-8, -4, 2.0f, true);
            // Light Root blooming bridge over chasm
            CreateLightRoot(new Vector2(-2.0f, 2.0f), radius: 4.2f);
            // Central Upper Platform
            CreatePlatformRow(0, 3, 2.0f, true);
            // Upper Switch Platform
            CreatePlatformRow(5, 8, 2.0f, true);
            // Upper Cooperative Switch
            CooperativeSwitch upperCoop = CreateCooperativeSwitch(new Vector2(7.0f, 2.8f));

            // === LOWER PATH (The Shadow's Route) ===
            // Lower Spawn ledge
            CreatePlatformRow(-8, -4, -1.5f, true);
            // Small thorn hazard under first shadow jump
            CreateForestHazard(new Vector2(-2.5f, -3.2f), new Vector2(2.5f, 0.8f));
            // Shadow Vine lattice permeable only by Shadow
            CreateShadowVine(new Vector2(-2.5f, -0.5f));
            // Mid Shadow stepping platform
            CreatePlatformRow(-1, 1, -1.5f, true);
            // Thorn pit across lower chasm
            CreateForestHazard(new Vector2(3.5f, -3.2f), new Vector2(4.5f, 0.8f));
            // Kinematic Moving Platform crossing the thorn pit
            CreateMovingPlatform(new[] { new Vector2(1.8f, -1.5f), new Vector2(5.2f, -1.5f) }, speed: 2.0f, wait: 0.6f);
            // Lower Switch Platform
            CreatePlatformRow(6, 8, -1.5f, true);
            // Lower Cooperative Switch
            CooperativeSwitch lowerCoop = CreateCooperativeSwitch(new Vector2(7.0f, -0.7f));

            // Configure Dual Cooperative Switches with generous 6.0s timed activation
            upperCoop.Configure(lowerCoop, CooperativeMode.TimedActivation, timer: 6.0f, stayActivated: true);
            lowerCoop.Configure(upperCoop, CooperativeMode.TimedActivation, timer: 6.0f, stayActivated: true);

            // === CONVERGENCE & EXIT SANCTUM ===
            // Intermediate ascending step for lower path to join sanctum
            CreatePlatformRow(9, 9, -0.6f, true);
            // Sanctum Platform
            CreatePlatformRow(10, 14, 0.2f, true);
            // Ancient Door unlocked by the cooperative switches
            AncientDoor door = CreateAncientDoor(new Vector2(10.5f, 1.2f), new List<PuzzleNode> { upperCoop });
            // Exit Sanctuary Portal
            CreateExitPortal(new Vector2(12.8f, 1.2f), door, false);

            // Spawn Protagonist on Upper Ledge with Torch; Spawn Shadow on Lower Ledge separated
            SpawnCharactersAndTorch(new Vector2(-6.0f, 3.0f), new Vector2(-6.0f, -0.5f), new Vector2(-6.0f, 3.0f), true);
        }

        // Level 20: Heart of the Inverted Forest (5-Phase Grand Climax)
        // Level 20: Heart of the Inverted Forest (Climactic Grand Finale of World 2)
        // Clean 4-Moment Architecture:
        // Moment 1: Entrance & Vertical Ascent (Kinematic Elevator from -2.0 to 2.5)
        // Moment 2: High Canopy & Light Roots (Torch blooms bridge to upper sanctuary)
        // Moment 3: Shadow Separation & High Mechanism (Shadow Vine lattice at X=12, crumbling bough at X=14 to high Shadow Switch at 16.5)
        // Moment 4: Cooperative Descent & Sanctuary Arch (Two tiered cooperative plates at X=19, Arch at X=24)
        private void BuildLevel20_HeartOfTheInvertedForest()
        {
            // MOMENT 1: Entrance & Ascent
            CreatePlatformRow(-8, -4, -2.0f, true);
            CreateMovingPlatform(new[] { new Vector2(-2.5f, -2.0f), new Vector2(-2.5f, 2.5f) }, speed: 2.2f);

            // Hazard pit spanning the abyss floor
            CreateForestHazard(new Vector2(3.5f, -3.4f), new Vector2(10.0f, 0.8f));

            // MOMENT 2: High Canopy with Light Roots
            // Left canopy bough where elevator arrives
            CreatePlatformRow(-1, 2, 2.5f, true);
            // Light root blooms when Protagonist carrying Torch approaches
            CreateLightRoot(new Vector2(4.0f, 2.5f), radius: 4.2f);
            // Central canopy bough across the light root bridge
            CreatePlatformRow(6, 9, 2.5f, true);

            // MOMENT 3: Shadow Separation & High Switch
            // Permeable Shadow Vine passage (no opaque solid wall column behind it)
            CreatePlatformRow(10, 11, 2.5f, true);
            ShadowVinePassage vine = CreateShadowVine(new Vector2(11.0f, 2.5f));

            // Crumbling fungal platform and high branch for Shadow
            CreateTemporaryPlatform(new Vector2(13.5f, 2.5f), collapseTime: 1.5f, respawnTime: 2.5f);
            CreatePlatformRow(15, 18, 2.5f, true);
            ShadowSwitch highSwitch = CreateShadowSwitch(new Vector2(16.5f, 3.3f));
            vine.Configure(new List<PuzzleNode> { highSwitch }, stayOpen: true);

            // MOMENT 4: Cooperative Chamber & Sanctuary Arch
            // Upper cooperative ledge (Y=1.5)
            CreatePlatformRow(18, 20, 1.5f, true);
            CooperativeSwitch coopUpper = CreateCooperativeSwitch(new Vector2(19.0f, 2.3f));

            // Lower cooperative floor (Y=-1.5)
            CreatePlatformRow(18, 20, -1.5f, true);
            CooperativeSwitch coopLower = CreateCooperativeSwitch(new Vector2(19.0f, -0.7f));

            // Connect dual switches with generous timed activation and latched completion
            coopUpper.Configure(coopLower, CooperativeMode.TimedActivation, timer: 6.0f, stayActivated: true);
            coopLower.Configure(coopUpper, CooperativeMode.TimedActivation, timer: 6.0f, stayActivated: true);

            // Natural stepping bough connecting lower tier to sanctuary level
            CreatePlatformRow(22, 23, 0.0f, true);

            // Sacred Grove Exit Platform (Y=1.5)
            CreatePlatformRow(24, 28, 1.5f, true);
            AncientDoor finalDoor = CreateAncientDoor(new Vector2(24.5f, 2.5f), new List<PuzzleNode> { highSwitch, coopUpper }, requireAll: true);
            CreateExitPortal(new Vector2(26.5f, 2.5f), finalDoor, requireShadowNear: true);

            // Spawn both characters on the entrance terrace with Torch
            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // =========================================================================
        // WORLD 3: THE HOLLOW CLOCKTOWER (Levels 21–30)
        // Dark steel blue, aged bronze, midnight indigo, ticking clockwork mechanisms
        // =========================================================================

        // Level 21: The Silent Hands (Horizontal Gear Platform introduction)
        private void BuildLevel21_TheSilentHands()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(4, 12, -2.0f, true);

            // NPC: The Last Clockmaker
            CreateNPC(
                name: "The Last Clockmaker",
                sprite: "npc_clockmaker",
                pos: new Vector2(-5.5f, -1.15f),
                sequence: new DialogueSequence
                {
                    dialogueId = "clockmaker_dialogue",
                    unlockedLoreEntryId = "lore_clockmaker_emberblade",
                    lines = new[]
                    {
                        new DialogueLine { speakerName = "The Last Clockmaker", portraitSpriteName = "portrait_clockmaker", text = "Tick... tack... the gears of this world ground to a halt centuries ago." },
                        new DialogueLine { speakerName = "The Last Clockmaker", portraitSpriteName = "portrait_clockmaker", text = "I left the Emberblade further up in the tower (Level 23). Take it to cut down the automatons!" }
                    }
                }
            );

            // Enemy: Clockwork Sentinel
            CreateEnemy<ClockworkSentinel>(new Vector2(7.5f, -1.2f));

            CreateGearPlatform(GearMotionType.HorizontalOscillation, new Vector2(0.5f, -2.0f), radius: 2.2f, speed: 1.8f, angle: 0f);

            PressurePlate plate = CreatePressurePlate(new Vector2(6.5f, -1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.0f), new List<PuzzleNode> { plate });
            CreateExitPortal(new Vector2(10.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 22: The First Tick (Pendulum Obstacle introduction)
        private void BuildLevel22_TheFirstTick()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(0, 1, -2.0f, true);
            CreatePlatformRow(4, 12, -2.0f, true);

            CreatePendulumObstacle(new Vector2(0.5f, 2.5f), length: 3.2f, angle: 50f, speed: 1.8f, phase: 0f);

            LightSwitch ls = CreateLightSwitch(new Vector2(6.5f, -1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.0f), new List<PuzzleNode> { ls });
            CreateExitPortal(new Vector2(10.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 23: Broken Gears (Vertical Gear Platform climb)
        private void BuildLevel23_BrokenGears()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(1, 3, 0.5f, true);
            CreatePlatformRow(7, 13, 2.8f, true);

            // WEAPON UNLOCK 1: EMBERBLADE
            CreateWeaponPickup(WeaponType.Emberblade, new Vector2(2.0f, 1.4f));

            CreateGearPlatform(GearMotionType.VerticalOscillation, new Vector2(-1.0f, 0.0f), radius: 1.6f, speed: 1.5f, angle: 0f);
            CreateGearPlatform(GearMotionType.VerticalOscillation, new Vector2(5.0f, 1.8f), radius: 1.5f, speed: 1.5f, angle: 180f);

            PressurePlate plate = CreatePressurePlate(new Vector2(8.5f, 3.6f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(10.0f, 3.8f), new List<PuzzleNode> { plate });
            CreateExitPortal(new Vector2(11.5f, 3.8f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 24: The Swinging Hall (Double Pendulums out of phase + Shadow Switch)
        private void BuildLevel24_TheSwingingHall()
        {
            CreatePlatformRow(-8, -4, -2.0f, true);
            CreatePlatformRow(-1, 1, -2.0f, true);
            CreatePlatformRow(4, 6, -2.0f, true);
            CreatePlatformRow(9, 14, -2.0f, true);

            CreatePendulumObstacle(new Vector2(-2.5f, 2.4f), length: 3.2f, angle: 45f, speed: 1.9f, phase: 0f);
            CreatePendulumObstacle(new Vector2(2.5f, 2.4f), length: 3.2f, angle: 45f, speed: 1.9f, phase: 180f);

            ShadowSwitch ss = CreateShadowSwitch(new Vector2(5.0f, 0.8f));
            AncientDoor door = CreateAncientDoor(new Vector2(10.5f, -1.0f), new List<PuzzleNode> { ss });
            CreateExitPortal(new Vector2(12.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 25: The Frozen Minute (Clockwork Switch freezing fast pendulum)
        private void BuildLevel25_TheFrozenMinute()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(-1, 6, -2.0f, true);
            CreatePlatformRow(8, 13, -2.0f, true);

            PendulumObstacle fastPendulum = CreatePendulumObstacle(new Vector2(2.5f, 2.4f), length: 3.2f, angle: 55f, speed: 3.6f, phase: 0f);
            CreateClockworkSwitch(new Vector2(-4.0f, -1.2f), timed: true, duration: 6.5f, pendulums: new List<PendulumObstacle> { fastPendulum });

            PressurePlate plate = CreatePressurePlate(new Vector2(9.5f, -1.2f), latch: true);
            AncientDoor door = CreateAncientDoor(new Vector2(11.0f, -1.0f), new List<PuzzleNode> { plate });
            CreateExitPortal(new Vector2(12.5f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 26: The Lens of Embers (TorchPoweredClockwork activating gear platform)
        private void BuildLevel26_TheLensOfEmbers()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(-5, -3, 0.5f, true);
            CreatePlatformRow(6, 12, -2.0f, true);

            RotatingGearPlatform gear = CreateGearPlatform(GearMotionType.HorizontalOscillation, new Vector2(1.5f, -2.0f), radius: 2.5f, speed: 1.8f, angle: 0f);
            gear.IsPaused = true;

            AncientDoor door = CreateAncientDoor(new Vector2(9.0f, -1.0f), new List<PuzzleNode>());
            TorchPoweredClockwork lens = CreateTorchLens(new Vector2(-4.0f, 1.3f), radius: 2.8f, stayLit: true, door: door, gears: new List<RotatingGearPlatform> { gear });
            door.Configure(new List<PuzzleNode> { lens });

            CreateExitPortal(new Vector2(11.0f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 27: Between Two Ticks (ShadowClockworkPassage + Clockwork Switch unlocking grate)
        private void BuildLevel27_BetweenTwoTicks()
        {
            CreatePlatformRow(-8, -1, -2.0f, true);
            CreatePlatformRow(1, 13, -2.0f, true);

            CreateWallColumn(0, 0, 5);
            ShadowClockworkPassage grate = CreateShadowGrate(new Vector2(0.0f, -1.0f));

            PendulumObstacle pendulum = CreatePendulumObstacle(new Vector2(4.0f, 2.5f), length: 3.2f, angle: 45f, speed: 2.0f);

            AncientDoor door = CreateAncientDoor(new Vector2(10.0f, -1.0f), new List<PuzzleNode>());
            ClockworkSwitch cSwitch = CreateClockworkSwitch(new Vector2(6.0f, -1.2f), timed: true, duration: 7.0f, door: door, pendulums: new List<PendulumObstacle> { pendulum });
            door.Configure(new List<PuzzleNode> { cSwitch });
            grate.Configure(new List<PuzzleNode> { cSwitch }, stayOpen: true);

            CreateExitPortal(new Vector2(12.0f, -1.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 28: The Fallen Bells (Vertical belfry climb with bells and solar lens)
        private void BuildLevel28_TheFallenBells()
        {
            CreatePlatformRow(-8, -3, -2.0f, true);
            CreatePlatformRow(1, 4, 0.8f, true);
            CreatePlatformRow(8, 13, 2.6f, true);

            CreateGearPlatform(GearMotionType.VerticalOscillation, new Vector2(-1.0f, 0.0f), radius: 1.6f, speed: 1.5f, angle: 0f);
            CreatePendulumObstacle(new Vector2(2.5f, 4.0f), length: 2.2f, angle: 40f, speed: 1.8f);

            RotatingGearPlatform gear2 = CreateGearPlatform(GearMotionType.HorizontalOscillation, new Vector2(6.0f, 1.8f), radius: 1.6f, speed: 1.6f, angle: 0f);
            _ = gear2;

            AncientDoor door = CreateAncientDoor(new Vector2(10.5f, 3.6f), new List<PuzzleNode>());
            TorchPoweredClockwork lens = CreateTorchLens(new Vector2(2.5f, 1.6f), radius: 2.6f, stayLit: true, door: door);
            door.Configure(new List<PuzzleNode> { lens });

            CreateExitPortal(new Vector2(12.0f, 3.6f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, -1.0f), new Vector2(-7.0f, -1.0f), new Vector2(-6.0f, -1.0f), true);
        }

        // Level 29: The Last Hour (Dual-tier upper gantry and lower gear tunnel)
        private void BuildLevel29_TheLastHour()
        {
            // Upper Gantry (Protagonist + Torch path)
            CreatePlatformRow(-8, 12, 2.0f, true);

            // Lower Tunnel (Shadow path)
            CreatePlatformRow(-8, 12, -2.0f, true);

            // Grates in lower tunnel (ShadowClockworkPassage manages its own selective collider: solid to Protagonist, permeable to Shadow)
            CreateShadowGrate(new Vector2(-4.0f, -1.0f));
            CreateShadowGrate(new Vector2(2.0f, -1.0f));
            CreateShadowGrate(new Vector2(6.0f, -1.0f));

            // Lower tunnel pendulum
            CreatePendulumObstacle(new Vector2(-1.0f, 0.8f), length: 2.0f, angle: 40f, speed: 1.8f);

            // Upper Gantry Lenses
            TorchPoweredClockwork lens1 = CreateTorchLens(new Vector2(-1.5f, 2.8f), radius: 2.6f, stayLit: true);
            TorchPoweredClockwork lens2 = CreateTorchLens(new Vector2(4.5f, 2.8f), radius: 2.6f, stayLit: true);

            // Lower Tunnel Clockwork Switch
            ClockworkSwitch shadowClockSwitch = CreateClockworkSwitch(new Vector2(8.5f, -1.2f), timed: false, duration: 0f);

            // Upper Gantry Door and Portal
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, 3.0f), new List<PuzzleNode> { lens1, lens2, shadowClockSwitch }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, 3.0f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.0f, 3.0f), new Vector2(-6.0f, -1.0f), new Vector2(-6.0f, 3.0f), true);
        }

        // Level 30: Heart of the Clocktower (4-Phase Grand Climax)
        private void BuildLevel30_HeartOfTheClocktower()
        {
            // === PHASE 1: Gear Ascent (x = -8 to -1) ===
            CreatePlatformRow(-8, -4, -2.0f, true);
            CreateGearPlatform(GearMotionType.VerticalOscillation, new Vector2(-2.5f, -0.5f), radius: 1.6f, speed: 1.5f, angle: 0f);

            // === PHASE 2: Hall of Pendulums & Clockwork Switch (x = 0 to 8) ===
            CreatePlatformRow(0, 8, 1.2f, true);
            PendulumObstacle pen1 = CreatePendulumObstacle(new Vector2(2.5f, 4.8f), length: 2.8f, angle: 50f, speed: 2.5f, phase: 0f);
            PendulumObstacle pen2 = CreatePendulumObstacle(new Vector2(5.5f, 4.8f), length: 2.8f, angle: 50f, speed: 2.5f, phase: 90f);

            ClockworkSwitch freezeSwitch = CreateClockworkSwitch(new Vector2(0.5f, 2.0f), timed: true, duration: 7.0f, pendulums: new List<PendulumObstacle> { pen1, pen2 });
            _ = freezeSwitch;

            // === PHASE 3: Spectral Grate & Internal Machinery (x = 9 to 16) ===
            CreatePlatformRow(9, 16, 1.2f, true);
            // ShadowClockworkPassage manages protagonist-blocking collision while letting Shadow pass through
            ShadowClockworkPassage grate = CreateShadowGrate(new Vector2(10.0f, 2.2f));

            ShadowSwitch shadowSecretSwitch = CreateShadowSwitch(new Vector2(13.0f, 2.0f));
            grate.Configure(new List<PuzzleNode> { shadowSecretSwitch }, stayOpen: true);

            // Gear lift to Phase 4
            RotatingGearPlatform apexLift = CreateGearPlatform(GearMotionType.VerticalOscillation, new Vector2(15.5f, 2.2f), radius: 1.5f, speed: 1.5f, angle: 0f);
            _ = apexLift;

            // === PHASE 4: The Apex Belfry (x = 17 to 28) ===
            CreatePlatformRow(17, 28, 2.5f, true);

            TorchPoweredClockwork apexLens = CreateTorchLens(new Vector2(19.0f, 3.3f), radius: 2.8f, stayLit: true);

            AncientDoor grandDoor = CreateAncientDoor(
                new Vector2(23.5f, 3.5f),
                new List<PuzzleNode> { shadowSecretSwitch, apexLens },
                requireAll: true
            );
            grandDoor.ConfigureCustomSprites("Environment/ceremonial_door_closed", "Environment/ceremonial_door_open");

            CreateProp("Environment/dawn_portal_arch", new Vector2(26.5f, 3.5f), 1);
            CreateExitPortal(new Vector2(26.5f, 3.5f), grandDoor, requireShadowNear: true);

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

        private CooperativeSwitch CreateCooperativeSwitch(Vector2 pos, CooperativeSwitch partner = null, CooperativeMode mode = CooperativeMode.BothPresent, float timer = 5.0f)
        {
            GameObject go = new GameObject("CooperativeSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            CooperativeSwitch cs = go.AddComponent<CooperativeSwitch>();
            cs.Configure(partner, mode, timer);
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

        // =========================================================================
        // WORLD 3 FACTORY HELPERS
        // =========================================================================

        private RotatingGearPlatform CreateGearPlatform(GearMotionType type, Vector2 center, float radius = 2.2f, float speed = 1.2f, float angle = 0f, Vector2? size = null)
        {
            GameObject go = new GameObject("RotatingGearPlatform");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = center;
            RotatingGearPlatform gear = go.AddComponent<RotatingGearPlatform>();
            gear.Configure(type, center, radius, speed, angle, size);
            return gear;
        }

        private PendulumObstacle CreatePendulumObstacle(Vector2 pivotPos, float length = 3.2f, float angle = 55f, float speed = 2.2f, float phase = 0f)
        {
            GameObject go = new GameObject("PendulumObstacle");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pivotPos;
            PendulumObstacle p = go.AddComponent<PendulumObstacle>();
            p.Configure(length, angle, speed, phase);
            return p;
        }

        private ClockworkSwitch CreateClockworkSwitch(Vector2 pos, bool timed = true, float duration = 6.0f, AncientDoor door = null, List<PendulumObstacle> pendulums = null, List<RotatingGearPlatform> gears = null)
        {
            GameObject go = new GameObject("ClockworkSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ClockworkSwitch cs = go.AddComponent<ClockworkSwitch>();
            cs.Configure(timed, duration, door, pendulums, gears);
            return cs;
        }

        private TorchPoweredClockwork CreateTorchLens(Vector2 pos, float radius = 2.8f, bool stayLit = true, AncientDoor door = null, List<RotatingGearPlatform> gears = null, List<GameObject> enableObjects = null)
        {
            GameObject go = new GameObject("TorchPoweredClockwork");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            TorchPoweredClockwork tc = go.AddComponent<TorchPoweredClockwork>();
            tc.Configure(radius, stayLit, door, gears, enableObjects);
            return tc;
        }

        private ShadowClockworkPassage CreateShadowGrate(Vector2 pos, List<PuzzleNode> unlockNodes = null, bool stayOpen = true)
        {
            GameObject go = new GameObject("ShadowClockworkPassage");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            ShadowClockworkPassage sp = go.AddComponent<ShadowClockworkPassage>();
            sp.Configure(unlockNodes, stayOpen);
            return sp;
        }

        // =========================================================================
        // WORLD 4: THE DROWNED CATHEDRAL (Levels 31–40)
        // =========================================================================

        private WaterLevelZone CreateWaterZone(Vector2 pos, float lowY = -2f, float highY = 2f, bool startHigh = false, float width = 36f)
        {
            GameObject go = new GameObject("WaterLevelZone");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            WaterLevelZone wz = go.AddComponent<WaterLevelZone>();

            // Water surface visual tile row
            GameObject vis = new GameObject("WaterSurfaceVisual");
            vis.transform.SetParent(go.transform, false);
            SpriteRenderer sr = vis.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/water_surface", 32f);
            sr.color = new Color(0.25f, 0.85f, 0.95f, 0.85f);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(width, 0.5f);
            sr.sortingOrder = 5;

            // Submerged body visual: atmospheric deep cyan aquatic tint
            GameObject body = new GameObject("WaterBodyVisual");
            body.transform.SetParent(go.transform, false);
            SpriteRenderer bodySr = body.AddComponent<SpriteRenderer>();
            bodySr.sprite = AssetRegistry.GetSprite("UI/ui_panel", 32f);
            bodySr.color = new Color(0.04f, 0.22f, 0.28f, 0.60f);
            bodySr.drawMode = SpriteDrawMode.Sliced;
            bodySr.sortingOrder = 4;

            wz.Configure(lowY, highY, startHigh, 2.5f, width);
            return wz;
        }

        private BuoyantPlatform2D CreateBuoyantPlatform(Vector2 pos, WaterLevelZone zone, float width = 2f)
        {
            GameObject go = new GameObject("BuoyantPlatform");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/buoyant_platform", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 0.45f);

            BuoyantPlatform2D bp = go.AddComponent<BuoyantPlatform2D>();
            bp.Configure(zone);
            return bp;
        }

        private TideSwitch CreateTideSwitch(Vector2 pos, WaterLevelZone zone, bool toggle = true, bool highOnTrigger = true)
        {
            GameObject go = new GameObject("TideSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/tide_switch_off", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);

            TideSwitch ts = go.AddComponent<TideSwitch>();
            ts.Configure(zone, toggle, highOnTrigger);
            return ts;
        }

        private RefractionCrystal CreateRefractionCrystal(Vector2 pos, AncientDoor door, float radius = 2.4f)
        {
            GameObject go = new GameObject("RefractionCrystal");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/crystal_off", 32f);
            sr.sortingOrder = 2;

            RefractionCrystal rc = go.AddComponent<RefractionCrystal>();
            rc.Configure(door, radius);
            return rc;
        }

        private SubmergedGratePassage CreateSubmergedGrate(Vector2 pos, float openOffset = 2.5f)
        {
            GameObject go = new GameObject("SubmergedGrate");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/submerged_grate", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 1.8f);

            SubmergedGratePassage sg = go.AddComponent<SubmergedGratePassage>();
            return sg;
        }

        private CathedralBellHazard CreateCathedralBell(Vector2 pos, float maxAngle = 45f, float speed = 1.8f, float offset = 0f)
        {
            GameObject go = new GameObject("CathedralBell");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World4/bell_hazard", 32f);
            sr.sortingOrder = 3;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.65f;

            CathedralBellHazard cb = go.AddComponent<CathedralBellHazard>();
            cb.Configure(maxAngle, speed, offset);
            return cb;
        }

        // Level 31: Beneath the Black Tide
        private void BuildLevel31_BeneathTheBlackTide()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(3f, -2.5f), lowY: -3f, highY: -1.2f, startHigh: false);
            CreatePlatformRow(-8, -2, -2f, true);
            CreateBuoyantPlatform(new Vector2(2f, -2f), water, 2.5f);
            CreatePlatformRow(6, 12, -2f, true);

            // NPC: The Drowned Oracle
            CreateNPC(
                name: "The Drowned Oracle",
                sprite: "npc_oracle",
                pos: new Vector2(-5.5f, -1.15f),
                sequence: new DialogueSequence
                {
                    dialogueId = "oracle_dialogue",
                    unlockedLoreEntryId = "lore_oracle_cathedral",
                    lines = new[]
                    {
                        new DialogueLine { speakerName = "The Drowned Oracle", portraitSpriteName = "portrait_oracle", text = "The black tides drown our prayers... but not your flame." },
                        new DialogueLine { speakerName = "The Drowned Oracle", portraitSpriteName = "portrait_oracle", text = "Deep in the stained glass nave (Level 34), the Hollow Crossbow rests in the shallows." }
                    }
                }
            );

            // World 4 Enemy: Drowned Guardian
            CreateEnemy<DrownedGuardian>(new Vector2(8.5f, -1.2f));

            ShadowSwitch sw31 = CreateShadowSwitch(new Vector2(8.0f, -1.25f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw31 }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 32: The Flooded Nave
        private void BuildLevel32_TheFloodedNave()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(3f, -1.5f), lowY: -3f, highY: 0.5f, startHigh: true);
            CreatePlatformRow(-8, -3, 0f, true);
            CreatePlatformRow(7, 12, 0f, true);

            CreateBuoyantPlatform(new Vector2(2f, 0.5f), water, 2.5f);
            CreateTideSwitch(new Vector2(-4f, 0.8f), water, toggle: true);

            ShadowSwitch sw32 = CreateShadowSwitch(new Vector2(8.5f, 0.75f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, 0.8f), new List<PuzzleNode> { sw32 }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, 0.8f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, 0.8f), new Vector2(-7.2f, 0.8f), new Vector2(-6.5f, 0.8f), true);
        }

        // Level 33: Drowned Steps
        private void BuildLevel33_DrownedSteps()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(2f, -1.5f), lowY: -2.5f, highY: 1.2f, startHigh: false);
            CreatePlatformRow(-8, -4, -1.5f, true);
            CreateBuoyantPlatform(new Vector2(-1.5f, -1.5f), water, 2f);
            CreateBuoyantPlatform(new Vector2(3.5f, -1.0f), water, 2f);
            CreatePlatformRow(7, 12, 1.5f, true);

            CreateTideSwitch(new Vector2(-5f, -0.7f), water, toggle: true);

            ShadowSwitch sw33 = CreateShadowSwitch(new Vector2(8.5f, 2.25f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, 2.3f), new List<PuzzleNode> { sw33 }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, 2.3f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -0.7f), new Vector2(-7.2f, -0.7f), new Vector2(-6.5f, -0.7f), true);
        }

        // Level 34: Glass Beneath the Sea
        private void BuildLevel34_GlassBeneathTheSea()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(2f, -2.2f), lowY: -2.8f, highY: -1.0f, startHigh: false);
            CreatePlatformRow(-8, 12, -2f, true);

            // WEAPON UNLOCK 2: HOLLOW CROSSBOW
            CreateWeaponPickup(WeaponType.HollowCrossbow, new Vector2(-1.5f, -1.2f));

            AncientDoor door = CreateAncientDoor(new Vector2(8f, -1.2f), new List<PuzzleNode>());
            CreateRefractionCrystal(new Vector2(1f, -1.2f), door, radius: 2.6f);
            CreateExitPortal(new Vector2(11f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 35: The Silent Choir
        private void BuildLevel35_TheSilentChoir()
        {
            // The Silent Choir: Sacred dual resonant choral plates.
            // Plate A: Protagonist only (amber illumination). Non-latching.
            // Plate B: Shadow only (violet illumination). Non-latching.
            // When both choral plates are occupied simultaneously, the sacred ancient choir sings in harmony,
            // latching the ancient choral seal open permanently (latchOpen: true)!
            WaterLevelZone water = CreateWaterZone(new Vector2(2f, -2.5f), lowY: -3.0f, highY: -1.2f, startHigh: false);
            CreatePlatformRow(-8, 12, -2f, true);

            // Sacred Choral Plate A (Left Choir - stepped upon only by Protagonist, amber)
            // Platform top is at Y = -1.53f. Position plate at Y = -1.50f so it rests cleanly on the surface.
            PressurePlate p1 = CreatePressurePlate(new Vector2(-1.5f, -1.50f), latch: false, activator: PressurePlateAllowedActivator.ProtagonistOnly);

            // Sacred Choral Plate B (Right Choir - stepped upon only by Shadow, violet)
            PressurePlate p2 = CreatePressurePlate(new Vector2(4.5f, -1.50f), latch: false, activator: PressurePlateAllowedActivator.ShadowOnly);

            // Ancient Door latches open permanently once both plates are active simultaneously
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { p1, p2 }, requireAll: true, latchOpen: true);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 36: Rising Waters
        private void BuildLevel36_RisingWaters()
        {
            // Level 36: Rising Waters
            // Puzzle layout:
            // 1. Safe starting terrace: (-8..-3, Y = -1.5). Characters spawn safely at x=-6.5, Y=-0.7.
            // 2. TideSwitch at (-4.5, -0.7) lets either character toggle the cathedral tide.
            // 3. Flood basin: Low tide Y = -2.5 (flooring well below platform). High tide Y = 0.5.
            // 4. Buoyant Platform: Spans x = -0.5 to 3.5 (width 4.0m) at initial pos Y = -2.25.
            //    At low tide: platform rests comfortably in the basin at Y = -2.0. Characters can jump onto it.
            //    At high tide (Y = 0.5): platform floats up smoothly to top surface Y = 0.5 + 0.25 + 0.225 = 0.975m.
            // 5. Destination terrace: (4..12, Y = 1.0), with walkable surface at Y = 1.47m.
            //    From high-tide platform (surface Y ~ 1.0m), reaching the destination terrace (Y = 1.47m, gap ~ 0.5m)
            //    is a comfortable, intuitive jump of ~0.5m height difference for both Protagonist (jump 10.6) and Shadow (jump 7.8)!
            // 6. Cooperative action: Shadow activates high ShadowSwitch at (7.5, 2.05), which unseals the AncientDoor (9.5, 2.0).
            // 7. Both characters reunite at the Exit Portal (11.5, 2.0).
            WaterLevelZone water = CreateWaterZone(new Vector2(1.5f, -1.0f), lowY: -2.5f, highY: 0.5f, startHigh: false, width: 36f);
            CreatePlatformRow(-8, -3, -1.5f, true);
            CreateBuoyantPlatform(new Vector2(1.5f, -2.25f), water, 4.0f);
            CreatePlatformRow(4, 12, 1.0f, true);

            CreateTideSwitch(new Vector2(-4.5f, -0.7f), water, toggle: true);

            ShadowSwitch sw36 = CreateShadowSwitch(new Vector2(7.5f, 2.05f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, 1.8f), new List<PuzzleNode> { sw36 }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, 1.8f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -0.7f), new Vector2(-7.2f, -0.7f), new Vector2(-6.5f, -0.7f), true);
        }

        // Level 37: The Broken Baptistery
        private void BuildLevel37_TheBrokenBaptistery()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(2f, -2.4f), lowY: -3.0f, highY: -1.2f, startHigh: false);
            CreatePlatformRow(-8, 12, -2f, true);
            SubmergedGratePassage grate = CreateSubmergedGrate(new Vector2(0f, -1.2f));

            ShadowSwitch sw = CreateShadowSwitch(new Vector2(3.5f, -1.25f));
            AncientDoor door = CreateAncientDoor(new Vector2(8.5f, -1.2f), new List<PuzzleNode> { sw }, requireAll: true);
            CreateExitPortal(new Vector2(11f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 38: Beneath the Bells
        private void BuildLevel38_BeneathTheBells()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(2f, -2.4f), lowY: -3.0f, highY: -1.2f, startHigh: false);
            CreatePlatformRow(-8, 12, -2f, true);
            CreateCathedralBell(new Vector2(-1f, 1.8f), maxAngle: 50f, speed: 2.2f, offset: 0f);
            CreateCathedralBell(new Vector2(4f, 1.8f), maxAngle: 50f, speed: 2.2f, offset: 1.5f);

            ShadowSwitch sw38 = CreateShadowSwitch(new Vector2(8.5f, -1.25f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw38 }, requireAll: true);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 39: The Sunken Reliquary
        private void BuildLevel39_TheSunkenReliquary()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(0f, -1.5f), lowY: -2.5f, highY: 0.8f, startHigh: false);
            CreatePlatformRow(-8, -3, -1.5f, true);
            CreateBuoyantPlatform(new Vector2(0f, -1.5f), water, 2.5f);
            CreatePlatformRow(4, 12, 1.0f, true);

            CreateTideSwitch(new Vector2(-4.5f, -0.7f), water, toggle: true);

            ShadowSwitch sw39 = CreateShadowSwitch(new Vector2(8.0f, 1.75f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, 1.8f), new List<PuzzleNode> { sw39 }, requireAll: true);
            CreateRefractionCrystal(new Vector2(6f, 1.8f), door, radius: 2.5f);
            CreateExitPortal(new Vector2(11.5f, 1.8f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -0.7f), new Vector2(-7.2f, -0.7f), new Vector2(-6.5f, -0.7f), true);
        }

        // Level 40: The Last Cathedral Light
        private void BuildLevel40_TheLastCathedralLight()
        {
            WaterLevelZone water = CreateWaterZone(new Vector2(5f, -1.5f), lowY: -2.5f, highY: 1.2f, startHigh: false, width: 44f);
            CreatePlatformRow(-8, -2, -1.5f, true);
            CreateBuoyantPlatform(new Vector2(3f, -1.5f), water, 3f);
            CreatePlatformRow(8, 28, 1.5f, true);

            CreateTideSwitch(new Vector2(-4f, -0.7f), water, toggle: true);
            CreateCathedralBell(new Vector2(13f, 4.5f), maxAngle: 45f, speed: 2f);

            ShadowSwitch sw40 = CreateShadowSwitch(new Vector2(20.5f, 2.25f));
            AncientDoor door = CreateAncientDoor(new Vector2(23f, 2.3f), new List<PuzzleNode> { sw40 }, requireAll: true);
            CreateRefractionCrystal(new Vector2(18f, 2.3f), door, radius: 2.6f);
            CreateExitPortal(new Vector2(26f, 2.3f), door, requireShadowNear: true);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -0.7f), new Vector2(-7.2f, -0.7f), new Vector2(-6.5f, -0.7f), true);
        }

        // =========================================================================
        // WORLD 5: THE ABYSS OF THE LAST FLAME (Levels 41–50)
        // =========================================================================

        private DualRealityPlatform CreateDualRealityPlatform(Vector2 pos, DualRealityPlatform.RealityState mode = DualRealityPlatform.RealityState.AlternatingTimer, float interval = 3.5f, bool startLight = true)
        {
            GameObject go = new GameObject("DualRealityPlatform");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World5/dual_plat_light", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.45f);

            DualRealityPlatform dr = go.AddComponent<DualRealityPlatform>();
            dr.Configure(mode, interval, startLight);
            return dr;
        }

        private FlameAnchor CreateFlameAnchor(Vector2 pos, DualRealityPlatform[] platforms, float radius = 3.5f)
        {
            GameObject go = new GameObject("FlameAnchor");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World5/flame_anchor", 32f);
            sr.sortingOrder = 2;

            FlameAnchor fa = go.AddComponent<FlameAnchor>();
            fa.Configure(platforms, radius);
            return fa;
        }

        private ShadowEchoSwitch CreateEchoSwitch(Vector2 pos, AncientDoor door = null, AbyssalBridge bridge = null)
        {
            GameObject go = new GameObject("ShadowEchoSwitch");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World5/echo_switch_off", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);

            ShadowEchoSwitch es = go.AddComponent<ShadowEchoSwitch>();
            es.Configure(door, bridge);
            return es;
        }

        private AbyssalBridge CreateAbyssalBridge(Vector2 pos, bool startActive = false, float width = 3f)
        {
            GameObject go = new GameObject("AbyssalBridge");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World5/abyssal_bridge", 32f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 0.45f);

            AbyssalBridge ab = go.AddComponent<AbyssalBridge>();
            ab.Configure(startActive);
            return ab;
        }

        private FinalFlameAltar CreateFinalAltar(Vector2 pos)
        {
            GameObject go = new GameObject("FinalFlameAltar");
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetRegistry.GetSprite("World5/final_flame_altar", 32f);
            sr.sortingOrder = 2;

            FinalFlameAltar ffa = go.AddComponent<FinalFlameAltar>();
            return ffa;
        }

        // Level 41: Edge of Oblivion
        private void BuildLevel41_EdgeOfOblivion()
        {
            CreatePlatformRow(-8, -3, -2f, true);
            CreatePlatformRow(4, 12, -2f, true);
            CreateDualRealityPlatform(new Vector2(0.5f, -2f), DualRealityPlatform.RealityState.LightActive);

            // NPC: The Hollow Warden
            CreateNPC(
                name: "The Hollow Warden",
                sprite: "npc_warden",
                pos: new Vector2(-5.5f, -1.15f),
                sequence: new DialogueSequence
                {
                    dialogueId = "warden_dialogue",
                    unlockedLoreEntryId = "lore_warden_final_truth",
                    lines = new[]
                    {
                        new DialogueLine { speakerName = "The Hollow Warden", portraitSpriteName = "portrait_warden", text = "You have reached the boundary where existence shatters. Ahead lies the Abyss of the Last Flame." },
                        new DialogueLine { speakerName = "The Hollow Warden", portraitSpriteName = "portrait_warden", text = "At Level 43 lies the Abyssal Scepter. Fire and Void must merge to conquer the final altar." }
                    }
                }
            );

            // World 5 Enemy: Abyssal Echo
            CreateEnemy<AbyssalEcho>(new Vector2(8.5f, -1.2f));

            ShadowEchoSwitch sw41 = CreateEchoSwitch(new Vector2(8.0f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw41 }, requireAll: true);
            sw41.Configure(door);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 42: Where Light Breaks
        private void BuildLevel42_WhereLightBreaks()
        {
            CreatePlatformRow(-8, -4, -2f, true);
            CreateDualRealityPlatform(new Vector2(-1f, -2f), DualRealityPlatform.RealityState.AlternatingTimer, 3.5f, true);
            CreateDualRealityPlatform(new Vector2(3f, -2f), DualRealityPlatform.RealityState.AlternatingTimer, 3.5f, false);
            CreatePlatformRow(6, 12, -2f, true);

            ShadowEchoSwitch sw42 = CreateEchoSwitch(new Vector2(8.0f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw42 }, requireAll: true);
            sw42.Configure(door);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 43: Echoes Without Form
        private void BuildLevel43_EchoesWithoutForm()
        {
            // Tier 1: West Spawning Terrace & Weapon Altar (x = -8..-3, y = -2)
            CreatePlatformRow(-8, -3, -2f, true);

            // WEAPON UNLOCK 3: ABYSSAL SCEPTER placed cleanly on the entry tier
            CreateWeaponPickup(WeaponType.AbyssalScepter, new Vector2(-4.5f, -1.2f));

            // Shadow upper roost with ShadowEchoSwitch that materializes the spectral void bridge
            CreatePlatformRow(-6, -4, 0.6f, true);
            AbyssalBridge bridge = CreateAbyssalBridge(new Vector2(0f, -2f), startActive: false, width: 4.5f);
            CreateEchoSwitch(new Vector2(-5f, 1.4f), null, bridge);

            // Tier 2: Central stepping platform across the chasm (x = 3..5, y = -2)
            CreatePlatformRow(3, 5, -2f, true);

            // Step up to upper Sanctuary Ledge for Protagonist (x = 6..7, y = 0.0)
            CreatePlatformRow(6, 7, 0.0f, true);

            // Tier 3: East Sanctuary Platform (x = 8..13, y = 1.8)
            CreatePlatformRow(8, 13, 1.8f, true);

            // East Puzzle Mechanism: Shadow switch unseals the ancient abyssal arch
            ShadowEchoSwitch eastEchoSwitch = CreateEchoSwitch(new Vector2(9.5f, 2.6f));
            AncientDoor door = CreateAncientDoor(new Vector2(10.8f, 2.6f), new List<PuzzleNode> { eastEchoSwitch }, requireAll: true, latchOpen: true);
            eastEchoSwitch.Configure(door);
            CreateExitPortal(new Vector2(12.2f, 2.6f), door, requireShadowNear: false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 44: The Flame Anchors
        private void BuildLevel44_TheFlameAnchors()
        {
            CreatePlatformRow(-8, -4, -2f, true);
            DualRealityPlatform p1 = CreateDualRealityPlatform(new Vector2(0f, -2f), DualRealityPlatform.RealityState.AlternatingTimer, 2.5f, false);
            CreatePlatformRow(4, 12, -2f, true);

            CreateFlameAnchor(new Vector2(-5f, -1.2f), new[] { p1 }, radius: 3.5f);

            ShadowEchoSwitch sw44 = CreateEchoSwitch(new Vector2(8.0f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw44 }, requireAll: true);
            sw44.Configure(door);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 45: Bridges of Nothing
        private void BuildLevel45_BridgesOfNothing()
        {
            CreatePlatformRow(-8, -4, -2f, true);
            AbyssalBridge b1 = CreateAbyssalBridge(new Vector2(-1f, -2f), false, 3f);
            AbyssalBridge b2 = CreateAbyssalBridge(new Vector2(3f, -2f), false, 3f);
            CreatePlatformRow(6, 12, -2f, true);

            CreateEchoSwitch(new Vector2(-5f, 0.5f), null, b1);
            CreateEchoSwitch(new Vector2(8f, 0.5f), null, b2);

            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode>());
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 46: Fractured Reflections
        private void BuildLevel46_FracturedReflections()
        {
            CreatePlatformRow(-8, 12, -2f, true);
            DualRealityPlatform pLight = CreateDualRealityPlatform(new Vector2(-1f, 0.5f), DualRealityPlatform.RealityState.LightActive);
            DualRealityPlatform pShadow = CreateDualRealityPlatform(new Vector2(3f, 0.5f), DualRealityPlatform.RealityState.ShadowActive);
            _ = pLight; _ = pShadow;

            ShadowEchoSwitch sw46 = CreateEchoSwitch(new Vector2(8.0f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw46 }, requireAll: true);
            sw46.Configure(door);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 47: The Forgotten Divide
        private void BuildLevel47_TheForgottenDivide()
        {
            CreatePlatformRow(-8, 12, -2f, true);
            ShadowPassage passage = CreateShadowPassage(new Vector2(0f, -1.2f));

            ShadowEchoSwitch sw = CreateEchoSwitch(new Vector2(3f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw }, requireAll: true);
            sw.Configure(door);
            passage.ConfigureUnlockNodes(new List<PuzzleNode> { sw }, requireAll: true, latchOpen: true);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 48: A World Without Fire
        private void BuildLevel48_AWorldWithoutFire()
        {
            CreatePlatformRow(-8, -3, -2f, true);
            CreateDualRealityPlatform(new Vector2(1f, -2f), DualRealityPlatform.RealityState.AlternatingTimer, 3f, true);
            CreatePlatformRow(5, 12, -2f, true);

            ShadowEchoSwitch sw48 = CreateEchoSwitch(new Vector2(8.0f, -1.2f));
            AncientDoor door = CreateAncientDoor(new Vector2(9.5f, -1.2f), new List<PuzzleNode> { sw48 }, requireAll: true);
            sw48.Configure(door);
            CreateExitPortal(new Vector2(11.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 49: Before the Last Dawn
        private void BuildLevel49_BeforeTheLastDawn()
        {
            CreatePlatformRow(-8, -4, -2f, true);
            DualRealityPlatform p1 = CreateDualRealityPlatform(new Vector2(-1f, -2f), DualRealityPlatform.RealityState.AlternatingTimer, 3.5f, true);
            AbyssalBridge b1 = CreateAbyssalBridge(new Vector2(3.5f, -2f), false, 3.5f);
            CreatePlatformRow(7, 14, -2f, true);

            CreateEchoSwitch(new Vector2(-5f, 0.5f), null, b1);
            CreateFlameAnchor(new Vector2(-6f, -1.2f), new[] { p1 });

            AncientDoor door = CreateAncientDoor(new Vector2(11.5f, -1.2f), new List<PuzzleNode>());
            CreateExitPortal(new Vector2(13.5f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        // Level 50: The Last Flame (Grand Finale of the Prototype)
        private void BuildLevel50_TheLastFlame()
        {
            CreatePlatformRow(-8, 28, -2f, true);

            // Phase 1: Dual reality bridge
            CreateDualRealityPlatform(new Vector2(2f, -0.5f), DualRealityPlatform.RealityState.LightActive);
            CreateDualRealityPlatform(new Vector2(6f, -0.5f), DualRealityPlatform.RealityState.ShadowActive);

            // Phase 2: Abyssal altar chamber
            CreateFinalAltar(new Vector2(18f, -0.8f));

            AncientDoor door = CreateAncientDoor(new Vector2(23f, -1.2f), new List<PuzzleNode>());
            CreateExitPortal(new Vector2(26f, -1.2f), door, false);

            SpawnCharactersAndTorch(new Vector2(-6.5f, -1.2f), new Vector2(-7.2f, -1.2f), new Vector2(-6.5f, -1.2f), true);
        }

        public WorldEnvironmentProfile GetActiveEnvironmentProfile()
        {
            int worldIdx = (ActiveWorld != null) ? ActiveWorld.WorldIndex : 0;
            WorldDefinition wDef = WorldCatalog.GetWorld(worldIdx);
            return wDef?.EnvironmentProfile ?? new WorldEnvironmentProfile();
        }

        private void CreateSporeParticles(Color color)
        {
            GameObject sporeObj = new GameObject("ForestSporeParticles");
            sporeObj.transform.SetParent(_levelRoot.transform, false);
            sporeObj.transform.position = new Vector3(6f, 1f, 0f);

            ParticleSystem ps = sporeObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.duration = 5f;
            main.startLifetime = 7f;
            main.startSpeed = 0.22f;
            main.startSize = 0.08f;
            main.startColor = color;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 3.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 10f, 1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.08f, 0.18f);

            ParticleSystemRenderer psr = sporeObj.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                if (shader != null) psr.material = new Material(shader);
                psr.sortingOrder = 8;
            }
        }

        private NPCInteraction CreateNPC(string name, string sprite, Vector2 pos, DialogueSequence sequence)
        {
            GameObject go = new GameObject("NPC_" + name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;

            NPCInteraction npc = go.AddComponent<NPCInteraction>();
            npc.Initialize(name, sprite, sequence);
            return npc;
        }

        private T CreateEnemy<T>(Vector2 pos) where T : EnemyBase
        {
            GameObject go = new GameObject("Enemy_" + typeof(T).Name);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<SpriteRenderer>();

            T enemy = go.AddComponent<T>();
            return enemy;
        }

        private WeaponPickup CreateWeaponPickup(WeaponType type, Vector2 pos)
        {
            GameObject go = new GameObject("WeaponPickup_" + type);
            go.transform.SetParent(_levelRoot.transform, false);
            go.transform.position = pos;

            WeaponPickup pickup = go.AddComponent<WeaponPickup>();
            pickup.Initialize(type);
            return pickup;
        }
    }
}
