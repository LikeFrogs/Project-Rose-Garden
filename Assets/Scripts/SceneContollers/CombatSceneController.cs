﻿﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CombatSceneState { OpeningDialogue, ClosingDialogue, Combat, CameraMode, MovingCamera, DampingCamera }

/// <summary>
/// Handles the game loop during combat
/// </summary>
public abstract class CombatSceneController : MonoBehaviour
{
    [SerializeField] protected Image dialogueSprite;
    [SerializeField] protected Text dialogueText;
    [SerializeField] protected DialogueSequence openingDialogueSequence;
    protected int dialogueSequenceIndex;



    #region Fields and properties
    protected CombatSceneState state;

    //world UI
    [SerializeField] protected Vector3 topRightCorner;
    [SerializeField] protected Canvas worldCanvas;
    protected List<GameObject> unusedPathSegments;
    protected List<GameObject> pathSegments;
    protected List<GameObject> unusedActionButtons;
    protected Dictionary<Vector2, GameObject> actionButtons;
    protected List<GameObject> unusedTargetIcons;
    protected Dictionary<Vector3, GameObject> targetIcons;
    protected List<GameObject> unusedMoveRangeIndicators;
    protected List<GameObject> unusedRangeIndicators;
    protected Dictionary<Vector3, GameObject> moveRangeIndicators;
    protected Dictionary<Vector3, GameObject> attackRangeIndicators;
    
    //overlay UI
    [SerializeField] protected OverlayCanvas overlayCanvas;
    protected GameObject inspectionReticule;  
    
    //camera
    protected Camera camera;
    protected Vector3 cameraMoveStart;
    protected Vector3 cameraMoveEnd;
    protected float lerpTime;
    protected Vector3 dampVelocity;

    //combatant book keeping
    protected Dictionary<Vector3, CombatChar> currentCombatantPositions;
    protected List<CombatChar> finishedList;
    protected List<CombatChar> currentTurnBlock;
    protected List<CombatChar> nextList;
    protected static List<CombatChar> goodGuys;
    protected static List<Enemy> enemies;
    protected static float[,] moveCosts;
    

    /// <summary>
    /// Gets the top right corner of the map
    /// </summary>
    public Vector3 TopRightCorner { get { return topRightCorner; } }

    /// <summary>
    /// Gets the list of playable characters and their allies
    /// </summary>
    public static List<CombatChar> GoodGuys { get { return goodGuys; } }
    /// <summary>
    /// Gets a list of all enemies in the scene
    /// </summary>
    public static List<Enemy> Enemies { get { return enemies; } }
    /// <summary>
    /// Gets a matrix representing the cost to move to any tile [x, y]
    /// </summary>
    public static float[,] MoveCosts { get { return (float[,])moveCosts.Clone(); } }

    #endregion

    /// <summary>
    /// Used for initialization
    /// </summary>
    void Start()
    {
        finishedList = new List<CombatChar>();
        currentTurnBlock = new List<CombatChar>();
        nextList = new List<CombatChar>();

        currentCombatantPositions = new Dictionary<Vector3, CombatChar>();

        goodGuys = new List<CombatChar>();
        enemies = new List<Enemy>();

        camera = Camera.main;
        camera.transform.position = new Vector3((int)(topRightCorner.x / 2), (int)(topRightCorner.y / 2), -10);

        //worldCanvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        worldCanvas.GetComponent<RectTransform>().sizeDelta = topRightCorner;

        inspectionReticule = Instantiate(GameController.SelectedPrefab);
        inspectionReticule.SetActive(false);
        inspectionReticule.transform.SetParent(worldCanvas.transform);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (state == CombatSceneState.OpeningDialogue)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                dialogueSequenceIndex++;
                DisplayCurrentDialogueNode();
            }
        }

        //switches into free movement of camera for character analysis
        if (Input.GetKeyDown(KeyCode.C) && currentTurnBlock[0] is PlayerCharacter && (state == CombatSceneState.Combat || state == CombatSceneState.CameraMode))
        {
            if (state != CombatSceneState.CameraMode)
            {
                currentTurnBlock[0].FinishedTurn = true;
                state = CombatSceneState.CameraMode;

                //stores the positions of the combatants at the current time
                currentCombatantPositions.Clear();
                for (int i = 0; i < finishedList.Count; i++) { currentCombatantPositions[finishedList[i].transform.position] = finishedList[i]; }
                for (int i = 0; i < currentTurnBlock.Count; i++) { currentCombatantPositions[currentTurnBlock[i].transform.position] = currentTurnBlock[i]; }
                for (int i = 0; i < nextList.Count; i++) { currentCombatantPositions[nextList[i].transform.position] = nextList[i]; }

                //draw what camera is currently looking at
                inspectionReticule.SetActive(true);
                inspectionReticule.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(.5f, .5f);
                inspectionReticule.transform.GetComponent<RectTransform>().anchoredPosition = camera.transform.position;

                //display ui for the current character (will already be under camera)
                Vector3 normalizedCameraPos = new Vector3((int)camera.transform.position.x, (int)camera.transform.position.y);
                if (currentCombatantPositions.ContainsKey(normalizedCameraPos))
                {
                    overlayCanvas.InspectCharacter(currentCombatantPositions[normalizedCameraPos]);
                }
                else
                {
                    overlayCanvas.HideUI();
                }
            }
            else
            {
                overlayCanvas.HideUI();

                state = CombatSceneState.DampingCamera;
                state = CombatSceneState.DampingCamera;
                cameraMoveEnd = currentTurnBlock[0].transform.position;
                cameraMoveEnd.z = -10;
                dampVelocity = Vector3.zero;


                inspectionReticule.SetActive(false);
            }
        }

        else if (state == CombatSceneState.Combat)
        {
            if (currentTurnBlock[0].FinishedTurn)
            {
                //sets the character who was taking a turn as finished
                finishedList.Add(currentTurnBlock[0]);
                currentTurnBlock.RemoveAt(0);

                //removes all dead characters from lists
                while (finishedList.Contains(null)) { finishedList.Remove(null); }
                while (currentTurnBlock.Contains(null)) { currentTurnBlock.Remove(null); }
                while (nextList.Contains(null)) { nextList.Remove(null); }
                while (goodGuys.Contains(null)) { goodGuys.Remove(null); }
                while (enemies.Contains(null)) { enemies.Remove(null); }

                //****************************************************************Check Objective


                //if there is no character who is "up next" sort the lists
                if (currentTurnBlock.Count <= 0) { SortLists(); }

                //move the camera to the next character
                if (currentTurnBlock.Count > 0)
                {
                    state = CombatSceneState.DampingCamera;
                    cameraMoveEnd = currentTurnBlock[0].transform.position;
                    cameraMoveEnd.z = -10;
                    dampVelocity = Vector3.zero;
                }

                CheckObjective();
            }
            //switces to the next player character when the user presses tab while in a block of PlayerCharacters
            else if (Input.GetKeyDown(KeyCode.Tab) && currentTurnBlock.Count > 1 && currentTurnBlock[0] is PlayerCharacter) //******************************************************************change to GetButtonDown
            {
                //ends the current characters turn
                currentTurnBlock[0].FinishedTurn = true;
                currentTurnBlock.Add(currentTurnBlock[0]);
                currentTurnBlock.RemoveAt(0);

                //moves camera to the next character
                state = CombatSceneState.DampingCamera;
                cameraMoveEnd = currentTurnBlock[0].transform.position;
                cameraMoveEnd.z = -10;
                dampVelocity = Vector3.zero;
            }
        }

        if (state == CombatSceneState.DampingCamera)
        {
            //smoothly damps the camera to its new position
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, cameraMoveEnd, ref dampVelocity, .5f);
            //prevents velocity from becoming too low
            if (dampVelocity.magnitude < .5f) { dampVelocity *= 15; }

            //float deltaX = System.Math.Abs(camera.transform.position.x - cameraMoveEnd.x);
            //float deltaY = System.Math.Abs(camera.transform.position.y - cameraMoveEnd.y);
            //Debug.Log("Delta x : " + deltaX + " Delta y : " + deltaY + " Velocity : " + dampVelocity.magnitude);
            //camera.transform.position = new Vector3((int)camera.transform.position.x, (int)camera.transform.position.y)

            if (camera.transform.position == cameraMoveEnd)
            //if (deltaX <= .4f && deltaY <= .4f)
            {
                camera.transform.position = cameraMoveEnd;
                currentTurnBlock[0].BeginTurn();
                state = CombatSceneState.Combat;
            }
        }
        
        if (state == CombatSceneState.CameraMode)
        {
            //gets input for moving the camera
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            cameraMoveStart = camera.transform.position;
            cameraMoveEnd = new Vector3(camera.transform.position.x + System.Math.Sign(input.x), camera.transform.position.y + System.Math.Sign(input.y), -10);
            lerpTime = 0f;

            //sets camera to lerp
            if (cameraMoveEnd != cameraMoveStart) { state = CombatSceneState.MovingCamera; }
        }

        if (state == CombatSceneState.MovingCamera)
        {
            //smoothly moves camera to destination
            lerpTime += Time.deltaTime * 5;
            camera.transform.position = Vector3.Lerp(cameraMoveStart, cameraMoveEnd, lerpTime);

            inspectionReticule.GetComponent<RectTransform>().anchoredPosition = camera.transform.position;

            //return to camera mode after moving
            if (lerpTime >= 1f)
            {
                state = CombatSceneState.CameraMode;
                              
                Vector3 normalizedCameraPos = new Vector3((int)camera.transform.position.x, (int)camera.transform.position.y);
                if (currentCombatantPositions.ContainsKey(normalizedCameraPos))
                {
                    overlayCanvas.InspectCharacter(currentCombatantPositions[normalizedCameraPos]);
                }
                else
                {
                    overlayCanvas.HideUI();
                }
            }
        }

        if (state == CombatSceneState.ClosingDialogue)
        {
            // TODO
        }
    }

    /// <summary>
    /// Sets up and begins the scene
    /// </summary>
    /// <param name="party"></param>
    public void StartScene(List<PlayerCharacter> party)
    {
        //set up the move cost matrix based on walls and the size of the map
        moveCosts = new float[(int)topRightCorner.x + 1, (int)TopRightCorner.y + 1];
        for (int i = 0; i < moveCosts.GetLength(0); i++)
        {
            for (int j = 0; j < moveCosts.GetLength(1); j++)
            {
                moveCosts[i, j] = 1;
            }
        }
        List<Vector3> blockedPositions = (from gameObject in GameObject.FindGameObjectsWithTag("Blocking") select gameObject.transform.position).ToList();
        for (int i = 0; i < blockedPositions.Count; i++)
        {
            moveCosts[(int)blockedPositions[i].x, (int)blockedPositions[i].y] = 0;
        }

        //check max speed and range of the party so as to know how much UI to make
        int maxSpeed = 0;
        int maxRange = 0;
        //adds all playable character to nextList and goodGuys
        for (int i = 0; i < party.Count; i++)
        {
            nextList.Add(party[i]);
            goodGuys.Add(party[i]);

            if(party[i].MaxSpeed > maxSpeed) { maxSpeed = party[i].MaxSpeed; }
            if(party[i].AttackRange > maxRange) { maxRange = party[i].AttackRange; }
        }

        //UI pooling for this scene
        moveRangeIndicators = new Dictionary<Vector3, GameObject>();
        attackRangeIndicators = new Dictionary<Vector3, GameObject>();
        unusedMoveRangeIndicators = new List<GameObject>();
        unusedRangeIndicators = new List<GameObject>();
        actionButtons = new Dictionary<Vector2, GameObject>();
        unusedActionButtons = new List<GameObject>();
        targetIcons = new Dictionary<Vector3, GameObject>();
        unusedTargetIcons = new List<GameObject>();
        pathSegments = new List<GameObject>();
        unusedPathSegments = new List<GameObject>();
        //movement range sprites
        for (int x = 0 - maxSpeed; x <= maxSpeed; x++)
        {
            for (int y = 0 - (maxSpeed - System.Math.Abs(0 - x)); System.Math.Abs(0 - x) + System.Math.Abs(0 - y) <= maxSpeed; y++)
            {
                GameObject indicator = Instantiate(GameController.MoveRangeSprite);
                indicator.SetActive(false);

                unusedMoveRangeIndicators.Add(indicator);
                DontDestroyOnLoad(indicator);

                indicator.SetActive(false);
                indicator.transform.SetParent(worldCanvas.transform);
            }
        }
        //attack range sprites
        for (int i = 0; i < unusedMoveRangeIndicators.Count; i++)
        {
            GameObject indicator = Instantiate(GameController.AttackSquarePrefab);
            indicator.SetActive(false);

            unusedRangeIndicators.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(worldCanvas.transform);
        }
        //action buttons
        for (int i = 0; i < 15; i++)
        {
            GameObject button = Instantiate(GameController.ButtonPrefab);
            button.SetActive(false);

            unusedActionButtons.Add(button);
            DontDestroyOnLoad(button);

            button.SetActive(false);
            button.transform.SetParent(worldCanvas.transform);
        }
        //target icons
        for (int i = 0; i < 10; i++)
        {
            GameObject indicator = Instantiate(GameController.SelectionPrefab);
            indicator.SetActive(false);

            unusedTargetIcons.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(worldCanvas.transform);
        }
        //path segments
        for (int i = 0; i < maxSpeed; i++)
        {
            GameObject indicator = Instantiate(GameController.PathPrefab);
            indicator.SetActive(false);

            unusedPathSegments.Add(indicator);
            DontDestroyOnLoad(indicator);

            indicator.SetActive(false);
            indicator.transform.SetParent(worldCanvas.transform);
        }

        //attach the UI in this scene to the party
        for (int i = 0; i < party.Count; i++)
        {
            party[i].MoveRangeIndicators = moveRangeIndicators;
            party[i].AttackRangeIndicators = attackRangeIndicators;
            party[i].UnusedMoveRangeIndicators = unusedMoveRangeIndicators;
            party[i].UnusedAttackRangeIndicators = unusedRangeIndicators;
            party[i].ActionButtons = actionButtons;
            party[i].UnusedActionButtons = unusedActionButtons;
            party[i].TargetIcons = targetIcons;
            party[i].UnusedTargetIcons = unusedTargetIcons;
            party[i].PathSegments = pathSegments;
            party[i].UnusedPathSegments = unusedPathSegments;



            party[i].Canvas = worldCanvas;
        }

        //add enemies to nextList and runs their targeting set up
        enemies = (from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.GetComponent<Enemy>()).ToList();
        foreach (Enemy enemy in enemies)
        {
            enemy.CreateTargetList();
            nextList.Add(enemy);
        }

        //ensures all combatants are active before commencing battle
        for (int i = 0; i < nextList.Count; i++)
        {
            nextList[i].gameObject.SetActive(true);
        }

        //sort all the lists
        SortLists();












        state = CombatSceneState.OpeningDialogue;
        dialogueSequenceIndex = 0;
        DisplayCurrentDialogueNode();






        ////begin the combat stage of the scene
        //state = CombatSceneState.Combat;

        ////set up the first char to go
        //camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
        //currentTurnBlock[0].BeginTurn();
    }

    /// <summary>
    /// Sorts all lists of combatants
    /// </summary>
    protected void SortLists()
    {
        //all consecutive player turns are within the same turn block and can be freely switched between
        if (nextList.Count > 0 && nextList[0] is PlayerCharacter)
        {
            while (nextList.Count > 0 && nextList[0] is PlayerCharacter)
            {
                currentTurnBlock.Add(nextList[0]);
                nextList.RemoveAt(0);
            }
        }
        //all consecutive enemy turns are within the same turn block
        else if (nextList.Count > 0 && nextList[0] is Enemy)
        {
            while (nextList.Count > 0 && nextList[0] is Enemy)
            {
                currentTurnBlock.Add(nextList[0]);
                nextList.RemoveAt(0);
            }
        }
        //the only other possibility is that nextList is empty
        else
        {
            nextList.AddRange(finishedList);
            finishedList.Clear();
        }

        //if no characters were added to currentTurnBlock during this method, rerun it (max # of runs is 2)
        if (currentTurnBlock.Count == 0)
        {
            SortLists();
        }
    }


    protected void DisplayCurrentDialogueNode()
    {
        if(dialogueSequenceIndex >= openingDialogueSequence.Nodes.Count)
        {
            dialogueSprite.transform.parent.gameObject.SetActive(false);

            //begin the combat stage of the scene
            state = CombatSceneState.Combat;

            //set up the first char to go
            camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
            currentTurnBlock[0].BeginTurn();
            return;
        }

        DialogueNode node = openingDialogueSequence[dialogueSequenceIndex];
        dialogueSprite.sprite = node.Portratit;
        dialogueText.text = node.Text;
    }

    protected abstract void CheckObjective();
}