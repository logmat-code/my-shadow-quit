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

        public string WorldId => worldId;
        public int WorldIndex => Mathf.Max(0, worldNumber - 1);
        public string WorldTitle => worldName;
        public string WorldSubtitle => worldSubtitle;
        public string CompletionQuote => completionQuote;
        public bool IsImplemented => isImplemented;
        public bool IsUnlocked => unlockedByDefault;
        public IReadOnlyList<LevelEntry> Levels => levels;
        public int LevelCount => levels != null ? levels.Count : 0;

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

            WorldDefinition w2 = Resources.Load<WorldDefinition>("Worlds/World2_ComingSoon");
            if (w2 == null)
            {
                w2 = CreateDefaultWorld2();
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

        public static WorldDefinition CreateDefaultWorld2()
        {
            WorldDefinition w2 = ScriptableObject.CreateInstance<WorldDefinition>();
            w2.name = "World2_ComingSoon";
            w2.worldId = "world_2";
            w2.worldNumber = 2;
            w2.worldName = "COMING SOON";
            w2.worldSubtitle = "WORLD 2";
            w2.initialSceneName = "";
            w2.artDirection = "Reserved for future expansion beyond the Ceremonial Gate.";
            w2.musicTrackPath = "Music/dark_ambient_theme";
            w2.availableMechanics = Array.Empty<string>();
            w2.isImplemented = false;
            w2.unlockedByDefault = false;
            w2.levels = new List<LevelEntry>();
            return w2;
        }
    }

    /// <summary>
    /// Local progress persistence for worlds and levels using PlayerPrefs.
    /// </summary>
    public static class WorldProgressSave
    {
        private const string KeyPrefixLastLevel = "MSQ_LastLevel_";
        private const string KeyPrefixHighestLevel = "MSQ_HighestLevel_";
        private const string KeyPrefixWorldComplete = "MSQ_WorldComplete_";
        private const string KeyHasAnySave = "MSQ_HasSavedProgress_";

        public static bool HasSavedProgress(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            return PlayerPrefs.GetInt(KeyHasAnySave + worldId, 0) == 1 ||
                   PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0) > 0 ||
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
            return Mathf.Clamp(saved, 0, maxIdx);
        }

        public static int GetHighestUnlockedLevelIndex(string worldId, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            int highest = PlayerPrefs.GetInt(KeyPrefixHighestLevel + worldId, 0);
            return Mathf.Clamp(highest, 0, maxIdx);
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
            if (clamped > 0)
            {
                PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);
            }
            PlayerPrefs.Save();
        }

        public static void MarkWorldCompleted(string worldId, int totalLevels)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int maxIdx = Mathf.Max(0, totalLevels - 1);
            PlayerPrefs.SetInt(KeyPrefixWorldComplete + worldId, 1);
            PlayerPrefs.SetInt(KeyPrefixHighestLevel + worldId, maxIdx);
            PlayerPrefs.SetInt(KeyPrefixLastLevel + worldId, maxIdx);
            PlayerPrefs.SetInt(KeyHasAnySave + worldId, 1);
            PlayerPrefs.Save();
        }

        public static bool IsWorldCompleted(string worldId)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            return PlayerPrefs.GetInt(KeyPrefixWorldComplete + worldId, 0) == 1;
        }
    }
}
