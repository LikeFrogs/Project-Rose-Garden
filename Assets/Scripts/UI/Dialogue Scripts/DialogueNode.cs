using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single segmet of Dialogue with text and portrait
/// </summary>
[System.Serializable]
public class DialogueNode
{
    [SerializeField] private Sprite portrait;
    [SerializeField] [TextArea] private string text;

    /// <summary>
    /// Gets the portrait of this DialogueNode
    /// </summary>
    public Sprite Portratit { get { return portrait; } }
    /// <summary>
    /// Gets the text of this DialogueNode
    /// </summary>
    public string Text { get { return text; } }
}
 