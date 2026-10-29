using System;
using UnityEngine;

namespace MyShadowQuit.Combat
{
    public enum WeaponType
    {
        None,
        Emberblade,
        HollowCrossbow,
        AbyssalScepter
    }

    [Serializable]
    public class WeaponDefinition
    {
        public WeaponType type;
        public string id;
        public string displayName;
        public string description;
        public int damage;
        public float cooldown;
        public float range;
        public string unlockLevel; // e.g. "LEVEL 23"
        public string spritePath;
        public WeaponType weaponType => type;
        public float attackCooldown => cooldown;
        public float knockbackForce => 5.0f;
        public string spriteName => spritePath != null && spritePath.Contains("/") ? spritePath.Substring(spritePath.LastIndexOf('/') + 1) : spritePath;
        public string sfxName => (type == WeaponType.Emberblade) ? "attack_slash" : ((type == WeaponType.HollowCrossbow) ? "attack_shoot" : "attack_spell");
        public string sfxAttack { get; set; }
    }

    public static class WeaponCatalog
    {
        public const string PrefKeyUnlockedWeapons = "MSQ_UnlockedWeaponsMask";
        public const string PrefKeyEquippedWeapon = "MSQ_EquippedWeaponType";

        public static readonly WeaponDefinition Emberblade = new WeaponDefinition
        {
            type = WeaponType.Emberblade,
            id = "weapon_emberblade",
            displayName = "EMBERBLADE",
            description = "A short blade of charred steel etched with living amber runes. Fast melee slash.",
            damage = 1,
            cooldown = 0.45f,
            range = 1.35f,
            unlockLevel = "LEVEL 23",
            spritePath = "Combat/weapon_emberblade",
            sfxAttack = "slash_amber"
        };

        public static readonly WeaponDefinition HollowCrossbow = new WeaponDefinition
        {
            type = WeaponType.HollowCrossbow,
            id = "weapon_crossbow",
            displayName = "HOLLOW CROSSBOW",
            description = "Ancient spring-loaded clockwork crossbow firing turquoise energy bolts.",
            damage = 1,
            cooldown = 0.85f,
            range = 8.0f,
            unlockLevel = "LEVEL 34",
            spritePath = "Combat/weapon_crossbow",
            sfxAttack = "crossbow_shot"
        };

        public static readonly WeaponDefinition AbyssalScepter = new WeaponDefinition
        {
            type = WeaponType.AbyssalScepter,
            id = "weapon_scepter",
            displayName = "ABYSSAL SCEPTER",
            description = "Carved from dark stone, channeling pure spectral violet orbs across the void.",
            damage = 2,
            cooldown = 1.25f,
            range = 10.0f,
            unlockLevel = "LEVEL 43",
            spritePath = "Combat/weapon_scepter",
            sfxAttack = "scepter_pulse"
        };

        public static event Action<WeaponType> OnWeaponEquippedChanged;

        public static WeaponType EquippedWeapon
        {
            get => GetEquippedWeapon();
            set => SetEquippedWeapon(value);
        }

        public static WeaponDefinition GetDefinition(WeaponType type) => Get(type);

        public static int GetUnlockedWeaponsCount()
        {
            int count = 0;
            if (IsWeaponUnlocked(WeaponType.Emberblade)) count++;
            if (IsWeaponUnlocked(WeaponType.HollowCrossbow)) count++;
            if (IsWeaponUnlocked(WeaponType.AbyssalScepter)) count++;
            return count;
        }

        public static void CycleNextUnlockedWeapon()
        {
            int count = GetUnlockedWeaponsCount();
            if (count == 0) return;

            // Only cycle between weapons that are actually unlocked (and None if desired, or unlocked weapons)
            WeaponType current = EquippedWeapon;
            WeaponType[] realWeapons = { WeaponType.Emberblade, WeaponType.HollowCrossbow, WeaponType.AbyssalScepter };
            
            // Collect list of unlocked weapons
            var unlockedList = new System.Collections.Generic.List<WeaponType>();
            foreach (var w in realWeapons)
            {
                if (IsWeaponUnlocked(w)) unlockedList.Add(w);
            }

            if (unlockedList.Count == 0) return;
            if (unlockedList.Count == 1)
            {
                // If only 1 weapon unlocked and already equipped, no false change
                if (current == unlockedList[0]) return;
                SetEquippedWeapon(unlockedList[0]);
                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.2f);
                }
                return;
            }

            int currIdx = unlockedList.IndexOf(current);
            int nextIdx = (currIdx + 1) % unlockedList.Count;
            SetEquippedWeapon(unlockedList[nextIdx]);

            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.2f);
            }
        }

        public static WeaponDefinition Get(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Emberblade: return Emberblade;
                case WeaponType.HollowCrossbow: return HollowCrossbow;
                case WeaponType.AbyssalScepter: return AbyssalScepter;
                default: return null;
            }
        }

        public static bool IsWeaponUnlocked(WeaponType type)
        {
            if (type == WeaponType.None) return false;
            ReconcileWeaponProgression();
            int mask = PlayerPrefs.GetInt(PrefKeyUnlockedWeapons, 0);
            return (mask & (1 << (int)type)) != 0;
        }

        public static void ReconcileWeaponProgression()
        {
            // Authoritative progression check: ensure player never loses earned weapons on continue / restart
            int mask = PlayerPrefs.GetInt(PrefKeyUnlockedWeapons, 0);

            // Level 23+ (index 22+) unlocks Emberblade
            // Level 34+ (index 33+) unlocks Hollow Crossbow
            // Level 43+ (index 42+) unlocks Abyssal Scepter
            int currentLevel = (Core.GameManager.Instance != null) ? Core.GameManager.Instance.CurrentLevelIndex : 0;
            
            // Check world progression or active level
            bool hasWorld3 = Core.WorldProgressSave.IsWorldCompleted("world_2") || currentLevel >= 22;
            bool hasWorld4 = Core.WorldProgressSave.IsWorldCompleted("world_3") || currentLevel >= 33;
            bool hasWorld5 = Core.WorldProgressSave.IsWorldCompleted("world_4") || currentLevel >= 42;

            int initialMask = mask;
            if (hasWorld3) mask |= (1 << (int)WeaponType.Emberblade);
            if (hasWorld4) mask |= (1 << (int)WeaponType.HollowCrossbow);
            if (hasWorld5) mask |= (1 << (int)WeaponType.AbyssalScepter);

            if (mask != initialMask)
            {
                PlayerPrefs.SetInt(PrefKeyUnlockedWeapons, mask);
                PlayerPrefs.Save();
            }
        }

        public static void UnlockWeapon(WeaponType type)
        {
            if (type == WeaponType.None) return;
            int mask = PlayerPrefs.GetInt(PrefKeyUnlockedWeapons, 0);
            mask |= (1 << (int)type);
            PlayerPrefs.SetInt(PrefKeyUnlockedWeapons, mask);
            PlayerPrefs.Save();
        }

        public static WeaponType GetEquippedWeapon()
        {
            ReconcileWeaponProgression();
            int val = PlayerPrefs.GetInt(PrefKeyEquippedWeapon, (int)WeaponType.None);
            WeaponType type = (WeaponType)val;
            if (IsWeaponUnlocked(type)) return type;

            // If equipped was None or invalidated, but weapons are unlocked, equip highest available
            if (IsWeaponUnlocked(WeaponType.AbyssalScepter)) return WeaponType.AbyssalScepter;
            if (IsWeaponUnlocked(WeaponType.HollowCrossbow)) return WeaponType.HollowCrossbow;
            if (IsWeaponUnlocked(WeaponType.Emberblade)) return WeaponType.Emberblade;

            return WeaponType.None;
        }

        public static void SetEquippedWeapon(WeaponType type)
        {
            PlayerPrefs.SetInt(PrefKeyEquippedWeapon, (int)type);
            PlayerPrefs.Save();
            OnWeaponEquippedChanged?.Invoke(type);
        }

        public static void ResetToInitialWeapons()
        {
            PlayerPrefs.DeleteKey(PrefKeyUnlockedWeapons);
            PlayerPrefs.DeleteKey(PrefKeyEquippedWeapon);
            PlayerPrefs.Save();
            OnWeaponEquippedChanged?.Invoke(WeaponType.None);
        }
    }
}
