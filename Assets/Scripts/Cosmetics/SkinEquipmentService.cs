using System;
using UnityEngine;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Cosmetics
{
    /// <summary>
    /// Manages player, shadow, and torch cosmetic skin equipment and persistence.
    /// Ensures equipped skins are automatically preserved across scenes, level transitions,
    /// restarts, and game relaunch sessions.
    /// </summary>
    public class SkinEquipmentService : MonoBehaviour
    {
        public static SkinEquipmentService Instance { get; private set; }

        public const string KeyEquippedPlayer = "MSQ_Equipped_Player";
        public const string KeyEquippedShadow = "MSQ_Equipped_Shadow";
        public const string KeyEquippedTorch = "MSQ_Equipped_Torch";

        public event Action<SkinDefinition> OnSkinEquipped;

        private string _equippedPlayerId = "player_original";
        private string _equippedShadowId = "shadow_original";
        private string _equippedTorchId = "torch_original_flame";

        public string EquippedPlayerSkinId => _equippedPlayerId;
        public string EquippedShadowSkinId => _equippedShadowId;
        public string EquippedTorchSkinId => _equippedTorchId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            LoadEquippedSkins();
        }

        public void LoadEquippedSkins()
        {
            _equippedPlayerId = PlayerPrefs.GetString(KeyEquippedPlayer, "player_original");
            if (SkinCatalog.GetSkin(_equippedPlayerId) == null) _equippedPlayerId = "player_original";

            _equippedShadowId = PlayerPrefs.GetString(KeyEquippedShadow, "shadow_original");
            if (SkinCatalog.GetSkin(_equippedShadowId) == null) _equippedShadowId = "shadow_original";

            _equippedTorchId = PlayerPrefs.GetString(KeyEquippedTorch, "torch_original_flame");
            if (SkinCatalog.GetSkin(_equippedTorchId) == null) _equippedTorchId = "torch_original_flame";
        }

        public bool IsEquipped(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return false;
            return _equippedPlayerId == skinId || _equippedShadowId == skinId || _equippedTorchId == skinId;
        }

        public string GetEquippedSkinId(SkinType type)
        {
            switch (type)
            {
                case SkinType.Player: return _equippedPlayerId;
                case SkinType.Shadow: return _equippedShadowId;
                case SkinType.Torch: return _equippedTorchId;
                default: return string.Empty;
            }
        }

        public SkinDefinition GetEquippedSkin(SkinType type)
        {
            return SkinCatalog.GetSkin(GetEquippedSkinId(type));
        }

        public bool EquipSkin(string skinId)
        {
            SkinDefinition def = SkinCatalog.GetSkin(skinId);
            if (def == null)
            {
                Debug.LogWarning($"[SkinEquipmentService] Cannot equip non-existent skin: {skinId}");
                return false;
            }

            switch (def.Type)
            {
                case SkinType.Player:
                    _equippedPlayerId = def.Id;
                    PlayerPrefs.SetString(KeyEquippedPlayer, _equippedPlayerId);
                    break;
                case SkinType.Shadow:
                    _equippedShadowId = def.Id;
                    PlayerPrefs.SetString(KeyEquippedShadow, _equippedShadowId);
                    break;
                case SkinType.Torch:
                    _equippedTorchId = def.Id;
                    PlayerPrefs.SetString(KeyEquippedTorch, _equippedTorchId);
                    break;
            }

            PlayerPrefs.Save();
            ApplyEquippedSkinsToActiveEntities();
            OnSkinEquipped?.Invoke(def);
            return true;
        }

        /// <summary>
        /// Applies current equipped cosmetic skins to any active player, shadow, or torch entities
        /// in the scene without resetting rigidbodies, positions, or puzzle mechanics.
        /// </summary>
        public void ApplyEquippedSkinsToActiveEntities()
        {
            PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ApplySkin(_equippedPlayerId);
            }

            ShadowController shadow = UnityEngine.Object.FindAnyObjectByType<ShadowController>();
            if (shadow != null)
            {
                shadow.ApplySkin(_equippedShadowId);
            }

            TorchController torch = UnityEngine.Object.FindAnyObjectByType<TorchController>();
            if (torch != null)
            {
                torch.ApplySkin(_equippedTorchId);
            }
        }
    }
}
