using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float laneWidth = 3f; // Width of each lane
    [SerializeField] private float moveSpeed = 5f; // Speed of player movement
    [SerializeField] private float jumpHeight = 1.5f; // Height of the jump
    [SerializeField] private float slideDuration = 0.5f; // Duration of the slide animation
    [SerializeField] private float slideHeight = 0.5f; // Duration of the slide animation
    [SerializeField] private Animator animator; // Reference to the Animator component

    private int currentLane = 1; // Current lane (0 for left, 1 for middle, 2 for right)
    private bool isGrounded = true; // Flag to check if the player is grounded

    void Start()
    {
        GameManager.onGameStart += onGameStart;
    }

    private void OnDestroy()
    {
        GameManager.onGameStart -= onGameStart;
    }

    void onGameStart()
    {
        animator.Play("Run");
    }

    void Update()
    {
        if (!GameManager.instance.isRunning())
        {
            if (GameManager.instance.isPaused)
            {
                animator.StartPlayback();
            }
            return;
        }
        animator.StopPlayback();

        GameManager.instance.PlayerScore += (moveSpeed / 4) * Time.deltaTime;
        GameManager.instance.PlayerTime += Time.deltaTime;

        // Check for input to move left or right
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(-1); // Move left
            animator.CrossFade("Left", 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(1); // Move right
            animator.CrossFade("Right", 0, 0);
        }

        // Check for input to jump
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
            animator.CrossFade("Jump", 0, 0);
        }

        // Check for input to slide
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Slide();
            animator.CrossFade("Slide", 0, 0);
        }

        // Move the player forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        moveSpeed += Time.deltaTime / 10;
    }

    // Method to move the player left or right between lanes
    void MoveLane(int direction)
    {
        int targetLane = currentLane + direction;
        // Clamp target lane between 0 and 2
        targetLane = Mathf.Clamp(targetLane, 0, 2);
        // Calculate the target position
        float targetX = (targetLane - 1) * laneWidth; // Assuming the middle lane is at x = 0

        // Move the player to the target position using DOTween for smoothness
        transform.GetChild(0).DOMoveX(targetX, 0.25f).SetEase(Ease.OutQuad);

        // Update the current lane
        currentLane = targetLane;
    }

    // Method to make the player jump
    void Jump()
    {
        // Perform a jump animation (you can replace this with your own jump animation)
        transform.GetChild(0).DOMoveY(transform.position.y + jumpHeight, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.GetChild(0).DOMoveY(transform.position.y, 0.5f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                isGrounded = true;
            });
        });
        // Set isGrounded to false to prevent double jumping
        isGrounded = false;
    }

    // Method to make the player slide
    void Slide()
    {
        // Perform a slide animation (you can replace this with your own slide animation)
        transform.GetChild(0).DOMoveY(transform.position.y - slideHeight, slideDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            transform.GetChild(0).DOMoveY(transform.position.y, slideDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                isGrounded = true;
            });
        });
        // Set isGrounded to false to prevent actions during sliding
        isGrounded = false;
    }
}
