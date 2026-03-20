using System;
using System.IO;
using System.Text;
using UnityEngine;
using COLShared.Security.Logging;

namespace COLShared.Security.Integrity
{
    [Flags]
    public enum TamperFlag
    {
        None = 0,
        SaveChecksumMismatch = 1 << 0,
        TimeManipulation = 1 << 1,
        ExcessiveCurrencyDelta = 1 << 2,
        BuildIntegrityFail = 1 << 3
    }

    public class TamperDetector : MonoBehaviour
    {
        public static TamperDetector Instance { get; private set; }
        public event Action<TamperFlag> OnTamperDetected;
        private const string FingerprintKey = "TamperDetector_Fingerprint";
        private const string LastSaveTimeKey = "TamperDetector_LastSaveTime";
        private TamperFlag activeFlags = TamperFlag.None;
        public bool IsTampered => activeFlags != TamperFlag.None;
        private string cachedFingerprint;
        private long lastSaveTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFingerprint();
        }

        private void LoadFingerprint()
        {
            cachedFingerprint = Encryption.EncryptionService.Decrypt(PlayerPrefs.GetString(FingerprintKey, null));
            lastSaveTime = long.Parse(PlayerPrefs.GetString(LastSaveTimeKey, "0"));
        }

        public void ValidateSession()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string appVersion = Application.version;
            string firstLaunchTime = PlayerPrefs.GetString("TamperDetector_FirstLaunchTime", null);
            if (string.IsNullOrEmpty(firstLaunchTime))
            {
                firstLaunchTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                PlayerPrefs.SetString("TamperDetector_FirstLaunchTime", firstLaunchTime);
                PlayerPrefs.Save();
            }
            string currentFingerprint = Encryption.EncryptionService.GenerateChecksum(deviceId + firstLaunchTime + appVersion);
            if (string.IsNullOrEmpty(cachedFingerprint))
            {
                string encrypted = Encryption.EncryptionService.Encrypt(currentFingerprint);
                PlayerPrefs.SetString(FingerprintKey, encrypted);
                PlayerPrefs.Save();
                cachedFingerprint = currentFingerprint;
            }
            else if (cachedFingerprint != currentFingerprint)
            {
                SetFlag(TamperFlag.BuildIntegrityFail);
            }
            // Time manipulation check
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now < lastSaveTime)
            {
                SetFlag(TamperFlag.TimeManipulation);
            }
        }

        public void ReportSaveChecksumMismatch()
        {
            SetFlag(TamperFlag.SaveChecksumMismatch);
        }

        public void ReportExcessiveCurrencyDelta()
        {
            SetFlag(TamperFlag.ExcessiveCurrencyDelta);
        }

        public void ReportBuildIntegrityFail()
        {
            SetFlag(TamperFlag.BuildIntegrityFail);
        }

        public void UpdateLastSaveTime()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastSaveTime = now;
            PlayerPrefs.SetString(LastSaveTimeKey, now.ToString());
            PlayerPrefs.Save();
        }

        public void SetFlag(TamperFlag flag)
        {
            if ((activeFlags & flag) == 0)
            {
                activeFlags |= flag;
                SecureLogger.Log(LogLevel.Security, "SEC_TAMPER", $"[TamperDetector] Tamper detected: {flag}");
                OnTamperDetected?.Invoke(flag);
            }
        }
    }
}