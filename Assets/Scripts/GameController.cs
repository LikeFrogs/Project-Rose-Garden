﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the overall flow of the game and holds all important static GameObjects
/// </summary>
public class GameController : MonoBehaviour
{
    #region Prefabs
    //editor assigned prefabs
    [SerializeField] GameObject NEWPlayer;

    [SerializeField] GameObject bluePlayer;
    [SerializeField] GameObject redPlayer;
    [SerializeField] GameObject purpleSquare;
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject button;
    [SerializeField] GameObject selectionSquare;
    [SerializeField] GameObject selectedSquare;
    [SerializeField] GameObject attackSquare;
    [SerializeField] GameObject sightSquare;

    //static version of prefabs to be set in Awake()
    private static GameObject moveRangeSprite;
    private static GameObject buttonPrefab;
    private static GameObject canvasPrefab;
    private static GameObject selectionPrefab;
    private static GameObject selectedPrefab;
    private static GameObject attackPrefab;
    private static GameObject sightPrefab;

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
    /// <summary>
    /// Gets a UI element to higlight selectable enemies
    /// </summary>
    public static GameObject SelectionPrefab
    {
        get { return selectionPrefab; }
    }
    /// <summary>
    /// Gets a UI element to higlight selectabled enemy
    /// </summary>
    public static GameObject SelectedPrefab
    {
        get { return selectedPrefab; }
    }
    /// <summary>
    /// Gets the visual for displaying attack range
    /// </summary>
    public static GameObject AttackSquarePrefab
    {
        get { return attackPrefab; }
    }
    /// <summary>
    /// Gets the visual for displaying an enemy sight cone
    /// </summary>
    public static GameObject SightSquarePrefab
    {
        get { return sightPrefab; }
    }
    #endregion

    #region Fields
    //tells this when to give control to the SceneController
    private bool sceneLoaded;
    //holds the playable party members between scenes
    //private List<PlayerCharacter> party;
    private List<PlayerCharacter> party;
    #endregion

    #region Party inventory
    protected List<Item> inventory;

    /// <summary>
    /// All items in this character's inventory
    /// </summary>
    public List<Item> Inventory
    {
        get { return inventory; }
    }
    #endregion

    // Use this for initialization
    void Awake ()
    {
        //this will be used for the entire game
        DontDestroyOnLoad(transform.parent);
        //sets the BeginScene method to run whenever a new scene is loaded
        SceneManager.sceneLoaded += BeginScene;

        //creates the statics from the editor assigned prefabs
        moveRangeSprite = purpleSquare;
        buttonPrefab = button;
        canvasPrefab = canvas;
        selectionPrefab = selectionSquare;
        selectedPrefab = selectedSquare;
        attackPrefab = attackSquare;
        sightPrefab = sightSquare;

        //begin play before any new scenes have been loaded
        sceneLoaded = false;

        //this is where character creation and such should be done
        party = new List<PlayerCharacter>();

        //party.Add(Instantiate(NEWPlayer, new Vector3(23, 10), Quaternion.identity).GetComponent<PlayerCharacter>());
        //party.Add(Instantiate(NEWPlayer, new Vector3(32, 4), Quaternion.identity).GetComponent<PlayerCharacter>());

        //party[0].GetComponent<PlayerCharacter>().Init(30, 8, 5, 15, 15, 2);
        //party[1].GetComponent<PlayerCharacter>().Init(30, 4, 5, 15, 15, 2);

        SceneManager.LoadScene("DijkstraTest");
    }

	// Update is called once per frame
	void Update ()
    {
        //runs the BeginPlay method of the manager in the new scene
        if (sceneLoaded)
        {
            sceneLoaded = false;
            //SceneController controller = GameObject.FindWithTag("SceneController").GetComponent<SceneController>();
            //for(int i = 0; i < party.Count; i++) { party[i].OnSceneLoad(); }

            CombatSceneController controller = GameObject.FindWithTag("SceneController").GetComponent<CombatSceneController>();

            controller.StartScene(party);
        }
	}

    /// <summary>
    /// Sets sceneLoaded to true which causes this to give control to the SceneController
    /// </summary>
    private void BeginScene(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
    }
}
