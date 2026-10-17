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
        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private bool _hasTriggered;

        public void Configure(Vector2 size)
        {
            EnsureInitialized();
            if (_col != null)
            {
                _col.size = size;
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
                _col.size = new Vector2(1.0f, 0.55f);
                _col.offset = new Vector2(0f, -0.05f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EvaluateHazardContact(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            EvaluateHazardContact(other);
        }

        private void EvaluateHazardContact(Collider2D other)
        {
            if (_hasTriggered || other == null) return;

            PlayerController player = other.GetComponent<PlayerController>();
            ShadowController shadow = other.GetComponent<ShadowController>();

            if (player != null || shadow != null)
            {
                _hasTriggered = true;
                TriggerHazardReset();
            }
        }

        private void TriggerHazardReset()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("web_burn", 0.75f, 0.85f);
            }

            CameraController cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            if (cam != null)
            {
                cam.TriggerShake(0.25f, 0.12f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }
    }
}
