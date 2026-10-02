using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RunnerGame
{
    /// <summary>
    /// Tracks the player's remaining health (the three heart icons), applies
    /// damage feedback (shake, flash, slow-mo, invulnerability frames) and
    /// ends the run when health runs out.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private Image[] playerHealthImages;
        [SerializeField] private Camera shakeCamera;

        [Header("Damage feedback")]
        [SerializeField] private float invulnerableDuration = 1.2f;
        [SerializeField] private float slowMoDuration = 0.3f;
        [SerializeField] private float slowMoTimeScale = 0.35f;
        [SerializeField] private float cameraShakeDuration = 0.25f;
        [SerializeField] private float cameraShakeStrength = 1.2f;

        private int _healthIndex;
        private float _invulnerableUntil;

        private void Start()
        {
            _healthIndex = playerHealthImages != null ? playerHealthImages.Length - 1 : 0;
            if (shakeCamera == null)
            {
                shakeCamera = Camera.main;
            }
        }

        public bool IsInvulnerable => Time.time < _invulnerableUntil;

        /// <summary>Applies one point of damage. Safe to call repeatedly.</summary>
        public void Hurt()
        {
            var game = GameManager.Instance;
            if (game == null || game.IsOver)
            {
                return;
            }

            // Invulnerability frames so a lingering overlap (or a double hit in the
            // same frame) cannot drain all remaining health at once.
            if (Time.time < _invulnerableUntil)
            {
                return;
            }

            _invulnerableUntil = Time.time + invulnerableDuration;

            if (playerHealthImages != null && _healthIndex >= 0 && _healthIndex < playerHealthImages.Length)
            {
                playerHealthImages[_healthIndex].gameObject.SetActive(false);
                _healthIndex--;
            }

            AudioDirector.Instance?.PlayHit();
            ScreenFX.Instance?.FlashHit();
            ShakeCamera();

            if (!game.IsPaused)
            {
                StartCoroutine(SlowMoRoutine());
            }

            if (_healthIndex < 0)
            {
                GameEvents.RaisePlayerDied();
                game.GameOver();
            }
            else
            {
                GameEvents.RaisePlayerHurt();
            }
        }

        private void ShakeCamera()
        {
            if (shakeCamera == null)
            {
                return;
            }

            shakeCamera.DOKill();
            // duration, strength, vibrato, sharpness (fadeOut defaults to true).
            shakeCamera.DOShakePosition(cameraShakeDuration, cameraShakeStrength, 15, 2f);
        }

        private IEnumerator SlowMoRoutine()
        {
            Time.timeScale = slowMoTimeScale;
            yield return new WaitForSecondsRealtime(slowMoDuration);
            Time.timeScale = 1f;
        }
    }
}
