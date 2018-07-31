using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSequence : ScriptableObject
{
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
    public List<PortraitTable> Portraits { get { return portraits; } set { portraits = value; } }



    [HideInInspector] private List<DialogueNode> nodes;
    public List<DialogueNode> Nodes { get { return nodes; } set { nodes = value; } }
}
