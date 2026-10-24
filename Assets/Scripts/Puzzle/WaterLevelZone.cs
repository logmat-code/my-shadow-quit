using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Manages the local water tide level within a World 4 cathedral room.
    /// Can raise or lower the tide when triggered by Tide switches.
    /// </summary>
    public class WaterLevelZone : MonoBehaviour
    {
        [Header("Tide Levels")]
        [SerializeField] private float lowWaterY = -2f;
        [SerializeField] private float highWaterY = 2f;
        [SerializeField] private float transitionSpeed = 2.5f;
        [SerializeField] private bool startsHigh = false;

        private float currentWaterY;
        private float targetWaterY;
        private Transform surfaceVisual;
        private BoxCollider2D waterCollider;

        public float CurrentWaterY => currentWaterY;
        public float TargetWaterY => targetWaterY;
        public bool IsHigh => Mathf.Approximately(targetWaterY, highWaterY);

        private static readonly List<WaterLevelZone> activeZones = new List<WaterLevelZone>();

        public static WaterLevelZone GetActiveZone()
        {
            return activeZones.Count > 0 ? activeZones[0] : null;
        }

        private void Awake()
        {
            currentWaterY = startsHigh ? highWaterY : lowWaterY;
            targetWaterY = currentWaterY;
            surfaceVisual = transform.Find("WaterSurfaceVisual");
            waterCollider = GetComponent<BoxCollider2D>();
            UpdateVisualAndCollider();
        }

        private void OnEnable()
        {
            if (!activeZones.Contains(this))
                activeZones.Add(this);
        }

        private void OnDisable()
        {
            activeZones.Remove(this);
        }

        private void Update()
        {
            if (!Mathf.Approximately(currentWaterY, targetWaterY))
            {
                currentWaterY = Mathf.MoveTowards(currentWaterY, targetWaterY, transitionSpeed * Time.deltaTime);
                UpdateVisualAndCollider();
            }
        }

        public void SetTideState(bool high)
        {
            targetWaterY = high ? highWaterY : lowWaterY;
        }

        public void ToggleTide()
        {
            SetTideState(!IsHigh);
        }

        public void Configure(float lowY, float highY, bool highAtStart, float speed = 2.5f)
        {
            lowWaterY = lowY;
            highWaterY = highY;
            startsHigh = highAtStart;
            transitionSpeed = speed;
            currentWaterY = startsHigh ? highWaterY : lowWaterY;
            targetWaterY = currentWaterY;
            UpdateVisualAndCollider();
        }

        private void UpdateVisualAndCollider()
        {
            if (surfaceVisual != null)
            {
                surfaceVisual.position = new Vector3(transform.position.x, currentWaterY, transform.position.z);
            }
            if (waterCollider != null)
            {
                float height = Mathf.Max(0.5f, currentWaterY - (lowWaterY - 5f));
                waterCollider.size = new Vector2(waterCollider.size.x, height);
                waterCollider.offset = new Vector2(0f, currentWaterY - transform.position.y - (height * 0.5f));
            }
        }
    }
}
