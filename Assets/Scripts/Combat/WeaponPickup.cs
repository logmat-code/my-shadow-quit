using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// In-game pedestal or chest that permanently unlocks a weapon when reached by the player.
    /// Spawns a floating weapon icon and triggers audio/notification fanfare.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponType weaponToUnlock;
        private bool _collected;
        private SpriteRenderer _sr;

        public void Initialize(WeaponType type)
        {
            weaponToUnlock = type;

            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

            WeaponDefinition def = WeaponCatalog.GetDefinition(type);
            if (def != null)
            {
                _sr.sprite = Resources.Load<Sprite>("Sprites/Combat/" + def.spriteName);
                _sr.sortingOrder = 9;
            }

            BoxCollider2D col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);

            // Hide if already unlocked
            if (WeaponCatalog.IsWeaponUnlocked(type))
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Floating bobbing animation
            float bob = Mathf.Sin(Time.time * 3f) * 0.1f;
            transform.localPosition += new Vector3(0f, bob * Time.deltaTime, 0f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_collected) return;

            if (collision.CompareTag("Player") || collision.GetComponent<Entities.PlayerController>() != null)
            {
                _collected = true;
                WeaponCatalog.UnlockWeapon(weaponToUnlock);
                WeaponCatalog.SetEquippedWeapon(weaponToUnlock);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("puzzle_complete", 1.0f, 1.2f);
                }

                WeaponDefinition def = WeaponCatalog.GetDefinition(weaponToUnlock);
                if (def != null && GameManager.Instance != null)
                {
                    var ui = Object.FindFirstObjectByType<UI.UIManager>();
                    if (ui != null)
                    {
                        ui.ShowTemporaryRewardNotification($"UNLOCKED: {def.displayName}!");
                    }
                }

                Destroy(gameObject);
            }
        }
    }
}
