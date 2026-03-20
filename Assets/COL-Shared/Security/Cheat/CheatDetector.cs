using System;
using UnityEngine;
using COLShared.Security.Integrity;
using COLShared.Security.Economy;
using COLShared.Security.Logging;

namespace COLShared.Security.Cheat
{
    public class CheatDetector : MonoBehaviour
    {
        public static CheatDetector Instance { get; private set; }

        private float checkInterval = 30f;
        private float lastCheckTime;
        private float lastRealtime;
        private DateTime lastSystemTime;
        private int lastGems;
        private int lastCoins;
        private int frameAnomalyCount = 0;
        private const int FrameAnomalyThreshold = 100;
        private const float MinDeltaTime = 0.001f;
        private const int MaxGemsDelta = 500;
        private const int MaxCoinsDelta = 5000;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            lastCheckTime = Time.realtimeSinceStartup;
            lastRealtime = Time.realtimeSinceStartup;
            lastSystemTime = DateTime.UtcNow;
            lastGems = SecureCurrencyManager.Instance != null ? (int)SecureCurrencyManager.Instance.CurrentGems : 0;
            lastCoins = SecureCurrencyManager.Instance != null ? (int)SecureCurrencyManager.Instance.CurrentCoins : 0;
        }

        private void Update()
        {
            // Frame Rate Anomaly
            if (Time.deltaTime < MinDeltaTime)
            {
                frameAnomalyCount++;
                if (frameAnomalyCount > FrameAnomalyThreshold)
                {
                    TamperDetector.Instance?.SetFlag(TamperFlag.TimeManipulation);
                    SecureLogger.Log(LogLevel.Security, "SEC_CHEAT_FRAMERATE", "[CheatDetector] Frame rate anomaly detected");
                    frameAnomalyCount = 0;
                }
            }
            else
            {
                frameAnomalyCount = 0;
            }

            if (Time.realtimeSinceStartup - lastCheckTime >= checkInterval)
            {
                PerformChecks();
                lastCheckTime = Time.realtimeSinceStartup;
            }
        }

        private void PerformChecks()
        {
            // Time Integrity
            float realtimeNow = Time.realtimeSinceStartup;
            DateTime systemNow = DateTime.UtcNow;
            double systemDelta = (systemNow - lastSystemTime).TotalSeconds;
            float realtimeDelta = realtimeNow - lastRealtime;
            if (Math.Abs(systemDelta - realtimeDelta) > 10f)
            {
                TamperDetector.Instance?.SetFlag(TamperFlag.TimeManipulation);
                SecureLogger.Log(LogLevel.Security, "SEC_CHEAT_TIME", "[CheatDetector] Time integrity anomaly");
            }
            lastRealtime = realtimeNow;
            lastSystemTime = systemNow;

            // Currency Sanity
            int gems = SecureCurrencyManager.Instance != null ? (int)SecureCurrencyManager.Instance.CurrentGems : 0;
            int coins = SecureCurrencyManager.Instance != null ? (int)SecureCurrencyManager.Instance.CurrentCoins : 0;
            if (gems - lastGems > MaxGemsDelta)
            {
                TamperDetector.Instance?.SetFlag(TamperFlag.ExcessiveCurrencyDelta);
                SecureLogger.Log(LogLevel.Security, "SEC_CHEAT_GEMS", $"[CheatDetector] Gems anomaly: +{gems - lastGems}");
            }
            if (coins - lastCoins > MaxCoinsDelta)
            {
                TamperDetector.Instance?.SetFlag(TamperFlag.ExcessiveCurrencyDelta);
                SecureLogger.Log(LogLevel.Security, "SEC_CHEAT_COINS", $"[CheatDetector] Coins anomaly: +{coins - lastCoins}");
            }
            lastGems = gems;
            lastCoins = coins;
        }
    }
}