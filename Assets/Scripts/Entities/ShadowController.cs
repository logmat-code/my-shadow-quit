using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Puzzle;

namespace MyShadowQuit.Entities
{
    /// <summary>
    /// Controllable conscious Shadow entity.
    /// Can separate from the protagonist when the torch is lit, traverse shadow passages,
    /// activate shadow-only switches, and reconnect with the protagonist.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class ShadowController : MonoBehaviour
    {
        [Header("Shadow Movement")]
        [SerializeField] private float maxSpeed = 6.0f;
        [SerializeField] private float acceleration = 54f;
        [SerializeField] private float deceleration = 62f;
        [SerializeField] private float jumpForce = 11.2f;
        [SerializeField] private float coyoteTimeDuration = 0.13f;
        [SerializeField] private float jumpBufferDuration = 0.14f;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private SpriteRenderer _sr;

        private bool _isControlActive;
        private bool _isSeparated;
        private float _moveInput;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isGrounded;
        private int _facingDir = 1;

        private float _transitionTimer;
        private bool _isSeparatingAnim;
        private bool _isReconnectingAnim;
        private float _interactAnimTimer;

        private Sprite[] _idleSprites;
        private Sprite[] _moveSprites;
        private Sprite[] _jumpSprites;
        private Sprite[] _separateSprites;
        private Sprite[] _reconnectSprites;
        private Sprite[] _interactSprites;

        private float _animTimer;
        private int _animFrame;

        private PlayerController _protagonist;
        private TorchController _torch;

        public bool IsControlActive => _isControlActive;
        public bool IsSeparated => _isSeparated;
        public BoxCollider2D Collider => _col;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _sr = GetComponent<SpriteRenderer>();

            _rb.gravityScale = 2.4f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _col.size = new Vector2(0.44f, 0.80f);
            _col.offset = new Vector2(0f, -0.03f);
            _sr.sortingOrder = 11;

            LoadSprites();
        }

        private void LoadSprites()
        {
            _idleSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_idle", 4);
            _moveSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_move", 6);
            _jumpSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_jump", 2);
            _separateSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_separate", 4);
            _reconnectSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_reconnect", 4);
            _interactSprites = AssetRegistry.GetAnimationFrames("Characters/shadow_interact", 2);

            if (_idleSprites != null && _idleSprites.Length > 0)
            {
                _sr.sprite = _idleSprites[0];
            }
        }

        public void Initialize(PlayerController protagonist, TorchController torch, bool startSeparated = false)
        {
            _protagonist = protagonist;
            _torch = torch;
            _isSeparated = startSeparated;

            // Ignore physical collision between protagonist and shadow so they never block each other
            if (_protagonist != null)
            {
                Collider2D playerCol = _protagonist.GetComponent<Collider2D>();
                if (playerCol != null && _col != null)
                {
                    Physics2D.IgnoreCollision(_col, playerCol, true);
                }
            }

            if (startSeparated)
            {
                PlaySeparationEffect();
            }
        }

        public void SeparateFromProtagonist(Vector2 spawnOffset)
        {
            if (_isSeparated) return;
            _isSeparated = true;
            if (_protagonist != null)
            {
                transform.position = (Vector2)_protagonist.transform.position + spawnOffset;
            }
            PlaySeparationEffect();
        }

        public void PlaySeparationEffect()
        {
            _isSeparatingAnim = true;
            _isReconnectingAnim = false;
            _transitionTimer = 0.4f;
            _animFrame = 0;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("shadow_shift", 0.85f, 1.0f);
            }
        }

        public void ReconnectToProtagonist()
        {
            if (_protagonist == null) return;
            _isReconnectingAnim = true;
            _isSeparatingAnim = false;
            _transitionTimer = 0.35f;
            _animFrame = 0;
            transform.position = _protagonist.transform.position + new Vector3(-_protagonist.FacingDirection * 0.6f, 0f, 0f);
            _rb.linearVelocity = Vector2.zero;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("shadow_shift", 0.8f, 1.18f);
            }
        }

        public void SetControlActive(bool active)
        {
            if (active && !_isSeparated)
            {
                SeparateFromProtagonist(new Vector2(-0.75f, 0.1f));
            }
            _isControlActive = active;
            if (!active)
            {
                _moveInput = 0f;
                _jumpBufferTimer = 0f;
            }
        }

        private void Update()
        {
            if (_transitionTimer > 0f)
            {
                _transitionTimer -= Time.deltaTime;
                if (_transitionTimer <= 0f)
                {
                    _isSeparatingAnim = false;
                    _isReconnectingAnim = false;
                }
            }

            if (_interactAnimTimer > 0f) _interactAnimTimer -= Time.deltaTime;

            if (_isControlActive)
            {
                _moveInput = GameInput.GetHorizontal();
                if (Mathf.Abs(_moveInput) > 0.05f)
                {
                    _facingDir = _moveInput > 0f ? 1 : -1;
                }

                if (GameInput.GetJumpDown())
                {
                    _jumpBufferTimer = jumpBufferDuration;
                }

                if (GameInput.GetInteractDown())
                {
                    TriggerShadowInteract();
                }
            }
            else
            {
                _moveInput = 0f;

                // Subtle reactive lean away from torch light when idle
                if (_torch != null)
                {
                    float dx = transform.position.x - _torch.transform.position.x;
                    if (Mathf.Abs(dx) > 0.2f)
                    {
                        _facingDir = dx > 0f ? 1 : -1;
                    }
                }
            }

            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;

            UpdateVisualsAndAnimations();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (_isGrounded)
            {
                _coyoteTimer = coyoteTimeDuration;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            float targetSpeed = _moveInput * maxSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

            float newVelX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            float newVelY = _rb.linearVelocity.y;

            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                newVelY = jumpForce;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                _isGrounded = false;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("jump", 0.65f, 1.25f);
                }
            }

            if (newVelY < 0f)
            {
                newVelY += Physics2D.gravity.y * 0.55f * Time.fixedDeltaTime;
            }
            else if (newVelY > 0f && _isControlActive && !GameInput.GetJumpHeld())
            {
                newVelY += Physics2D.gravity.y * 0.85f * Time.fixedDeltaTime;
            }

            _rb.linearVelocity = new Vector2(newVelX, Mathf.Max(newVelY, -15f));
        }

        private void CheckGrounded()
        {
            Bounds b = _col.bounds;
            Vector2 boxCenter = new Vector2(b.center.x, b.min.y - 0.05f);
            Vector2 boxSize = new Vector2(b.size.x * 0.85f, 0.08f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
            _isGrounded = false;
            foreach (Collider2D hit in hits)
            {
                if (hit != null && hit != _col && !hit.isTrigger)
                {
                    // Ignore ShadowPassage colliders since shadow passes through them
                    if (hit.GetComponent<ShadowPassage>() != null) continue;
                    _isGrounded = true;
                    break;
                }
            }
        }

        private void TriggerShadowInteract()
        {
            _interactAnimTimer = 0.25f;

            // Find nearest ShadowSwitch within interaction range
            ShadowSwitch[] switches = FindObjectsByType<ShadowSwitch>(FindObjectsSortMode.None);
            foreach (ShadowSwitch sw in switches)
            {
                if (Vector2.Distance(transform.position, sw.transform.position) <= 1.45f)
                {
                    sw.ActivateByShadow(this);
                }
            }
        }

        private void UpdateVisualsAndAnimations()
        {
            _sr.flipX = (_facingDir < 0);

            // Visual pulse when controlled vs dormant
            if (_isControlActive)
            {
                float pulse = 0.92f + 0.08f * Mathf.Sin(Time.time * 6f);
                _sr.color = new Color(pulse, pulse, 1f, 0.98f);
            }
            else
            {
                _sr.color = new Color(0.75f, 0.7f, 0.9f, 0.82f);
            }

            Sprite[] activeSet;
            float frameDuration = 0.14f;

            if (_isSeparatingAnim && _separateSprites != null)
            {
                activeSet = _separateSprites;
                frameDuration = 0.09f;
            }
            else if (_isReconnectingAnim && _reconnectSprites != null)
            {
                activeSet = _reconnectSprites;
                frameDuration = 0.09f;
            }
            else if (_interactAnimTimer > 0f && _interactSprites != null)
            {
                activeSet = _interactSprites;
                frameDuration = 0.1f;
            }
            else if (!_isGrounded)
            {
                activeSet = _jumpSprites;
                frameDuration = 0.15f;
            }
            else if (Mathf.Abs(_rb.linearVelocity.x) > 0.35f)
            {
                activeSet = _moveSprites;
                frameDuration = 0.09f;
            }
            else
            {
                activeSet = _idleSprites;
                frameDuration = 0.18f;
            }

            if (activeSet == null || activeSet.Length == 0) return;

            _animTimer += Time.deltaTime;
            if (_animTimer >= frameDuration)
            {
                _animTimer = 0f;
                _animFrame = (_animFrame + 1) % activeSet.Length;
            }

            int idx = _animFrame % activeSet.Length;
            if (activeSet[idx] != null)
            {
                _sr.sprite = activeSet[idx];
            }
        }
    }
}
