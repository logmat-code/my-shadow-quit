using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// Ranged physical/spectral projectile launched by the Hollow Crossbow or Abyssal Scepter.
    /// Moves with velocity, checks obstacle and enemy collisions, applies damage and impacts.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 2.5f;

        private Vector2 direction = Vector2.right;
        private SpriteRenderer sr;
        private bool hasHit = false;

        public void Initialize(Vector2 dir, int dmg, float spd, float range, float knockback, string spriteResourceName, float homingTurnRate = 0f, float maxLifetime = 2.5f)
        {
            direction = dir.normalized;
            damage = dmg;
            speed = spd;
            lifetime = maxLifetime;

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

            sr.sprite = Resources.Load<Sprite>("Sprites/Combat/" + spriteResourceName);
            sr.sortingOrder = 12;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.25f;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (hasHit) return;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;
            if (other.CompareTag("Player") || other.CompareTag("Shadow")) return;

            // Damage enemy
            IDamageable target = other.GetComponent<IDamageable>();
            if (target == null && other.transform.parent != null) target = other.transform.parent.GetComponent<IDamageable>();

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(damage, direction * 4.0f, 0.2f);
                ImpactAndDestroy();
                return;
            }

            // Hit environment wall / floor
            if (!other.isTrigger)
            {
                ImpactAndDestroy();
            }
        }

        private void ImpactAndDestroy()
        {
            hasHit = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("switch_activate", 0.5f, 1.4f);
            }
            Destroy(gameObject);
        }
    }
}
