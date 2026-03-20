using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COLShared.Security.Logging
{
    public enum LogLevel
    {
        Info,
        Warning,
        Security,
        Critical
    }

    public static class SecureLogger
    {
        private const string LogFileName = "securelog.txt";
        private const int MaxLogLines = 500;
        private static readonly object fileLock = new object();

        public static void Log(LogLevel level, string eventCode, string details = "")
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string msg = $"[COL][{level}][{eventCode}] {details}";
            if (Debug.isDebugBuild)
            {
                Debug.Log(msg);
            }
            else
            {
                if (level == LogLevel.Info || level == LogLevel.Warning)
                    return;
            }
            if (level == LogLevel.Security || level == LogLevel.Critical)
            {
                WriteSecurityLog($"{timestamp} {msg}");
            }
        }

        private static void WriteSecurityLog(string line)
        {
            lock (fileLock)
            {
                string path = Path.Combine(Application.persistentDataPath, LogFileName);
                List<string> lines = new List<string>();
                if (File.Exists(path))
                {
                    lines.AddRange(File.ReadAllLines(path));
                }
                lines.Add(line);
                if (lines.Count > MaxLogLines)
                {
                    lines.RemoveRange(0, lines.Count - MaxLogLines);
                }
                File.WriteAllLines(path, lines);
            }
        }

        public static List<string> GetSecurityLog()
        {
            lock (fileLock)
            {
                string path = Path.Combine(Application.persistentDataPath, LogFileName);
                if (!File.Exists(path))
                    return new List<string>();
                var lines = new List<string>(File.ReadAllLines(path));
                int count = lines.Count;
                if (count > 50)
                    return lines.GetRange(count - 50, 50);
                return lines;
            }
        }
    }
}