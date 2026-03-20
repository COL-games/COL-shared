using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COLShared.Currency;
using COLShared.SaveSystem;

namespace COLShared.BattlePass
{
    public class BattlePassManager : MonoBehaviour
    {
        public static BattlePassManager Instance { get; private set; }
        public event Action OnLevelUp;
        public event Action<BattlePassReward> OnRewardClaimed;

        private const string XPKey = "BattlePass_XP";
        private const string LevelKey = "BattlePass_Level";
        private const string ClaimedRewardsKey = "BattlePass_Claimed";
        private const string PremiumKey = "BattlePass_Premium";

        public BattlePassData battlePassData;
        public int CurrentLevel { get; private set; }
        public int CurrentXP { get; private set; }
        public int XPToNextLevel => 1000; // esempio: 1000 XP per livello
        public bool HasPremiumPass { get; private set; }

        private HashSet<string> claimedRewards = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadState();
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            CurrentXP += amount;
            while (CurrentLevel < 100 && CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                CurrentLevel++;
                SaveState();
                OnLevelUp?.Invoke();
            }
            SaveState();
        }

        public bool ClaimReward(int level, bool isPremium)
        {
            if (level < 1 || level > 100) return false;
            if (CurrentLevel < level) return false;
            if (isPremium && !HasPremiumPass) return false;
            string key = RewardKey(level, isPremium);
            if (claimedRewards.Contains(key)) return false;

            var reward = battlePassData.rewards.FirstOrDefault(r => r.level == level && r.isPremium == isPremium);
            if (reward == null) return false;

            // Grant currency
            if (reward.coinAmount > 0)
                CurrencyManager.Instance?.AddCoins(reward.coinAmount);
            if (reward.gemAmount > 0)
                CurrencyManager.Instance?.AddGems(reward.gemAmount);

            claimedRewards.Add(key);
            SaveState();
            OnRewardClaimed?.Invoke(reward);
            return true;
        }

        public bool IsRewardClaimed(int level, bool isPremium)
        {
            return claimedRewards.Contains(RewardKey(level, isPremium));
        }

        public void UnlockPremiumPass()
        {
            if (HasPremiumPass) return;
            HasPremiumPass = true;
            SaveState();
        }

        private string RewardKey(int level, bool isPremium)
        {
            return $"{level}_{(isPremium ? "P" : "F")}";
        }

        private void SaveState()
        {
            SaveManager.Instance?.Save(XPKey, CurrentXP);
            SaveManager.Instance?.Save(LevelKey, CurrentLevel);
            SaveManager.Instance?.Save(ClaimedRewardsKey, claimedRewards.ToList());
            SaveManager.Instance?.Save(PremiumKey, HasPremiumPass);
        }

        private void LoadState()
        {
            CurrentXP = SaveManager.Instance?.Load<int>(XPKey) ?? 0;
            CurrentLevel = SaveManager.Instance?.Load<int>(LevelKey) ?? 1;
            var claimed = SaveManager.Instance?.Load<List<string>>(ClaimedRewardsKey);
            claimedRewards = claimed != null ? new HashSet<string>(claimed) : new HashSet<string>();
            HasPremiumPass = SaveManager.Instance?.Load<bool>(PremiumKey) ?? false;
        }
    }
}