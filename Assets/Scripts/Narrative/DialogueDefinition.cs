using System;
using UnityEngine;

namespace MyShadowQuit.Narrative
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string portraitSpriteName;
        [TextArea(2, 5)]
        public string text;
    }

    [Serializable]
    public class DialogueSequence
    {
        public string dialogueId;
        public DialogueLine[] lines;
        public string unlockedLoreEntryId; // ID added to Lore Journal when dialogue concludes
    }
}
