using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueBuilderMode { Build, ReverseEngineer }

/// <summary>
/// Builds and reverse-engineers DialogueSequences
/// </summary>
public class DialogueBuilder : MonoBehaviour
{
    /// <summary>
    /// A psuedo DialogueNode used for making editor-serialized lists of DialogueNodes
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


    [SerializeField] private DialogueBuilderMode mode;
    [SerializeField] private string fileName;
    [SerializeField] private DialogueSequence sequenceToReverseEngineer;
    [SerializeField] private List<DialogueSequence.PortraitTable> portraits;
    [SerializeField] private List<DialogueNodeWrapper> nodes = new List<DialogueNodeWrapper>();

    /// <summary>
    /// Runs at the beginning of the scene
    /// </summary>
    private void Start()
    {
        if (mode == DialogueBuilderMode.Build)
        {
            //create a new DialogueSequence
            DialogueSequence dialogueSequence = (DialogueSequence)ScriptableObject.CreateInstance("DialogueSequence");

            //load the portraits into the DialogueSequence
            List<DialogueSequence.PortraitTable> sprites = new List<DialogueSequence.PortraitTable>();
            for (int i = 0; i < portraits.Count; i++)
            {
                sprites.Add(new DialogueSequence.PortraitTable(portraits[i].name, portraits[i].portrait));
            }
            dialogueSequence.Portraits = sprites;

            //load the DialogueNodes into the DialogueSequence
            dialogueSequence.Nodes = new List<DialogueNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                DialogueNode node = new DialogueNode(nodes[i].text, nodes[i].portrait);
                dialogueSequence.Nodes.Add(node);
            }

            //set up file name if not provided
            if(fileName == null) { fileName = "DialougeSequence"; }

            //save the file
            UnityEditor.AssetDatabase.CreateAsset(dialogueSequence, "Assets/Dialogue Sequences/" + fileName + ".asset");
        }
        else if (mode == DialogueBuilderMode.ReverseEngineer)
        {
            //error message
            if(sequenceToReverseEngineer == null)
            {
                Debug.Log("No dialogue sequence present to reverse engineer");
                return;
            }

            //read portraits from saved DialogueSequence
            portraits = sequenceToReverseEngineer.Portraits;

            //read DialogueNodes from saved DialogueSequence
            nodes = new List<DialogueNodeWrapper>();
            for(int i = 0; i < sequenceToReverseEngineer.Nodes.Count; i++)
            {
                nodes.Add(new DialogueNodeWrapper(sequenceToReverseEngineer.Nodes[i].Portratit, sequenceToReverseEngineer.Nodes[i].Text));
            }

            //instruction message
            Debug.Log("Go to the Scene tab to edit the sequence, then change Mode to Build, return to Game view and press Enter.");
        }
    }

    /// <summary>
    /// Runs once every frame
    /// </summary>
    private void Update()
    {
        //allows the user to run the algorithm again by pressing enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Start();
        }
    }
}
