using UnityEngine;

namespace COLShared.Skins
{
    [CreateAssetMenu(fileName = "SkinData", menuName = "COLShared/SkinData", order = 1)]
    public class SkinData : ScriptableObject
    {
        public string skinId;
        public string displayName;
        public Sprite previewImage;
        public SkinRarity rarity;
        public int gemCost;
        public bool isDefault;
    }
}