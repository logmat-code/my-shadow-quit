using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Environmental hazard for World 2 ("The Inverted Forest").
    /// Represents toxic brambles and thorny spikes.
    /// Resets the current chamber if touched by either the Protagonist or Shadow.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ForestHazard : MonoBehaviour
    {
        private static float s_lastHazardRestartTime = -10f;
        private const float DebounceDuration = 0.85f;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _hasTriggered;

        public void Configure(Vector2 size)
        {
            EnsureInitialized();
            if (_col != null)
            {
                _col.size = new Vector2(size.x * 0.95f, size.y * 0.85f);
                _col.offset = Vector2.zero;
            }

            // Populate tiled thorn sprites across the width so it looks full and dense
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            Sprite thornSprite = AssetRegistry.GetSprite("World2/forest_thorns", 32f);
            if (thornSprite != null)
            {
                if (_sr != null) _sr.enabled = false;

                int count = Mathf.Max(1, Mathf.RoundToInt(size.x));
                float startX = -size.x * 0.5f + 0.5f;
                float step = (count > 1) ? (size.x - 1f) / (count - 1) : 0f;
                for (int i = 0; i < count; i++)
                {
                    GameObject thornChild = new GameObject($"Thorn_{i}");
                    thornChild.transform.SetParent(transform, false);
                    thornChild.transform.localPosition = new Vector3(startX + i * step, 0f, 0f);
                    SpriteRenderer csr = thornChild.AddComponent<SpriteRenderer>();
                    csr.sprite = thornSprite;
                    csr.sortingOrder = 5;
                }
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (_sr != null)
            {
                _sr.sortingOrder = 5;
                if (_sr.sprite == null)
                {
                    _sr.sprite = AssetRegistry.GetSprite("World2/forest_thorns", 32f);
                }
            }

            if (_col != null)
            {
                _col.isTrigger = true;
                _col.size = new Vector2(0.95f, 0.60f);
                _col.offset = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => EvaluateHazardContact(other);
        private void OnTriggerStay2D(Collider2D other) => EvaluateHazardContact(other);
        private void OnCollisionEnter2D(Collision2D col) { if (col != null && col.collider != null) EvaluateHazardContact(col.collider); }
        private void OnCollisionStay2D(Collision2D col) { if (col != null && col.collider != null) EvaluateHazardContact(col.collider); }

        private void EvaluateHazardContact(Collider2D other)
        {
            if (_hasTriggered || other == null) return;

            PlayerController player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
            ShadowController shadow = other.GetComponent<ShadowController>() ?? other.GetComponentInParent<ShadowController>();

            if (player != null || shadow != null)
            {
                _hasTriggered = true;
                TriggerHazardReset();
            }
        }

        private void TriggerHazardReset()
        {
            if (Time.unscaledTime - s_lastHazardRestartTime < DebounceDuration) return;
            s_lastHazardRestartTime = Time.unscaledTime;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("web_burn", 0.75f, 0.85f);
            }

            CameraController cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            if (cam != null)
            {
                cam.TriggerShake(0.25f, 0.12f);
            }

            if (Combat.HealthSystem.Instance != null)
            {
                Combat.HealthSystem.Instance.TakeDamage(1, Vector2.up * 5f, 0.85f);
                _hasTriggered = false; // Reset trigger state so it can damage again if still in hazard
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }
    }
}
