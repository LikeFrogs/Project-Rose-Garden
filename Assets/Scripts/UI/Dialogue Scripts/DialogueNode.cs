using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    private string text;
    private string portrait;

    public string Text { get { return text; } }
    public string Portratit { get { return portrait; } }

    public DialogueNode(string text, string portrait)
    {
        this.text = text;
        this.portrait = portrait;
    }
}
