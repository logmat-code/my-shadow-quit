using System;
using System.Collections.Generic;

namespace MyShadowQuit.Monetization.IAP
{
    public enum IAPProductType
    {
        Consumable,
        NonConsumable
    }

    [Serializable]
    public class IAPProductDefinition
    {
        public string ProductId;
        public IAPProductType ProductType;
        public int SoulCoinsGranted;
        public string SkinIdGranted;
        public string LocalizedTitle;
        public string LocalizedDescription;
        public string DefaultPriceString;

        public IAPProductDefinition(
            string productId,
            IAPProductType productType,
            int soulCoinsGranted,
            string skinIdGranted,
            string localizedTitle,
            string localizedDescription,
            string defaultPriceString)
        {
            ProductId = productId;
            ProductType = productType;
            SoulCoinsGranted = soulCoinsGranted;
            SkinIdGranted = skinIdGranted;
            LocalizedTitle = localizedTitle;
            LocalizedDescription = localizedDescription;
            DefaultPriceString = defaultPriceString;
        }
    }

    /// <summary>
    /// Common interface for in-app billing across platforms (Android Google Play vs WebGL / Itch).
    /// </summary>
    public interface IIAPService
    {
        bool IsInitialized { get; }
        bool IsPlatformPurchasingSupported { get; }
        IReadOnlyList<IAPProductDefinition> Catalog { get; }

        void Initialize(Action<bool, string> onComplete);
        void PurchaseProduct(string productId, Action<bool, string> onComplete);
        void RestorePurchases(Action<bool, string> onComplete);
    }
}
