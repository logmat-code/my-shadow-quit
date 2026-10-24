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

        public static WeaponType EquippedWeapon
        {
            get => GetEquippedWeapon();
            set => SetEquippedWeapon(value);
        }

        public static WeaponDefinition GetDefinition(WeaponType type) => Get(type);

        public static void CycleNextUnlockedWeapon()
        {
            WeaponType current = EquippedWeapon;
            WeaponType[] all = { WeaponType.None, WeaponType.Emberblade, WeaponType.HollowCrossbow, WeaponType.AbyssalScepter };
            int currIndex = Array.IndexOf(all, current);

            for (int i = 1; i <= all.Length; i++)
            {
                int nextIdx = (currIndex + i) % all.Length;
                WeaponType candidate = all[nextIdx];
                if (candidate == WeaponType.None || IsWeaponUnlocked(candidate))
                {
                    SetEquippedWeapon(candidate);
                    if (Core.AudioManager.Instance != null)
                    {
                        Core.AudioManager.Instance.PlaySFX("switch_activate", 0.7f, 1.2f);
                    }
                    break;
                }
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
            if (type == WeaponType.None) return true;
            int mask = PlayerPrefs.GetInt(PrefKeyUnlockedWeapons, 0);
            return (mask & (1 << (int)type)) != 0;
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
            int val = PlayerPrefs.GetInt(PrefKeyEquippedWeapon, (int)WeaponType.None);
            WeaponType type = (WeaponType)val;
            if (IsWeaponUnlocked(type)) return type;
            return WeaponType.None;
        }

        public static void SetEquippedWeapon(WeaponType type)
        {
            PlayerPrefs.SetInt(PrefKeyEquippedWeapon, (int)type);
            PlayerPrefs.Save();
        }
    }
}
