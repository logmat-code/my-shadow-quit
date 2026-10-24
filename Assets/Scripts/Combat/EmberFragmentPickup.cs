using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// Collectible pickup that restores 1 HP to the shared HealthSystem.
    /// Does not increase Max HP. Disappears with subtle audio and particle feedback.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class EmberFragmentPickup : MonoBehaviour
    {
        [SerializeField] private int healAmount = 1;
        [SerializeField] private float bobbingSpeed = 2.5f;
        [SerializeField] private float bobbingAmount = 0.12f;

        private Vector3 startPos;
        private bool collected = false;
        private SpriteRenderer sr;

        public static EmberFragmentPickup SpawnFragment(Vector2 pos)
        {
            GameObject go = new GameObject("EmberFragmentPickup");
            go.transform.position = pos;
            return go.AddComponent<EmberFragmentPickup>();
        }

        private void Awake()
        {
            startPos = transform.position;
            sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }
            if (sr.sprite == null)
            {
                sr.sprite = Resources.Load<Sprite>("Sprites/Combat/ember_fragment");
            }
            sr.sortingOrder = 8;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
        }

        private void Update()
        {
            if (collected) return;
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;

            if (other.CompareTag("Player") || other.GetComponent<Entities.PlayerController>() != null || other.GetComponent<Entities.ShadowController>() != null)
            {
                if (HealthSystem.Instance != null && HealthSystem.Instance.CurrentHealth < HealthSystem.Instance.MaxHealth)
                {
                    collected = true;
                    HealthSystem.Instance.Heal(healAmount);
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("ember_pickup", 0.9f);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
