using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Sits on the player's "Body" (which carries the trigger-compatible collider)
    /// and reacts to obstacle and coin triggers.
    /// </summary>
    public class CollisionManager : MonoBehaviour
    {
        private const string TagObstacle = "Obstacle";
        private const string TagCoin = "Coin";

        [SerializeField] private PlayerHealth playerHealth;

        private void OnTriggerEnter(Collider other)
        {
            if (!GameManager.IsGameRunning())
            {
                return;
            }

            if (other.CompareTag(TagObstacle))
            {
                if (playerHealth != null)
                {
                    playerHealth.Hurt();
                }
            }
            else if (other.CompareTag(TagCoin))
            {
                other.gameObject.SetActive(false);
                GameManager.Instance.CollectCoin(other.transform.position);
            }
        }
    }
}
