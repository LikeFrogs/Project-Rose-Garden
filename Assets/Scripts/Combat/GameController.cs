﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Prefabs
    //editor assigned prefabs
    [SerializeField] GameObject bluePlayer;
    [SerializeField] GameObject redPlayer;
    [SerializeField] GameObject purpleSquare;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject button;

    //static version of prefabs to be set in Awake()
    private static GameObject moveRangeSprite;
    private static GameObject buttonPrefab;
    private static GameObject canvasPrefab;

    //static prefab properties to be accessed by other classes
    /// <summary>
    /// Gets the visual for displaying move range
    /// </summary>
    public static GameObject MoveRangeSprite
    {
        get { return moveRangeSprite; }
    }
    /// <summary>
    /// Gets a blank action button
    /// </summary>
    public static GameObject ButtonPrefab
    {
        get { return buttonPrefab; }
    }
    /// <summary>
    /// Gets a blank canvas for instantiating new UI
    /// </summary>
    public static GameObject CanvasPrefab
    {
        get { return canvasPrefab; }
    }
    #endregion

    #region Instance data
    //tells this when to give control to the SceneController
    private bool sceneLoaded;
    //holds the playable party members between scenes
    private List<GameObject> party;
    //holds the next unused ID for character ID tagging for targets
    private static uint nextID;
    #endregion

    #region Properties
    /// <summary>
    /// holds the next unused ID for character ID tagging for targets
    /// </summary>
    public static uint NextID
    {
        get { return nextID; }
        set
        {
            if(value > nextID)
            {
                nextID = value;
            }
        }
    }
    #endregion

    // Use this for initialization
    void Awake ()
    {
        //this will be used for the entire game
        DontDestroyOnLoad(transform);
        //sets the BeginScene method to run whenever a new scene is loaded
        SceneManager.sceneLoaded += BeginScene;

        //creates the statics from the editor assigned prefabs
        moveRangeSprite = purpleSquare;
        buttonPrefab = button;
        canvasPrefab = canvas;

        //begin play before any new scenes have been loaded
        sceneLoaded = false;

        //this is where character creation and such should be done
        party = new List<GameObject>();

        party.Add(bluePlayer);
        party.Add(redPlayer);


        SceneManager.LoadScene("TestScene");
    }

	// Update is called once per frame
	void Update ()
    {
        //runs the BeginPlay method of the manager in the new scene
        if (sceneLoaded)
        {
            sceneLoaded = false;
            SceneController manager = GameObject.FindWithTag("SceneManager").GetComponent<SceneController>();
            manager.BeginPlay(party);
        }
	}

    /// <summary>
    /// Sets sceneLoaded to true which causes this to give control to the SceneController
    /// </summary>
    private void BeginScene(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
    }
    
    /// <summary>
    /// Removes the sceneLoaded delegate if this object is ever disabled
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= BeginScene;
    }

}
