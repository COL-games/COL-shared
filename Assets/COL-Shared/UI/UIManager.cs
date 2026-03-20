using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace COLShared.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        private Dictionary<string, UIScreen> screens = new Dictionary<string, UIScreen>();
        public Canvas rootCanvas;
        public GameObject popupPrefab;
        public GameObject toastPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterScreens();
        }

        private void RegisterScreens()
        {
            var foundScreens = GetComponentsInChildren<UIScreen>(true);
            foreach (var screen in foundScreens)
            {
                screens[screen.GetType().Name] = screen;
            }
        }

        public void ShowScreen(string screenName)
        {
            if (screens.TryGetValue(screenName, out var screen))
                screen.Show();
        }

        public void HideScreen(string screenName)
        {
            if (screens.TryGetValue(screenName, out var screen))
                screen.Hide();
        }

        public void ShowPopup(string message, string confirmText, Action onConfirm)
        {
            if (popupPrefab == null || rootCanvas == null) return;
            var popupObj = Instantiate(popupPrefab, rootCanvas.transform);
            var popup = popupObj.GetComponent<PopupUI>();
            if (popup != null)
                popup.Setup(message, confirmText, onConfirm);
        }

        public void ShowToast(string message, float duration = 2f)
        {
            if (toastPrefab == null || rootCanvas == null) return;
            var toastObj = Instantiate(toastPrefab, rootCanvas.transform);
            var toast = toastObj.GetComponent<ToastUI>();
            if (toast != null)
                toast.Setup(message, duration);
        }
    }
}