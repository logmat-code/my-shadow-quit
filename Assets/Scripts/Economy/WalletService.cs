using System;
using UnityEngine;

namespace MyShadowQuit.Economy
{
    /// <summary>
    /// Manages the player's virtual Soul Coins currency balance.
    /// Strictly guarantees non-negative balance, local persistence, and event notifications.
    /// </summary>
    public class WalletService : MonoBehaviour
    {
        public static WalletService Instance { get; private set; }

        public const string KeySoulCoinsBalance = "MSQ_SoulCoins_Balance";

        public event Action<int> OnBalanceChanged;

        private int _balance;

        public int Balance => _balance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            LoadBalance();
        }

        public void LoadBalance()
        {
            _balance = Mathf.Max(0, PlayerPrefs.GetInt(KeySoulCoinsBalance, 0));
        }

        public bool CanAfford(int amount)
        {
            if (amount <= 0) return true;
            return _balance >= amount;
        }

        public void AddCoins(int amount, string reason = "")
        {
            if (amount <= 0) return;
            _balance += amount;
            PlayerPrefs.SetInt(KeySoulCoinsBalance, _balance);
            PlayerPrefs.Save();
            OnBalanceChanged?.Invoke(_balance);
        }

        public bool SpendCoins(int amount, string reason = "")
        {
            if (amount < 0) return false;
            if (amount == 0) return true;
            if (_balance < amount)
            {
                return false;
            }

            _balance -= amount;
            PlayerPrefs.SetInt(KeySoulCoinsBalance, _balance);
            PlayerPrefs.Save();
            OnBalanceChanged?.Invoke(_balance);
            return true;
        }

        public void SetBalanceForTesting(int amount)
        {
            _balance = Mathf.Max(0, amount);
            PlayerPrefs.SetInt(KeySoulCoinsBalance, _balance);
            PlayerPrefs.Save();
            OnBalanceChanged?.Invoke(_balance);
        }

        public void ResetWallet()
        {
            _balance = 0;
            PlayerPrefs.SetInt(KeySoulCoinsBalance, 0);
            PlayerPrefs.Save();
            OnBalanceChanged?.Invoke(_balance);
        }
    }
}
