using UnityEngine;
using MyShadowQuit.Core;

namespace MyShadowQuit.Narrative
{
    /// <summary>
    /// Attach to an NPC GameObject. Shows interaction prompt [E / Tap] when player is in range,
    /// and triggers associated DialogueSequence.
    /// </summary>
    public class NPCInteraction : MonoBehaviour
    {
        [SerializeField] private string npcName = "Traveler";
        [SerializeField] private string spriteResourceName;
        [SerializeField] private DialogueSequence dialogueSequence;
        [SerializeField] private float interactionRadius = 1.6f;

        private GameObject _promptVisual;
        private bool _playerInRange = false;

        public void Initialize(string name, string spriteRes, DialogueSequence sequence)
        {
            npcName = name;
            spriteResourceName = spriteRes;
            dialogueSequence = sequence;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/Narrative/" + spriteRes);
            sr.sortingOrder = 9;

            CreatePromptIndicator();
        }

        private void Awake()
        {
            CreatePromptIndicator();
        }

        private void CreatePromptIndicator()
        {
            if (_promptVisual != null) return;

            _promptVisual = new GameObject("InteractPrompt");
            _promptVisual.transform.SetParent(transform, false);
            _promptVisual.transform.localPosition = new Vector3(0f, 1.1f, 0f);

            var sr = _promptVisual.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/UI/button_interact");
            sr.sortingOrder = 15;
            _promptVisual.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            _promptVisual.SetActive(false);
        }

        private void Update()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            _playerInRange = (dist <= interactionRadius);

            if (_promptVisual != null)
            {
                bool show = _playerInRange && (DialogueController.Instance == null || !DialogueController.Instance.IsDialogueActive);
                _promptVisual.SetActive(show);

                if (show)
                {
                    float bob = Mathf.Sin(Time.time * 4f) * 0.08f;
                    _promptVisual.transform.localPosition = new Vector3(0f, 1.1f + bob, 0f);
                }
            }

            if (_playerInRange && GameInput.GetInteractDown())
            {
                if (DialogueController.Instance != null && !DialogueController.Instance.IsDialogueActive)
                {
                    DialogueController.Instance.StartDialogue(dialogueSequence);
                }
            }
        }
    }
}
