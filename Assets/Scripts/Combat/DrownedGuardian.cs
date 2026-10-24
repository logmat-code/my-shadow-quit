using UnityEngine;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// World 4 Enemy: Heavy submerged cathedral gargoyle.
    /// Resilient, lunges forward when in line of sight.
    /// </summary>
    public class DrownedGuardian : EnemyBase
    {
        [SerializeField] private float patrolDistance = 2.5f;
        private float _leftBound;
        private float _rightBound;
        private float _lungeCooldown = 0f;

        protected override void Awake()
        {
            maxHealth = 5;
            patrolSpeed = 1.0f;
            chaseSpeed = 2.0f;
            aggroRadius = 5.0f;
            contactDamage = 2; // Heavy hitter
            enemyName = "Drowned Guardian";

            base.Awake();

            _leftBound = initialSpawnPos.x - patrolDistance;
            _rightBound = initialSpawnPos.x + patrolDistance;

            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/enemy_drowned_guardian");
                sr.sortingOrder = 9;
            }

            if (col is BoxCollider2D box)
            {
                box.size = new Vector2(0.7f, 0.75f);
            }
        }

        protected override void UpdateBehavior()
        {
            if (_lungeCooldown > 0f) _lungeCooldown -= Time.deltaTime;

            float speed = patrolSpeed;
            bool chasing = false;

            if (targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.position);
                if (dist <= aggroRadius)
                {
                    chasing = true;
                    moveDirection = (targetPlayer.position.x > transform.position.x) ? 1 : -1;

                    // Lunge surge if lined up
                    if (Mathf.Abs(targetPlayer.position.y - transform.position.y) < 1.2f && _lungeCooldown <= 0f)
                    {
                        _lungeCooldown = 2.5f;
                        if (rb != null)
                        {
                            rb.linearVelocity = new Vector2(moveDirection * 5.5f, 2.0f);
                        }
                    }
                    else
                    {
                        speed = chaseSpeed;
                    }
                }
            }

            if (!chasing)
            {
                if (transform.position.x >= _rightBound) moveDirection = -1;
                else if (transform.position.x <= _leftBound) moveDirection = 1;
            }

            if (rb != null && _lungeCooldown < 1.8f) // Don't override lunge momentum immediately
            {
                rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);
            }

            if (sr != null)
            {
                sr.flipX = (moveDirection < 0);
            }
        }
    }
}
