using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// World 5 Enemy: Corrupted abyssal mirror wraith.
    /// Distorts space, teleports short distances, and attacks aggressively.
    /// </summary>
    public class AbyssalEcho : EnemyBase
    {
        private float _teleportCooldown = 3.5f;

        protected override void Awake()
        {
            maxHealth = 6;
            patrolSpeed = 1.6f;
            chaseSpeed = 2.6f;
            aggroRadius = 6.0f;
            contactDamage = 2;
            enemyName = "Abyssal Echo";

            base.Awake();

            if (sr != null && sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/enemy_abyssal_echo");
                sr.sortingOrder = 9;
            }

            if (col is BoxCollider2D box)
            {
                box.size = new Vector2(0.6f, 0.75f);
            }
        }

        protected override void UpdateBehavior()
        {
            _teleportCooldown -= Time.deltaTime;

            if (targetPlayer != null)
            {
                float dist = Vector2.Distance(transform.position, targetPlayer.position);
                if (dist <= aggroRadius)
                {
                    moveDirection = (targetPlayer.position.x > transform.position.x) ? 1 : -1;

                    // Short warp teleport behind player if cooldown ready
                    if (_teleportCooldown <= 0f && dist > 2.0f)
                    {
                        _teleportCooldown = 4.0f;
                        transform.position = targetPlayer.position + new Vector3(-moveDirection * 1.5f, 0.2f, 0f);
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySFX("shadow_shift", 0.9f);
                        }
                    }

                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector2(moveDirection * chaseSpeed, rb.linearVelocity.y);
                    }
                }
                else
                {
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    }
                }
            }

            if (sr != null)
            {
                sr.flipX = (moveDirection < 0);
            }
        }
    }
}
