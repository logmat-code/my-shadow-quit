using UnityEngine;

namespace MyShadowQuit.Puzzle
{
    /// <summary>
    /// World 5 pedestal/anchor where placing or carrying the Torch locks nearby
    /// reality-shifting platforms into an anchored, stable state.
    /// </summary>
    public class FlameAnchor : MonoBehaviour
    {
        [Header("Anchor Settings")]
        [SerializeField] private float anchorRadius = 3.5f;
        [SerializeField] private DualRealityPlatform[] linkedPlatforms;
        [SerializeField] private bool lockToLight = true;

        [Header("Visuals")]
        [SerializeField] private Sprite anchorSprite;

        private SpriteRenderer sr;
        private bool isAnchored = false;

        public bool IsAnchored => isAnchored;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (anchorSprite == null)
                anchorSprite = Resources.Load<Sprite>("Sprites/World5/flame_anchor");
        }

        public void Configure(DualRealityPlatform[] platforms, float radius = 3.5f, bool lockLight = true)
        {
            linkedPlatforms = platforms;
            anchorRadius = radius;
            lockToLight = lockLight;
        }

        private void Update()
        {
            Entities.TorchController[] torches = FindObjectsByType<Entities.TorchController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            bool torchNearby = false;

            for (int i = 0; i < torches.Length; i++)
            {
                if (torches[i] != null)
                {
                    if (Vector2.Distance(transform.position, torches[i].transform.position) <= anchorRadius)
                    {
                        torchNearby = true;
                        break;
                    }
                }
            }

            if (torchNearby != isAnchored)
            {
                isAnchored = torchNearby;
                if (linkedPlatforms != null)
                {
                    for (int i = 0; i < linkedPlatforms.Length; i++)
                    {
                        if (linkedPlatforms[i] != null)
                        {
                            linkedPlatforms[i].SetLightState(lockToLight ? isAnchored : !isAnchored);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isAnchored ? Color.yellow : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, anchorRadius);
        }
    }
}
