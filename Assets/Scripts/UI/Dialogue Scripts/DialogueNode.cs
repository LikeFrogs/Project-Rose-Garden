using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single segmet of Dialogue with text and portrait
/// </summary>
[System.Serializable]
public class DialogueNode
{
    private string text;
    private string portrait;

    /// <summary>
    /// Gets the text of this DialogueNode
    /// </summary>
    public string Text { get { return text; } }
    /// <summary>
    /// Gets the portrait of this DialogueNode
    /// </summary>
    public string Portratit { get { return portrait; } }

    public DialogueNode(string text, string portrait)
    {
        this.text = text;
        this.portrait = portrait;
    }
}
 