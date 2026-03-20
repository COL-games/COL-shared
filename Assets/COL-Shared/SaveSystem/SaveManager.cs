using UnityEngine;
using System;
using System.IO;

namespace COLShared.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

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

        public void Save<T>(string key, T data)
        {
            string json = JsonUtility.ToJson(data);
            string path = GetPath(key);
            File.WriteAllText(path, json);
        }

        public T Load<T>(string key)
        {
            string path = GetPath(key);
            if (!File.Exists(path))
                return default;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public void DeleteSave(string key)
        {
            string path = GetPath(key);
            if (File.Exists(path))
                File.Delete(path);
        }

        public void DeleteAll()
        {
            string dir = Application.persistentDataPath;
            DirectoryInfo di = new DirectoryInfo(dir);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private string GetPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key + ".json");
        }
    }
}
