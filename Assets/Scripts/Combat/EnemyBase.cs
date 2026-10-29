using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// Base class for all hostile entities across the 5 worlds.
    /// Implements IDamageable, horizontal patrol with ledge/wall detection,
    /// player proximity detection, knockback reaction, and respawn on level reload.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected int maxHealth = 3;
        [SerializeField] protected int currentHealth;
        [SerializeField] protected float patrolSpeed = 1.6f;
        [SerializeField] protected float chaseSpeed = 2.4f;
        [SerializeField] protected float aggroRadius = 4.5f;
        [SerializeField] protected int contactDamage = 1;
        [SerializeField] protected string enemyName = "Hostile";

        [Header("Audio & Visuals")]
        [SerializeField] protected string hurtSfx = "enemy_hurt";
        [SerializeField] protected string deathSfx = "enemy_death";

        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected Collider2D col;

        protected Vector3 initialSpawnPos;
        protected int moveDirection = 1; // 1 = right, -1 = left
        protected bool isDead = false;
        protected float invulnerabilityTimer = 0f;
        protected float hurtFlashTimer = 0f;
        protected Transform targetPlayer;

        public bool IsDead => isDead;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            initialSpawnPos = transform.position;
            currentHealth = maxHealth;

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        protected virtual void Start()
        {
            FindPlayer();
        }

        protected void FindPlayer()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                targetPlayer = p.transform;
            }
            else
            {
                PlayerController pc = FindAnyObjectByType<PlayerController>();
                if (pc != null) targetPlayer = pc.transform;
            }
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (invulnerabilityTimer > 0f)
            {
                invulnerabilityTimer -= Time.deltaTime;
            }

            if (hurtFlashTimer > 0f)
            {
                hurtFlashTimer -= Time.deltaTime;
                if (hurtFlashTimer <= 0f && sr != null)
                {
                    sr.color = Color.white;
                }
            }

            if (targetPlayer == null)
            {
                FindPlayer();
            }

            UpdateBehavior();
        }

        protected abstract void UpdateBehavior();

        public virtual bool TakeDamage(int damage, Vector2 knockbackForce, float invulnDuration = 0.2f)
        {
            if (isDead || invulnerabilityTimer > 0f) return false;

            currentHealth -= damage;
            invulnerabilityTimer = invulnDuration;

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(hurtSfx))
            {
                AudioManager.Instance.PlaySFX(hurtSfx, 0.85f);
            }

            // Hurt flash
            if (sr != null)
            {
                sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                hurtFlashTimer = 0.12f;
            }

            // Knockback
            if (rb != null && knockbackForce.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = knockbackForce;
            }

            if (currentHealth <= 0)
            {
                Die();
            }

            return true;
        }

        public virtual void KillInstantly()
        {
            if (isDead) return;
            currentHealth = 0;
            Die();
        }

        protected virtual void Die()
        {
            isDead = true;

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(deathSfx))
            {
                AudioManager.Instance.PlaySFX(deathSfx, 0.9f);
            }

            // Chance to drop Ember Fragment on defeat
            if (Random.value < 0.45f)
            {
                EmberFragmentPickup.SpawnFragment(transform.position);
            }

            Destroy(gameObject, 0.15f);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            CheckPlayerContact(collision.collider);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            CheckPlayerContact(collider);
        }

        protected void CheckPlayerContact(Collider2D other)
        {
            if (isDead || other == null) return;

            // Route to IDamageable on player, shadow, or parent
            IDamageable dmg = other.GetComponent<IDamageable>();
            if (dmg == null) dmg = other.GetComponentInParent<IDamageable>();

            bool isPlayerOrShadow = other.CompareTag("Player") || other.CompareTag("Shadow") ||
                                    other.GetComponent<PlayerController>() != null || other.GetComponentInParent<PlayerController>() != null ||
                                    other.GetComponent<ShadowController>() != null || other.GetComponentInParent<ShadowController>() != null;

            if (dmg != null && isPlayerOrShadow)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                Vector2 knockback = new Vector2(dir.x * 6.5f, 5.0f);
                dmg.TakeDamage(contactDamage, knockback, 0.85f);
            }
            else if (dmg != null && dmg is HealthSystem)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                Vector2 knockback = new Vector2(dir.x * 6.5f, 5.0f);
                dmg.TakeDamage(contactDamage, knockback, 0.85f);
            }
            else if (isPlayerOrShadow && HealthSystem.Instance != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                Vector2 knockback = new Vector2(dir.x * 6.5f, 5.0f);
                HealthSystem.Instance.TakeDamage(contactDamage, knockback, 0.85f);
            }
        }
    }
}
