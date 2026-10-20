using System;
using UnityEngine;
namespace Guisha.Framework
{
    [Serializable] public sealed class DialogueChoice { public string text; public int next = -1; }
    [Serializable] public sealed class DialogueNode
    {
        public string speaker;
        [TextArea] public string text;
        public int next = -1;
        public DialogueChoice[] choices = Array.Empty<DialogueChoice>();
    }
    [CreateAssetMenu(menuName = "Guisha/Dialogue")]
    public sealed class DialogueAsset : ScriptableObject { public DialogueNode[] nodes = Array.Empty<DialogueNode>(); }
}
