using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A full sequence of Dialogue, mad up of DialogueNode's
/// </summary>
[CreateAssetMenu] public class DialogueSequence : ScriptableObject
{
    [SerializeField] private List<DialogueNode> nodes;
    
    /// <summary>
    /// Gets or sets the list of DialogueNodes
    /// </summary>
    public List<DialogueNode> Nodes { get { return nodes; } set { nodes = value; } }

    /// <summary>
    /// Indexer property for getting a specific node from the seuqence
    /// </summary>
    /// <param name="i">The supplied index for the property</param>
    public DialogueNode this[int i] { get { return nodes[i]; } }
}
