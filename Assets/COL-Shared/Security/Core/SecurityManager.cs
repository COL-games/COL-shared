using System;
using UnityEngine;
using COLShared.Security.Logging;
using COLShared.Security.Encryption;
using COLShared.Security.Integrity;
using COLShared.Security.RateLimit;
using COLShared.Security.Cheat;

namespace COLShared.Security.Core
{
    public class SecurityManager : MonoBehaviour
    {
        public static SecurityManager Instance { get; private set; }
        public static bool IsSecure =>
            _loggerInitialized &&
            _encryptionInitialized &&
            _saveManagerInitialized &&
            _tamperInitialized &&
            _rateLimiterInitialized &&
            _cheatDetectorInitialized &&
            (TamperDetector.Instance != null && !TamperDetector.Instance.IsTampered);

        public event Action OnSecurityCompromised;

        private static bool _loggerInitialized = false;
        private static bool _encryptionInitialized = false;
        private static bool _saveManagerInitialized = false;
        private static bool _tamperInitialized = false;
        private static bool _rateLimiterInitialized = false;
        private static bool _cheatDetectorInitialized = false;

        private bool _compromisedNotified = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                // 1. SecureLogger
                _loggerInitialized = true; // Static, no explicit init needed
                // 2. EncryptionService
                EncryptionService.Encrypt("init"); // Triggers key generation if needed
                _encryptionInitialized = true;
                // 3. SecureSaveManager
                if (SecureSaveManager.Instance == null)
                    new GameObject("SecureSaveManager").AddComponent<SecureSaveManager>();
                _saveManagerInitialized = true;
                // 4. TamperDetector
                if (TamperDetector.Instance == null)
                    new GameObject("TamperDetector").AddComponent<TamperDetector>();
                TamperDetector.Instance.ValidateSession();
                TamperDetector.Instance.OnTamperDetected += HandleTamper;
                _tamperInitialized = true;
                // 5. ActionRateLimiter
                if (ActionRateLimiter.Instance == null)
                    new GameObject("ActionRateLimiter").AddComponent<ActionRateLimiter>();
                _rateLimiterInitialized = true;
                // 6. CheatDetector
                if (CheatDetector.Instance == null)
                    new GameObject("CheatDetector").AddComponent<CheatDetector>();
                _cheatDetectorInitialized = true;
            }
            catch (Exception ex)
            {
                SecureLogger.Log(LogLevel.Critical, "SEC_INIT_FAIL", ex.Message);
            }
        }

        private void HandleTamper(TamperFlag flag)
        {
            if (!_compromisedNotified)
            {
                _compromisedNotified = true;
                OnSecurityCompromised?.Invoke();
            }
        }

        public string GetSecurityReport()
        {
            if (!Debug.isDebugBuild) return null;
            return $"Logger: {_loggerInitialized}\n" +
                   $"Encryption: {_encryptionInitialized}\n" +
                   $"SaveManager: {_saveManagerInitialized}\n" +
                   $"Tamper: {_tamperInitialized}\n" +
                   $"RateLimiter: {_rateLimiterInitialized}\n" +
                   $"CheatDetector: {_cheatDetectorInitialized}\n" +
                   $"IsTampered: {(TamperDetector.Instance != null && TamperDetector.Instance.IsTampered)}\n";
        }
    }
}