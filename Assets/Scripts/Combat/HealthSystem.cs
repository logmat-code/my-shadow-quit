using System;
using System.Collections;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;
using MyShadowQuit.UI;

namespace MyShadowQuit.Combat
{
    /// <summary>
    /// Shared Player & Shadow health system.
    /// Manages HP (3 to 6 max based on world completion), damage debouncing,
    /// invulnerability flashes, knockback, and restart on death.
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        public static HealthSystem Instance { get; private set; }

        public const string PrefKeyMaxHealthBonus = "MSQ_MaxHealthBonus";
        public const int BaseMaxHealth = 3;
        public const int AbsoluteMaxHealth = 6;

        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnDeath;
        public event Action<int, Vector2> OnDamaged; // amount, hitDirection

        [SerializeField] private float invulnerabilityDuration = 0.85f;
        [SerializeField] private float defaultKnockbackForce = 4.2f;

        private int _maxHealth = BaseMaxHealth;
        private int _currentHealth = BaseMaxHealth;
        private bool _isInvulnerable = false;
        private Coroutine _invulnerableCoroutine;
        private bool _isDead = false;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsInvulnerable => _isInvulnerable;
        public bool IsDead => _isDead;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            EvaluateMaxHealthFromProgress();
            ResetHealthToMax();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void EvaluateMaxHealthFromProgress()
        {
            int hp = BaseMaxHealth;
            if (WorldProgressSave.IsWorldCompleted("world_2")) hp = 4;
            if (WorldProgressSave.IsWorldCompleted("world_3")) hp = 5;
            if (WorldProgressSave.IsWorldCompleted("world_4")) hp = 6;

            _maxHealth = Mathf.Clamp(hp, BaseMaxHealth, AbsoluteMaxHealth);
            if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();
        }

        public void ResetHealthToMax()
        {
            _isDead = false;
            _currentHealth = _maxHealth;
            _isInvulnerable = false;
            if (_invulnerableCoroutine != null)
            {
                StopCoroutine(_invulnerableCoroutine);
                _invulnerableCoroutine = null;
            }
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();
        }

        public void ResetAllProgression()
        {
            _maxHealth = BaseMaxHealth;
            _currentHealth = BaseMaxHealth;
            _isDead = false;
            _isInvulnerable = false;
            if (_invulnerableCoroutine != null)
            {
                StopCoroutine(_invulnerableCoroutine);
                _invulnerableCoroutine = null;
            }
            PlayerPrefs.DeleteKey(PrefKeyMaxHealthBonus);
            PlayerPrefs.Save();
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();
        }

        public bool TakeDamage(int amount, Vector2 hitDirection, float knockback = 0f)
        {
            if (_isDead || _isInvulnerable || amount <= 0) return false;

            int currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex + 1 : -1;
            Debug.Log($"[DEATH_DIAGNOSTIC] Level={currentLevel}, Frame={Time.frameCount}, Method=TakeDamage, Amount={amount}, HP={_currentHealth}->{Mathf.Max(0, _currentHealth - amount)}");

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _isInvulnerable = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("web_burn", 0.85f, 1.15f);
            }

            if (Camera.main != null && Camera.main.TryGetComponent(out CameraController cam))
            {
                cam.TriggerShake(0.2f, 0.08f);
            }

            OnDamaged?.Invoke(amount, hitDirection);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();

            // Apply feedback to Player and Shadow
            ApplyHurtFeedback(hitDirection, knockback > 0f ? knockback : defaultKnockbackForce);

            if (_currentHealth <= 0)
            {
                TriggerDeath();
            }
            else
            {
                if (gameObject.activeInHierarchy)
                {
                    _invulnerableCoroutine = StartCoroutine(InvulnerabilityRoutine());
                }
            }

            return true;
        }

        public void KillInstantly()
        {
            if (_isDead) return;
            string callerTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
            int currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex + 1 : -1;
            Debug.Log($"[DEATH_DIAGNOSTIC] Level={currentLevel}, Frame={Time.frameCount}, Method=KillInstantly, CallerTrace:\n{callerTrace}");

            _currentHealth = 0;
            OnHealthChanged?.Invoke(0, _maxHealth);
            NotifyUIRefresh();
            TriggerDeath();
        }

        public bool Heal(int amount)
        {
            if (_isDead || amount <= 0 || _currentHealth >= _maxHealth) return false;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.7f, 1.25f);
            }
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();
            return true;
        }

        public void IncreaseMaxHealth(int amount)
        {
            _maxHealth = Mathf.Clamp(_maxHealth + amount, BaseMaxHealth, AbsoluteMaxHealth);
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            NotifyUIRefresh();
        }

        private void NotifyUIRefresh()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RefreshHeartsHUD();
            }
        }

        private void TriggerDeath()
        {
            if (_isDead) return;
            _isDead = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("puzzle_complete", 0.6f, 0.5f);
            }

            OnDeath?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
        }

        private void ApplyHurtFeedback(Vector2 hitDirection, float force)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                Rigidbody2D prb = player.GetComponent<Rigidbody2D>();
                if (prb != null)
                {
                    Vector2 dir = hitDirection != Vector2.zero ? hitDirection.normalized : Vector2.up;
                    prb.linearVelocity = new Vector2(dir.x * force, Mathf.Max(prb.linearVelocity.y, 3.5f));
                    StartCoroutine(FlashRenderer(player.GetComponent<SpriteRenderer>()));
                }
            }

            ShadowController shadow = FindAnyObjectByType<ShadowController>();
            if (shadow != null)
            {
                Rigidbody2D srb = shadow.GetComponent<Rigidbody2D>();
                if (srb != null)
                {
                    Vector2 dir = hitDirection != Vector2.zero ? hitDirection.normalized : Vector2.up;
                    srb.linearVelocity = new Vector2(dir.x * force, Mathf.Max(srb.linearVelocity.y, 3.5f));
                    StartCoroutine(FlashRenderer(shadow.GetComponent<SpriteRenderer>()));
                }
            }
        }

        private IEnumerator FlashRenderer(SpriteRenderer sr)
        {
            if (sr == null) yield break;
            Color orig = sr.color;
            for (int i = 0; i < 4; i++)
            {
                sr.color = new Color(1f, 0.3f, 0.3f, 0.6f);
                yield return new WaitForSeconds(0.08f);
                sr.color = orig;
                yield return new WaitForSeconds(0.08f);
            }
            sr.color = orig;
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
            _isInvulnerable = false;
            _invulnerableCoroutine = null;
        }
    }
}
