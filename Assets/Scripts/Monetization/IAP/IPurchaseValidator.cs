using System;

namespace MyShadowQuit.Monetization.IAP
{
    public enum ValidationState
    {
        Valid,
        Invalid,
        Pending,
        NetworkError,
        NotConfigured
    }

    public struct ValidationResult
    {
        public ValidationState State;
        public string TransactionId;
        public string ProductId;
        public string ErrorMessage;

        public bool IsSuccess => State == ValidationState.Valid;
    }

    /// <summary>
    /// Contract for verifying purchase receipts securely against Google Play Developer API
    /// or a trusted game backend (Cloud Functions / Firebase / custom server).
    /// </summary>
    public interface IPurchaseValidator
    {
        bool IsBackendConfigured { get; }
        void ValidateReceipt(string productId, string purchaseToken, string receiptPayload, Action<ValidationResult> onComplete);
    }

    /// <summary>
    /// Default adapter used prior to live backend deployment.
    /// Explicitly flags transactions as NotConfigured rather than faking receipt security.
    /// Never stores private service account keys inside client binaries.
    /// </summary>
    public class PendingBackendValidator : IPurchaseValidator
    {
        public bool IsBackendConfigured => false;

        public void ValidateReceipt(string productId, string purchaseToken, string receiptPayload, Action<ValidationResult> onComplete)
        {
            // Explicitly report pending external configuration
            onComplete?.Invoke(new ValidationResult
            {
                State = ValidationState.NotConfigured,
                ProductId = productId,
                ErrorMessage = "Backend receipt verification endpoint is pending Google Play Service Account configuration."
            });
        }
    }
}
