using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Narrative
{
    [Serializable]
    public class LoreEntry
    {
        public string id;
        public string title;
        public string worldOrigin;
        [TextArea(3, 8)]
        public string content;
    }

    public static class JournalService
    {
        private const string PrefLorePrefix = "MSQ_LoreUnlocked_";

        private static readonly Dictionary<string, LoreEntry> s_entries = new Dictionary<string, LoreEntry>
        {
            {
                "lore_archivist_intro",
                new LoreEntry
                {
                    id = "lore_archivist_intro",
                    title = "The Shattered Flame",
                    worldOrigin = "World 1 — The Ashen Depths",
                    content = "The First Flame was never meant to burn forever. When the cataclysm struck, your shadow severed itself to carry the weight you could not bear. Reclaim the embers, but beware: darkness is not an enemy to be slaughtered, but a mirror to be understood."
                }
            },
            {
                "lore_rootkeeper_secrets",
                new LoreEntry
                {
                    id = "lore_rootkeeper_secrets",
                    title = "Inverted Canopy",
                    worldOrigin = "World 2 — The Inverted Forest",
                    content = "The trees here drink light and weep shadows. The Rootkeeper speaks of an ancient era when shadow and vessel walked as one. He warns that deep in the clockwork towers, time has frozen around an eternal wound."
                }
            },
            {
                "lore_clockmaker_emberblade",
                new LoreEntry
                {
                    id = "lore_clockmaker_emberblade",
                    title = "The Emberblade Forge",
                    worldOrigin = "World 3 — The Sunken Clocktower",
                    content = "Forged in the heart of the Grand Chronometer, the Emberblade resonates with the spark inside your torch. It severs the temporal binds of the Sentinels and lights the way through gears frozen in obsidian time."
                }
            },
            {
                "lore_oracle_cathedral",
                new LoreEntry
                {
                    id = "lore_oracle_cathedral",
                    title = "Submerged Prayers",
                    worldOrigin = "World 4 — The Drowned Cathedral",
                    content = "Beneath silent black tides lie the bells that once kept the abyss asleep. The Drowned Oracle whispers that the shadow did not leave in hatred, but in desperation to protect the last spark from your own despair."
                }
            },
            {
                "lore_warden_final_truth",
                new LoreEntry
                {
                    id = "lore_warden_final_truth",
                    title = "The Abyssal Covenant",
                    worldOrigin = "World 5 — The Abyss of the Last Flame",
                    content = "At the edge of oblivion, the Hollow Warden guards the final crucible. The flame can never be rekindled by light alone. Only when the bearer and the shadow strike the altar together will the cycle break and eternal dawn return."
                }
            }
        };

        public static bool IsUnlocked(string entryId)
        {
            return PlayerPrefs.GetInt(PrefLorePrefix + entryId, 0) == 1;
        }

        public static void UnlockEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return;
            if (!IsUnlocked(entryId))
            {
                PlayerPrefs.SetInt(PrefLorePrefix + entryId, 1);
                PlayerPrefs.Save();
            }
        }

        public static LoreEntry GetEntry(string entryId)
        {
            if (s_entries.TryGetValue(entryId, out var entry))
            {
                return entry;
            }
            return null;
        }

        public static List<LoreEntry> GetAllUnlockedEntries()
        {
            List<LoreEntry> list = new List<LoreEntry>();
            foreach (var kvp in s_entries)
            {
                if (IsUnlocked(kvp.Key))
                {
                    list.Add(kvp.Value);
                }
            }
            return list;
        }

        public static int TotalAvailableCount => s_entries.Count;
    }
}
