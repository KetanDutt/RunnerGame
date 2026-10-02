using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Spins the attached object (used by coins). Respects the game state so
    /// it stops during the countdown, pause and game over.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        public Vector3 speed;

        private void Start()
        {
            transform.localRotation *= Quaternion.Euler(speed * Random.Range(0f, 180f));
        }

        private void Update()
        {
            if (!GameManager.IsGameRunning())
            {
                return;
            }

            transform.localRotation *= Quaternion.Euler(speed * Time.deltaTime);
        }
    }
}
