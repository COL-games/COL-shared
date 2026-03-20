using System;
using System.Collections.Generic;
using UnityEngine;
using COLShared.Security.Integrity;
using COLShared.Security.Logging;

namespace COLShared.Security.RateLimit
{
    public class ActionRateLimiter : MonoBehaviour
    {
        public static ActionRateLimiter Instance { get; private set; }

        private class ActionLimit
        {
            public int MaxCount;
            public float WindowSeconds;
            public Queue<float> Timestamps = new Queue<float>();
        }

        private Dictionary<string, ActionLimit> limits = new Dictionary<string, ActionLimit>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterDefaults();
        }

        private void RegisterDefaults()
        {
            RegisterAction("gem_spend", 10, 60f);
            RegisterAction("coin_spend", 20, 60f);
            RegisterAction("purchase", 5, 60f);
            RegisterAction("level_complete", 30, 60f);
            RegisterAction("reward_claim", 50, 60f);
        }

        public void RegisterAction(string actionId, int maxCount, float windowSeconds)
        {
            if (limits.ContainsKey(actionId))
            {
                limits[actionId].MaxCount = maxCount;
                limits[actionId].WindowSeconds = windowSeconds;
                limits[actionId].Timestamps.Clear();
            }
            else
            {
                limits[actionId] = new ActionLimit { MaxCount = maxCount, WindowSeconds = windowSeconds };
            }
        }

        public bool TryPerformAction(string actionId)
        {
            if (!limits.TryGetValue(actionId, out var limit))
                return true; // No limit registered, allow
            float now = Time.unscaledTime;
            // Remove expired timestamps
            while (limit.Timestamps.Count > 0 && now - limit.Timestamps.Peek() > limit.WindowSeconds)
                limit.Timestamps.Dequeue();
            if (limit.Timestamps.Count < limit.MaxCount)
            {
                limit.Timestamps.Enqueue(now);
                return true;
            }
            else
            {
                SecureLogger.Log(LogLevel.Security, "SEC_RATELIMIT", $"[RateLimit] Exceeded: {actionId}");
                TamperDetector.Instance?.SetFlag(TamperFlag.ExcessiveCurrencyDelta);
                return false;
            }
        }
    }
}