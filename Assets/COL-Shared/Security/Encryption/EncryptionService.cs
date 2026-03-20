using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace COLShared.Security.Encryption
{
    public static class EncryptionService
    {
        private const string KeyPref = "EncryptionService_AESKey";
        private const int KeySize = 32; // 256 bit
        private const int IvSize = 16; // 128 bit
        private const string VersionPrefix = "v1_";

        private static byte[] aesKey;
        private static bool keyLoaded = false;

        private static void EnsureKey()
        {
            if (keyLoaded) return;
            string keyBase64 = null;
#if UNITY_IOS && !UNITY_EDITOR
            // iOS Keychain (semplificato, implementare plugin nativo per reale sicurezza)
            keyBase64 = PlayerPrefs.GetString(KeyPref, null);
#else
            keyBase64 = PlayerPrefs.GetString(KeyPref, null);
#endif
            if (string.IsNullOrEmpty(keyBase64))
            {
                aesKey = new byte[KeySize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(aesKey);
                }
                keyBase64 = Convert.ToBase64String(aesKey);
                PlayerPrefs.SetString(KeyPref, keyBase64);
                PlayerPrefs.Save();
            }
            else
            {
                aesKey = Convert.FromBase64String(keyBase64);
            }
            keyLoaded = true;
        }

        public static string Encrypt(string plainText)
        {
            try
            {
                EnsureKey();
                using (var aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV();
                    var iv = aes.IV;
                    using (var encryptor = aes.CreateEncryptor())
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        var result = new byte[IvSize + cipherBytes.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, IvSize);
                        Buffer.BlockCopy(cipherBytes, 0, result, IvSize, cipherBytes.Length);
                        return VersionPrefix + Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError($"[EncryptionService] Encrypt error: 01");
                return null;
            }
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                EnsureKey();
                if (!cipherText.StartsWith(VersionPrefix))
                    throw new Exception("Unsupported version");
                var base64 = cipherText.Substring(VersionPrefix.Length);
                var allBytes = Convert.FromBase64String(base64);
                var iv = new byte[IvSize];
                Buffer.BlockCopy(allBytes, 0, iv, 0, IvSize);
                var cipherBytes = new byte[allBytes.Length - IvSize];
                Buffer.BlockCopy(allBytes, IvSize, cipherBytes, 0, cipherBytes.Length);
                using (var aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var decryptor = aes.CreateDecryptor())
                    {
                        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(plainBytes);
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError($"[EncryptionService] Decrypt error: 02");
                return null;
            }
        }

        public static string GenerateChecksum(string data)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static bool VerifyChecksum(string data, string checksum)
        {
            var hash = GenerateChecksum(data);
            return string.Equals(hash, checksum, StringComparison.OrdinalIgnoreCase);
        }
    }
}