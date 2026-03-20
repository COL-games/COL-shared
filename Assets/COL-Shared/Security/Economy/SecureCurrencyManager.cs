using System;
using UnityEngine;
using COLShared.Security.Integrity;
using COLShared.Security.Encryption;
using COLShared.Security.Logging;

namespace COLShared.Security.Economy
{
    public class SecureCurrencyManager : MonoBehaviour
    {
        public static SecureCurrencyManager Instance { get; private set; }

        private const string GemsKey = "CurrencyManager_Gems";
        private const string CoinsKey = "CurrencyManager_Coins";
        private const int MaxGems = 999999;
        private const int MaxCoins = 9999999;

        public event Action OnCurrencyChanged;

        public ObfuscatedInt CurrentGems { get; private set; }
        public ObfuscatedInt CurrentCoins { get; private set; }

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
            int newValue = CurrentGems + amount;
            if (newValue > MaxGems)
            {
                newValue = MaxGems;
                TamperDetector.Instance?.ReportExcessiveCurrencyDelta();
            }
            CurrentGems = newValue;
            SaveCurrency();
            LogTransaction("AddGems", amount);
            OnCurrencyChanged?.Invoke();
        }

        public bool SpendGems(int amount)
        {
            if (TamperDetector.Instance != null && TamperDetector.Instance.IsTampered)
                return false;
            if (amount <= 0 || CurrentGems < amount)
                return false;
            CurrentGems = CurrentGems - amount;
            SaveCurrency();
            LogTransaction("SpendGems", amount);
            OnCurrencyChanged?.Invoke();
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            int newValue = CurrentCoins + amount;
            if (newValue > MaxCoins)
            {
                newValue = MaxCoins;
                TamperDetector.Instance?.ReportExcessiveCurrencyDelta();
            }
            CurrentCoins = newValue;
            SaveCurrency();
            LogTransaction("AddCoins", amount);
            OnCurrencyChanged?.Invoke();
        }

        public bool SpendCoins(int amount)
        {
            if (TamperDetector.Instance != null && TamperDetector.Instance.IsTampered)
                return false;
            if (amount <= 0 || CurrentCoins < amount)
                return false;
            CurrentCoins = CurrentCoins - amount;
            SaveCurrency();
            LogTransaction("SpendCoins", amount);
            OnCurrencyChanged?.Invoke();
            return true;
        }

        private void SaveCurrency()
        {
            SecureSaveManager.Instance?.SecureSave(GemsKey, (int)CurrentGems);
            SecureSaveManager.Instance?.SecureSave(CoinsKey, (int)CurrentCoins);
        }

        private void LoadCurrency()
        {
            int gems = SecureSaveManager.Instance?.SecureLoad<int>(GemsKey) ?? 0;
            int coins = SecureSaveManager.Instance?.SecureLoad<int>(CoinsKey) ?? 0;
            CurrentGems = gems;
            CurrentCoins = coins;
        }

        private void LogTransaction(string type, int amount)
        {
            string hash = EncryptionService.GenerateChecksum(type + amount + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            SecureLogger.Log(LogLevel.Security, "SEC_CURRENCY", $"[Currency] {type} {amount} {hash}");
        }
    }
}