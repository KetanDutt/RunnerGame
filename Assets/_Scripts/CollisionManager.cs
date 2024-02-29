using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    int i = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Handle collision with obstacles
            Debug.Log("Player collided with obstacle!  " + i);
            i++;
            // You can add any code here to handle obstacle collision, like decreasing health or triggering an animation.
        }
        else if (other.CompareTag("Coin"))
        {
            // Handle collision with coins
            Debug.Log("Player collected a coin!");
            // You can add any code here to handle coin collection, like increasing score or triggering a sound effect.
            // Destroy(other.gameObject); // Destroy the coin object upon collection
            other.gameObject.SetActive(false);
            GameManager.instance.CoinCollected();
        }
    }
}
