using DG.Tweening;
using UnityEngine;

namespace RunnerGame
{
    /// <summary>
    /// Drives the runner: forward movement, lane switching, jumping, sliding,
    /// input (keyboard + touch swipes), score accumulation and the death pose.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private enum PlayerState
        {
            Grounded,
            Jumping,
            Sliding,
            Dead
        }

        public static PlayerController Instance { get; private set; }

        private const string ParamIsDead = "isDead";
        private const string LeanTweenId = "lean";

        [Header("Lanes")]
        [SerializeField] private float laneWidth = 2.5f;
        [SerializeField] private float laneChangeTime = 0.22f;
        [SerializeField] private float laneLeanDegrees = 10f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float maxMoveSpeed = 26f;
        [SerializeField] private float speedRampPerSecond = 0.4f;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.75f;
        [SerializeField] private float jumpUpTime = 0.42f;
        [SerializeField] private float jumpDownTime = 0.5f;

        [Header("Slide")]
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float slideHeight = 0.5f;

        [Header("Touch")]
        [SerializeField] private float swipeThreshold = 24f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private Transform _body;
        private int _currentLane = 1; // 0 = left, 1 = middle, 2 = right
        private float _moveSpeed;
        private PlayerState _state = PlayerState.Grounded;

        private bool _touchActive;
        private bool _touchConsumed;
        private Vector2 _touchStartPosition;

        public float CurrentSpeed => _moveSpeed;
        public bool IsGrounded => _state == PlayerState.Grounded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _moveSpeed = moveSpeed;

            // The visual model (and the collider) live on the "Body" child;
            // jump/slide tweens and collision all happen on that transform.
            _body = transform.Find("Body") != null ? transform.Find("Body") : (transform.childCount > 0 ? transform.GetChild(0) : transform);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            GameEvents.GameStarted -= OnGameStarted;
            GameEvents.PlayerDied -= PlayDeath;
        }

        private void OnEnable()
        {
            GameEvents.GameStarted += OnGameStarted;
            GameEvents.PlayerDied += PlayDeath;
        }

        private void Start()
        {
            // Start on the middle lane, exactly at x = 0.
            _currentLane = 1;
            if (_body != null)
            {
                Vector3 p = _body.position;
                _body.position = new Vector3(0f, p.y, p.z);
            }
        }

        private void OnGameStarted()
        {
            if (animator != null)
            {
                animator.CrossFade("Run", 0.25f, 0f);
            }
        }

        private void Update()
        {
            var game = GameManager.Instance;
            if (game == null || !game.IsRunning || _state == PlayerState.Dead)
            {
                return;
            }

            HandleInput();

            // Move forward and ramp the speed up slowly, with a cap.
            transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
            _moveSpeed = Mathf.Min(maxMoveSpeed, _moveSpeed + speedRampPerSecond * Time.deltaTime);

            // Score: one point per metre travelled.
            game.AddScore(_moveSpeed * Time.deltaTime);
        }

        // ---- Input ----

        private void HandleInput()
        {
            bool left = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
            bool right = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            bool jump = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);
            bool slide = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

            if (left)
            {
                MoveLane(-1);
            }
            else if (right)
            {
                MoveLane(1);
            }
            else if (jump)
            {
                Jump();
            }
            else if (slide)
            {
                Slide();
            }

            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (!Input.touchSupported || Input.touchCount == 0)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _touchActive = true;
                _touchConsumed = false;
                _touchStartPosition = touch.position;
                return;
            }

            if (!_touchActive || _touchConsumed)
            {
                return;
            }

            if (touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Ended)
            {
                return;
            }

            Vector2 delta = touch.position - _touchStartPosition;
            if (delta.sqrMagnitude < swipeThreshold * swipeThreshold)
            {
                return;
            }

            _touchConsumed = true;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                MoveLane(delta.x > 0f ? 1 : -1);
            }
            else if (delta.y > 0f)
            {
                Jump();
            }
            else
            {
                Slide();
            }
        }

        // ---- Actions ----

        private void MoveLane(int direction)
        {
            if (_state != PlayerState.Grounded)
            {
                return;
            }

            int targetLane = Mathf.Clamp(_currentLane + direction, 0, 2);
            if (targetLane == _currentLane)
            {
                return;
            }

            _currentLane = targetLane;
            float targetX = (_currentLane - 1) * laneWidth;

            _body.DOMoveX(targetX, laneChangeTime).SetEase(Ease.OutQuad);

            // Small lean into the lane change, easing back to straight.
            // NOTE: bundled DOTween 1.2.340 has no SetAs(id)/DOKill(id) — use
            // SetId and the static DOTween.Kill(target, id) instead.
            if (laneLeanDegrees > 0f)
            {
                DOTween.Kill(_body, LeanTweenId);
                _body.DOLocalRotate(new Vector3(0f, 0f, direction * laneLeanDegrees), 0.14f, RotateMode.Fast)
                    .SetEase(Ease.OutQuad)
                    .SetId(LeanTweenId)
                    .OnComplete(() =>
                    {
                        _body.DOLocalRotate(Vector3.zero, 0.2f, RotateMode.Fast)
                            .SetEase(Ease.OutBack)
                            .SetId(LeanTweenId);
                    });
            }
        }

        private void Jump()
        {
            if (_state != PlayerState.Grounded)
            {
                return;
            }

            _state = PlayerState.Jumping;
            AudioDirector.Instance?.PlayJump();

            float startY = _body.position.y;
            if (animator != null)
            {
                animator.CrossFade("Jump", 0.1f, 0f);
            }

            _body.DOMoveY(startY + jumpHeight, jumpUpTime).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _body.DOMoveY(startY, jumpDownTime).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    if (_state == PlayerState.Jumping)
                    {
                        _state = PlayerState.Grounded;
                        ParticleFX.Instance?.BurstDust(transform.position);
                    }
                });
            });
        }

        private void Slide()
        {
            if (_state != PlayerState.Grounded)
            {
                return;
            }

            _state = PlayerState.Sliding;
            AudioDirector.Instance?.PlaySlide();

            float startY = _body.position.y;
            if (animator != null)
            {
                animator.CrossFade("Slide", 0.1f, 0f);
            }

            var sequence = DOTween.Sequence();
            sequence.Append(_body.DOMoveY(startY - slideHeight, 0.15f).SetEase(Ease.OutQuad));
            sequence.AppendInterval(Mathf.Max(0.05f, slideDuration - 0.4f));
            sequence.Append(_body.DOMoveY(startY, 0.25f).SetEase(Ease.OutBack));
            sequence.OnComplete(() =>
            {
                if (_state == PlayerState.Sliding)
                {
                    _state = PlayerState.Grounded;
                }
            });
        }

        // ---- Death ----

        /// <summary>Plays the death pose. Triggered by the PlayerDied event.</summary>
        public void PlayDeath()
        {
            if (_state == PlayerState.Dead || _body == null)
            {
                return;
            }

            _state = PlayerState.Dead;
            _body.DOKill();

            // Sink the body slightly while the death animation plays.
            _body.DOMoveY(_body.position.y - 0.3f, 0.7f).SetEase(Ease.InQuad);

            if (animator != null)
            {
                // The "Character" controller exposes an "isDead" bool that any state
                // transitions out of with into the "Death" state.
                animator.SetBool(ParamIsDead, true);
            }
        }
    }
}
