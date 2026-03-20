using UnityEngine;
using System;

namespace COLShared.Currency
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public event Action OnCurrencyChanged;

        private const string GemsKey = "CurrencyManager_Gems";
        private const string CoinsKey = "CurrencyManager_Coins";

        public int CurrentGems { get; private set; }
        public int CurrentCoins { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCurrency();
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            CurrentGems += amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0 || CurrentGems < amount)
                return false;
            CurrentGems -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            CurrentCoins += amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || CurrentCoins < amount)
                return false;
            CurrentCoins -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke();
            return true;
        }

        private void SaveCurrency()
        {
            PlayerPrefs.SetInt(GemsKey, CurrentGems);
            PlayerPrefs.SetInt(CoinsKey, CurrentCoins);
            PlayerPrefs.Save();
        }

        private void LoadCurrency()
        {
            CurrentGems = PlayerPrefs.GetInt(GemsKey, 0);
            CurrentCoins = PlayerPrefs.GetInt(CoinsKey, 0);
        }
    }
}
