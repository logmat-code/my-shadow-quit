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
        private Coroutine _meleeMotionCoroutine;

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

            // Only allow combat inputs when control is active, player is not dead,
            // no dialogues are open, and game is not paused
            if (!CanCombatInput()) return;

            // Weapon Cycling (F on keyboard or Mobile Cycle Button)
            if (GameInput.GetCycleWeaponDown())
            {
                WeaponCatalog.CycleNextUnlockedWeapon();
            }

            // Weapon Attack (J / Left-click / Mobile Attack Button)
            if (GameInput.GetAttackDown())
            {
                TryAttack();
            }
        }

        private bool CanCombatInput()
        {
            if (_player == null || !_player.IsControlActive) return false;
            if (_player.IsDead) return false;

            // Check if dialogues are active
            if (Narrative.DialogueController.Instance != null && Narrative.DialogueController.Instance.IsDialogueActive)
            {
                return false;
            }

            // Check if game is paused or in menus
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.IsPaused || GameManager.Instance.IsInMainMenu || GameManager.Instance.IsEndingSequenceActive)
                {
                    return false;
                }
                if (GameManager.Instance.IsControllingShadow)
                {
                    return false;
                }
            }

            return true;
        }

        public void TryAttack()
        {
            if (!CanCombatInput()) return;
            if (_attackCooldownTimer > 0f) return;

            WeaponType current = WeaponCatalog.EquippedWeapon;
            if (current == WeaponType.None) return;

            WeaponDefinition def = WeaponCatalog.GetDefinition(current);
            if (def == null) return;

            _attackCooldownTimer = def.attackCooldown;

            int dir = _player.FacingDirection;

            switch (current)
            {
                case WeaponType.Emberblade:
                {
                    Vector2 meleeHitCenter = (Vector2)transform.position + new Vector2(dir * 0.40f, -0.04f);
                    PerformMeleeAttack(def, meleeHitCenter, dir);
                    break;
                }

                case WeaponType.HollowCrossbow:
                {
                    // Muzzle is at the tip of the crossbow held at the hands
                    Vector2 crossbowMuzzle = (Vector2)transform.position + new Vector2(dir * 0.42f, -0.05f);
                    PerformRangedAttack(def, crossbowMuzzle, dir, "proj_crossbow_bolt", 14f, 0f, 1.8f);
                    break;
                }

                case WeaponType.AbyssalScepter:
                {
                    Vector2 scepterOrbOrigin = (Vector2)transform.position + new Vector2(dir * 0.38f, 0.05f);
                    PerformRangedAttack(def, scepterOrbOrigin, dir, "proj_scepter_orb", 9f, 0.5f, 2.5f);
                    break;
                }
            }
        }

        private void PerformMeleeAttack(WeaponDefinition def, Vector2 origin, int dir)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(def.sfxName))
            {
                AudioManager.Instance.PlaySFX(def.sfxName, 0.9f);
            }

            // Animate Emberblade slash arc originating crisply from the protagonist's hand
            if (_weaponVisualObj != null && _weaponSpriteRenderer != null)
            {
                Sprite s = Resources.Load<Sprite>("Sprites/Combat/" + def.spriteName);
                if (s != null)
                {
                    _weaponSpriteRenderer.sprite = s;
                    _weaponSpriteRenderer.flipX = (dir < 0);
                    _weaponSpriteRenderer.enabled = true;
                    if (_meleeMotionCoroutine != null) StopCoroutine(_meleeMotionCoroutine);
                    _meleeMotionCoroutine = StartCoroutine(MeleeMotionCoroutine(dir));
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

        private System.Collections.IEnumerator MeleeMotionCoroutine(int dir)
        {
            float duration = 0.12f;
            float elapsed = 0f;

            // Start angle: raised strike (+40 deg), swinging down through follow-through (-45 deg)
            float startAngle = (dir < 0) ? -40f : 40f;
            float endAngle = (dir < 0) ? 45f : -45f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smooth forward slash curve anchored at protagonist's hand (x ~ dir * 0.22, y ~ -0.04)
                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                float offsetX = Mathf.Lerp(dir * 0.18f, dir * 0.32f, t);
                float offsetY = Mathf.Lerp(0.04f, -0.08f, t);

                if (_weaponVisualObj != null)
                {
                    _weaponVisualObj.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
                    _weaponVisualObj.transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                yield return null;
            }

            if (_weaponSpriteRenderer != null)
            {
                _weaponSpriteRenderer.enabled = false;
            }
            _meleeMotionCoroutine = null;
        }

        private void PerformRangedAttack(WeaponDefinition def, Vector2 muzzleOrigin, int dir, string projSprite, float speed, float homing, float life)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(def.sfxName))
            {
                AudioManager.Instance.PlaySFX(def.sfxName, 0.9f);
            }

            // Flash weapon at hand anchor
            if (_weaponVisualObj != null && _weaponSpriteRenderer != null)
            {
                Sprite s = Resources.Load<Sprite>("Sprites/Combat/" + def.spriteName);
                if (s != null)
                {
                    _weaponSpriteRenderer.sprite = s;
                    _weaponSpriteRenderer.flipX = (dir < 0);
                    // Held naturally at the waist/hands
                    float offsetY = (def.weaponType == WeaponType.AbyssalScepter) ? 0.05f : -0.05f;
                    _weaponVisualObj.transform.localPosition = new Vector3(dir * 0.22f, offsetY, 0f);
                    _weaponVisualObj.transform.localRotation = Quaternion.identity;
                    _weaponSpriteRenderer.enabled = true;
                    _slashVisualTimer = 0.10f;
                }
            }

            // Spawn projectile directly at muzzleOrigin
            GameObject pObj = new GameObject("Projectile_" + def.weaponType);
            pObj.transform.position = muzzleOrigin;

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
