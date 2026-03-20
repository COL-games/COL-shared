using UnityEngine;

namespace COLShared.Shop
{
    [CreateAssetMenu(fileName = "ShopItemData", menuName = "COLShared/ShopItemData", order = 1)]
    public class ShopItemData : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        [TextArea]
        public string description;
        public int gemCost;
        public int coinCost;
        public ShopItemType itemType;
    }
}