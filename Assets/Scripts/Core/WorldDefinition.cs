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
        public bool IsImplemented => isImplemented;
        public bool IsUnlocked => unlockedByDefault || (worldNumber == 2 && WorldProgressSave.IsWorldCompleted("world_1"));
        public IReadOnlyList<LevelEntry> Levels => levels;
        public int LevelCount => levels != null ? levels.Count : 0;
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

            _worlds.Add(w1);
            _worlds.Add(w2);
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
                new LevelEntry(0, "w1_lvl1", "I. AWAKENING", "A/D: Move   SPACE: Jump   E: Pick up / Place Torch   TAB: Control Shadow"),
                new LevelEntry(1, "w1_lvl2", "II. THE SEPARATION", "Stand on the lower stone plate, switch to Shadow [TAB], and Double Jump [SPACE x2 in mid-air] to the upper ledge."),
                new LevelEntry(2, "w1_lvl3", "III. FOLLOW THE LIGHT", "Darkness webs dissolve in Torchlight. Guide your Shadow across the illuminated path."),
                new LevelEntry(3, "w1_lvl4", "IV. THE FORGOTTEN FLAME", "Ancient solar crystals awaken when bathed in the Torch's golden flame."),
                new LevelEntry(4, "w1_lvl5", "V. DARK PASSAGE", "Push the carved stone block onto the plate while your Shadow unlocks the inner sanctum."),
                new LevelEntry(5, "w1_lvl6", "VI. TWO SIDES", "Two parallel tiers: the Flame walks above, the Shadow walks below."),
                new LevelEntry(6, "w1_lvl7", "VII. THE LAST LIGHT", "Transfer the Torch between pedestals to clear Darkness Webs and open the final seal."),
                new LevelEntry(7, "w1_lvl8", "VIII. MY SHADOW QUIT", "Unite the three ancient seals and reconnect Flame and Shadow at the Sanctuary Arch."),
                new LevelEntry(8, "w1_lvl9", "IX. ECHOES IN THE DARK", "Mount the Torch to awaken the upper path, Double Jump with your Shadow across the high vault, and weight the lower seal."),
                new LevelEntry(9, "w1_lvl10", "X. THE FIRST DAWN", "Awaken all five ceremonial seals of light and shadow to open the Great Gate toward the First Dawn.")
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
                new LevelEntry(0, "w2_lvl1", "XI. WHISPERS IN THE ROOTS", "Moving platforms carry the Flame across deep abysses. Switch between characters [TAB] to guide each other."),
                new LevelEntry(1, "w2_lvl2", "XII. THE RISING PATH", "The vertical elevator ascends through the ancient tree canopy. Step aboard and ride upward."),
                new LevelEntry(2, "w2_lvl3", "XIII. ROOTS OF LIGHT", "Dormant roots bloom into solid bridges under the Torch's golden warmth. Light the way for your Shadow."),
                new LevelEntry(3, "w2_lvl4", "XIV. FADING FOOTSTEPS", "Bioluminescent fungal platforms crumble shortly after being touched. Move swiftly with double jumps."),
                new LevelEntry(4, "w2_lvl5", "XV. THE HOLLOW CANOPY", "Dense shadow vines bar the Flame's passage, but your Shadow slips through freely to activate the lower mechanism."),
                new LevelEntry(5, "w2_lvl6", "XVI. A BRIDGE OF EMBERS", "Mount the Torch on the ancient pedestal to awaken the shimmering Light Bridge across the chasm."),
                new LevelEntry(6, "w2_lvl7", "XVII. THE SILENT THORNS", "Beware the toxic forest briars. Ride moving platforms carefully above the thorn beds."),
                new LevelEntry(7, "w2_lvl8", "XVIII. THE FRACTURED GROVE", "Synchronize temporary platforms, light roots, and shadow passages to unlock the triad seals."),
                new LevelEntry(8, "w2_lvl9", "XIX. TWO PATHS, ONE FLAME", "Two separate chambers: the Flame activates the light roots while the Shadow coordinates the dual switches."),
                new LevelEntry(9, "w2_lvl10", "XX. HEART OF THE INVERTED FOREST", "Ascend to the core of the Great Tree. Overcome all forest mechanisms and reunite Flame and Shadow at the Ancient Arch.")
            };
        }

        public static WorldDefinition CreateDefaultWorld2()
        {
            WorldDefinition w2 = ScriptableObject.CreateInstance<WorldDefinition>();
            w2.name = "World2_TheInvertedForest";
            EnsureWorld2HasTenLevels(w2);
            return w2;
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
            // Check if World 2 has any progress recorded
            if (HasSavedProgress("world_2"))
            {
                return "world_2";
            }

            // Otherwise default to World 1
            return "world_1";
        }

        public static bool HasAnySavedProgress()
        {
            return HasSavedProgress("world_2") || HasSavedProgress("world_1");
        }

        public static void GetGlobalContinuePoint(out string worldId, out int worldIndex, out int levelIndex, out int displayLevelNumber)
        {
            worldId = GetLastPlayedWorldId();
            if (worldId == "world_2")
            {
                worldIndex = 1;
                WorldDefinition w2 = WorldCatalog.GetWorld(1);
                int total = w2 != null && w2.LevelCount > 0 ? w2.LevelCount : 10;
                levelIndex = GetContinueLevelIndex("world_2", total);
                displayLevelNumber = 10 + levelIndex + 1; // 11 to 20
            }
            else
            {
                worldId = "world_1";
                worldIndex = 0;
                WorldDefinition w1 = WorldCatalog.GetWorld(0);
                int total = w1 != null && w1.LevelCount > 0 ? w1.LevelCount : 10;
                levelIndex = GetContinueLevelIndex("world_1", total);
                displayLevelNumber = levelIndex + 1; // 1 to 10
            }
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
