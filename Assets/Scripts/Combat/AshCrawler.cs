using UnityEngine;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// World 1 Enemy: Skittering charred bug that patrols platforms and rushes when player is near.
    /// </summary>
    public class AshCrawler : EnemyBase
    {
        [SerializeField] private float patrolDistance = 3.0f;
        private float _leftBound;
        private float _rightBound;

        protected override void Awake()
        {
            maxHealth = 2;
            patrolSpeed = 1.8f;
            chaseSpeed = 2.8f;
            aggroRadius = 4.0f;
            contactDamage = 1;
            enemyName = "Ash Crawler";

            base.Awake();

            _leftBound = initialSpawnPos.x - patrolDistance;
            _rightBound = initialSpawnPos.x + patrolDistance;

            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/enemy_ash_crawler");
                sr.sortingOrder = 9;
            }

            if (col is BoxCollider2D box)
            {
                box.size = new Vector2(0.6f, 0.45f);
                box.offset = new Vector2(0f, -0.05f);
            }
        }

        protected override void UpdateBehavior()
        {
            float speed = patrolSpeed;
            bool chasing = false;

            if (targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.position);
                if (dist <= aggroRadius)
                {
                    chasing = true;
                    speed = chaseSpeed;
                    moveDirection = (targetPlayer.position.x > transform.position.x) ? 1 : -1;
                }
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
