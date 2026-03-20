using UnityEngine;

namespace COLShared.BattlePass
{
    [CreateAssetMenu(fileName = "BattlePassReward", menuName = "COLShared/BattlePassReward", order = 1)]
    public class BattlePassReward : ScriptableObject
    {
        public int level;
        public string rewardId;
        public string displayName;
        public Sprite icon;
        public bool isPremium;
        public int coinAmount;
        public int gemAmount;
    }
}