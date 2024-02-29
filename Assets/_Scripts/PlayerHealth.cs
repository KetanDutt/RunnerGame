using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Image[] playerHealthImages;
    private int currentHealthIndex;

    private void Start()
    {
        currentHealthIndex = playerHealthImages.Length - 1;
    }

    public void Hurt()
    {
        if (currentHealthIndex >= 0)
        {
            playerHealthImages[currentHealthIndex].gameObject.SetActive(false);
            currentHealthIndex--;
            Camera.main.DOShakePosition(.2f, 1);

            if (currentHealthIndex < 0)
            {
                GameManager.instance.GameOver();
            }
        }
    }
}
