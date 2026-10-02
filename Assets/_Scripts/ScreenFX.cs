using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RunnerGame
{
    /// <summary>
    /// Screen-space post effect layer: a soft vignette and a damage flash.
    /// The overlay is built at runtime on the Canvas this component is attached
    /// to, so no extra scene objects are required.
    /// </summary>
    public class ScreenFX : MonoBehaviour
    {
        public static ScreenFX Instance { get; private set; }

        [Header("Vignette")]
        [SerializeField] [Range(0f, 1f)] private float vignetteStrength = 0.45f;

        [Header("Hit flash")]
        [SerializeField] private Color hitFlashColor = new Color(0.9f, 0.15f, 0.12f);
        [SerializeField] private float hitFlashAlpha = 0.45f;
        [SerializeField] private float hitFlashDuration = 0.4f;

        private Image _vignette;
        private Image _flash;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;
            BuildOverlay();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void BuildOverlay()
        {
            var overlay = new GameObject("FX_Overlay");
            var rect = overlay.AddComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetSiblingIndex(int.MaxValue);

            _vignette = CreateImage(rect, "Vignette", CreateRadialTexture());
            _vignette.color = new Color(1f, 1f, 1f, vignetteStrength);

            _flash = CreateImage(rect, "HitFlash", null);
            _flash.color = new Color(hitFlashColor.r, hitFlashColor.g, hitFlashColor.b, 0f);
        }

        private static Image CreateImage(RectTransform parent, string name, Texture2D texture)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static Texture2D CreateRadialTexture()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x / (float)size) * 2f - 1f;
                    float dy = (y / (float)size) * 2f - 1f;
                    float distance = Mathf.Clamp01((dx * dx + dy * dy) * 0.7f);
                    // Fully clear in the middle, dark towards the edges.
                    pixels[y * size + x] = new Color(0f, 0f, 0f, Mathf.Pow(distance, 1.6f));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false);
            return texture;
        }

        /// <summary>Red flash shown when the player takes damage.</summary>
        public void FlashHit()
        {
            if (_flash == null)
            {
                return;
            }

            _flash.DOKill();
            _flash.color = new Color(hitFlashColor.r, hitFlashColor.g, hitFlashColor.b, hitFlashAlpha);
            _flash.DOFade(0f, hitFlashDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        /// <summary>Generic coloured flash.</summary>
        public void Flash(Color color, float alpha, float duration)
        {
            if (_flash == null)
            {
                return;
            }

            _flash.DOKill();
            _flash.color = new Color(color.r, color.g, color.b, alpha);
            _flash.DOFade(0f, duration).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }
}
