using System.Collections;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Fragile bioluminescent fungal platform for World 2 ("The Inverted Forest").
    /// Collapses shortly after being stepped on, with a visual flicker warning,
    /// and respawns after a set cooldown.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class TemporaryPlatform2D : MonoBehaviour
    {
        [SerializeField] private float collapseDelay = 1.15f;
        [SerializeField] private float respawnDelay = 2.5f;
        [SerializeField] private float warningDuration = 0.45f;

        private SpriteRenderer _sr;
        private BoxCollider2D _col;
        private Sprite _normalSprite;
        private Sprite _fadingSprite;

        private bool _isCollapsing;
        private bool _isCollapsed;
        private float _collapseTimer;
        private Vector3 _baseLocalPosition;

        public bool IsCollapsed => _isCollapsed;
        public bool IsCollapsing => _isCollapsing;

        public void Configure(float collapseTime = 1.15f, float respawnTime = 2.5f)
        {
            collapseDelay = collapseTime;
            respawnDelay = respawnTime;
            EnsureInitialized();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (_normalSprite == null) _normalSprite = AssetRegistry.GetSprite("World2/temp_platform", 32f);
            if (_fadingSprite == null) _fadingSprite = AssetRegistry.GetSprite("World2/temp_platform_fading", 32f) ?? _normalSprite;

            if (_sr != null)
            {
                _sr.sortingOrder = 4;
                if (_sr.sprite == null) _sr.sprite = _normalSprite;
            }

            if (_col != null)
            {
                _col.size = new Vector2(1.5f, 0.62f);
                _col.offset = Vector2.zero;
            }

            _baseLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (!_isCollapsing || _isCollapsed) return;

            _collapseTimer -= Time.deltaTime;

            if (_collapseTimer <= warningDuration)
            {
                // Visual flicker & minor tremor warning
                if (_sr != null)
                {
                    _sr.sprite = _fadingSprite;
                    float flicker = (Mathf.Sin(Time.time * 28f) > 0f) ? 1f : 0.45f;
                    _sr.color = new Color(1f, 1f, 1f, flicker);
                }
                float shake = (Random.value - 0.5f) * 0.05f;
                transform.localPosition = _baseLocalPosition + new Vector3(shake, 0f, 0f);
            }

            if (_collapseTimer <= 0f)
            {
                CollapsePlatform();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            EvaluateContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            EvaluateContact(collision);
        }

        private void EvaluateContact(Collision2D collision)
        {
            if (_isCollapsing || _isCollapsed) return;

            // Check if contact is from above
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.45f)
                {
                    StartCollapse();
                    break;
                }
            }
        }

        public void StartCollapse()
        {
            if (_isCollapsing || _isCollapsed) return;

            _isCollapsing = true;
            _collapseTimer = collapseDelay;
            _baseLocalPosition = transform.localPosition;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("footstep", 0.5f, 0.82f);
            }
        }

        private void CollapsePlatform()
        {
            _isCollapsed = true;
            _isCollapsing = false;
            transform.localPosition = _baseLocalPosition;

            if (_col != null) _col.enabled = false;
            if (_sr != null) _sr.enabled = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("web_burn", 0.65f, 1.25f);
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            _isCollapsed = false;
            _isCollapsing = false;
            transform.localPosition = _baseLocalPosition;

            if (_sr != null)
            {
                _sr.sprite = _normalSprite;
                _sr.color = Color.white;
                _sr.enabled = true;
            }

            if (_col != null)
            {
                _col.enabled = true;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("crystal_resonate", 0.45f, 1.35f);
            }
        }

        public void ResetPlatform()
        {
            StopAllCoroutines();
            _isCollapsed = false;
            _isCollapsing = false;
            _collapseTimer = 0f;
            transform.localPosition = _baseLocalPosition;

            if (_sr != null)
            {
                _sr.sprite = _normalSprite;
                _sr.color = Color.white;
                _sr.enabled = true;
            }

            if (_col != null)
            {
                _col.enabled = true;
            }
        }

        private void OnDisable()
        {
            ResetPlatform();
        }
    }
}
