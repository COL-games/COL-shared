namespace COLShared.Shop
{
    public enum ShopItemType
    {
        Skin,
        GemPack,
        CoinPack,
        BattlePassTier
    }

    public enum PurchaseResult
    {
        Success,
        InsufficientFunds,
        AlreadyOwned
    }
}