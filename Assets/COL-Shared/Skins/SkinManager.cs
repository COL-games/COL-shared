using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using COLShared.SaveSystem;

namespace COLShared.Skins
{
    public class SkinManager : MonoBehaviour
    {
        public static SkinManager Instance { get; private set; }
        public event Action OnSkinChanged;

        private const string UnlockedSkinsKey = "UnlockedSkins";
        private const string ActiveSkinKey = "ActiveSkin";

        private List<SkinData> allSkins;
        private HashSet<string> unlockedSkins = new HashSet<string>();
        private string activeSkinId;

        public IReadOnlyList<SkinData> AllSkins => allSkins;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSkins();
            LoadState();
        }

        private void LoadAllSkins()
        {
            allSkins = Resources.LoadAll<SkinData>("").ToList();
        }

        private void LoadState()
        {
            var unlocked = SaveManager.Instance?.Load<List<string>>(UnlockedSkinsKey);
            if (unlocked != null)
                unlockedSkins = new HashSet<string>(unlocked);
            else
                unlockedSkins = new HashSet<string>(allSkins.Where(s => s.isDefault).Select(s => s.skinId));

            activeSkinId = SaveManager.Instance?.Load<string>(ActiveSkinKey);
            if (string.IsNullOrEmpty(activeSkinId) || !IsSkinUnlocked(activeSkinId))
            {
                var defaultSkin = allSkins.FirstOrDefault(s => s.isDefault);
                activeSkinId = defaultSkin != null ? defaultSkin.skinId : allSkins.FirstOrDefault()?.skinId;
            }
        }

        private void SaveState()
        {
            SaveManager.Instance?.Save(UnlockedSkinsKey, unlockedSkins.ToList());
            SaveManager.Instance?.Save(ActiveSkinKey, activeSkinId);
        }

        public void UnlockSkin(string skinId)
        {
            if (unlockedSkins.Add(skinId))
            {
                SaveState();
                OnSkinChanged?.Invoke();
            }
        }

        public bool IsSkinUnlocked(string skinId)
        {
            return unlockedSkins.Contains(skinId);
        }

        public void SetActiveSkin(string skinId)
        {
            if (!IsSkinUnlocked(skinId)) return;
            if (activeSkinId == skinId) return;
            activeSkinId = skinId;
            SaveState();
            OnSkinChanged?.Invoke();
        }

        public SkinData GetActiveSkin()
        {
            return allSkins.FirstOrDefault(s => s.skinId == activeSkinId);
        }
    }
}