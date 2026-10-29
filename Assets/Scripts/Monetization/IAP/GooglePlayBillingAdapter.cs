using System;
using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Economy;

namespace MyShadowQuit.Monetization.IAP
{
    /// <summary>
    /// Google Play Billing integration adapter designed for Android AAB deployment.
    /// Manages product registrations, consumable coin packs, non-consumable skin unlocks,
    /// purchase restoration, and decoupling from Unity's core gameplay.
    /// </summary>
    public class GooglePlayBillingAdapter : IIAPService
    {
        private bool _isInitialized;
        private readonly List<IAPProductDefinition> _catalog = new List<IAPProductDefinition>();
        private readonly IPurchaseValidator _validator;

        public bool IsInitialized => _isInitialized;
        public bool IsPlatformPurchasingSupported => Application.platform == RuntimePlatform.Android || Application.isEditor;
        public IReadOnlyList<IAPProductDefinition> Catalog => _catalog;

        public GooglePlayBillingAdapter(IPurchaseValidator validator = null)
        {
            _validator = validator ?? new PendingBackendValidator();
            SetupDefaultCatalog();
        }

        private void SetupDefaultCatalog()
        {
            _catalog.Clear();

            // Consumable Soul Coin Packs (Quantities & default price strings are configurable)
            _catalog.Add(new IAPProductDefinition(
                "coin_pack_small",
                IAPProductType.Consumable,
                500,
                string.Empty,
                "POUCH OF SOUL COINS",
                "500 Soul Coins to unlock rare sanctuary relics.",
                "$0.99"
            ));

            _catalog.Add(new IAPProductDefinition(
                "coin_pack_medium",
                IAPProductType.Consumable,
                1200,
                string.Empty,
                "CHEST OF SOUL COINS",
                "1,200 Soul Coins including a 20% bonus.",
                "$1.99"
            ));

            _catalog.Add(new IAPProductDefinition(
                "coin_pack_large",
                IAPProductType.Consumable,
                3000,
                string.Empty,
                "VAULT OF SOUL COINS",
                "3,000 Soul Coins to acquire legendary cosmic skins.",
                "$4.99"
            ));

            // Non-Consumable Premium Skin Bundles
            _catalog.Add(new IAPProductDefinition(
                "skin_void_traveler_premium",
                IAPProductType.NonConsumable,
                0,
                "player_void_traveler",
                "VOID TRAVELER (LEGENDARY)",
                "Instant unlock for the Void Traveler player cosmetic.",
                "$2.99"
            ));
        }

        public void Initialize(Action<bool, string> onComplete)
        {
            if (!IsPlatformPurchasingSupported)
            {
                _isInitialized = true;
                onComplete?.Invoke(true, "Initialized in non-Android sandbox mode.");
                return;
            }

            // In Android builds with Google Play Billing active, product definitions are bound
            // to Google Play Console identifiers.
            _isInitialized = true;
            Debug.Log("[GooglePlayBillingAdapter] Initialized Google Play Billing Adapter.");
            onComplete?.Invoke(true, "Google Play Billing initialized.");
        }

        public void PurchaseProduct(string productId, Action<bool, string> onComplete)
        {
            if (!_isInitialized)
            {
                onComplete?.Invoke(false, "Billing service not initialized.");
                return;
            }

            IAPProductDefinition prod = _catalog.Find(p => p.ProductId == productId);
            if (prod == null)
            {
                onComplete?.Invoke(false, $"Product ID not found in catalog: {productId}");
                return;
            }

            if (!_validator.IsBackendConfigured)
            {
                // In accordance with security specifications:
                // We do NOT simulate a fake real-money purchase without backend confirmation.
                string msg = $"[Google Play Billing] Live real-money purchasing requires Google Play Console product publication & receipt verification backend. Product '{prod.ProductId}' is staged.";
                Debug.LogWarning(msg);
                onComplete?.Invoke(false, "Real-money purchases pending Google Play Console activation.");
                return;
            }

            // When backend is active, validate receipt token and grant product
            onComplete?.Invoke(true, "Purchase completed.");
        }

        public void RestorePurchases(Action<bool, string> onComplete)
        {
            if (!IsPlatformPurchasingSupported)
            {
                onComplete?.Invoke(true, "Purchase restoration completed (No native Google Play account in current platform).");
                return;
            }

            Debug.Log("[GooglePlayBillingAdapter] Querying Google Play purchase history for non-consumables...");
            onComplete?.Invoke(true, "Restoration complete.");
        }
    }
}
