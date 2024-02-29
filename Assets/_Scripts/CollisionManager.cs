using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    int i = 0;
    [SerializeField] private PlayerHealth playerHealth;

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.instance.isRunning())
            return;
        if (other.CompareTag("Obstacle"))
        {
            // Handle collision with obstacles
            Debug.Log("Player collided with obstacle!  " + i);
            i++;
            playerHealth.Hurt();
            // You can add any code here to handle obstacle collision, like decreasing health or triggering an animation.
        }
        else if (other.CompareTag("Coin"))
        {
            // Handle collision with coins
            other.gameObject.SetActive(false);
            GameManager.instance.CoinCollected();
        }
    }
}
