using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Puzzle;
using MyShadowQuit.Cosmetics;

namespace MyShadowQuit.Entities
{
    /// <summary>
    /// Controllable conscious Shadow entity.
    /// Features supernatural reduced gravity (~50% of Protagonist), responsive air control,
    /// and a 2-jump state machine (Ground Jump + Mid-Air Double Jump with violet pulse & dark particles).
    /// Can separate from the protagonist, traverse shadow passages, activate shadow-only switches,
    /// and reconnect with the protagonist.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class ShadowController : MonoBehaviour
    {
        public const float DefaultGravityScale = 1.3f; // 50% of Protagonist's 2.6f gravityScale
        public const int MaxJumps = 2;

        [Header("Shadow Movement")]
        [SerializeField] private float maxSpeed = 6.0f;
        [SerializeField] private float acceleration = 54f;
        [SerializeField] private float deceleration = 62f;
        [SerializeField] private float airControlMultiplier = 1.15f;

        [Header("Shadow Jump & Reduced Gravity")]
        [SerializeField] private float shadowGravityScale = DefaultGravityScale;
        [SerializeField] private float jumpForce = 7.8f;
        [SerializeField] private float doubleJumpForce = 7.4f;
        [SerializeField] private float coyoteTimeDuration = 0.13f;
        [SerializeField] private float jumpBufferDuration = 0.14f;
        [SerializeField] private float fallGravityMultiplier = 1.35f;

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private SpriteRenderer _sr;

        private bool _isControlActive;
        private bool _isSeparated;
        private float _moveInput;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool _jumpPressedThisStep;
        private bool _isGrounded;
        private int _jumpsRemaining = MaxJumps;
        private int _facingDir = 1;

        private float _transitionTimer;
        private bool _isSeparatingAnim;
        private bool _isReconnectingAnim;
        private float _interactAnimTimer;

        // Second jump visual feedback (violet pulse, brief squash/stretch, dark particles)
        private Vector3 _baseScale = Vector3.one;
        private float _doubleJumpPulseTimer;
        private SpriteRenderer _doubleJumpPulseRenderer;
        private ParticleSystem _doubleJumpParticles;

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
        public bool IsGrounded => _isGrounded;
        public int JumpsRemaining => _jumpsRemaining;
        public float GravityScale => shadowGravityScale;
        public float FirstJumpForce => jumpForce;
        public float SecondJumpForce => doubleJumpForce;
        public BoxCollider2D Collider => _col != null ? _col : (_col = GetComponent<BoxCollider2D>());

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();

            if (_rb != null)
            {
                _rb.gravityScale = shadowGravityScale;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (_col != null)
            {
                _col.size = new Vector2(0.44f, 0.80f);
                _col.offset = new Vector2(0f, -0.03f);
            }

            if (_sr != null)
            {
                _sr.sortingOrder = 11;
            }

            _baseScale = transform.localScale;

            if (_idleSprites == null || _idleSprites.Length == 0)
            {
                string equipped = SkinEquipmentService.Instance != null
                    ? SkinEquipmentService.Instance.EquippedShadowSkinId
                    : PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedShadow, "shadow_original");
                ApplySkin(equipped);
            }

            if (_doubleJumpPulseRenderer == null || _doubleJumpParticles == null)
            {
                CreateDoubleJumpEffects();
            }
        }

        private string _activeSkinId = "shadow_original";
        public string ActiveSkinId => _activeSkinId;

        public void ApplySkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) skinId = "shadow_original";
            _activeSkinId = skinId;

            bool isOriginal = (skinId == "shadow_original");
            string prefix = isOriginal ? "Characters/shadow" : $"Skins/Shadow/{skinId}";
            string sep = isOriginal ? "_" : "/";

            _idleSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "idle", 4);
            _moveSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "move", 6);
            _jumpSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "jump", 2);
            _separateSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "separate", 4);
            _reconnectSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "reconnect", 4);
            _interactSprites = AssetRegistry.GetAnimationFrames(prefix + sep + "interact", 2);

            if (_idleSprites != null && _idleSprites.Length > 0 && _sr != null)
            {
                _sr.sprite = _idleSprites[0];
            }

            // Tint double-jump pulse effect to match the shadow's skin palette
            SkinDefinition skinDef = SkinCatalog.GetSkin(skinId);
            if (skinDef != null && _doubleJumpPulseRenderer != null)
            {
                _doubleJumpPulseRenderer.color = new Color(skinDef.ThemeColor.r, skinDef.ThemeColor.g, skinDef.ThemeColor.b, 0.72f);
            }
        }

        private void CreateDoubleJumpEffects()
        {
            if (_doubleJumpPulseRenderer == null)
            {
                // 1. Subtle violet ring/pulse child sprite using existing light_halo asset tinted violet
                GameObject pulseObj = new GameObject("ShadowDoubleJumpPulse");
                pulseObj.transform.SetParent(transform, false);
                pulseObj.transform.localPosition = new Vector3(0f, -0.28f, 0f);
                _doubleJumpPulseRenderer = pulseObj.AddComponent<SpriteRenderer>();
                _doubleJumpPulseRenderer.sprite = AssetRegistry.GetSprite("Torch/light_halo", 32f, FilterMode.Bilinear);
                _doubleJumpPulseRenderer.sortingOrder = 10;
                _doubleJumpPulseRenderer.color = new Color(0.68f, 0.35f, 1f, 0f);
                pulseObj.transform.localScale = new Vector3(0.35f, 0.16f, 1f);
                pulseObj.SetActive(false);
            }

            if (_doubleJumpParticles == null)
            {
                // 2. Discrete dark-violet particle burst on second jump using existing ember_particle sprite
                GameObject partObj = new GameObject("ShadowDoubleJumpParticles");
                partObj.transform.SetParent(transform, false);
                partObj.transform.localPosition = new Vector3(0f, -0.25f, 0f);

                _doubleJumpParticles = partObj.AddComponent<ParticleSystem>();
                var main = _doubleJumpParticles.main;
                main.playOnAwake = false;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.38f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.1f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.13f);
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(0.14f, 0.08f, 0.24f, 0.92f),
                    new Color(0.65f, 0.36f, 0.95f, 0.88f)
                );
                main.gravityModifier = 0.2f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 16;

                var emission = _doubleJumpParticles.emission;
                emission.enabled = false;

                var shape = _doubleJumpParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.16f;

                var psRenderer = partObj.GetComponent<ParticleSystemRenderer>();
                psRenderer.sortingOrder = 12;
                Sprite partSprite = AssetRegistry.GetSprite("Torch/ember_particle", 32f);
                if (partSprite != null)
                {
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.mainTexture = partSprite.texture;
                    psRenderer.material = mat;
                }
            }
        }

        public void Initialize(PlayerController protagonist, TorchController torch, bool startSeparated = false)
        {
            EnsureInitialized();
            _protagonist = protagonist;
            _torch = torch;
            _isSeparated = startSeparated;

            // Ignore physical collision between protagonist and shadow so they never block each other
            if (_protagonist != null)
            {
                _protagonist.EnsureInitialized();
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

        public void StepEndingApproach(Vector2 targetPos, float speed)
        {
            EnsureInitialized();
            _isControlActive = false;
            float dx = targetPos.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                _facingDir = dx > 0f ? 1 : -1;
            }
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            if (_rb != null)
            {
                _rb.linearVelocity = new Vector2(Mathf.Sign(dx) * Mathf.Min(Mathf.Abs(dx) * 4f, speed), 0f);
            }
        }

        public void ReconnectToProtagonist()
        {
            EnsureInitialized();
            if (_protagonist == null) return;
            _isControlActive = false;
            _isSeparated = false;
            _isReconnectingAnim = true;
            _isSeparatingAnim = false;
            _transitionTimer = 0.65f;
            _animFrame = 0;
            _facingDir = _protagonist.FacingDirection;
            transform.position = _protagonist.transform.position + new Vector3(-_protagonist.FacingDirection * 0.48f, 0f, 0f);
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            if (_doubleJumpParticles != null)
            {
                _doubleJumpParticles.Emit(14);
            }

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
                _jumpPressedThisStep = false;
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

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

                // Require a distinct SPACE press for each jump so holding SPACE never consumes both jumps
                if (GameInput.GetJumpDown())
                {
                    _jumpBufferTimer = jumpBufferDuration;
                    _jumpPressedThisStep = true;
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

            UpdateDoubleJumpFeedback();
            UpdateVisualsAndAnimations();
        }

        private void FixedUpdate()
        {
            if (_rb != null && Mathf.Abs(_rb.gravityScale - shadowGravityScale) > 0.01f)
            {
                _rb.gravityScale = shadowGravityScale;
            }

            CheckGrounded();

            if (_isGrounded)
            {
                _coyoteTimer = coyoteTimeDuration;
                // Restore full 2 jumps only when landing on valid floor/platform (never when rising through a platform)
                if (_rb.linearVelocity.y <= 0.1f)
                {
                    _jumpsRemaining = MaxJumps;
                }
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }

            float targetSpeed = _moveInput * maxSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            if (!_isGrounded)
            {
                // Enhanced responsive mid-air horizontal control without raising maxSpeed
                accelRate *= airControlMultiplier;
            }

            float newVelX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            float newVelY = _rb.linearVelocity.y;

            // 1. First Jump: From ground or within Coyote Time window
            if (_jumpBufferTimer > 0f && (_coyoteTimer > 0f || (_isGrounded && _jumpsRemaining == MaxJumps)))
            {
                ExecuteFirstJump(ref newVelY);
            }
            // 2. Second Jump (Mid-Air Double Jump): Requires a fresh SPACE press while airborne with at least 1 jump left
            else if (_jumpPressedThisStep && !_isGrounded && _coyoteTimer <= 0f && _jumpsRemaining > 0)
            {
                // If the player walked off a ledge without jumping first, walking off consumes the ground jump slot
                // so they have exactly 1 mid-air jump (never 2 mid-air jumps after falling off a ledge)
                ExecuteSecondJump(ref newVelY);
            }

            _jumpPressedThisStep = false;

            // Smooth low-gravity apex and controlled descent
            if (newVelY < 0f)
            {
                newVelY += Physics2D.gravity.y * shadowGravityScale * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }
            else if (newVelY > 0f && _isControlActive && !GameInput.GetJumpHeld())
            {
                newVelY += Physics2D.gravity.y * shadowGravityScale * 0.65f * Time.fixedDeltaTime;
            }

            _rb.linearVelocity = new Vector2(newVelX, Mathf.Max(newVelY, -11.5f));
        }

        public void SetGroundedStateForTest(bool grounded)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();
            _isGrounded = grounded;
            if (grounded)
            {
                _coyoteTimer = coyoteTimeDuration;
                _jumpsRemaining = MaxJumps;
                if (_rb != null)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                }
            }
            else
            {
                _coyoteTimer = 0f;
            }
        }

        public bool TryPerformJumpForTest(bool isNewPress = true)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            float velY = _rb.linearVelocity.y;
            if (_isGrounded)
            {
                _jumpsRemaining = MaxJumps;
                ExecuteFirstJump(ref velY);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, velY);
                return true;
            }

            if (isNewPress && !_isGrounded && _jumpsRemaining > 0)
            {
                ExecuteSecondJump(ref velY);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, velY);
                return true;
            }

            return false;
        }

        private void ExecuteFirstJump(ref float newVelY)
        {
            newVelY = jumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _isGrounded = false;
            _jumpsRemaining = MaxJumps - 1; // 1 jump remaining for mid-air double jump

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("jump", 0.65f, 1.22f);
            }
        }

        private void ExecuteSecondJump(ref float newVelY)
        {
            // Resets vertical velocity (works cleanly both while ascending and while falling!)
            newVelY = doubleJumpForce;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _isGrounded = false;
            _jumpsRemaining = 0; // Exhaust all jumps until landing on valid ground

            TriggerDoubleJumpFeedback();
        }

        private void TriggerDoubleJumpFeedback()
        {
            _doubleJumpPulseTimer = 0.26f;

            // Brief upward stretch / deformation
            transform.localScale = new Vector3(_baseScale.x * 0.84f, _baseScale.y * 1.18f, _baseScale.z);

            if (_doubleJumpPulseRenderer != null)
            {
                _doubleJumpPulseRenderer.gameObject.SetActive(true);
                _doubleJumpPulseRenderer.transform.localScale = new Vector3(0.28f, 0.12f, 1f);
                _doubleJumpPulseRenderer.color = new Color(0.75f, 0.42f, 1f, 0.78f);
            }

            if (_doubleJumpParticles != null)
            {
                _doubleJumpParticles.Emit(10);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("shadow_shift", 0.55f, 1.32f);
            }
        }

        private void UpdateDoubleJumpFeedback()
        {
            // Smoothly restore character scale deformation back to baseScale
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, Time.deltaTime * 14f);

            if (_doubleJumpPulseTimer > 0f)
            {
                _doubleJumpPulseTimer -= Time.deltaTime;
                if (_doubleJumpPulseRenderer != null)
                {
                    float t = 1f - (_doubleJumpPulseTimer / 0.26f);
                    float rx = Mathf.Lerp(0.28f, 0.78f, t);
                    float ry = Mathf.Lerp(0.12f, 0.28f, t);
                    _doubleJumpPulseRenderer.transform.localScale = new Vector3(rx, ry, 1f);
                    _doubleJumpPulseRenderer.color = new Color(0.75f, 0.42f, 1f, Mathf.Lerp(0.78f, 0f, t));

                    if (_doubleJumpPulseTimer <= 0f)
                    {
                        _doubleJumpPulseRenderer.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void CheckGrounded()
        {
            Bounds b = _col.bounds;
            // Strict narrow foot check (72% of width) directly beneath feet so touching side walls
            // or grazing platform edges never falsely resets jumps!
            Vector2 boxCenter = new Vector2(b.center.x, b.min.y - 0.04f);
            Vector2 boxSize = new Vector2(b.size.x * 0.72f, 0.06f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
            _isGrounded = false;
            foreach (Collider2D hit in hits)
            {
                if (hit != null && hit != _col && !hit.isTrigger)
                {
                    // Ignore ShadowPassage colliders since shadow passes through them
                    if (hit.GetComponent<ShadowPassage>() != null) continue;
                    // Ignore Protagonist collider
                    if (hit.GetComponent<PlayerController>() != null) continue;
                    // Only count as ground if the collider's top surface is at or below the shadow's feet
                    if (hit.bounds.max.y <= b.min.y + 0.08f)
                    {
                        _isGrounded = true;
                        break;
                    }
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
