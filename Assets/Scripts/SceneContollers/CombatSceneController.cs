﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CombatSceneState { OpeningDialogue, ClosingDialogue, Combat, CameraMode, MovingCamera }

public class CombatSceneController : MonoBehaviour
{
    private CombatSceneState state;

    private Camera camera;
    private Vector3 cameraMoveStart;
    private Vector3 cameraMoveEnd;
    private float lerpTime;

    private List<CombatChar> finishedList;
    private List<CombatChar> currentTurnBlock;
    private List<CombatChar> nextList;

    [SerializeField] private Vector3 topRightCorner;

    private static List<CombatChar> goodGuys;
    private static List<Enemy> enemies;
    private static float[,] moveCosts;

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




	// Use this for initialization
	void Start ()
    {
        finishedList = new List<CombatChar>();
        currentTurnBlock = new List<CombatChar>();
        nextList = new List<CombatChar>();

        goodGuys = new List<CombatChar>();
        enemies = new List<Enemy>();

        camera = Camera.main;
        camera.transform.position = new Vector3((int)(topRightCorner.x / 2), (int)(topRightCorner.y / 2), -10);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (state == CombatSceneState.OpeningDialogue)
        {
            // TODO
        }
        
        else if(state == CombatSceneState.Combat)
        {
            if (currentTurnBlock[0].FinishedTurn)
            {
                finishedList.Add(currentTurnBlock[0]);
                currentTurnBlock.RemoveAt(0);

                //removes all dead characters from lists
                while (finishedList.Contains(null)) { finishedList.Remove(null); }
                while (currentTurnBlock.Contains(null)) { currentTurnBlock.Remove(null); }
                while (nextList.Contains(null)) { nextList.Remove(null); }
                while (goodGuys.Contains(null)) { goodGuys.Remove(null); }
                while (enemies.Contains(null)) { enemies.Remove(null); }

                //****************************************************************Check Objective

                //starts the next turn
                if(currentTurnBlock.Count > 0)
                {
                    camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
                    currentTurnBlock[0].BeginTurn();
                }
                else
                {
                    SortLists();
                    if(currentTurnBlock.Count > 0)
                    {
                        camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
                        currentTurnBlock[0].BeginTurn();
                    }
                }
            }
            //switces to the next player character when the user presses tab while in a block of PlayerCharacters
            else if (Input.GetKeyDown(KeyCode.Tab) && currentTurnBlock.Count > 1 && currentTurnBlock[0] is PlayerCharacter) //******************************************************************Switch to button
            {
                currentTurnBlock[0].FinishedTurn = true;
                currentTurnBlock.Add(currentTurnBlock[0]);
                currentTurnBlock.RemoveAt(0);

                camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
                currentTurnBlock[0].BeginTurn();
            }
            //switches into free movement of camera for character analysis
            else if(Input.GetKeyDown(KeyCode.C)  && currentTurnBlock[0] is PlayerCharacter)
            {
                currentTurnBlock[0].FinishedTurn = true;
                state = CombatSceneState.CameraMode;
            }
        }

        if(state == CombatSceneState.CameraMode)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            cameraMoveStart = camera.transform.position;
            cameraMoveEnd = new Vector3(camera.transform.position.x + System.Math.Sign(input.x), camera.transform.position.y + System.Math.Sign(input.y), -10);
            lerpTime = 0f;

            state = CombatSceneState.MovingCamera;
        }

        if(state == CombatSceneState.MovingCamera)
        {
            lerpTime += Time.deltaTime * 5;
            camera.transform.position = Vector3.Lerp(cameraMoveStart, cameraMoveEnd, lerpTime);

            if(lerpTime >= 1f)
            {
                state = CombatSceneState.CameraMode;
            }
        }

        else if(state == CombatSceneState.ClosingDialogue)
        {
            // TODO
        }
	}




    /// <summary>
    /// Begins the scene
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

        //adds all playable character to nextList and goodGuys
        if (party.Count != 0)
        {
            for(int i = 0; i < party.Count; i++)
            {
                nextList.Add(party[i]);
                goodGuys.Add(party[i]);
            }
        }

        ////add enemies to nextList and runs their targeting set up
        //enemies = (from GameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.GetComponent<Enemy>()).ToList();
        //for (int i = 0; i < enemies.Count; i++)
        //{
        //    enemies[i].CreateTargetList();
        //    nextList.Add(enemies[i]);
        //}
        //adds enemies to charList
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

        //begin the combat stage of the scene
        state = CombatSceneState.Combat;

        //set up the first char to go
        camera.transform.position = new Vector3(currentTurnBlock[0].transform.position.x, currentTurnBlock[0].transform.position.y, -10);
        currentTurnBlock[0].BeginTurn();
    }

    /// <summary>
    /// Sorts all lists of combatants
    /// </summary>
    private void SortLists()
    {
        if(nextList.Count > 0 && nextList[0] is PlayerCharacter)
        {
            while(nextList.Count > 0 && nextList[0] is PlayerCharacter)
            {
                currentTurnBlock.Add(nextList[0]);
                nextList.RemoveAt(0);
            }
        }
        else if(nextList.Count > 0 && nextList[0] is Enemy)
        {
            while(nextList.Count > 0 && nextList[0] is Enemy)
            {
                currentTurnBlock.Add(nextList[0]);
                nextList.RemoveAt(0);
            }
        }
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


    //protected abstract void CheckObjective();
}
