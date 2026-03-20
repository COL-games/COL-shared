using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COLShared.Currency;
using COLShared.Skins;

namespace COLShared.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        public event Action<ShopItemData, PurchaseResult> OnPurchaseCompleted;

        private List<ShopItemData> allItems;

        public IReadOnlyList<ShopItemData> AllItems => allItems;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllItems();
        }

        private void LoadAllItems()
        {
            allItems = Resources.LoadAll<ShopItemData>("").ToList();
        }

        public PurchaseResult PurchaseItem(string itemId)
        {
            var item = allItems.FirstOrDefault(i => i.itemId == itemId);
            if (item == null) return PurchaseResult.InsufficientFunds;

            // Check ownership for Skin
            if (item.itemType == ShopItemType.Skin && SkinManager.Instance != null)
            {
                if (SkinManager.Instance.IsSkinUnlocked(itemId))
                {
                    OnPurchaseCompleted?.Invoke(item, PurchaseResult.AlreadyOwned);
                    return PurchaseResult.AlreadyOwned;
                }
            }

            // Check currency
            bool hasFunds = true;
            if (item.gemCost > 0)
                hasFunds = CurrencyManager.Instance != null && CurrencyManager.Instance.CurrentGems >= item.gemCost;
            else if (item.coinCost > 0)
                hasFunds = CurrencyManager.Instance != null && CurrencyManager.Instance.CurrentCoins >= item.coinCost;

            if (!hasFunds)
            {
                OnPurchaseCompleted?.Invoke(item, PurchaseResult.InsufficientFunds);
                return PurchaseResult.InsufficientFunds;
            }

            // Deduct currency
            bool spent = false;
            if (item.gemCost > 0)
                spent = CurrencyManager.Instance.SpendGems(item.gemCost);
            else if (item.coinCost > 0)
                spent = CurrencyManager.Instance.SpendCoins(item.coinCost);
            else
                spent = true;

            if (!spent)
            {
                OnPurchaseCompleted?.Invoke(item, PurchaseResult.InsufficientFunds);
                return PurchaseResult.InsufficientFunds;
            }

            // Grant item
            if (item.itemType == ShopItemType.Skin && SkinManager.Instance != null)
            {
                SkinManager.Instance.UnlockSkin(itemId);
            }
            // For GemPack, CoinPack, BattlePassTier: implement logic as needed

            OnPurchaseCompleted?.Invoke(item, PurchaseResult.Success);
            return PurchaseResult.Success;
        }
    }
}