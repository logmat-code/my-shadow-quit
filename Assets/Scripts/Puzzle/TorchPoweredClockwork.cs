using System.Collections.Generic;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Entities;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Ancient brass focusing lens that activates clockwork mechanisms when illuminated by the Torch.
    /// Emits a warm solar ember glow and resonates with the tower machinery.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class TorchPoweredClockwork : PuzzleNode
    {
        [Header("Lens Settings")]
        [SerializeField] private float activationRadius = 2.4f;
        [SerializeField] private float chargeDuration = 0.8f;
        [SerializeField] private bool stayLitOnceCharged = true;
        [SerializeField] private AncientDoor targetDoor;
        [SerializeField] private List<RotatingGearPlatform> linkedGears = new List<RotatingGearPlatform>();
        [SerializeField] private List<GameObject> objectsToEnable = new List<GameObject>();

        private SpriteRenderer _sr;
        private CircleCollider2D _col;
        private Sprite _spriteOff;
        private Sprite _spriteLit;
        private float _charge;
        private bool _hasPlayedIgniteSfx;

        public float Charge => _charge;

        public void Configure(
            float radius = 2.4f,
            bool stayLit = true,
            AncientDoor door = null,
            List<RotatingGearPlatform> gears = null,
            List<GameObject> enableObjects = null)
        {
            activationRadius = radius;
            stayLitOnceCharged = stayLit;
            targetDoor = door;
            if (gears != null) linkedGears = gears;
            if (enableObjects != null) objectsToEnable = enableObjects;

            EnsureInitialized();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_col == null) _col = GetComponent<CircleCollider2D>();

            _spriteOff = AssetRegistry.GetSprite("World3/torch_lens_off", 32f);
            _spriteLit = AssetRegistry.GetSprite("World3/torch_lens_lit", 32f);

            if (_sr != null)
            {
                _sr.sortingOrder = 5;
                _sr.sprite = isActivated ? _spriteLit : _spriteOff;
            }

            if (_col != null)
            {
                _col.radius = activationRadius;
                _col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isActivated && stayLitOnceCharged) return;

            bool isTorchNear = CheckTorchProximity();

            if (isTorchNear)
            {
                _charge = Mathf.Clamp01(_charge + (Time.deltaTime / Mathf.Max(0.1f, chargeDuration)));
            }
            else if (!stayLitOnceCharged)
            {
                _charge = Mathf.Clamp01(_charge - (Time.deltaTime / Mathf.Max(0.1f, chargeDuration * 1.5f)));
            }

            if (_charge >= 0.98f && !isActivated)
            {
                SetActivated(true);
                if (_sr != null && _spriteLit != null) _sr.sprite = _spriteLit;

                if (!_hasPlayedIgniteSfx)
                {
                    _hasPlayedIgniteSfx = true;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("lens_hum", 0.85f, 1.0f);
                    }
                }

                ApplyTargets(true);
            }
            else if (_charge <= 0.05f && isActivated && !stayLitOnceCharged)
            {
                SetActivated(false);
                _hasPlayedIgniteSfx = false;
                if (_sr != null && _spriteOff != null) _sr.sprite = _spriteOff;

                ApplyTargets(false);
            }
        }

        private bool CheckTorchProximity()
        {
            TorchController torch = (GameManager.Instance != null && GameManager.Instance.Levels != null && GameManager.Instance.Levels.Torch != null)
                ? GameManager.Instance.Levels.Torch
                : FindFirstObjectByType<TorchController>();

            if (torch == null) return false;

            float dist = Vector2.Distance(transform.position, torch.transform.position);
            return dist <= activationRadius;
        }

        private void ApplyTargets(bool active)
        {
            if (targetDoor != null)
            {
                if (active) targetDoor.Open();
                else targetDoor.Close();
            }

            foreach (var g in linkedGears)
            {
                if (g != null) g.SetPaused(!active);
            }

            foreach (var go in objectsToEnable)
            {
                if (go != null) go.SetActive(active);
            }
        }
    }
}
