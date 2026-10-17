using System;
using UnityEngine;
using MyShadowQuit.Core;
using MyShadowQuit.Puzzle;
using MyShadowQuit.Cosmetics;

namespace MyShadowQuit.Entities
{
    public enum TorchState
    {
        WorldGround,
        CarriedByPlayer,
        PlacedInPedestal
    }

    /// <summary>
    /// Modular TorchController managing the central flame, illumination radius,
    /// animated flame & halo sprites, ember particles, and light-reactive mechanisms.
    /// </summary>
    public class TorchController : MonoBehaviour
    {
        public static event Action<TorchController, TorchState> OnTorchStateChanged;

        [Header("Torch Settings")]
        [SerializeField] private float lightRadius = 4.6f;
        [SerializeField] private float pickupRadius = 1.35f;

        private TorchState _state = TorchState.WorldGround;
        private Transform _carrier;
        private TorchPedestal _currentPedestal;

        private SpriteRenderer _torchRenderer;
        private SpriteRenderer _haloRenderer;
        private ParticleSystem _emberParticles;
        private Sprite[] _flameFrames;
        private float _animTimer;
        private int _currentFrame;

        private Vector2 _aimOffset = new Vector2(0.45f, 0.15f);
        private float _endingRadiusScale = 1f;
        private float _endingIntensity = 1f;

        public TorchState State => _state;
        public float LightRadius => lightRadius * _endingRadiusScale;
        public float PickupRadius => pickupRadius;
        public TorchPedestal CurrentPedestal => _currentPedestal;

        private void Awake()
        {
            _torchRenderer = GetComponent<SpriteRenderer>();
            if (_torchRenderer == null)
            {
                _torchRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            _torchRenderer.sortingOrder = 15;

            CreateLightHalo();
            CreateEmberParticles();

            string equipped = SkinEquipmentService.Instance != null
                ? SkinEquipmentService.Instance.EquippedTorchSkinId
                : PlayerPrefs.GetString(SkinEquipmentService.KeyEquippedTorch, "torch_original_flame");
            ApplySkin(equipped);
        }

        private string _activeSkinId = "torch_original_flame";
        public string ActiveSkinId => _activeSkinId;

        public void ApplySkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId)) skinId = "torch_original_flame";
            _activeSkinId = skinId;

            bool isOriginal = (skinId == "torch_original_flame");
            string prefix = isOriginal ? "Torch/torch_item" : $"Skins/Torch/{skinId}/flame";

            _flameFrames = AssetRegistry.GetAnimationFrames(prefix, 4);
            if (_flameFrames != null && _flameFrames.Length > 0 && _torchRenderer != null)
            {
                _torchRenderer.sprite = _flameFrames[0];
            }

            SkinDefinition def = SkinCatalog.GetSkin(skinId);
            if (def != null)
            {
                if (_haloRenderer != null)
                {
                    _haloRenderer.color = def.HaloColor;
                }
                if (_emberParticles != null)
                {
                    var main = _emberParticles.main;
                    main.startColor = def.EmberColor;
                }
            }
        }

        private void CreateLightHalo()
        {
            GameObject haloObj = new GameObject("TorchLightHalo");
            haloObj.transform.SetParent(transform, false);
            haloObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            _haloRenderer = haloObj.AddComponent<SpriteRenderer>();
            _haloRenderer.sprite = AssetRegistry.GetSprite("Torch/light_halo", 32f, FilterMode.Bilinear);
            _haloRenderer.sortingOrder = 5;
            _haloRenderer.color = new Color(1f, 0.82f, 0.45f, 0.52f);
            float scale = (lightRadius * 2f) / 4f; // 128px at 32PPU = 4 units base diameter
            haloObj.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void CreateEmberParticles()
        {
            GameObject pObj = new GameObject("EmberParticles");
            pObj.transform.SetParent(transform, false);
            pObj.transform.localPosition = new Vector3(0f, 0.25f, 0f);

            _emberParticles = pObj.AddComponent<ParticleSystem>();
            var main = _emberParticles.main;
            main.startLifetime = 1.1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new Color(1f, 0.68f, 0.2f, 0.9f);
            main.gravityModifier = -0.25f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 32;

            var emission = _emberParticles.emission;
            emission.rateOverTime = 10f;

            var shape = _emberParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.12f;

            var renderer = pObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 16;
            Sprite emberSprite = AssetRegistry.GetSprite("Torch/ember_particle", 32f);
            if (emberSprite != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = emberSprite.texture;
                renderer.material = mat;
            }
        }

        private void Update()
        {
            // Animate flame frames
            _animTimer += Time.deltaTime;
            if (_animTimer >= 0.1f)
            {
                _animTimer = 0f;
                _currentFrame = (_currentFrame + 1) % 4;
                if (_flameFrames != null && _flameFrames[_currentFrame] != null)
                {
                    _torchRenderer.sprite = _flameFrames[_currentFrame];
                }
            }

            // Organic warm flicker on light halo modulated by ending sequence glow/attenuation
            if (_haloRenderer != null)
            {
                float flicker = 1f + Mathf.Sin(Time.time * 11.3f) * 0.04f + Mathf.Cos(Time.time * 19.7f) * 0.03f;
                float baseScale = (lightRadius * _endingRadiusScale * 2f) / 4f;
                _haloRenderer.transform.localScale = new Vector3(baseScale * flicker, baseScale * flicker, 1f);
                float alpha = (0.46f + 0.05f * Mathf.Sin(Time.time * 8.5f)) * _endingIntensity;
                _haloRenderer.color = new Color(1f, 0.80f, 0.42f, Mathf.Clamp01(alpha));
            }

            if (_torchRenderer != null)
            {
                _torchRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(_endingIntensity));
            }

            // Follow carrier if carried
            if (_state == TorchState.CarriedByPlayer && _carrier != null)
            {
                transform.position = (Vector2)_carrier.position + _aimOffset;
                // Hide standalone sprite while protagonist's torch-holding sprite renders the torch in hand
                _torchRenderer.enabled = false;
            }
            else if (_state == TorchState.PlacedInPedestal)
            {
                // Pedestal has its own lit sprite, keep halo & particles active
                _torchRenderer.enabled = false;
            }
            else
            {
                _torchRenderer.enabled = true;
            }
        }

        public void SetEndingGlow(float radiusScale, float intensityAlpha)
        {
            _endingRadiusScale = Mathf.Max(0.05f, radiusScale);
            _endingIntensity = Mathf.Clamp(intensityAlpha, 0f, 1.5f);

            if (_haloRenderer != null)
            {
                float baseScale = (lightRadius * _endingRadiusScale * 2f) / 4f;
                _haloRenderer.transform.localScale = new Vector3(baseScale, baseScale, 1f);
                _haloRenderer.color = new Color(1f, 0.82f, 0.44f, Mathf.Clamp01(0.52f * _endingIntensity));
            }

            if (_emberParticles != null)
            {
                var emission = _emberParticles.emission;
                emission.rateOverTime = 10f * Mathf.Clamp01(_endingIntensity);
            }
        }

        public void ResetEndingGlow()
        {
            SetEndingGlow(1f, 1f);
        }

        public void SetAimDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                _aimOffset = new Vector2(dir.x * 0.42f, Mathf.Clamp(dir.y * 0.35f + 0.15f, -0.1f, 0.55f));
            }
        }

        public void PickUp(Transform carrier)
        {
            if (_currentPedestal != null)
            {
                _currentPedestal.RemoveTorch();
                _currentPedestal = null;
            }

            _carrier = carrier;
            _state = TorchState.CarriedByPlayer;
            if (_emberParticles != null) _emberParticles.Play();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.85f, 1.05f);
            }

            OnTorchStateChanged?.Invoke(this, _state);
        }

        public void PlaceInPedestal(TorchPedestal pedestal)
        {
            _carrier = null;
            _currentPedestal = pedestal;
            _state = TorchState.PlacedInPedestal;
            transform.position = pedestal.TorchMountPoint;
            pedestal.AcceptTorch(this);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.9f, 0.95f);
            }

            OnTorchStateChanged?.Invoke(this, _state);
        }

        public void DropOnGround(Vector2 dropPosition)
        {
            if (_currentPedestal != null)
            {
                _currentPedestal.RemoveTorch();
                _currentPedestal = null;
            }

            _carrier = null;
            _state = TorchState.WorldGround;
            transform.position = dropPosition;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("torch_ignite", 0.7f, 0.85f);
            }

            OnTorchStateChanged?.Invoke(this, _state);
        }

        public bool IsPointIlluminated(Vector2 worldPoint)
        {
            return Vector2.Distance(transform.position, worldPoint) <= lightRadius;
        }
    }
}
