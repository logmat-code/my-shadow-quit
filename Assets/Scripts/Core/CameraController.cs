using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Smooth side-scrolling 2D camera controller with level boundaries,
    /// subtle screen shake, multi-layer pixel-art parallax background scrolling,
    /// and strict 16:9 landscape viewport enforcement (automatic letterbox/pillarbox
    /// so non-16:9 or portrait containers never crop horizontal gameplay or distort sprites).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public const float TargetAspectRatio = 16f / 9f;
        public const float DefaultOrthoSize = 4.5f;

        [Header("Follow Settings")]
        [SerializeField] private float smoothTime = 0.16f;
        [SerializeField] private Vector2 followOffset = new Vector2(0f, 0.65f);
        [SerializeField] private Vector2 minBounds = new Vector2(-2.5f, -1.0f);
        [SerializeField] private Vector2 maxBounds = new Vector2(14.5f, 3.5f);

        private Camera _cam;
        private Camera _backgroundClearCam;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private Transform _target;
        private Vector3 _velocity;
        private float _shakeTimer;
        private float _shakeIntensity;
        private float _targetOrthoSize = DefaultOrthoSize;
        private float _zoomSpeed = 1.5f;

        private readonly List<Transform> _farLayer = new List<Transform>();
        private readonly List<Transform> _midLayer = new List<Transform>();
        private readonly List<Transform> _nearLayer = new List<Transform>();
        private Vector3 _lastCamPos;

        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void SetTargetZoom(float targetOrthoSize, float zoomSpeed = 1.2f)
        {
            _targetOrthoSize = Mathf.Clamp(targetOrthoSize, 2.5f, 6.0f);
            _zoomSpeed = Mathf.Max(0.1f, zoomSpeed);
        }

        public void ResetZoom()
        {
            _targetOrthoSize = DefaultOrthoSize;
            _zoomSpeed = 2.5f;
            if (_cam == null) _cam = GetComponent<Camera>();
            if (_cam != null) _cam.orthographicSize = DefaultOrthoSize;
        }

        public void TriggerShake(float duration = 0.25f, float intensity = 0.12f)
        {
            _shakeTimer = duration;
            _shakeIntensity = intensity;
        }

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = DefaultOrthoSize; // 9 world units tall x 16 world units wide at 16:9
            _targetOrthoSize = DefaultOrthoSize;
            _cam.backgroundColor = new Color(0.031f, 0.039f, 0.071f, 1f);
            _lastCamPos = transform.position;

            EnsureLandscapeResolutionAndViewport(force: true);
        }

        public void EnsureLandscapeResolutionAndViewport(bool force = false)
        {
            if (_cam == null) _cam = GetComponent<Camera>();

#if !UNITY_EDITOR && !UNITY_WEBGL
            if (Screen.height > Screen.width)
            {
                Screen.SetResolution(1920, 1080, Screen.fullScreenMode);
            }
#endif
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            if (!force && Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight)
            {
                return;
            }

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            if (_lastScreenWidth <= 0 || _lastScreenHeight <= 0) return;

            float windowAspect = (float)_lastScreenWidth / _lastScreenHeight;
            float scaleHeight = windowAspect / TargetAspectRatio;

            if (Mathf.Abs(scaleHeight - 1f) < 0.01f)
            {
                _cam.rect = new Rect(0f, 0f, 1f, 1f);
            }
            else if (scaleHeight < 1.0f)
            {
                // Window is narrower/taller than 16:9 (e.g., 4:3, 16:10, or portrait window):
                // Apply top/bottom letterbox bars so the full 16:9 horizontal width is preserved without cropping!
                Rect rect = _cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0f;
                rect.y = (1.0f - scaleHeight) * 0.5f;
                _cam.rect = rect;
                EnsureLetterboxClearCamera();
            }
            else
            {
                // Window is wider than 16:9 (e.g., 21:9 ultrawide): apply pillarbox bars
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = _cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) * 0.5f;
                rect.y = 0f;
                _cam.rect = rect;
                EnsureLetterboxClearCamera();
            }
        }

        private void EnsureLetterboxClearCamera()
        {
            if (!Application.isPlaying) return;
            if (_backgroundClearCam != null) return;

            GameObject bgCamObj = new GameObject("LetterboxClearCamera");
            DontDestroyOnLoad(bgCamObj);
            _backgroundClearCam = bgCamObj.AddComponent<Camera>();
            _backgroundClearCam.depth = -100;
            _backgroundClearCam.cullingMask = 0;
            _backgroundClearCam.clearFlags = CameraClearFlags.SolidColor;
            _backgroundClearCam.backgroundColor = new Color(0.02f, 0.025f, 0.045f, 1f);
        }

        private GameObject _parallaxRoot;

        public void BuildParallaxLayers(WorldEnvironmentProfile profile = null)
        {
            ClearParallaxLayers();

            if (!Application.isPlaying)
            {
                return;
            }

            if (profile == null)
            {
                int worldIdx = (GameManager.Instance != null) ? GameManager.Instance.CurrentWorldIndex : 0;
                WorldDefinition wDef = WorldCatalog.GetWorld(worldIdx);
                profile = wDef?.EnvironmentProfile ?? new WorldEnvironmentProfile();
            }

            _parallaxRoot = new GameObject("ParallaxBackground");

            Sprite farSp = AssetRegistry.GetSprite(profile.FarParallaxSpritePath, 16f) ?? AssetRegistry.GetSprite("Environment/parallax_far", 16f);
            Sprite midSp = AssetRegistry.GetSprite(profile.MidParallaxSpritePath, 16f) ?? AssetRegistry.GetSprite("Environment/parallax_mid", 16f);
            Sprite nearSp = AssetRegistry.GetSprite(profile.NearParallaxSpritePath, 16f) ?? AssetRegistry.GetSprite("Environment/parallax_near", 16f);

            bool isForest = profile.FarParallaxSpritePath.Contains("forest") || profile.FarParallaxSpritePath.Contains("World2");
            bool isClocktower = profile.FarParallaxSpritePath.Contains("clocktower") || profile.FarParallaxSpritePath.Contains("World3");

            Color farColor = isClocktower
                ? new Color(0.55f, 0.60f, 0.75f, 1f)
                : (isForest ? new Color(0.40f, 0.48f, 0.58f, 1f) : new Color(0.85f, 0.85f, 0.95f, 1f));

            Color midColor = isClocktower
                ? new Color(0.60f, 0.58f, 0.70f, 0.90f)
                : (isForest ? new Color(0.35f, 0.45f, 0.42f, 0.88f) : new Color(0.9f, 0.85f, 1f, 0.92f));

            Color nearColor = isClocktower
                ? new Color(0.45f, 0.42f, 0.55f, 0.75f)
                : (isForest ? new Color(0.30f, 0.40f, 0.38f, 0.65f) : new Color(1f, 1f, 1f, 0.65f));

            if (_cam != null)
            {
                _cam.backgroundColor = isClocktower
                    ? new Color(0.016f, 0.020f, 0.035f, 1f)
                    : (isForest ? new Color(0.012f, 0.020f, 0.030f, 1f) : new Color(0.031f, 0.039f, 0.071f, 1f));
            }

            for (int i = -1; i <= 3; i++)
            {
                float xPos = i * 16f;
                _farLayer.Add(CreateLayerTile("ParallaxFar_" + i, farSp, new Vector3(xPos, 1.5f, 10f), -30, farColor));
                _midLayer.Add(CreateLayerTile("ParallaxMid_" + i, midSp, new Vector3(xPos, 1.2f, 10f), -20, midColor));
                _nearLayer.Add(CreateLayerTile("ParallaxNear_" + i, nearSp, new Vector3(xPos, 1.0f, 10f), -10, nearColor));
            }
        }

        public void ClearParallaxLayers()
        {
            DestroyLayerList(_farLayer);
            DestroyLayerList(_midLayer);
            DestroyLayerList(_nearLayer);

            if (_parallaxRoot != null)
            {
                if (Application.isPlaying)
                {
                    _parallaxRoot.SetActive(false);
                    Destroy(_parallaxRoot);
                }
                else
                {
                    DestroyImmediate(_parallaxRoot);
                }
                _parallaxRoot = null;
            }

            // Also clean up any orphaned Parallax objects left in the scene
            SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null && sr.gameObject != null)
                {
                    string n = sr.gameObject.name;
                    if (n.StartsWith("ParallaxFar_") || n.StartsWith("ParallaxMid_") || n.StartsWith("ParallaxNear_"))
                    {
                        if (Application.isPlaying)
                        {
                            sr.gameObject.SetActive(false);
                            Destroy(sr.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(sr.gameObject);
                        }
                    }
                }
            }
        }

        private void DestroyLayerList(List<Transform> list)
        {
            foreach (Transform t in list)
            {
                if (t != null)
                {
                    if (Application.isPlaying)
                    {
                        t.gameObject.SetActive(false);
                        Destroy(t.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(t.gameObject);
                    }
                }
            }
            list.Clear();
        }

        private Transform CreateLayerTile(string objName, Sprite sp, Vector3 pos, int sortOrder, Color tint)
        {
            GameObject go = new GameObject(objName);
            if (_parallaxRoot != null)
            {
                go.transform.SetParent(_parallaxRoot.transform, false);
            }
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortOrder;
            sr.color = tint;
            return go.transform;
        }

        private void LateUpdate()
        {
            EnsureLandscapeResolutionAndViewport(force: false);

            if (_cam != null && Mathf.Abs(_cam.orthographicSize - _targetOrthoSize) > 0.005f)
            {
                _cam.orthographicSize = Mathf.MoveTowards(_cam.orthographicSize, _targetOrthoSize, Time.deltaTime * _zoomSpeed);
            }

            if (_target != null)
            {
                Vector3 desired = new Vector3(
                    Mathf.Clamp(_target.position.x + followOffset.x, minBounds.x, maxBounds.x),
                    Mathf.Clamp(_target.position.y + followOffset.y, minBounds.y, maxBounds.y),
                    -10f
                );

                Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

                if (_shakeTimer > 0f)
                {
                    _shakeTimer -= Time.deltaTime;
                    smoothed += (Vector3)(Random.insideUnitCircle * _shakeIntensity);
                }

                Vector3 delta = smoothed - _lastCamPos;
                transform.position = smoothed;
                _lastCamPos = smoothed;

                ShiftLayers(_farLayer, delta.x * 0.75f, delta.y * 0.85f);
                ShiftLayers(_midLayer, delta.x * 0.45f, delta.y * 0.60f);
                ShiftLayers(_nearLayer, delta.x * 0.18f, delta.y * 0.25f);
            }
        }

        private void ShiftLayers(List<Transform> layers, float dx, float dy)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] != null)
                {
                    layers[i].position += new Vector3(dx, dy, 0f);
                }
            }
        }
    }
}
