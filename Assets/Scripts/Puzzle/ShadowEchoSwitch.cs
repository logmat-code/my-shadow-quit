using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// World 5 switch responsive solely to Shadow's metaphysical presence.
    /// Used in cooperative abyssal puzzles where the Player cannot reach or trigger the mechanism.
    /// </summary>
    public class ShadowEchoSwitch : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private AncientDoor linkedDoor;
        [SerializeField] private AbyssalBridge linkedBridge;

        [Header("Visuals")]
        [SerializeField] private Sprite echoOffSprite;
        [SerializeField] private Sprite echoOnSprite;

        private SpriteRenderer sr;
        private bool isTriggered = false;

        public bool IsTriggered => isTriggered;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (echoOffSprite == null)
                echoOffSprite = Resources.Load<Sprite>("Sprites/World5/echo_switch_off");
            if (echoOnSprite == null)
                echoOnSprite = Resources.Load<Sprite>("Sprites/World5/echo_switch_on");
            UpdateVisual();
        }

        public void Configure(AncientDoor door, AbyssalBridge bridge = null)
        {
            linkedDoor = door;
            linkedBridge = bridge;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Shadow"))
            {
                SetActivated(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Shadow"))
            {
                SetActivated(false);
            }
        }

        private void SetActivated(bool active)
        {
            isTriggered = active;
            UpdateVisual();
            if (linkedDoor != null)
            {
                if (active) linkedDoor.Open();
                else linkedDoor.Close();
            }
            if (linkedBridge != null)
            {
                linkedBridge.SetBridgeActive(active);
            }
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                sr.sprite = isTriggered ? echoOnSprite : echoOffSprite;
            }
        }
    }
}
