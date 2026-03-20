using UnityEngine;
using UnityEngine.UI;
using COLShared.Currency;

namespace COLShared.UI
{
    public class HUDScreen : UIScreen
    {
        public Text gemsText;
        public Text coinsText;

        protected override void Awake()
        {
            base.Awake();
            UpdateCurrency();
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrency;
        }
        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrency;
        }
        private void UpdateCurrency()
        {
            if (gemsText != null)
                gemsText.text = CurrencyManager.Instance.CurrentGems.ToString();
            if (coinsText != null)
                coinsText.text = CurrencyManager.Instance.CurrentCoins.ToString();
        }
    }
}