using UnityEngine;
using UnityEngine.UI;
using MyShadowQuit.Core;

namespace MyShadowQuit.Narrative
{
    /// <summary>
    /// Attach to an NPC GameObject. Shows compact, pixel-art coherent floating interactive prompt [E] TALK
    /// when player is in range, animates a subtle breathing bob, and triggers associated DialogueSequence.
    /// Each NPC's dialogue can be experienced only once per saved playthrough.
    /// </summary>
    public class NPCInteraction : MonoBehaviour
    {
        [SerializeField] private string npcName = "Traveler";
        [SerializeField] private string spriteResourceName;
        [SerializeField] private DialogueSequence dialogueSequence;
        [SerializeField] private float interactionRadius = 1.7f;

        private GameObject _promptVisual;
        private Text _promptText;
        private SpriteRenderer _sr;
        private Vector3 _basePos;
        private bool _playerInRange = false;

        public string NpcId => (dialogueSequence != null && !string.IsNullOrEmpty(dialogueSequence.dialogueId)) 
            ? dialogueSequence.dialogueId 
            : npcName;

        public bool IsConversationCompleted()
        {
            if (dialogueSequence == null) return false;

            // Checked against dialogueId first
            if (!string.IsNullOrEmpty(dialogueSequence.dialogueId) && JournalService.IsNPCConversationCompleted(dialogueSequence.dialogueId))
            {
                return true;
            }

            // Reconcile with unlocked lore entry if already obtained
            if (!string.IsNullOrEmpty(dialogueSequence.unlockedLoreEntryId) && JournalService.IsUnlocked(dialogueSequence.unlockedLoreEntryId))
            {
                return true;
            }

            return false;
        }

        public void Initialize(string name, string spriteRes, DialogueSequence sequence)
        {
            npcName = name;
            spriteResourceName = spriteRes;
            dialogueSequence = sequence;

            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            
            Sprite loadedSprite = Resources.Load<Sprite>("Sprites/Narrative/" + spriteRes);
            if (loadedSprite != null)
            {
                _sr.sprite = loadedSprite;
            }
            _sr.sortingOrder = 9;

            CreatePromptIndicator();
        }

        private void Awake()
        {
            _basePos = transform.position;
            CreatePromptIndicator();
        }

        private void CreatePromptIndicator()
        {
            if (_promptVisual != null) return;

            // Create floating world-space prompt canvas positioned discreetly right above NPC head
            _promptVisual = new GameObject("InteractPrompt");
            _promptVisual.transform.SetParent(transform, false);
            _promptVisual.transform.localPosition = new Vector3(0f, 0.95f, 0f);

            Canvas canvas = _promptVisual.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            _promptVisual.AddComponent<CanvasScaler>();

            RectTransform crt = _promptVisual.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(100f, 32f);
            // Compact, pixel-art coherent scale
            _promptVisual.transform.localScale = new Vector3(0.009f, 0.009f, 1f);

            // Subtle dark badge background
            GameObject bgObj = new GameObject("BadgeBg", typeof(RectTransform), typeof(Image), typeof(Outline));
            bgObj.transform.SetParent(_promptVisual.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.06f, 0.04f, 0.12f, 0.88f);
            Outline outline = bgObj.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.82f, 0.35f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            // Text: [E] TALK / [TAP] TALK
            GameObject txtObj = new GameObject("BadgeText", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(bgObj.transform, false);
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            _promptText = txtObj.GetComponent<Text>();
            try
            {
                _promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") 
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch { }
            if (_promptText.font == null) _promptText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            _promptText.fontSize = 17;
            _promptText.fontStyle = FontStyle.Bold;
            _promptText.alignment = TextAnchor.MiddleCenter;
            _promptText.color = new Color(1f, 0.94f, 0.72f, 1f);

#if UNITY_ANDROID || UNITY_IOS
            _promptText.text = "[TAP] TALK";
#else
            _promptText.text = "[E] TALK";
#endif

            _promptVisual.SetActive(false);
        }

        private void Update()
        {
            if (_basePos == Vector3.zero) _basePos = transform.position;

            // Idle breathing animation for NPC
            float breath = Mathf.Sin(Time.time * 2.2f) * 0.025f;
            transform.position = new Vector3(_basePos.x, _basePos.y + breath, _basePos.z);

            // Once conversation is completed, the prompt disappears permanently and no more talks can be initiated
            if (IsConversationCompleted())
            {
                if (_promptVisual != null && _promptVisual.activeSelf)
                {
                    _promptVisual.SetActive(false);
                }
                return;
            }

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
                    float bob = Mathf.Sin(Time.time * 3.5f) * 0.04f;
                    _promptVisual.transform.localPosition = new Vector3(0f, 0.95f + bob, 0f);
                }
            }

            // Trigger dialogue on dedicated interact key [E] or touch interact
            if (_playerInRange && GameInput.GetInteractDown())
            {
                if (DialogueController.Instance != null && !DialogueController.Instance.IsDialogueActive)
                {
                    DialogueController.Instance.StartDialogue(dialogueSequence, onComplete: () =>
                    {
                        if (_promptVisual != null)
                        {
                            _promptVisual.SetActive(false);
                        }
                    });
                }
            }
        }
    }
}
