using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunnerGame
{
    /// <summary>
    /// Main menu: animated title, best score display and Play/Quit buttons.
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private float titleFloatAmplitude = 8f;
        [SerializeField] private float titleFloatDuration = 1.4f;

        private void Start()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(StartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }

            if (bestScoreText != null)
            {
                int highScore = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
                if (highScore > 0)
                {
                    bestScoreText.text = "Best : " + highScore;
                }
                else
                {
                    bestScoreText.gameObject.SetActive(false);
                }
            }

            var title = FindDeep(transform, "Title");
            if (title != null)
            {
                var anchor = title.anchoredPosition;
                title.DOLocalMove(anchor + Vector3.up * titleFloatAmplitude, titleFloatDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true)
                    .SetDelay(0.3f);
            }

            FadeInMenu();
        }

        private void OnDestroy()
        {
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(StartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }

        private void FadeInMenu()
        {
            // Buttons pop in with a slight delay for a snappier feel.
            foreach (var button in new[] { playButton, quitButton })
            {
                if (button == null)
                {
                    continue;
                }

                var rect = button.rectTransform;
                rect.DOKill();
                rect.localScale = Vector3.one * 0.85f;
                rect.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true).SetDelay(0.25f);
            }
        }

        private void StartGame()
        {
            AudioDirector.Instance?.PlayClick();
            SceneFader.Get().FadeAndLoad(SceneNames.Gameplay);
        }

        private void QuitGame()
        {
            AudioDirector.Instance?.PlayClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
