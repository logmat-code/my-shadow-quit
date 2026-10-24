using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyShadowQuit.Core;

namespace MyShadowQuit.Narrative
{
    /// <summary>
    /// Singleton manager for rendering interactive narrative dialogues.
    /// Freezes player movement, plays audio blips during typewriter effect,
    /// displays character portraits, and records unlocked lore entries.
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        public static DialogueController Instance { get; private set; }

        [SerializeField] private GameObject dialogContainer;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI dialogueBodyText;
        [SerializeField] private GameObject promptAdvanceIndicator;

        private DialogueSequence _currentSequence;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private Coroutine _typewriterCoroutine;
        private Action _onCompleteCallback;

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

            if (dialogContainer == null)
            {
                CreateDialogueUIElements();
            }
        }

        private void CreateDialogueUIElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canObj = GameObject.Find("UICanvas");
                if (canObj != null) canvas = canObj.GetComponent<Canvas>();
            }

            if (canvas == null) return;

            dialogContainer = new GameObject("DialogueModal");
            dialogContainer.transform.SetParent(canvas.transform, false);

            RectTransform rt = dialogContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.05f);
            rt.anchorMax = new Vector2(0.9f, 0.32f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = dialogContainer.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.12f, 0.94f);

            // Portrait
            GameObject portObj = new GameObject("Portrait");
            portObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform prt = portObj.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.02f, 0.1f);
            prt.anchorMax = new Vector2(0.18f, 0.9f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;
            portraitImage = portObj.AddComponent<Image>();
            portraitImage.preserveAspect = true;

            // Speaker Text
            GameObject spkObj = new GameObject("SpeakerText");
            spkObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform srt = spkObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.20f, 0.72f);
            srt.anchorMax = new Vector2(0.95f, 0.95f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            speakerText = spkObj.AddComponent<TextMeshProUGUI>();
            speakerText.fontSize = 20;
            speakerText.color = new Color(1f, 0.85f, 0.4f, 1f);
            speakerText.fontStyle = FontStyles.Bold;

            // Body Text
            GameObject bodyObj = new GameObject("BodyText");
            bodyObj.transform.SetParent(dialogContainer.transform, false);
            RectTransform brt = bodyObj.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.20f, 0.15f);
            brt.anchorMax = new Vector2(0.95f, 0.70f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            dialogueBodyText = bodyObj.AddComponent<TextMeshProUGUI>();
            dialogueBodyText.fontSize = 16;
            dialogueBodyText.color = new Color(0.92f, 0.92f, 0.94f, 1f);
            dialogueBodyText.textWrappingMode = TextWrappingModes.Normal;

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

            // Advance dialogue with E, Space, Return, or Touch Interact
            if (GameInput.GetInteractDown() || GameInput.GetConfirmDown() || GameInput.GetJumpDown())
            {
                if (_isTyping)
                {
                    // Fast-forward line
                    if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                    dialogueBodyText.text = _currentSequence.lines[_currentLineIndex].text;
                    _isTyping = false;
                }
                else
                {
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

            if (portraitImage != null && !string.IsNullOrEmpty(line.portraitSpriteName))
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

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));
        }

        private IEnumerator TypewriterRoutine(string fullText)
        {
            _isTyping = true;
            dialogueBodyText.text = "";

            int blipCounter = 0;
            for (int i = 0; i < fullText.Length; i++)
            {
                dialogueBodyText.text += fullText[i];
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

            var cb = _onCompleteCallback;
            _onCompleteCallback = null;
            _currentSequence = null;

            cb?.Invoke();
        }
    }
}
