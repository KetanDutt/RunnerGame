using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunnerGame
{
    /// <summary>
    /// Persistent black overlay used for smooth scene transitions.
    /// Created on demand and kept across scene loads (DontDestroyOnLoad).
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        private const float FadeOutDuration = 0.35f;
        private const float FadeInDuration = 0.5f;

        [SerializeField] private float fadeInDuration = FadeInDuration;
        [SerializeField] private float fadeOutDuration = FadeOutDuration;

        private Image _image;
        private bool _busy;

        /// <summary>Returns the fader, creating it on first use.</summary>
        public static SceneFader Get()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject("SceneFader");
            DontDestroyOnLoad(go);
            return go.AddComponent<SceneFader>();
        }

        private void Awake()
        {
            Instance = this;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var imageGo = new GameObject("Fade");
            var imageRect = imageGo.AddComponent<RectTransform>();
            imageRect.SetParent(rect, false);
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            _image = imageGo.AddComponent<Image>();
            _image.color = new Color(0f, 0f, 0f, 0f);
            _image.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Fades to black, loads the given scene, then fades back in.</summary>
        public void FadeAndLoad(string sceneName)
        {
            if (_busy || _image == null)
            {
                return;
            }

            _busy = true;
            StartCoroutine(TransitionRoutine(sceneName));
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _image.DOKill();
            _image.DOFade(1f, fadeOutDuration).SetUpdate(true);

            // Wait for the fade-out to finish (manual wait: compatible with the
            // bundled DOTween 1.2.x which lacks AsyncWaitable).
            while (_image.color.a < 0.999f)
            {
                yield return null;
            }

            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            yield return new WaitForEndOfFrame();

            _image.DOKill();
            _image.DOFade(0f, fadeInDuration).SetUpdate(true);
            while (_image.color.a > 0.001f)
            {
                yield return null;
            }

            _busy = false;
        }
    }
}
