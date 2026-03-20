using System;
using UnityEngine;
using UnityEngine.UI;

namespace COLShared.UI
{
    public class PopupUI : MonoBehaviour
    {
        public Text messageText;
        public Button confirmButton;
        public Text confirmButtonText;
        private Action onConfirm;

        public void Setup(string message, string confirmText, Action onConfirm)
        {
            messageText.text = message;
            confirmButtonText.text = confirmText;
            this.onConfirm = onConfirm;
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnConfirmClicked()
        {
            onConfirm?.Invoke();
            Destroy(gameObject);
        }
    }
}