using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace COLShared.UI
{
    public class ToastUI : MonoBehaviour
    {
        public Text messageText;
        public float fadeDuration = 0.3f;
        private float showDuration = 2f;

        public void Setup(string message, float duration)
        {
            messageText.text = message;
            showDuration = duration;
            StartCoroutine(ShowAndFade());
        }

        private IEnumerator ShowAndFade()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            cg.alpha = 1f;
            yield return new WaitForSecondsRealtime(showDuration);
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}