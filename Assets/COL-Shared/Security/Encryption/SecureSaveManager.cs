using System;
using System.IO;
using System.Text;
using UnityEngine;
using COLShared.SaveSystem;
using COLShared.Security.Logging;

namespace COLShared.Security.Encryption
{
    public class SecureSaveManager : MonoBehaviour
    {
        public static SecureSaveManager Instance { get; private set; }
        private const string SaveExt = ".dat";
        private const string ChecksumExt = ".chk";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SecureSave<T>(string key, T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                string encrypted = EncryptionService.Encrypt(json);
                if (encrypted == null)
                    throw new Exception("Encryption failed");
                string checksum = EncryptionService.GenerateChecksum(encrypted);
                string filePath = GetFilePath(key);
                string checksumPath = GetChecksumPath(key);
                File.WriteAllText(filePath, encrypted);
                File.WriteAllText(checksumPath, checksum);
            }
            catch (Exception)
            {
                SecureLogger.Log(LogLevel.Security, "SEC_SAVE_01", "[SecureSaveManager] Save error: 01");
            }
        }

        public T SecureLoad<T>(string key)
        {
            string filePath = GetFilePath(key);
            string checksumPath = GetChecksumPath(key);
            try
            {
                if (!File.Exists(filePath) || !File.Exists(checksumPath))
                    return default;
                string encrypted = File.ReadAllText(filePath);
                string checksum = File.ReadAllText(checksumPath);
                if (!EncryptionService.VerifyChecksum(encrypted, checksum))
                {
                    SecureLogger.Log(LogLevel.Security, "SEC_SAVE_CORRUPT", $"[SecureSaveManager] Corrupted: {key}");
                    return default;
                }
                string json = EncryptionService.Decrypt(encrypted);
                if (json == null)
                {
                    SecureLogger.Log(LogLevel.Security, "SEC_SAVE_DECRYPT", $"[SecureSaveManager] Decrypt error: {key}");
                    return default;
                }
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                SecureLogger.Log(LogLevel.Security, "SEC_SAVE_02", "[SecureSaveManager] Load error: 02");
                return default;
            }
        }

        public bool IsCorrupted(string key)
        {
            string filePath = GetFilePath(key);
            string checksumPath = GetChecksumPath(key);
            try
            {
                if (!File.Exists(filePath) || !File.Exists(checksumPath))
                    return false;
                string encrypted = File.ReadAllText(filePath);
                string checksum = File.ReadAllText(checksumPath);
                return !EncryptionService.VerifyChecksum(encrypted, checksum);
            }
            catch
            {
                return true;
            }
        }

        private string GetFilePath(string key)
        {
            string hash = EncryptionService.GenerateChecksum(key);
            return Path.Combine(Application.persistentDataPath, hash + SaveExt);
        }
        private string GetChecksumPath(string key)
        {
            string hash = EncryptionService.GenerateChecksum(key);
            return Path.Combine(Application.persistentDataPath, hash + ChecksumExt);
        }
    }
}