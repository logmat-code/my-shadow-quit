using System;
using System.Collections.Generic;

namespace MyShadowQuit.Monetization.IAP
{
    /// <summary>
    /// WebGL monetization adapter for itch.io and web browser platforms.
    /// Real-money purchases are completely disabled to respect Game Jam rules.
    /// All cosmetic skins are unlockable purely by earning Soul Coins through gameplay.
    /// </summary>
    public class WebGLMonetizationAdapter : IIAPService
    {
        private bool _isInitialized;
        private readonly List<IAPProductDefinition> _emptyCatalog = new List<IAPProductDefinition>();

        public bool IsInitialized => _isInitialized;
        public bool IsPlatformPurchasingSupported => false;
        public IReadOnlyList<IAPProductDefinition> Catalog => _emptyCatalog;

        public void Initialize(Action<bool, string> onComplete)
        {
            _isInitialized = true;
            onComplete?.Invoke(true, "WebGL mode active: All skins unlockable with in-game Soul Coins.");
        }

        public void PurchaseProduct(string productId, Action<bool, string> onComplete)
        {
            onComplete?.Invoke(false, "Real-money purchases are disabled in WebGL. Earn Soul Coins by playing!");
        }

        public void RestorePurchases(Action<bool, string> onComplete)
        {
            onComplete?.Invoke(true, "Progress is preserved locally via WebGL PlayerPrefs storage.");
        }
    }
}
