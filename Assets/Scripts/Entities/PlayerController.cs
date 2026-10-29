using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Puzzle;
using MyShadowQuit.Cosmetics;
using MyShadowQuit.Combat;

namespace MyShadowQuit.Entities
{
    /// <summary>
    /// Responsive 2D platformer controller for the Protagonist.
    /// Features smooth acceleration/deceleration, coyote time, jump buffering,
    /// torch carrying/placing, and custom pixel-art frame animation.
    /// Implements IDamageable by delegating damage to the shared HealthSystem.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 5.4f;
        [SerializeField] private float acceleration = 48f;
        [SerializeField] private float deceleration = 56f;
        [SerializeField] private float airControlMultiplier = 0.82f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 10.6f;
        [SerializeField] private float coyoteTimeDuration = 0.12f;
        [SerializeField] private float jumpBufferDuration = 0.14f;
        [SerializeField] private float fallGravityMultiplier = 1.65f;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private SpriteRenderer _sr;

        private bool _isControlActive = true;
        private float _moveInput;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _onMovingSurface;
        private Vector2 _surfaceVelocity;
        private float _airCarryX;
        private float _landAnimTimer;
        private float _interactAnimTimer;
        private float _footstepTimer;
        private int _facingDir = 1;

        private TorchController _carriedTorch;

        // Sprites
        private Sprite[] _idleSprites;
        private Sprite[] _runSprites;
        private Sprite[] _torchIdleSprites;
        private Sprite[] _torchRunSprites;
        private Sprite[] _jumpSprites;
        private Sprite[] _fallSprites;
        private Sprite[] _landSprites;
        private Sprite[] _interactSprites;

        private float _animTimer;
        private int _animFrame;

        public bool IsControlActive => _isControlActive;
        public bool IsGrounded => _isGrounded;
        public bool HasTorch => _carriedTorch != null && _carriedTorch.State == TorchState.CarriedByPlayer;
        public TorchController CarriedTorch => _carriedTorch;
        public int FacingDirection => _facingDir;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            gameObject.tag = "Player";
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();

            if (_rb != null)
            {
                _rb.gravityScale = 2.6f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (_col != null)
            {
                _col.size = new Vector2(0.46f, 0.82f);
                _col.offset = new Vector2(0f, -0.03f);
            }

            if (_sr != null)
            {
                _sr.sortingOrder = 10;
            }

            if (GetComponent<PlayerCombatController>() == null)
            {
                gameObject.AddComponent<PlayerCombatController>();
            }

            if (_idleSprites == null || _idleSprites.Length == 0)
            {
                string equipped = SkinEquipmentService.Instance != null
                    ? SkinEquipmentService.Instance.EquippedPlayerSkinId
                    : PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedPlayer, "player_original");
                ApplySkin(equipped);
            }
        }

        private string _activeSkinId = "player_original";
        public string ActiveSkinId => _activeSkinId;

        public void ApplySkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) skinId = "player_original";
            _activeSkinId = skinId;

            bool isOriginal = (skinId == "player_original");
            string prefix = isOriginal ? "Characters/protagonist" : $"Skins/Player/{skinId}";
            string sep = isOriginal ? "_" : "/";

            _idleSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "idle", 4);
            _runSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "run", 6);
            _torchIdleSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "torch_idle", 4);
            _torchRunSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "torch_run", 6);
            _jumpSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "jump", 2);
            _fallSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "fall", 2);
            _landSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "land", 2);
            _interactSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "interact", 2);

            if (_idleSprites != null && _idleSprites.Length > 0 && _sr != null)
            {
                _sr.sprite = _idleSprites[0];
            }
        }

        public void SetControlActive(bool active)
        {
            _isControlActive = active;
            if (!active)
            {
                _moveInput = 0f;
                _jumpBufferTimer = 0f;
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            if (_landAnimTimer > 0f) _landAnimTimer -= Time.deltaTime;
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

                // Dedicated Torch Action: pick up, drop, or place in pedestal (KeyCode.Q / Mobile Torch Button)
                if (GameInput.GetTorchActionDown())
                {
                    HandleTorchAction();
                }

                // Aim torch with mouse or facing direction
                if (HasTorch && Camera.main != null)
                {
                    Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(GameInput.GetMouseScreenPosition());
                    Vector2 toMouse = mouseWorld - (Vector2)transform.position;
                    if (toMouse.sqrMagnitude > 0.25f)
                    {
                        _carriedTorch.SetAimDirection(toMouse);
                    }
                    else
                    {
                        _carriedTorch.SetAimDirection(new Vector2(_facingDir, 0.2f));
                    }
                }
            }
            else
            {
                _moveInput = 0f;
            }

            if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;

            UpdateAnimations();
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

            // Horizontal movement with smooth acceleration & deceleration.
            // Movement is applied relative to the supporting surface (platform velocity while grounded,
            // decaying inherited platform velocity in the air) so riders are carried without drift.
            float targetSpeed = _moveInput * maxSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            if (!_isGrounded) accelRate *= airControlMultiplier;

            float dt = Time.fixedDeltaTime;
            float refVelX;
            float nextRefVelX;
            if (_isGrounded)
            {
                refVelX = _surfaceVelocity.x;
                nextRefVelX = refVelX;
                _airCarryX = refVelX;
            }
            else
            {
                refVelX = _airCarryX;
                _airCarryX = Mathf.MoveTowards(_airCarryX, 0f, 4f * dt);
                nextRefVelX = _airCarryX;
            }

            float newVelX = MovingSurfaceUtil.StepHorizontal(_rb.linearVelocity.x, refVelX, nextRefVelX, targetSpeed, accelRate, dt);
            float newVelY = _rb.linearVelocity.y;

            // Jump execution via coyote time + jump buffer
            bool jumpedThisStep = false;
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                newVelY = jumpForce;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                _isGrounded = false;
                jumpedThisStep = true;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("jump", 0.75f, Random.Range(0.95f, 1.05f));
                }
            }

            // Stay glued to a vertically moving surface (never when jumping / rising away from it)
            bool snapToSurface = _isGrounded && !jumpedThisStep && _onMovingSurface
                                 && _rb.linearVelocity.y <= Mathf.Max(_surfaceVelocity.y, 0f) + 0.5f;

            // Variable jump height & crisp fall gravity
            if (!snapToSurface)
            {
                if (newVelY < 0f)
                {
                    newVelY += Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
                }
                else if (newVelY > 0f && _isControlActive && !GameInput.GetJumpHeld())
                {
                    newVelY += Physics2D.gravity.y * 0.9f * Time.fixedDeltaTime;
                }
            }
            else
            {
                newVelY = _surfaceVelocity.y;
            }

            _rb.linearVelocity = new Vector2(newVelX, Mathf.Max(newVelY, -16f));

            // Footstep SFX
            if (_isGrounded && Mathf.Abs(newVelX) > 1.2f)
            {
                _footstepTimer -= Time.fixedDeltaTime;
                if (_footstepTimer <= 0f)
                {
                    _footstepTimer = 0.32f;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("footstep", 0.45f, Random.Range(0.9f, 1.12f));
                    }
                }
            }
        }

        private void CheckGrounded()
        {
            _wasGrounded = _isGrounded;
            Bounds b = _col.bounds;
            Vector2 boxCenter = new Vector2(b.center.x, b.min.y - 0.05f);
            Vector2 boxSize = new Vector2(b.size.x * 0.85f, 0.08f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
            _isGrounded = false;
            _onMovingSurface = false;
            _surfaceVelocity = Vector2.zero;
            foreach (Collider2D hit in hits)
            {
                if (hit != null && hit != _col && !hit.isTrigger)
                {
                    // Check if it's a solid barrier or ground
                    _isGrounded = true;
                    if (!_onMovingSurface && MovingSurfaceUtil.TryGetSurfaceVelocity(hit, out Vector2 surfVel))
                    {
                        _onMovingSurface = true;
                        _surfaceVelocity = surfVel;
                    }
                }
            }

            if (_isGrounded && !_wasGrounded && _rb.linearVelocity.y <= 0.1f)
            {
                _landAnimTimer = 0.12f;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("land", 0.65f, Random.Range(0.94f, 1.06f));
                }
            }
        }

        private void HandleTorchAction()
        {
            _interactAnimTimer = 0.2f;

            // 1. Check nearby TorchPedestal first
            TorchPedestal nearestPedestal = FindNearest<TorchPedestal>(1.4f);

            if (HasTorch)
            {
                if (nearestPedestal != null && !nearestPedestal.HasTorch)
                {
                    _carriedTorch.PlaceInPedestal(nearestPedestal);
                    _carriedTorch = null;
                    return;
                }

                // Drop torch on ground at feet
                Vector2 dropPos = (Vector2)transform.position + new Vector2(_facingDir * 0.45f, -0.1f);
                _carriedTorch.DropOnGround(dropPos);
                _carriedTorch = null;
                return;
            }
            else
            {
                // Try picking up torch from world or pedestal
                TorchController torch = FindNearest<TorchController>(1.5f);
                if (torch != null)
                {
                    _carriedTorch = torch;
                    _carriedTorch.PickUp(transform);
                    return;
                }
            }
        }

        public void ForceEquipTorch(TorchController torch)
        {
            if (torch != null)
            {
                _carriedTorch = torch;
                _carriedTorch.PickUp(transform);
            }
        }

        private T FindNearest<T>(float maxDist) where T : Component
        {
            T[] all = FindObjectsByType<T>(FindObjectsSortMode.None);
            T best = null;
            float bestDist = maxDist;
            foreach (T item in all)
            {
                float d = Vector2.Distance(transform.position, item.transform.position);
                if (d <= bestDist)
                {
                    bestDist = d;
                    best = item;
                }
            }
            return best;
        }

        private void UpdateAnimations()
        {
            _sr.flipX = (_facingDir < 0);

            Sprite[] activeSet;
            float frameDuration = 0.14f;

            if (_interactAnimTimer > 0f && _interactSprites != null && _interactSprites.Length > 0)
            {
                activeSet = _interactSprites;
                frameDuration = 0.1f;
            }
            else if (!_isGrounded)
            {
                activeSet = (_rb.linearVelocity.y > 0.2f) ? _jumpSprites : _fallSprites;
                frameDuration = 0.15f;
            }
            else if (_landAnimTimer > 0f && _landSprites != null && _landSprites.Length > 0)
            {
                activeSet = _landSprites;
                frameDuration = 0.08f;
            }
            else if (Mathf.Abs(_rb.linearVelocity.x) > 0.35f)
            {
                activeSet = HasTorch ? _torchRunSprites : _runSprites;
                frameDuration = 0.09f;
            }
            else
            {
                activeSet = HasTorch ? _torchIdleSprites : _idleSprites;
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

        #region IDamageable Implementation
        public bool IsDead => HealthSystem.Instance != null && HealthSystem.Instance.IsDead;

        public bool TakeDamage(int damage, Vector2 knockbackForce, float invulnDuration = 0.85f)
        {
            if (HealthSystem.Instance != null)
            {
                return HealthSystem.Instance.TakeDamage(damage, knockbackForce, invulnDuration);
            }
            return false;
        }

        public void KillInstantly()
        {
            if (HealthSystem.Instance != null)
            {
                HealthSystem.Instance.KillInstantly();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }
        #endregion
    }
}
