using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A full sequence of Dialogue, mad up of DialogueNode's
/// </summary>
public class DialogueSequence : ScriptableObject
{
    /// <summary>
    /// The easiest way to serialize a Dictionary
    /// </summary>
    [System.Serializable]
    public class PortraitTable
    {
        public string name;
        public Sprite portrait;

        public PortraitTable(string name, Sprite portrait)
        {
            this.name = name;
            this.portrait = portrait;
        }
    }


    [HideInInspector] private List<PortraitTable> portraits;
    [HideInInspector] private List<DialogueNode> nodes;
    
    /// <summary>
    /// Gets or sets the psuedo dictionary of Portraits
    /// </summary>
    public List<PortraitTable> Portraits { get { return portraits; } set { portraits = value; } }
    
    /// <summary>
    /// Gets or sets the list of DialogueNodes
    /// </summary>
    public List<DialogueNode> Nodes { get { return nodes; } set { nodes = value; } }
}
