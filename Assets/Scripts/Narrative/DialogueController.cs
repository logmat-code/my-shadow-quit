using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyShadowQuit.Core;

namespace MyShadowQuit.Narrative
{
    /// <summary>
    /// Singleton manager for rendering interactive narrative dialogues.
    /// Uses standard Unity UI components with built-in high-contrast styling
    /// to guarantee 100% visible, crisp text across all platforms without TMP font asset dependency.
    /// Freezes player movement, plays audio blips during typewriter effect,
    /// displays character portraits, and records unlocked lore entries.
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        [SerializeField] private GameObject dialogContainer;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text dialogueBodyText;
        [SerializeField] private Text promptAdvanceText;

        private DialogueSequence _currentSequence;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private Coroutine _typewriterCoroutine;
        private Action _onCompleteCallback;
        private Font _dialogueFont;

        public bool IsDialogueActive => dialogContainer != null && dialogContainer.activeSelf;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureFont();

            if (dialogContainer == null)
            {
                CreateDialogueUIElements();
            }
        }

        private void EnsureFont()
        {
            if (_dialogueFont != null) return;

            try
            {
                _dialogueFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch { }

            if (_dialogueFont == null)
            {
                try
                {
                    _dialogueFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch { }
            }

            if (_dialogueFont == null)
            {
                _dialogueFont = Font.CreateDynamicFontFromOSFont("Arial", 18);
            }
        }

        private void CreateDialogueUIElements()
        {
            EnsureFont();

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canObj = GameObject.Find("UICanvas");
                if (canObj != null) canvas = canObj.GetComponent<Canvas>();
            }

            if (canvas == null) return;

            dialogContainer = new GameObject("DialogueModal", typeof(RectTransform), typeof(Image));
            dialogContainer.transform.SetParent(canvas.transform, false);

            RectTransform rt = dialogContainer.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.04f);
            rt.anchorMax = new Vector2(0.92f, 0.32f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = dialogContainer.GetComponent<Image>();
            bg.color = new Color(0.07f, 0.05f, 0.11f, 0.95f);

            // Gold/violet inner border container
            GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Outline));
            borderObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform borderRt = borderObj.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = new Vector2(2f, 2f);
            borderRt.offsetMax = new Vector2(-2f, -2f);
            Outline outline = borderObj.GetComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.70f, 0.35f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Portrait Frame
            GameObject portFrame = new GameObject("PortraitFrame", typeof(RectTransform), typeof(Image));
            portFrame.transform.SetParent(dialogContainer.transform, false);
            RectTransform pfrt = portFrame.GetComponent<RectTransform>();
            pfrt.anchorMin = new Vector2(0.02f, 0.10f);
            pfrt.anchorMax = new Vector2(0.18f, 0.90f);
            pfrt.offsetMin = Vector2.zero;
            pfrt.offsetMax = Vector2.zero;
            Image frameImg = portFrame.GetComponent<Image>();
            frameImg.color = new Color(0.18f, 0.14f, 0.28f, 0.9f);

            // Portrait Image inside frame
            GameObject portObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portObj.transform.SetParent(portFrame.transform, false);
            RectTransform prt = portObj.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.06f, 0.06f);
            prt.anchorMax = new Vector2(0.94f, 0.94f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            portraitImage = portObj.GetComponent<Image>();
            portraitImage.preserveAspect = true;

            // Speaker Name Text
            GameObject spkObj = new GameObject("SpeakerText", typeof(RectTransform), typeof(Text), typeof(Outline));
            spkObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform srt = spkObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.20f, 0.72f);
            srt.anchorMax = new Vector2(0.96f, 0.94f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            speakerText = spkObj.GetComponent<Text>();
            speakerText.font = _dialogueFont;
            speakerText.fontSize = 22;
            speakerText.color = new Color(1f, 0.88f, 0.42f, 1f); // Warm gold
            speakerText.fontStyle = FontStyle.Bold;
            speakerText.alignment = TextAnchor.MiddleLeft;
            Outline spkOutline = spkObj.GetComponent<Outline>();
            spkOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            spkOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Dialogue Body Text
            GameObject bodyObj = new GameObject("BodyText", typeof(RectTransform), typeof(Text), typeof(Outline));
            bodyObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform brt = bodyObj.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.20f, 0.20f);
            brt.anchorMax = new Vector2(0.96f, 0.72f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            dialogueBodyText = bodyObj.GetComponent<Text>();
            dialogueBodyText.font = _dialogueFont;
            dialogueBodyText.fontSize = 18;
            dialogueBodyText.color = new Color(0.95f, 0.94f, 0.96f, 1f); // Crisp ivory
            dialogueBodyText.alignment = TextAnchor.UpperLeft;
            dialogueBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueBodyText.verticalOverflow = VerticalWrapMode.Truncate;
            Outline bodyOutline = bodyObj.GetComponent<Outline>();
            bodyOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            bodyOutline.effectDistance = new Vector2(1f, -1f);

            // Prompt Advance Indicator
            GameObject promptObj = new GameObject("PromptAdvance", typeof(RectTransform), typeof(Text));
            promptObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform art = promptObj.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0.70f, 0.04f);
            art.anchorMax = new Vector2(0.98f, 0.22f);
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;
            promptAdvanceText = promptObj.GetComponent<Text>();
            promptAdvanceText.font = _dialogueFont;
            promptAdvanceText.fontSize = 14;
            promptAdvanceText.color = new Color(0.85f, 0.78f, 0.60f, 0.9f);
            promptAdvanceText.alignment = TextAnchor.MiddleRight;
            promptAdvanceText.text = "[E / Tap] CONTINUE ▶";

            dialogContainer.SetActive(false);
        }

        public void StartDialogue(DialogueSequence sequence, Action onComplete = null)
        {
            if (sequence == null || sequence.lines == null || sequence.lines.Length == 0) return;

            _currentSequence = sequence;
            _currentLineIndex = 0;
            _onCompleteCallback = onComplete;

            if (dialogContainer == null)
            {
                CreateDialogueUIElements();
            }

            if (dialogContainer != null)
            {
                dialogContainer.SetActive(true);
            }

            ShowCurrentLine();
        }

        private void Update()
        {
            if (!IsDialogueActive) return;

            // Animate advance prompt blink
            if (promptAdvanceText != null && promptAdvanceText.gameObject.activeSelf)
            {
                float a = 0.5f + Mathf.PingPong(Time.unscaledTime * 2.2f, 0.5f);
                Color c = promptAdvanceText.color;
                c.a = a;
                promptAdvanceText.color = c;
            }

            // Advance dialogue with E, Space, Return, or Touch Interact
            if (GameInput.GetInteractDown() || GameInput.GetConfirmDown() || GameInput.GetJumpDown())
            {
                if (_isTyping)
                {
                    // First tap: skip typewriter and instantly reveal full text
                    if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                    if (_currentSequence != null && _currentLineIndex < _currentSequence.lines.Length)
                    {
                        dialogueBodyText.text = _currentSequence.lines[_currentLineIndex].text;
                    }
                    _isTyping = false;
                }
                else
                {
                    // Second tap: advance to next dialogue line
                    AdvanceLine();
                }
            }
        }

        private void ShowCurrentLine()
        {
            if (_currentSequence == null || _currentLineIndex >= _currentSequence.lines.Length)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = _currentSequence.lines[_currentLineIndex];
            if (speakerText != null) speakerText.text = line.speakerName;

            if (portraitImage != null)
            {
                if (!string.IsNullOrEmpty(line.portraitSpriteName))
                {
                    Sprite s = Resources.Load<Sprite>("Sprites/Narrative/" + line.portraitSpriteName);
                    if (s != null)
                    {
                        portraitImage.sprite = s;
                        portraitImage.enabled = true;
                    }
                    else
                    {
                        portraitImage.enabled = false;
                    }
                }
                else
                {
                    portraitImage.enabled = false;
                }
            }

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));
        }

        private IEnumerator TypewriterRoutine(string fullText)
        {
            _isTyping = true;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            int blipCounter = 0;
            for (int i = 0; i < fullText.Length; i++)
            {
                if (dialogueBodyText != null) dialogueBodyText.text += fullText[i];
                blipCounter++;

                if (blipCounter % 3 == 0 && AudioManager.Instance != null && !char.IsWhiteSpace(fullText[i]))
                {
                    AudioManager.Instance.PlaySFX("dialogue_blip", 0.5f, UnityEngine.Random.Range(0.95f, 1.05f));
                }

                yield return new WaitForSecondsRealtime(0.022f);
            }

            _isTyping = false;
        }

        private void AdvanceLine()
        {
            _currentLineIndex++;
            if (_currentLineIndex < _currentSequence.lines.Length)
            {
                ShowCurrentLine();
            }
            else
            {
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _isTyping = false;

            if (dialogContainer != null)
            {
                dialogContainer.SetActive(false);
            }

            if (_currentSequence != null && !string.IsNullOrEmpty(_currentSequence.unlockedLoreEntryId))
            {
                JournalService.UnlockEntry(_currentSequence.unlockedLoreEntryId);
            }

            if (_currentSequence != null && !string.IsNullOrEmpty(_currentSequence.dialogueId))
            {
                JournalService.MarkNPCConversationCompleted(_currentSequence.dialogueId);
            }

            var cb = _onCompleteCallback;
            _onCompleteCallback = null;
            _currentSequence = null;

            cb?.Invoke();
        }
    }
}
