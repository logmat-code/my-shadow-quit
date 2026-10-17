using System;
using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Cosmetics;

namespace MyShadowQuit.Economy
{
    /// <summary>
    /// Persistent player inventory tracking unlocked cosmetic skins.
    /// Guarantees default skins are always unlocked and owned without duplicate purchases.
    /// </summary>
    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }

        public const string KeyOwnedSkins = "MSQ_Owned_Skins";

        public event Action<string> OnSkinUnlocked;

        private readonly HashSet<string> _ownedSkinIds = new HashSet<string>();

        public IReadOnlyCollection<string> OwnedSkinIds => _ownedSkinIds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            LoadInventory();
        }

        public void LoadInventory()
        {
            _ownedSkinIds.Clear();

            // Default skins are always unlocked and owned
            _ownedSkinIds.Add("player_original");
            _ownedSkinIds.Add("shadow_original");
            _ownedSkinIds.Add("torch_original_flame");

            string raw = PlayerPrefs.GetString(KeyOwnedSkins, string.Empty);
            if (!string.IsNullOrEmpty(raw))
            {
                string[] parts = raw.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    string id = parts[i].Trim();
                    if (!string.IsNullOrEmpty(id))
                    {
                        _ownedSkinIds.Add(id);
                    }
                }
            }
        }

        public bool IsSkinOwned(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return false;
            SkinDefinition def = SkinCatalog.GetSkin(skinId);
            if (def != null && def.IsDefault) return true;
            return _ownedSkinIds.Contains(skinId);
        }

        public bool UnlockSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) return false;
            if (_ownedSkinIds.Contains(skinId))
            {
                return true; // Already owned
            }

            _ownedSkinIds.Add(skinId);
            SaveInventory();
            OnSkinUnlocked?.Invoke(skinId);
            return true;
        }

        private void SaveInventory()
        {
            string raw = string.Join(",", _ownedSkinIds);
            PlayerPrefs.SetString(KeyOwnedSkins, raw);
            PlayerPrefs.Save();
        }

        public void ResetInventory()
        {
            _ownedSkinIds.Clear();
            _ownedSkinIds.Add("player_original");
            _ownedSkinIds.Add("shadow_original");
            _ownedSkinIds.Add("torch_original_flame");
            PlayerPrefs.DeleteKey(KeyOwnedSkins);
            PlayerPrefs.Save();
        }
    }
}
