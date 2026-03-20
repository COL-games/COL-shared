using UnityEngine;

namespace COLShared.BattlePass
{
    [CreateAssetMenu(fileName = "BattlePassData", menuName = "COLShared/BattlePassData", order = 2)]
    public class BattlePassData : ScriptableObject
    {
        public BattlePassReward[] rewards = new BattlePassReward[100];
    }
}