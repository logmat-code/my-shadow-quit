using System;
using UnityEngine;

namespace MyShadowQuit.Economy
{
    /// <summary>
    /// Evaluates and awards non-repeatable Soul Coin rewards for level and world progression:
    /// - First level completion: 100 Soul Coins
    /// - Complete level without restart: 25 Soul Coins
    /// - First world completion: 300 Soul Coins
    /// All claimed rewards are persistently recorded to prevent exploit farming via level replay.
    /// </summary>
    public class RewardService : MonoBehaviour
    {
        public static RewardService Instance { get; private set; }

        public const int RewardFirstLevelClear = 100;
        public const int RewardNoRestartBonus = 25;
        public const int RewardFirstWorldClear = 300;

        private const string KeyRewardLevelClearPrefix = "MSQ_Reward_LvlClear_";
        private const string KeyRewardNoRestartPrefix = "MSQ_Reward_NoRestart_";
        private const string KeyRewardWorldClearPrefix = "MSQ_Reward_WorldClear_";

        public event Action<int, string> OnRewardGranted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public bool IsLevelClearRewardClaimed(string worldId, int levelIndex)
        {
            return PlayerPrefs.GetInt($"{KeyRewardLevelClearPrefix}{worldId}_{levelIndex}", 0) == 1;
        }

        public bool IsNoRestartRewardClaimed(string worldId, int levelIndex)
        {
            return PlayerPrefs.GetInt($"{KeyRewardNoRestartPrefix}{worldId}_{levelIndex}", 0) == 1;
        }

        public bool IsWorldClearRewardClaimed(string worldId)
        {
            return PlayerPrefs.GetInt($"{KeyRewardWorldClearPrefix}{worldId}", 0) == 1;
        }

        public int EvaluateAndClaimLevelRewards(string worldId, int levelIndex, bool noRestartUsed, out string rewardSummary)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            int totalAwarded = 0;
            rewardSummary = string.Empty;

            bool firstClear = !IsLevelClearRewardClaimed(worldId, levelIndex);
            if (firstClear)
            {
                PlayerPrefs.SetInt($"{KeyRewardLevelClearPrefix}{worldId}_{levelIndex}", 1);
                totalAwarded += RewardFirstLevelClear;
                rewardSummary += $"+{RewardFirstLevelClear} SOUL COINS (CHAMBER CLEAR)";
            }

            if (noRestartUsed && !IsNoRestartRewardClaimed(worldId, levelIndex))
            {
                PlayerPrefs.SetInt($"{KeyRewardNoRestartPrefix}{worldId}_{levelIndex}", 1);
                totalAwarded += RewardNoRestartBonus;
                if (!string.IsNullOrEmpty(rewardSummary)) rewardSummary += " | ";
                rewardSummary += $"+{RewardNoRestartBonus} (UNBROKEN RUN)";
            }

            if (totalAwarded > 0)
            {
                PlayerPrefs.Save();
                if (WalletService.Instance != null)
                {
                    WalletService.Instance.AddCoins(totalAwarded, $"Level {levelIndex + 1} rewards");
                }
                OnRewardGranted?.Invoke(totalAwarded, rewardSummary);
            }

            return totalAwarded;
        }

        public int EvaluateAndClaimWorldClearReward(string worldId, out string rewardSummary)
        {
            if (string.IsNullOrEmpty(worldId)) worldId = "world_1";
            rewardSummary = string.Empty;

            if (!IsWorldClearRewardClaimed(worldId))
            {
                PlayerPrefs.SetInt($"{KeyRewardWorldClearPrefix}{worldId}", 1);
                PlayerPrefs.Save();
                if (WalletService.Instance != null)
                {
                    WalletService.Instance.AddCoins(RewardFirstWorldClear, $"World {worldId} completed");
                }
                rewardSummary = $"+{RewardFirstWorldClear} SOUL COINS (SANCTUARY CONQUERED)";
                OnRewardGranted?.Invoke(RewardFirstWorldClear, rewardSummary);
                return RewardFirstWorldClear;
            }

            return 0;
        }

        public void ResetClaimedRewards()
        {
            for (int i = 0; i < 20; i++)
            {
                PlayerPrefs.DeleteKey($"{KeyRewardLevelClearPrefix}world_1_{i}");
                PlayerPrefs.DeleteKey($"{KeyRewardNoRestartPrefix}world_1_{i}");
            }
            PlayerPrefs.DeleteKey($"{KeyRewardWorldClearPrefix}world_1");
            PlayerPrefs.Save();
        }
    }
}
