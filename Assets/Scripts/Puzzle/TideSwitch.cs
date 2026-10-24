using System.Collections;
using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// Interactive tide lever in World 4. Can be activated by Player interact or Torch proximity
    /// to toggle the room's water level.
    /// </summary>
    public class TideSwitch : MonoBehaviour
    {
        [Header("Target Zone")]
        [SerializeField] private WaterLevelZone waterZone;
        [SerializeField] private bool targetHighState = true;
        [SerializeField] private bool toggleMode = true;

        [Header("Visuals")]
        [SerializeField] private Sprite switchOffSprite;
        [SerializeField] private Sprite switchOnSprite;

        private SpriteRenderer sr;
        private bool isActivated = false;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (switchOffSprite == null)
            {
                switchOffSprite = Resources.Load<Sprite>("Sprites/World4/tide_switch_off");
            }
            if (switchOnSprite == null)
            {
                switchOnSprite = Resources.Load<Sprite>("Sprites/World4/tide_switch_on");
            }
            UpdateVisual();
        }

        private void Start()
        {
            if (waterZone == null)
            {
                waterZone = WaterLevelZone.GetActiveZone();
            }
        }

        public void Configure(WaterLevelZone zone, bool toggle = true, bool highOnTrigger = true)
        {
            waterZone = zone;
            toggleMode = toggle;
            targetHighState = highOnTrigger;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Shadow") || collision.GetComponent<Entities.TorchController>() != null)
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (toggleMode)
            {
                isActivated = !isActivated;
                if (waterZone != null) waterZone.ToggleTide();
            }
            else
            {
                if (isActivated) return;
                isActivated = true;
                if (waterZone != null) waterZone.SetTideState(targetHighState);
            }

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                sr.sprite = isActivated ? switchOnSprite : switchOffSprite;
            }
        }
    }
}
