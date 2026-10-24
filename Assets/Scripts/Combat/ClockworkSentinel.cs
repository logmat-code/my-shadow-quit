using UnityEngine;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// World 3 Enemy: Armored mechanical sentinel that marches methodically and withstands extra hits.
    /// </summary>
    public class ClockworkSentinel : EnemyBase
    {
        [SerializeField] private float patrolDistance = 3.5f;
        private float _leftBound;
        private float _rightBound;

        protected override void Awake()
        {
            maxHealth = 4;
            patrolSpeed = 1.2f;
            chaseSpeed = 1.9f;
            aggroRadius = 4.5f;
            contactDamage = 1;
            enemyName = "Clockwork Sentinel";

            base.Awake();

            _leftBound = initialSpawnPos.x - patrolDistance;
            _rightBound = initialSpawnPos.x + patrolDistance;

            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/enemy_clockwork_sentinel");
                sr.sortingOrder = 9;
            }

            if (col is BoxCollider2D box)
            {
                box.size = new Vector2(0.55f, 0.65f);
            }
        }

        protected override void UpdateBehavior()
        {
            float speed = patrolSpeed;
            bool chasing = false;

            if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= aggroRadius)
            {
                chasing = true;
                speed = chaseSpeed;
                moveDirection = (targetPlayer.position.x > transform.position.x) ? 1 : -1;
            }

            if (!chasing)
            {
                if (transform.position.x >= _rightBound) moveDirection = -1;
                else if (transform.position.x <= _leftBound) moveDirection = 1;
            }

            if (rb != null)
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
