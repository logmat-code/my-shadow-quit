using UnityEngine;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// World 2 Enemy: Floating spirit in the inverted forest that bobs and swoops toward the player.
    /// </summary>
    public class ForestWisp : EnemyBase
    {
        private float _bobTimer = 0f;
        private Vector2 _targetHoverPos;

        protected override void Awake()
        {
            maxHealth = 2;
            patrolSpeed = 1.4f;
            chaseSpeed = 2.2f;
            aggroRadius = 5.0f;
            contactDamage = 1;
            enemyName = "Forest Wisp";

            base.Awake();

            if (rb != null)
            {
                rb.gravityScale = 0f; // Flying enemy
            }

            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/enemy_forest_wisp");
                sr.sortingOrder = 9;
            }

            if (col is CircleCollider2D circle)
            {
                circle.radius = 0.35f;
            }
        }

        protected override void UpdateBehavior()
        {
            _bobTimer += Time.deltaTime * 2.5f;
            float bobOffset = Mathf.Sin(_bobTimer) * 0.45f;

            if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= aggroRadius)
            {
                // Chase player while floating slightly above
                _targetHoverPos = (Vector2)targetPlayer.position + new Vector2(0f, 0.8f + bobOffset);
                Vector2 dir = (_targetHoverPos - (Vector2)transform.position).normalized;
                if (rb != null)
                {
                    rb.linearVelocity = dir * chaseSpeed;
                }
                if (sr != null) sr.flipX = (targetPlayer.position.x < transform.position.x);
            }
            else
            {
                // Idle hover near spawn
                _targetHoverPos = (Vector2)initialSpawnPos + new Vector2(Mathf.Sin(_bobTimer * 0.5f) * 1.5f, bobOffset);
                Vector2 dir = (_targetHoverPos - (Vector2)transform.position).normalized;
                if (rb != null)
                {
                    rb.linearVelocity = dir * patrolSpeed;
                }
            }
        }
    }
}
