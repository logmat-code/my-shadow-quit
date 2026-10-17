using System;
using UnityEngine;
using MyShadowQuit.Cosmetics;
using MyShadowQuit.Core;

namespace MyShadowQuit.Economy
{
    /// <summary>
    /// Transaction service managing purchases in the Shop.
    /// Ensures debouncing against rapid double clicks, validates balance,
    /// atomically transfers ownership, and triggers audio/visual feedback.
    /// </summary>
    public class ShopTransactionService : MonoBehaviour
    {
        public static ShopTransactionService Instance { get; private set; }

        public event Action<SkinDefinition> OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;

        private float _lastTransactionTime;
        private const float DebounceCooldownSeconds = 0.25f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public bool TryPurchaseSkinWithCoins(string skinId, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Debounce rapid double clicking
            if (Time.unscaledTime - _lastTransactionTime < DebounceCooldownSeconds)
            {
                errorMessage = "Transaction in progress...";
                return false;
            }
            _lastTransactionTime = Time.unscaledTime;

            SkinDefinition skin = SkinCatalog.GetSkin(skinId);
            if (skin == null)
            {
                errorMessage = "Unknown relic.";
                OnPurchaseFailed?.Invoke(errorMessage);
                return false;
            }

            if (InventoryService.Instance != null && InventoryService.Instance.IsSkinOwned(skinId))
            {
                errorMessage = "Already in your possession.";
                OnPurchaseFailed?.Invoke(errorMessage);
                return false;
            }

            if (WalletService.Instance == null || !WalletService.Instance.CanAfford(skin.Price))
            {
                errorMessage = "Insufficient Soul Coins.";
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("land", 0.5f, 0.75f);
                }
                OnPurchaseFailed?.Invoke(errorMessage);
                return false;
            }

            // Deduct coins atomically
            if (!WalletService.Instance.SpendCoins(skin.Price, $"Purchase skin: {skin.DisplayName}"))
            {
                errorMessage = "Transaction failed.";
                OnPurchaseFailed?.Invoke(errorMessage);
                return false;
            }

            // Unlock skin in inventory
            if (InventoryService.Instance != null)
            {
                InventoryService.Instance.UnlockSkin(skinId);
            }

            // Auto-equip newly acquired skin
            if (SkinEquipmentService.Instance != null)
            {
                SkinEquipmentService.Instance.EquipSkin(skinId);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("crystal_activate", 0.75f, 1.15f);
            }

            OnPurchaseSuccess?.Invoke(skin);
            return true;
        }

        public bool PurchaseSkin(string skinId)
        {
            return TryPurchaseSkinWithCoins(skinId, out _);
        }
    }
}
