using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Smooth side-scrolling 2D camera controller with level boundaries,
    /// subtle screen shake, and multi-layer pixel-art parallax background scrolling.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float smoothTime = 0.16f;
        [SerializeField] private Vector2 followOffset = new Vector2(0f, 0.65f);
        [SerializeField] private Vector2 minBounds = new Vector2(-8f, -3f);
        [SerializeField] private Vector2 maxBounds = new Vector2(18f, 6f);

        private Transform _target;
        private Vector3 _velocity;
        private float _shakeTimer;
        private float _shakeIntensity;

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

        public void TriggerShake(float duration = 0.25f, float intensity = 0.12f)
        {
            _shakeTimer = duration;
            _shakeIntensity = intensity;
        }

        private void Awake()
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.backgroundColor = new Color(0.031f, 0.039f, 0.071f, 1f);
            _lastCamPos = transform.position;
        }

        public void BuildParallaxLayers()
        {
            DestroyLayerList(_farLayer);
            DestroyLayerList(_midLayer);
            DestroyLayerList(_nearLayer);

            Sprite farSp = AssetRegistry.GetSprite("Environment/parallax_far", 16f);
            Sprite midSp = AssetRegistry.GetSprite("Environment/parallax_mid", 16f);
            Sprite nearSp = AssetRegistry.GetSprite("Environment/parallax_near", 16f);

            for (int i = -1; i <= 2; i++)
            {
                float xPos = i * 16f;
                _farLayer.Add(CreateLayerTile("ParallaxFar_" + i, farSp, new Vector3(xPos, 1.5f, 10f), -30, new Color(0.85f, 0.85f, 0.95f, 1f)));
                _midLayer.Add(CreateLayerTile("ParallaxMid_" + i, midSp, new Vector3(xPos, 1.2f, 10f), -20, new Color(0.9f, 0.85f, 1f, 0.92f)));
                _nearLayer.Add(CreateLayerTile("ParallaxNear_" + i, nearSp, new Vector3(xPos, 1.0f, 10f), -10, new Color(1f, 1f, 1f, 0.65f)));
            }
        }

        private void DestroyLayerList(List<Transform> list)
        {
            foreach (Transform t in list)
            {
                if (t != null)
                {
                    if (Application.isPlaying) Destroy(t.gameObject);
                    else DestroyImmediate(t.gameObject);
                }
            }
            list.Clear();
        }

        private Transform CreateLayerTile(string objName, Sprite sp, Vector3 pos, int sortOrder, Color tint)
        {
            GameObject go = new GameObject(objName);
            go.transform.position = pos;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = sortOrder;
            sr.color = tint;
            return go.transform;
        }

        private void LateUpdate()
        {
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
