using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Cosmetics
{
    public enum SkinType
    {
        Player,
        Shadow,
        Torch
    }

    public enum SkinRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public class SkinDefinition
    {
        public string Id;
        public string DisplayName;
        public string LoreDescription;
        public SkinType Type;
        public SkinRarity Rarity;
        public int Price;
        public bool IsDefault;
        public string SpritePathPrefix;
        public Color ThemeColor;
        public Color HaloColor;
        public Color EmberColor;

        public SkinDefinition(
            string id,
            string displayName,
            string loreDescription,
            SkinType type,
            SkinRarity rarity,
            int price,
            bool isDefault,
            string spritePathPrefix,
            Color themeColor,
            Color? haloColor = null,
            Color? emberColor = null)
        {
            Id = id;
            DisplayName = displayName;
            LoreDescription = loreDescription;
            Type = type;
            Rarity = rarity;
            Price = price;
            IsDefault = isDefault;
            SpritePathPrefix = spritePathPrefix;
            ThemeColor = themeColor;
            HaloColor = haloColor ?? themeColor;
            EmberColor = emberColor ?? themeColor;
        }
    }

    /// <summary>
    /// Centralized registry for all 15 cosmetic skins across Player, Shadow, and Torch.
    /// All skins are strictly cosmetic and never modify gameplay physics, light mechanics, or puzzle colliders.
    /// </summary>
    public static class SkinCatalog
    {
        private static readonly List<SkinDefinition> AllSkins = new List<SkinDefinition>();
        private static readonly Dictionary<string, SkinDefinition> SkinsById = new Dictionary<string, SkinDefinition>();

        static SkinCatalog()
        {
            InitializeCatalog();
        }

        private static void InitializeCatalog()
        {
            AllSkins.Clear();
            SkinsById.Clear();

            // ================================================================
            // 1. MAIN PLAYER SKINS (5 Skins)
            // ================================================================
            RegisterSkin(new SkinDefinition(
                "player_original",
                "ORIGINAL",
                "The cloaked wanderer of the forgotten sanctuary, guided by the last warm flame.",
                SkinType.Player,
                SkinRarity.Common,
                0,
                true,
                "Skins/Player/player_original",
                new Color(0.82f, 0.65f, 0.35f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "player_ashen_knight",
                "ASHEN KNIGHT",
                "Charred plate armor forged in the embers of ancient sanctuary wars.",
                SkinType.Player,
                SkinRarity.Rare,
                500,
                false,
                "Skins/Player/player_ashen_knight",
                new Color(0.68f, 0.74f, 0.85f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "player_forgotten_guardian",
                "FORGOTTEN GUARDIAN",
                "Carved mossy granite inscribed with glowing teal protective warding runes.",
                SkinType.Player,
                SkinRarity.Epic,
                900,
                false,
                "Skins/Player/player_forgotten_guardian",
                new Color(0.28f, 0.90f, 0.82f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "player_ember_wanderer",
                "EMBER WANDERER",
                "A nomadic desert traveler with molten gold seams and incandescent embers.",
                SkinType.Player,
                SkinRarity.Rare,
                500,
                false,
                "Skins/Player/player_ember_wanderer",
                new Color(1f, 0.65f, 0.20f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "player_void_traveler",
                "VOID TRAVELER",
                "Cosmic twilight garments woven from stardust and astral dimensional rifts.",
                SkinType.Player,
                SkinRarity.Legendary,
                1500,
                false,
                "Skins/Player/player_void_traveler",
                new Color(0.85f, 0.50f, 1f, 1f)
            ));

            // ================================================================
            // 2. SHADOW SKINS (5 Skins)
            // ================================================================
            RegisterSkin(new SkinDefinition(
                "shadow_original",
                "ORIGINAL",
                "The faithful companion born from light, bearing quiet lavender eyes.",
                SkinType.Shadow,
                SkinRarity.Common,
                0,
                true,
                "Skins/Shadow/shadow_original",
                new Color(0.72f, 0.45f, 0.95f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "shadow_violet_phantom",
                "VIOLET PHANTOM",
                "A manifestation of pure astral aether with pulsing brilliant magenta plasma.",
                SkinType.Shadow,
                SkinRarity.Rare,
                500,
                false,
                "Skins/Shadow/shadow_violet_phantom",
                new Color(0.85f, 0.30f, 1f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "shadow_crimson_wraith",
                "CRIMSON WRAITH",
                "A subterranean wrathful spirit forged in deep volcanic magma chambers.",
                SkinType.Shadow,
                SkinRarity.Epic,
                900,
                false,
                "Skins/Shadow/shadow_crimson_wraith",
                new Color(1f, 0.25f, 0.32f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "shadow_spectral_echo",
                "SPECTRAL ECHO",
                "A chilled silhouette from the sub-zero catacombs, trailing glacial azure frost.",
                SkinType.Shadow,
                SkinRarity.Rare,
                500,
                false,
                "Skins/Shadow/shadow_spectral_echo",
                new Color(0.35f, 0.82f, 1f, 1f)
            ));

            RegisterSkin(new SkinDefinition(
                "shadow_abyssal_shade",
                "ABYSSAL SHADE",
                "An event-horizon entity of true void where light bends to an eternal singularity.",
                SkinType.Shadow,
                SkinRarity.Legendary,
                1500,
                false,
                "Skins/Shadow/shadow_abyssal_shade",
                new Color(0.92f, 0.90f, 1f, 1f)
            ));

            // ================================================================
            // 3. TORCH SKINS (5 Skins)
            // ================================================================
            RegisterSkin(new SkinDefinition(
                "torch_original_flame",
                "ORIGINAL FLAME",
                "The sacred primordial flame that reawakens ancient mechanisms.",
                SkinType.Torch,
                SkinRarity.Common,
                0,
                true,
                "Skins/Torch/torch_original_flame",
                new Color(1f, 0.70f, 0.25f, 1f),
                new Color(1f, 0.80f, 0.42f, 0.52f),
                new Color(1f, 0.68f, 0.20f, 0.90f)
            ));

            RegisterSkin(new SkinDefinition(
                "torch_blue_soul",
                "BLUE SOUL",
                "Ethereal ghostfire burning with tranquil departed spirits in cold cobalt silver.",
                SkinType.Torch,
                SkinRarity.Common,
                200,
                false,
                "Skins/Torch/torch_blue_soul",
                new Color(0.25f, 0.75f, 1f, 1f),
                new Color(0.25f, 0.70f, 1f, 0.55f),
                new Color(0.20f, 0.80f, 1f, 0.90f)
            ));

            RegisterSkin(new SkinDefinition(
                "torch_violet_fire",
                "VIOLET FIRE",
                "Alchemical flame fueled by crushed void crystals in a dark obsidian holder.",
                SkinType.Torch,
                SkinRarity.Rare,
                500,
                false,
                "Skins/Torch/torch_violet_fire",
                new Color(0.85f, 0.40f, 1f, 1f),
                new Color(0.78f, 0.38f, 1f, 0.55f),
                new Color(0.82f, 0.35f, 0.95f, 0.90f)
            ));

            RegisterSkin(new SkinDefinition(
                "torch_emerald_ember",
                "EMERALD EMBER",
                "Eternal jade flame burning mineral salts in an antique polished brass brazier.",
                SkinType.Torch,
                SkinRarity.Epic,
                900,
                false,
                "Skins/Torch/torch_emerald_ember",
                new Color(0.25f, 0.92f, 0.52f, 1f),
                new Color(0.22f, 0.88f, 0.48f, 0.55f),
                new Color(0.25f, 0.90f, 0.50f, 0.90f)
            ));

            RegisterSkin(new SkinDefinition(
                "torch_crimson_inferno",
                "CRIMSON INFERNO",
                "Volcanic dragon-fire blazing with ruby intensity from a scorched dragon-iron cup.",
                SkinType.Torch,
                SkinRarity.Legendary,
                1500,
                false,
                "Skins/Torch/torch_crimson_inferno",
                new Color(1f, 0.28f, 0.20f, 1f),
                new Color(1f, 0.25f, 0.20f, 0.58f),
                new Color(1f, 0.30f, 0.18f, 0.90f)
            ));
        }

        private static void RegisterSkin(SkinDefinition def)
        {
            AllSkins.Add(def);
            SkinsById[def.Id] = def;
        }

        public static IReadOnlyList<SkinDefinition> GetAllSkins() => AllSkins;

        public static List<SkinDefinition> GetSkins(SkinType type)
        {
            List<SkinDefinition> list = new List<SkinDefinition>();
            for (int i = 0; i < AllSkins.Count; i++)
            {
                if (AllSkins[i].Type == type)
                {
                    list.Add(AllSkins[i]);
                }
            }
            return list;
        }

        public static List<SkinDefinition> GetSkinsByType(SkinType type) => GetSkins(type);

        public static SkinDefinition GetSkin(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            SkinsById.TryGetValue(id, out SkinDefinition skin);
            return skin;
        }

        public static string GetDefaultSkinId(SkinType type)
        {
            switch (type)
            {
                case SkinType.Player: return "player_original";
                case SkinType.Shadow: return "shadow_original";
                case SkinType.Torch: return "torch_original_flame";
                default: return string.Empty;
            }
        }
    }
}
