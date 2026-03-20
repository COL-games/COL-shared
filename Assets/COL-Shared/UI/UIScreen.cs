using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace COLShared.UI
{
    public class UIScreen : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        public virtual void Show()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(Fade(0f, 1f, 0.2f));
        }
        public virtual void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(Fade(1f, 0f, 0.2f, () => gameObject.SetActive(false)));
        }
        protected IEnumerator Fade(float from, float to, float duration, System.Action onComplete = null)
        {
            float t = 0f;
            canvasGroup.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            canvasGroup.alpha = to;
            onComplete?.Invoke();
        }
    }
}