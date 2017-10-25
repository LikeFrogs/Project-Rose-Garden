using System.Collections;
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
    [SerializeField] GameObject selectionSquare;
    [SerializeField] GameObject selectedSquare;
    [SerializeField] GameObject attackSquare;

    //static version of prefabs to be set in Awake()
    private static GameObject moveRangeSprite;
    private static GameObject buttonPrefab;
    private static GameObject canvasPrefab;
    private static GameObject selectionPrefab;
    private static GameObject selectedPrefab;
    private static GameObject attackPrefab;

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
    #endregion

    #region Instance data
    //tells this when to give control to the SceneController
    private bool sceneLoaded;
    //holds the playable party members between scenes
    private List<PlayableChar> party;
    //holds the next unused ID for character ID tagging for targets
    private static int nextID;
    #endregion

    #region Properties
    /// <summary>
    /// holds the next unused ID for character ID tagging for targets
    /// </summary>
    public static int NextID
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

    public List<PlayableChar> Party
    {
        get { return party; }
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
        selectionPrefab = selectionSquare;
        selectedPrefab = selectedSquare;
        attackPrefab = attackSquare;

        //begin play before any new scenes have been loaded
        sceneLoaded = false;

        //this is where character creation and such should be done
        party = new List<PlayableChar>();

        party.Add(Instantiate(bluePlayer).GetComponent<PlayableChar>());
        party.Add(Instantiate(redPlayer).GetComponent<PlayableChar>());

        party[0].GetComponent<PlayableChar>().Init(30, 3, 15, 15, 15, 15, 2, 5);
        party[1].GetComponent<PlayableChar>().Init(30, 7, 15, 15, 15, 15, 2, 1);

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
            //manager.AddToCharList(party);
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
