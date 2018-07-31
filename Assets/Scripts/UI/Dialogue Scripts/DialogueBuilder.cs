using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueBuilderMode { Build, ReverseEngineer }

public class DialogueBuilder : MonoBehaviour
{

    [SerializeField] private DialogueBuilderMode mode;
    [SerializeField] private string fileName;
    [SerializeField] private DialogueSequence sequenceToReverseEngineer;


    /// <summary>
    /// All possible sprites for the DialogueSequence
    /// </summary>
    [SerializeField] private List<DialogueSequence.PortraitTable> portraits;

    /// <summary>
    /// All dialogue nodes for the DialogueSequence
    /// </summary>
    [System.Serializable]
    private class DialogueNodeWrapper
    {
        public string portrait;
        [TextArea] public string text;   
        
        public DialogueNodeWrapper(string portrait, string text)
        {
            this.portrait = portrait;
            this.text = text;
        }
    }
    [SerializeField] private List<DialogueNodeWrapper> nodes = new List<DialogueNodeWrapper>();

    private void Start()
    {
        if (mode == DialogueBuilderMode.Build)
        {
            DialogueSequence dialogueSequence = (DialogueSequence)ScriptableObject.CreateInstance("DialogueSequence");

            List<DialogueSequence.PortraitTable> sprites = new List<DialogueSequence.PortraitTable>();
            for (int i = 0; i < portraits.Count; i++)
            {
                sprites.Add(new DialogueSequence.PortraitTable(portraits[i].name, portraits[i].portrait));
            }
            dialogueSequence.Portraits = sprites;

            dialogueSequence.Nodes = new List<DialogueNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                DialogueNode node = new DialogueNode(nodes[i].text, nodes[i].portrait);
                dialogueSequence.Nodes.Add(node);
            }

            if(fileName == null) { fileName = "DialougeSequence"; }
            UnityEditor.AssetDatabase.CreateAsset(dialogueSequence, "Assets/Dialogue Sequences/" + fileName + ".asset");

            sequenceToReverseEngineer = (DialogueSequence)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Dialogue Sequences/" + fileName + ".asset", System.Type.GetType("DialogueSequence"));
        }
        else if (mode == DialogueBuilderMode.ReverseEngineer)
        {
            if(sequenceToReverseEngineer == null)
            {
                Debug.Log("No dialogue sequence present to reverse engineer");
                return;
            }

            Debug.Log("Reverse Engineering");

            portraits = new List<DialogueSequence.PortraitTable>();
            //foreach (KeyValuePair<string, Sprite> portrait in sequenceToReverseEngineer.Portraits)
            //{
            //    portraits.Add(new DictionaryWrapper(portrait.Key, portrait.Value));
            //}

            nodes = new List<DialogueNodeWrapper>();
            for(int i = 0; i < sequenceToReverseEngineer.Nodes.Count; i++)
            {
                nodes.Add(new DialogueNodeWrapper(sequenceToReverseEngineer.Nodes[i].Portratit, sequenceToReverseEngineer.Nodes[i].Text));
            }

            Debug.Log("Successfully reverse engineered. Go to the Scene tab to edit the sequence, then change Mode to Build, return to Game view and press Enter.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Start();
        }
    }
}
