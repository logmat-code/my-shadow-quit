using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// World 4 submerged cathedral grate.
    /// Shadow passes through it freely (since shadow is immaterial/ethereal),
    /// but Player and solid blocks are obstructed until a mechanism opens or raises it.
    /// </summary>
    public class SubmergedGratePassage : MonoBehaviour
    {
        [Header("Grate Settings")]
        [SerializeField] private Collider2D solidCollider;
        [SerializeField] private bool isOpen = false;
        [SerializeField] private float openOffset = 2.5f;

        private Vector3 closedPos;
        private Vector3 openPos;

        private void Awake()
        {
            if (solidCollider == null)
            {
                solidCollider = GetComponent<Collider2D>();
            }
            closedPos = transform.position;
            openPos = closedPos + new Vector3(0f, openOffset, 0f);
            UpdatePassagePhysics();
        }

        private void Start()
        {
            // Ignore collisions with Shadow specifically so Shadow can always pass
            GameObject shadowObj = GameObject.FindWithTag("Shadow");
            if (shadowObj != null && solidCollider != null)
            {
                Collider2D shadowCol = shadowObj.GetComponent<Collider2D>();
                if (shadowCol != null)
                {
                    Physics2D.IgnoreCollision(solidCollider, shadowCol, true);
                }
            }
        }

        private void UpdatePassagePhysics()
        {
            if (solidCollider != null)
            {
                solidCollider.enabled = !isOpen;
            }
            transform.position = isOpen ? openPos : closedPos;
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            UpdatePassagePhysics();
        }

        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);
    }
}
