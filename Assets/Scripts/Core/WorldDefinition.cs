using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Core
{
    [Serializable]
    public class LevelEntry
    {
        public int levelIndex;
        public string levelId;
        public string title;
        public string hint;

        public LevelEntry(int index, string id, string levelTitle, string levelHint)
        {
            levelIndex = index;
            levelId = id;
            title = levelTitle;
            hint = levelHint;
        }
    }

    [Serializable]
    public class WorldEnvironmentProfile
    {
        public string FarParallaxSpritePath = "Environment/parallax_far";
        public string MidParallaxSpritePath = "Environment/parallax_mid";
        public string NearParallaxSpritePath = "Environment/parallax_near";
        public string FloorSpritePath = "Environment/stone_floor";
        public string WallSpritePath = "Environment/stone_wall";
        public string PlatformLeftSpritePath = "Environment/platform_edge_left";
        public string PlatformRightSpritePath = "Environment/platform_edge_right";
        public Color AmbientColor = new Color(0.08f, 0.08f, 0.14f, 1f);
        public Color SporeParticleColor = new Color(0.32f, 0.92f, 0.68f, 0.60f);
        public bool HasSporeParticles = false;
        public string MusicTrackPath = "Music/dark_ambient_theme";
    }

    /// <summary>
    /// ScriptableObject defining a World in MY SHADOW QUIT: THE LAST FLAME.
    /// Encapsulates world identity, level list, initial scene, art direction, music track,
    /// available mechanics, and unlock/implementation state so future worlds (World 2+)
    /// can be added without modifying GameManager logic.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldDefinition", menuName = "MyShadowQuit/World Definition")]
    public class WorldDefinition : ScriptableObject
    {
        public string worldId = "world_1";
        public int worldNumber = 1;
        public string worldName = "THE RUINS OF THE LAST FLAME";
        public string worldSubtitle = "WORLD 1";
        public string initialSceneName = "Level1_Awakening";
        public string artDirection = "Dark atmospheric pixel art, deep indigo/slate ruins, violet shadow energy, warm golden torchlight.";
        public string musicTrackPath = "Music/dark_ambient_theme";
        public string[] availableMechanics =
        {
            "Torch Illumination & Pedestals",
            "Conscious Shadow Separation & Reconnection [TAB]",
            "Shadow Double Jump & 50% Reduced Gravity",
            "Shadow Passages & Obelisk Switches",
            "Solar Light Crystals & Darkness Webs",
            "Movable Carved Blocks & Pressure Plates"
        };
        public string completionQuote = "\"Beyond the darkness, another light awaits.\"";
        public bool isImplemented = true;
        public bool unlockedByDefault = true;
        public List<LevelEntry> levels = new List<LevelEntry>();
        public WorldEnvironmentProfile environmentProfile = new WorldEnvironmentProfile();

        public string WorldId => worldId;
        public int WorldNumber => worldNumber;
        public int WorldIndex => Mathf.Max(0, worldNumber - 1);
        public string WorldTitle => worldName;
        public string WorldSubtitle => worldSubtitle;
        public string CompletionQuote => completionQuote;
        public bool IsUnlocked => unlockedByDefault ||
            (worldNumber > 1 && WorldProgressSave.IsWorldCompleted("world_" + (worldNumber - 1)));
        public bool IsImplemented => isImplemented;
        public int LevelCount => levels != null ? levels.Count : 0;
        public List<LevelEntry> Levels => levels;
        public WorldEnvironmentProfile EnvironmentProfile => environmentProfile ?? (environmentProfile = new WorldEnvironmentProfile());

        public void Configure(
            int worldIdx,
            string id,
            string title,
            string subtitle,
            string quote,
            bool unlocked,
            bool implemented,
            List<LevelEntry> levelList)
        {
            worldNumber = worldIdx + 1;
            worldId = id;
            worldName = title;
            worldSubtitle = subtitle;
            completionQuote = quote;
            unlockedByDefault = unlocked;
            isImplemented = implemented;
            levels = levelList ?? new List<LevelEntry>();
        }

        public LevelEntry GetLevel(int index)
        {
            if (levels == null || levels.Count == 0) return null;
            int clamped = Mathf.Clamp(index, 0, levels.Count - 1);
            return levels[clamped];
        }
    }

    /// <summary>
    /// Provides access to all configured Worlds (World 1: 10 playable levels; World 2: reserved/coming soon)
    /// and manages local progress persistence via PlayerPrefs.
    /// </summary>
    public static class WorldCatalog
    {
        private static List<WorldDefinition> _worlds;

        public static IReadOnlyList<WorldDefinition> Worlds
        {
            get
            {
                if (_worlds == null || _worlds.Count == 0 || _worlds[0] == null)
                {
                    InitializeWorlds();
                }
                return _worlds;
            }
        }

        public static WorldDefinition GetWorld(int worldIndex)
        {
            var list = Worlds;
            if (list == null || list.Count == 0) return null;
            int clamped = Mathf.Clamp(worldIndex, 0, list.Count - 1);
            return list[clamped];
        }

        public static void InitializeWorlds()
        {
            _worlds = new List<WorldDefinition>();

            WorldDefinition w1 = Resources.Load<WorldDefinition>("Worlds/World1_TheRuinsOfTheLastFlame");
            if (w1 == null || w1.LevelCount < 10)
            {
                w1 = CreateDefaultWorld1();
            }
            else
            {
                EnsureWorld1HasTenLevels(w1);
            }

            WorldDefinition w2 = Resources.Load<WorldDefinition>("Worlds/World2_TheInvertedForest");
            if (w2 == null)
            {
                w2 = Resources.Load<WorldDefinition>("Worlds/World2_ComingSoon");
            }

            if (w2 == null || w2.LevelCount < 10)
            {
                w2 = CreateDefaultWorld2();
            }
            else
            {
                EnsureWorld2HasTenLevels(w2);
            }

            WorldDefinition w3 = Resources.Load<WorldDefinition>("Worlds/World3_TheHollowClocktower");
            if (w3 == null || w3.LevelCount < 10)
            {
                w3 = CreateDefaultWorld3();
            }
            else
            {
                EnsureWorld3HasTenLevels(w3);
            }

            // Worlds 4 and 5 are code-defined (no .asset), so they cannot be shadowed by stale assets.
            WorldDefinition w4 = CreateDefaultWorld4();
            WorldDefinition w5 = CreateDefaultWorld5();

            _worlds.Add(w1);
            _worlds.Add(w2);
            _worlds.Add(w3);
            _worlds.Add(w4);
            _worlds.Add(w5);
        }

        public static void EnsureWorld4HasTenLevels(WorldDefinition w4)
        {
            w4.worldId = "world_4";
            w4.worldNumber = 4;
            w4.worldName = "THE DROWNED CATHEDRAL";
            w4.worldSubtitle = "WORLD 4";
            w4.initialSceneName = "Level1_Awakening";
            w4.artDirection = "Abyssal blue, marine black, grey-green, desaturated turquoise, wet stone, aged silver, torch amber. Submerged gothic cathedral.";
            w4.musicTrackPath = "Music/cathedral_ambient_theme";
            w4.completionQuote = "\"Beneath the black tide, the last choir still remembers the light.\"";
            w4.environmentProfile = new WorldEnvironmentProfile
            {
                FarParallaxSpritePath = "World4/cathedral_parallax_far",
                MidParallaxSpritePath = "World4/cathedral_parallax_mid",
                NearParallaxSpritePath = "World4/cathedral_parallax_near",
                FloorSpritePath = "World4/cathedral_floor",
                WallSpritePath = "World4/cathedral_wall",
                PlatformLeftSpritePath = "World4/cathedral_platform_left",
                PlatformRightSpritePath = "World4/cathedral_platform_right",
                AmbientColor = new Color(0.010f, 0.030f, 0.045f, 1f),
                SporeParticleColor = new Color(0.50f, 0.85f, 0.85f, 0.30f),
                HasSporeParticles = true,
                MusicTrackPath = "Music/cathedral_ambient_theme"
            };
            w4.availableMechanics = new[]
            {
                "Water Level Switches",
                "Buoyant Platforms",
                "Submerged Passages",
                "Light Refraction Crystals",
                "Cooperative Pressure Plates",
                "Cathedral Hazards"
            };
            w4.isImplemented = true;
            w4.unlockedByDefault = false;
            w4.levels = new List<LevelEntry>
            {
                new LevelEntry(0, "w4_lvl1", "LEVEL 31: BENEATH THE BLACK TIDE", "The cathedral lies drowned. Cross the flooded threshold and find the exit."),
                new LevelEntry(1, "w4_lvl2", "LEVEL 32: THE FLOODED NAVE", "Pull the tide lever to lower the water and reach the far side."),
                new LevelEntry(2, "w4_lvl3", "LEVEL 33: DROWNED STEPS", "Buoyant platforms rise and fall with the tide. Ride them across."),
                new LevelEntry(3, "w4_lvl4", "LEVEL 34: GLASS BENEATH THE SEA", "Aim the Torch at the crystal; its refracted beam wakes the sealed gate."),
                new LevelEntry(4, "w4_lvl5", "LEVEL 35: THE SILENT CHOIR", "Two plates, two voices. Player and Shadow must each hold one."),
                new LevelEntry(5, "w4_lvl6", "LEVEL 36: RISING WATERS", "Raise the tide to float the platform, then lower it to open the passage."),
                new LevelEntry(6, "w4_lvl7", "LEVEL 37: THE BROKEN BAPTISTERY", "Shadow slips through the submerged grate while the Flame works the lever."),
                new LevelEntry(7, "w4_lvl8", "LEVEL 38: BENEATH THE BELLS", "Mind the sunken bell blades and the cutting currents."),
                new LevelEntry(8, "w4_lvl9", "LEVEL 39: THE SUNKEN RELIQUARY", "Combine tide, crystal and plates to unseal the reliquary."),
                new LevelEntry(9, "w4_lvl10", "LEVEL 40: THE LAST CATHEDRAL LIGHT", "Player, Shadow and Torch must work together to kindle the final light.")
            };
        }

        public static WorldDefinition CreateDefaultWorld4()
        {
            WorldDefinition w4 = ScriptableObject.CreateInstance<WorldDefinition>();
            w4.name = "World4_TheDrownedCathedral";
            EnsureWorld4HasTenLevels(w4);
            return w4;
        }

        public static void EnsureWorld5HasTenLevels(WorldDefinition w5)
        {
            w5.worldId = "world_5";
            w5.worldNumber = 5;
            w5.worldName = "THE ABYSS OF THE LAST FLAME";
            w5.worldSubtitle = "WORLD 5";
            w5.initialSceneName = "Level1_Awakening";
            w5.artDirection = "Deep black, indigo, spectral violet, ash grey, old gold, incandescent amber. Fractured light/shadow abyss.";
            w5.musicTrackPath = "Music/abyss_ambient_theme";
            w5.completionQuote = "\"The last flame was never a fire. It was the two of us.\"";
            w5.environmentProfile = new WorldEnvironmentProfile
            {
                FarParallaxSpritePath = "World5/abyss_parallax_far",
                MidParallaxSpritePath = "World5/abyss_parallax_mid",
                NearParallaxSpritePath = "World5/abyss_parallax_near",
                FloorSpritePath = "World5/abyss_floor",
                WallSpritePath = "World5/abyss_wall",
                PlatformLeftSpritePath = "World5/abyss_platform_left",
                PlatformRightSpritePath = "World5/abyss_platform_right",
                AmbientColor = new Color(0.012f, 0.008f, 0.030f, 1f),
                SporeParticleColor = new Color(0.95f, 0.75f, 0.35f, 0.30f),
                HasSporeParticles = true,
                MusicTrackPath = "Music/abyss_ambient_theme"
            };
            w5.availableMechanics = new[]
            {
                "Dual-Reality Platforms",
                "Flame Anchors",
                "Shadow Echo Switches",
                "Abyssal Bridges",
                "Light and Shadow Gates",
                "Final Convergence"
            };
            w5.isImplemented = true;
            w5.unlockedByDefault = false;
            w5.levels = new List<LevelEntry>
            {
                new LevelEntry(0, "w5_lvl1", "LEVEL 41: EDGE OF OBLIVION", "The ground ends here. Step across the fractured stones."),
                new LevelEntry(1, "w5_lvl2", "LEVEL 42: WHERE LIGHT BREAKS", "Some stones exist only in light, others only in shadow. Watch the pulse."),
                new LevelEntry(2, "w5_lvl3", "LEVEL 43: ECHOES WITHOUT FORM", "Only your Shadow can wake the echo switch."),
                new LevelEntry(3, "w5_lvl4", "LEVEL 44: THE FLAME ANCHORS", "Set the Torch on the anchor to keep the mechanism alive."),
                new LevelEntry(4, "w5_lvl5", "LEVEL 45: BRIDGES OF NOTHING", "Torch and Shadow together weave a bridge. Cross before it fades."),
                new LevelEntry(5, "w5_lvl6", "LEVEL 46: FRACTURED REFLECTIONS", "Light and shadow routes must meet at the far gate."),
                new LevelEntry(6, "w5_lvl7", "LEVEL 47: THE FORGOTTEN DIVIDE", "A gate of light and a gate of shadow. Open both."),
                new LevelEntry(7, "w5_lvl8", "LEVEL 48: A WORLD WITHOUT FIRE", "Only the Torch's glow reveals the path. Carry it carefully."),
                new LevelEntry(8, "w5_lvl9", "LEVEL 49: BEFORE THE LAST DAWN", "Every lesson of the descent, once more."),
                new LevelEntry(9, "w5_lvl10", "LEVEL 50: THE LAST FLAME", "Reach the heart of the abyss and kindle the Last Flame.")
            };
        }

        public static WorldDefinition CreateDefaultWorld5()
        {
            WorldDefinition w5 = ScriptableObject.CreateInstance<WorldDefinition>();
            w5.name = "World5_TheAbyssOfTheLastFlame";
            EnsureWorld5HasTenLevels(w5);
            return w5;
        }

        public static WorldDefinition CreateDefaultWorld1()
        {
            WorldDefinition w1 = ScriptableObject.CreateInstance<WorldDefinition>();
            w1.name = "World1_TheRuinsOfTheLastFlame";
            EnsureWorld1HasTenLevels(w1);
            return w1;
        }

        public static void EnsureWorld1HasTenLevels(WorldDefinition w1)
        {
            w1.worldId = "world_1";
            w1.worldNumber = 1;
            w1.worldName = "THE RUINS OF THE LAST FLAME";
            w1.worldSubtitle = "WORLD 1";
            w1.initialSceneName = "Level1_Awakening";
            w1.artDirection = "Dark atmospheric pixel art, deep indigo/slate stone ruins, luminous violet shadow energy, warm golden torchlight.";
            w1.musicTrackPath = "Music/dark_ambient_theme";
            w1.availableMechanics = new[]
            {
                "Torch Illumination & Pedestals",
                "Conscious Shadow Separation & Reconnection [TAB]",
                "Shadow Double Jump & 50% Reduced Gravity",
                "Shadow Passages & Obelisk Switches",
                "Solar Light Crystals & Darkness Webs",
                "Movable Carved Blocks & Pressure Plates"
            };
            w1.isImplemented = true;
            w1.unlockedByDefault = true;
            w1.environmentProfile = new WorldEnvironmentProfile
            {
                FarParallaxSpritePath = "Environment/parallax_far",
                MidParallaxSpritePath = "Environment/parallax_mid",
                NearParallaxSpritePath = "Environment/parallax_near",
                FloorSpritePath = "Environment/stone_floor",
                WallSpritePath = "Environment/stone_wall",
                PlatformLeftSpritePath = "Environment/platform_edge_left",
                PlatformRightSpritePath = "Environment/platform_edge_right",
                AmbientColor = new Color(0.08f, 0.08f, 0.14f, 1f),
                SporeParticleColor = new Color(0.78f, 0.65f, 1f, 0.35f),
                HasSporeParticles = false,
                MusicTrackPath = "Music/dark_ambient_theme"
            };
            w1.levels = new List<LevelEntry>
            {
                new LevelEntry(0, "w1_lvl1", "LEVEL 1: AWAKENING", "A/D: Move   SPACE: Jump   E: Pick up / Place Torch   TAB: Control Shadow"),
                new LevelEntry(1, "w1_lvl2", "LEVEL 2: THE SEPARATION", "Stand on the lower stone plate, switch to Shadow [TAB], and Double Jump [SPACE x2 in mid-air] to the upper ledge."),
                new LevelEntry(2, "w1_lvl3", "LEVEL 3: FOLLOW THE LIGHT", "Darkness webs dissolve in Torchlight. Guide your Shadow across the illuminated path."),
                new LevelEntry(3, "w1_lvl4", "LEVEL 4: THE FORGOTTEN FLAME", "Ancient solar crystals awaken when bathed in the Torch's golden flame."),
                new LevelEntry(4, "w1_lvl5", "LEVEL 5: DARK PASSAGE", "Push the carved stone block onto the plate while your Shadow unlocks the inner sanctum."),
                new LevelEntry(5, "w1_lvl6", "LEVEL 6: TWO SIDES", "Two parallel tiers: the Flame walks above, the Shadow walks below."),
                new LevelEntry(6, "w1_lvl7", "LEVEL 7: THE LAST LIGHT", "Transfer the Torch between pedestals to clear Darkness Webs and open the final seal."),
                new LevelEntry(7, "w1_lvl8", "LEVEL 8: MY SHADOW QUIT", "Unite the three ancient seals and reconnect Flame and Shadow at the Sanctuary Arch."),
                new LevelEntry(8, "w1_lvl9", "LEVEL 9: ECHOES IN THE DARK", "Mount the Torch to awaken the upper path, Double Jump with your Shadow across the high vault, and weight the lower seal."),
                new LevelEntry(9, "w1_lvl10", "LEVEL 10: THE FIRST DAWN", "Awaken all five ceremonial seals of light and shadow to open the Great Gate toward the First Dawn.")
            };
        }

        public static void EnsureWorld2HasTenLevels(WorldDefinition w2)
        {
            w2.worldId = "world_2";
            w2.worldNumber = 2;
            w2.worldName = "THE INVERTED FOREST";
            w2.worldSubtitle = "WORLD 2";
            w2.initialSceneName = "Level1_Awakening";
            w2.artDirection = "Dark petroleum green, jade and deep turquoise bioluminescent forest, ancient twisted roots, mist, glowing flora.";
            w2.musicTrackPath = "Music/forest_ambient_theme";
            w2.completionQuote = "\"The roots remember the first spark. Light and Shadow breathe as one.\"";
            w2.environmentProfile = new WorldEnvironmentProfile
            {
                FarParallaxSpritePath = "World2/forest_parallax_far",
                MidParallaxSpritePath = "World2/forest_parallax_mid",
                NearParallaxSpritePath = "World2/forest_parallax_near",
                FloorSpritePath = "World2/forest_floor",
                WallSpritePath = "World2/forest_wall",
                PlatformLeftSpritePath = "World2/forest_platform_left",
                PlatformRightSpritePath = "World2/forest_platform_right",
                AmbientColor = new Color(0.015f, 0.028f, 0.035f, 1f),
                SporeParticleColor = new Color(0.18f, 0.70f, 0.55f, 0.28f),
                HasSporeParticles = true,
                MusicTrackPath = "Music/forest_ambient_theme"
            };
            w2.availableMechanics = new[]
            {
                "Kinematic Moving Platforms (Horizontal & Elevators)",
                "Bioluminescent Temporary Fungal Platforms",
                "Illuminated Light Root Bridges",
                "Permeable Shadow Vine Passages",
                "Shimmering Light Bridges",
                "Cooperative Dual Switches",
                "Environmental Forest Briars & Thorns"
            };
            w2.isImplemented = true;
            w2.unlockedByDefault = false;
            w2.levels = new List<LevelEntry>
            {
                new LevelEntry(0, "w2_lvl1", "LEVEL 11: WHISPERS IN THE ROOTS", "Moving platforms carry the Flame across deep abysses. Switch between characters [TAB] to guide each other."),
                new LevelEntry(1, "w2_lvl2", "LEVEL 12: THE RISING PATH", "The vertical elevator ascends through the ancient tree canopy. Step aboard and ride upward."),
                new LevelEntry(2, "w2_lvl3", "LEVEL 13: ROOTS OF LIGHT", "Dormant roots bloom into solid bridges under the Torch's golden warmth. Light the way for your Shadow."),
                new LevelEntry(3, "w2_lvl4", "LEVEL 14: FADING FOOTSTEPS", "Bioluminescent fungal platforms crumble shortly after being touched. Move swiftly with double jumps."),
                new LevelEntry(4, "w2_lvl5", "LEVEL 15: THE HOLLOW CANOPY", "Dense shadow vines bar the Flame's passage, but your Shadow slips through freely to activate the lower mechanism."),
                new LevelEntry(5, "w2_lvl6", "LEVEL 16: A BRIDGE OF EMBERS", "Mount the Torch on the ancient pedestal to awaken the shimmering Light Bridge across the chasm."),
                new LevelEntry(6, "w2_lvl7", "LEVEL 17: THE SILENT THORNS", "Beware the toxic forest briars. Ride moving platforms carefully above the thorn beds."),
                new LevelEntry(7, "w2_lvl8", "LEVEL 18: THE FRACTURED GROVE", "Synchronize temporary platforms, light roots, and shadow passages to unlock the triad seals."),
                new LevelEntry(8, "w2_lvl9", "LEVEL 19: TWO PATHS, ONE FLAME", "Two separate chambers: the Flame activates the light roots while the Shadow coordinates the dual switches."),
                new LevelEntry(9, "w2_lvl10", "LEVEL 20: HEART OF THE INVERTED FOREST", "Ascend to the core of the Great Tree. Overcome all forest mechanisms and reunite Flame and Shadow at the Ancient Arch.")
            };
        }

        public static WorldDefinition CreateDefaultWorld2()
        {
            WorldDefinition w2 = ScriptableObject.CreateInstance<WorldDefinition>();
            w2.name = "World2_TheInvertedForest";
            EnsureWorld2HasTenLevels(w2);
            return w2;
        }

        public static void EnsureWorld3HasTenLevels(WorldDefinition w3)
        {
            w3.worldId = "world_3";
            w3.worldNumber = 3;
            w3.worldName = "THE HOLLOW CLOCKTOWER";
            w3.worldSubtitle = "WORLD 3";
            w3.initialSceneName = "Level1_Awakening";
            w3.artDirection = "Dark steel blue, charcoal grey, aged bronze, gothic machinery, rotating gears, and pendulum clockworks.";
            w3.musicTrackPath = "Music/clocktower_ambient_theme";
            w3.completionQuote = "\"Time stood still, but our flames kept moving. The clocktower yields its secret.\"";
            w3.environmentProfile = new WorldEnvironmentProfile
            {
                FarParallaxSpritePath = "World3/clocktower_parallax_far",
                MidParallaxSpritePath = "World3/clocktower_parallax_mid",
                NearParallaxSpritePath = "World3/clocktower_parallax_near",
                FloorSpritePath = "World3/clocktower_floor",
                WallSpritePath = "World3/clocktower_wall",
                PlatformLeftSpritePath = "World3/clocktower_platform_left",
                PlatformRightSpritePath = "World3/clocktower_platform_right",
                AmbientColor = new Color(0.016f, 0.020f, 0.035f, 1f),
                SporeParticleColor = new Color(0.92f, 0.78f, 0.38f, 0.32f),
                HasSporeParticles = true,
                MusicTrackPath = "Music/clocktower_ambient_theme"
            };
            w3.availableMechanics = new[]
            {
                "Rotating Gear Platforms",
                "Swinging Pendulum Obstacles",
                "Timed Clockwork Switches",
                "Torch-Powered Clockwork Lenses",
                "Spectral Shadow Grate Passages"
            };
            w3.isImplemented = true;
            w3.unlockedByDefault = false;
            w3.levels = new List<LevelEntry>
            {
                new LevelEntry(0, "w3_lvl1", "LEVEL 21: THE SILENT HANDS", "Step aboard the slow-turning gear platform to traverse the clocktower vestibule."),
                new LevelEntry(1, "w3_lvl2", "LEVEL 22: THE FIRST TICK", "The clockwork switch winds the ancient spring, opening the timed gate for a brief interval."),
                new LevelEntry(2, "w3_lvl3", "LEVEL 23: BROKEN GEARS", "Gear platforms rotate at differing cadences. Use your Shadow's double jump [SPACE x2] to bridge the high gap."),
                new LevelEntry(3, "w3_lvl4", "LEVEL 24: THE SWINGING HALL", "Observe the pendulum's rhythmic swing. Wait for the apex before dashing through the hazard zone."),
                new LevelEntry(4, "w3_lvl5", "LEVEL 25: THE FROZEN MINUTE", "Hit the clockwork switch to temporarily lock the swinging pendulums in place."),
                new LevelEntry(5, "w3_lvl6", "LEVEL 26: THE LENS OF EMBERS", "Focus the Torch's golden flame upon the ancient lens to activate the dormant clockwork bridge."),
                new LevelEntry(6, "w3_lvl7", "LEVEL 27: BETWEEN TWO TICKS", "Your Shadow slips through the spectral iron grate while the Flame operates the mechanical lever."),
                new LevelEntry(7, "w3_lvl8", "LEVEL 28: THE FALLEN BELLS", "Ascend the vertical belfry. Coordinate moving gear platforms and evade the resonant pendulum weights."),
                new LevelEntry(8, "w3_lvl9", "LEVEL 29: THE LAST HOUR", "Synchronize gear elevators, clockwork levers, and the solar lens to unseal the high sanctuary."),
                new LevelEntry(9, "w3_lvl10", "LEVEL 30: HEART OF THE CLOCKTOWER", "The Heart of the Clocktower. Unwind all five ancient celestial clockworks to open the Grand Chrono Portal.")
            };
        }

        public static WorldDefinition CreateDefaultWorld3()
        {
            WorldDefinition w3 = ScriptableObject.CreateInstance<WorldDefinition>();
            w3.name = "World3_TheHollowClocktower";
            EnsureWorld3HasTenLevels(w3);
            return w3;
        }
    }

    /// <summary>
    /// Local progress persistence for worlds and levels using PlayerPrefs.
    /// Maintains backward compatibility with existing save keys while tracking
    /// unlocked levels, completed levels, last played level, and world completion.
    /// </summary>
    public static class WorldProgressSave
    {
        private const string KeyPrefixLastLevel = "MSQ_LastLevel_";
        private const string KeyPrefixHighestLevel = "MSQ_HighestLevel_";
        private const string KeyPrefixCompletedMask = "MSQ_CompletedMask_";
        private const string KeyPrefixWorldComplete = "MSQ_WorldComplete_";
        private const string KeyHasAnySave = "MSQ_HasSavedProgress_";
        public const string KeyLastPlayedWorld = "MSQ_LastPlayedWorld";

        public static void SetLastPlayedWorld(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) return;
            PlayerPrefs.SetString(KeyLastPlayedWorld, worldId);
            PlayerPrefs.Save();
        }

        public static string GetLastPlayedWorldId()
        {
            string saved = PlayerPrefs.GetString(KeyLastPlayedWorld, string.Empty);
            if (!string.IsNullOrEmpty(saved))
            {
                return saved;
            }

            // Fallback for previous saves that don't have KeyLastPlayedWorld:
            // pick the highest-numbered world that has any recorded progress.
            for (int n = WorldCount; n >= 2; n--)
            {
                if (HasSavedProgress("world_" + n)) return "world_" + n;
            }

            // Otherwise default to World 1
            return "world_1";
        }

        /// <summary>Total number of worlds in the prototype (stable, independent of Resources loading).</summary>
        public const int WorldCount = 5;
        public const int LevelsPerWorld = 10;

        public static bool HasAnySavedProgress()
        {
            for (int n = 1; n <= WorldCount; n++)
            {
                if (HasSavedProgress("world_" + n)) return true;
            }
            return false;
        }

        public static void GetGlobalContinuePoint(out string worldId, out int worldIndex, out int levelIndex, out int displayLevelNumber)
        {
            worldId = GetLastPlayedWorldId();
            worldIndex = 0;
            if (worldId != null && worldId.StartsWith("world_") && int.TryParse(worldId.Substring(6), out int n) && n >= 1 && n <= WorldCount)
            {
                worldIndex = n - 1;
            }
            else
            {
                worldId = "world_1";
            }

            WorldDefinition wd = WorldCatalog.GetWorld(worldIndex);
            int total = wd != null && wd.LevelCount > 0 ? wd.LevelCount : LevelsPerWorld;
            levelIndex = GetContinueLevelIndex(worldId, total);
            displayLevelNumber = worldIndex * LevelsPerWorld + levelIndex + 1;
        }

        public static bool HasSavedProgress(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            return PlayerPrefs.GetInt(KeyHasAnySave + worldId, 0) == 1 ||
                   PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0) > 0 ||
                   PlayerPrefs.GetInt(KeyPrefixCompletedMask + worldId, 0) > 0 ||
                   IsWorldCompleted(worldId);
        }

        public static int GetLastPlayedLevel(string worldId, int totalLevels)
        {
            return GetContinueLevelIndex(worldId, totalLevels);
        }

        public static int GetHighestUnlockedLevel(string worldId, int totalLevels)
        {
            return GetHighestUnlockedLevelIndex(worldId, totalLevels);
        }

        public static int GetContinueLevelIndex(string worldId, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            int saved = PlayerPrefs.GetInt(KeyPrefixLastLevel + worldId, 0);
            int highest = GetHighestUnlockedLevelIndex(worldId, totalLevels);
            return Mathf.Clamp(Mathf.Min(saved, highest), 0, maxIdx);
        }

        public static int GetHighestUnlockedLevelIndex(string worldId, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            if (IsWorldCompleted(worldId)) return maxIdx;
            int highest = PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0);
            return Mathf.Clamp(highest, 0, maxIdx);
        }

        public static bool IsLevelUnlocked(string worldId, int levelIndex, int totalLevels)
        {
            if (levelIndex <= 0) return true;
            if (levelIndex >= totalLevels) return false;
            return levelIndex <= GetHighestUnlockedLevelIndex(worldId, totalLevels);
        }

        public static bool IsLevelCompleted(string worldId, int levelIndex, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            if (levelIndex < 0 || levelIndex >= totalLevels) return false;
            if (IsWorldCompleted(worldId)) return true;

            int mask = PlayerPrefs.GetInt(KeyPrefixCompletedMask + worldId, 0);
            if ((mask & (1 << levelIndex)) != 0) return true;

            // Backward compatibility: if a higher level is already unlocked, earlier levels were completed
            int highest = PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0);
            return levelIndex < highest;
        }

        public static int GetCompletedLevelsCount(string worldId, int totalLevels)
        {
            int count = 0;
            for (int i = 0; i < totalLevels; i++)
            {
                if (IsLevelCompleted(worldId, i, totalLevels))
                {
                    count++;
                }
            }
            return count;
        }

        public static void SaveLevelProgress(string worldId, int levelIndex, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            int clamped = Mathf.Clamp(levelIndex, 0, maxIdx);

            PlayerPrefs.SetInt(KeyPrefixLastLevel + worldId, clamped);
            int prevHighest = PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0);
            if (clamped > prevHighest)
            {
                PlayerPrefs.SetInt(KeyPrefixHighestLevel + worldId, clamped);
            }
            PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);
            SetLastPlayedWorld(worldId);
            PlayerPrefs.Save();
        }

        public static void MarkLevelCompleted(string worldId, int completedLevelIndex, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            int clamped = Mathf.Clamp(completedLevelIndex, 0, maxIdx);

            int mask = PlayerPrefs.GetInt(KeyPrefixCompletedMask + worldId, 0);
            mask |= (1 << clamped);
            PlayerPrefs.SetInt(KeyPrefixCompletedMask + worldId, mask);

            int nextIdx = Mathf.Min(clamped + 1, maxIdx);
            int prevHighest = PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0);
            if (nextIdx > prevHighest)
            {
                PlayerPrefs.SetInt(KeyPrefixHighestLevel + worldId, nextIdx);
            }

            PlayerPrefs.SetInt(KeyPrefixLastLevel + worldId, nextIdx);
            PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);

            if (clamped >= maxIdx)
            {
                PlayerPrefs.SetInt(KeyPrefixWorldComplete + worldId, 1);
            }

            SetLastPlayedWorld(worldId);
            PlayerPrefs.Save();
        }

        public static void MarkWorldCompleted(string worldId, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            int fullMask = (1 << Mathf.Min(totalLevels, 30)) - 1;
            PlayerPrefs.SetInt(KeyPrefixCompletedMask + worldId, fullMask);
            PlayerPrefs.SetInt(KeyPrefixWorldComplete + worldId, 1);
            PlayerPrefs.SetInt(KeyPrefixHighestLevel + worldId, maxIdx);
            PlayerPrefs.SetInt(KeyPrefixLastLevel + worldId, maxIdx);
            PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);
            SetLastPlayedWorld(worldId);
            PlayerPrefs.Save();
        }

        public static void ResetWorldProgress(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            PlayerPrefs.SetInt(KeyPrefixLastLevel + worldId, 0);
            PlayerPrefs.SetInt(KeyPrefixHighestLevel + worldId, 0);
            PlayerPrefs.SetInt(KeyPrefixCompletedMask + worldId, 0);
            PlayerPrefs.SetInt(KeyPrefixWorldComplete + worldId, 0);
            PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);
            PlayerPrefs.Save();
        }

        public static void ClearWorldProgress(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            PlayerPrefs.DeleteKey(KeyPrefixLastLevel + worldId);
            PlayerPrefs.DeleteKey(KeyPrefixHighestLevel + worldId);
            PlayerPrefs.DeleteKey(KeyPrefixCompletedMask + worldId);
            PlayerPrefs.DeleteKey(KeyPrefixWorldComplete + worldId);
            PlayerPrefs.DeleteKey(KeyHasAnySave + worldId);
            PlayerPrefs.Save();
        }

        public static bool IsWorldCompleted(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            return PlayerPrefs.GetInt(KeyPrefixWorldComplete + worldId, 0) == 1;
        }
    }
}
