using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// Attach to Player GameObject. Handles attacks for currently equipped weapon,
    /// melee slash collision detection, ranged projectile firing, and weapon cycling.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        private PlayerController _player;
        private float _attackCooldownTimer = 0f;
        private SpriteRenderer _weaponSpriteRenderer;
        private GameObject _weaponVisualObj;
        private float _slashVisualTimer = 0f;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            CreateWeaponVisual();
        }

        private void CreateWeaponVisual()
        {
            _weaponVisualObj = new GameObject("WeaponVisual");
            _weaponVisualObj.transform.SetParent(transform, false);
            _weaponVisualObj.transform.localPosition = new Vector3(0.35f, 0f, 0f);

            _weaponSpriteRenderer = _weaponVisualObj.AddComponent<SpriteRenderer>();
            _weaponSpriteRenderer.sortingOrder = 12; // In front of player
            _weaponSpriteRenderer.enabled = false;
        }

        private void Update()
        {
            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            if (_slashVisualTimer > 0f)
            {
                _slashVisualTimer -= Time.deltaTime;
                if (_slashVisualTimer <= 0f && _weaponSpriteRenderer != null)
                {
                    _weaponSpriteRenderer.enabled = false;
                }
            }

            // Only allow combat inputs when control is active and player is controlled
            if (_player == null || !_player.IsControlActive) return;

            // Weapon Cycling (Q or Mobile Button)
            if (GameInput.GetCycleWeaponDown())
            {
                WeaponCatalog.CycleNextUnlockedWeapon();
            }

            // Weapon Attack (Left-click / J / Mobile Button)
            if (GameInput.GetAttackDown())
            {
                TryAttack();
            }
        }

        public void TryAttack()
        {
            if (_attackCooldownTimer > 0f) return;

            WeaponType current = WeaponCatalog.EquippedWeapon;
            if (current == WeaponType.None) return;

            WeaponDefinition def = WeaponCatalog.GetDefinition(current);
            if (def == null) return;

            _attackCooldownTimer = def.attackCooldown;

            int dir = _player.FacingDirection;
            Vector2 origin = (Vector2)transform.position + new Vector2(dir * 0.45f, 0f);

            switch (current)
            {
                case WeaponType.Emberblade:
                    PerformMeleeAttack(def, origin, dir);
                    break;

                case WeaponType.HollowCrossbow:
                    PerformRangedAttack(def, origin, dir, "proj_crossbow_bolt", 14f, 0f, 1.8f);
                    break;

                case WeaponType.AbyssalScepter:
                    PerformRangedAttack(def, origin, dir, "proj_scepter_orb", 9f, 0.5f, 2.5f);
                    break;
            }
        }

        private void PerformMeleeAttack(WeaponDefinition def, Vector2 origin, int dir)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(def.sfxName))
            {
                AudioManager.Instance.PlaySFX(def.sfxName, 0.9f);
            }

            // Show slash visual
            if (_weaponVisualObj != null && _weaponSpriteRenderer != null)
            {
                Sprite s = Resources.Load<Sprite>("Sprites/Combat/" + def.spriteName);
                if (s != null)
                {
                    _weaponSpriteRenderer.sprite = s;
                    _weaponSpriteRenderer.flipX = (dir < 0);
                    _weaponVisualObj.transform.localPosition = new Vector3(dir * 0.45f, 0.05f, 0f);
                    _weaponSpriteRenderer.enabled = true;
                    _slashVisualTimer = 0.15f;
                }
            }

            // Detect hit targets in melee arc
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, def.range);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                IDamageable dmg = hit.GetComponent<IDamageable>();
                if (dmg == null) dmg = hit.GetComponentInParent<IDamageable>();

                if (dmg != null && !dmg.IsDead)
                {
                    Vector2 knockback = new Vector2(dir * def.knockbackForce, 2.5f);
                    dmg.TakeDamage(def.damage, knockback, 0.2f);
                }
            }
        }

        private void PerformRangedAttack(WeaponDefinition def, Vector2 origin, int dir, string projSprite, float speed, float homing, float life)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(def.sfxName))
            {
                AudioManager.Instance.PlaySFX(def.sfxName, 0.9f);
            }

            // Spawn projectile
            GameObject pObj = new GameObject("Projectile_" + def.weaponType);
            pObj.transform.position = origin;

            ProjectileController proj = pObj.AddComponent<ProjectileController>();
            proj.Initialize(
                new Vector2(dir, 0f),
                def.damage,
                speed,
                def.range,
                def.knockbackForce,
                projSprite,
                homing,
                life
            );
        }
    }
}
