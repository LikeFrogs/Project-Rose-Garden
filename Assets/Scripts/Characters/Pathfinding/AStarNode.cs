using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nodes used for A* pathfinding
/// </summary>
public class AStarNode
{
    #region Fields and Properties
    private AStarNode parent;

    private float g;
    private int h;
    private float f;

    private bool permanent;

    private int x;
    private int y;

    public float F { get { return f; } }
    #endregion

    public AStarNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        g = 0;
        h = 0;
        f = 0;
        permanent = false;
        parent = null;
    }
    public AStarNode(int x, int y, int targetX, int targetY, float cost, AStarNode parent)
    {
        this.x = x;
        this.y = y;

        g = parent.g + cost;
        h = System.Math.Abs(targetX - x) + System.Math.Abs(targetY - y);
        f = g + h;

        this.parent = parent;

        permanent = false;
    }

    /// <summary>
    /// Gets the shortest path from one position to another
    /// </summary>
    /// <param name="startPos">The starting position</param>
    /// <param name="endPos">The final position</param>
    /// <param name="moveCosts"> A matrix representing the cost of moving to any tile in the map</param>
    /// <returns>The path</returns>
    public static List<Vector3> FindPath(Vector3 startPos, Vector3 endPos, float[,] moveCosts)
    {
        //set up search parameters
        AStarPriorityQueue openQueue = new AStarPriorityQueue();
        AStarNode[,] graph = new AStarNode[moveCosts.GetLength(0), moveCosts.GetLength(1)];
        int targetX = (int)endPos.x;
        int targetY = (int)endPos.y;
        AStarNode startNode = new AStarNode((int)startPos.x, (int)startPos.y);
        openQueue.Insert(startNode);
        graph[startNode.x, startNode.y] = startNode;

        AStarNode current;
        while (!openQueue.IsEmpty())
        {
            //get the item from the open set with the lowest priority
            current = openQueue.ExtractMin();
            current.permanent = true;
            //if the target position is found, return the path
            if(current.x == targetX && current.y == targetY) { return current.DeterminePath(); }

            //evaluate each neighbor of current
            List<AStarNode> neighbors = current.GetNeighbors(moveCosts, graph, targetX, targetY, openQueue);
            for(int i = 0; i < neighbors.Count; i++)
            {
                //if moving to the neighbor is more efficient through current than through its previous parent
                //then updaate its fields
                float distanceToNeighbor = current.g + moveCosts[neighbors[i].x, neighbors[i].y];
                if(distanceToNeighbor < neighbors[i].g)
                {
                    neighbors[i].g = distanceToNeighbor;
                    neighbors[i].f = neighbors[i].g + neighbors[i].h;
                    neighbors[i].parent = current;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Determines the cost of movement from one position to another using the shortest possible path
    /// </summary>
    /// <returns>The cost</returns>
    public static int PathDistance(Vector3 startPos, Vector3 endPos, float[,] moveCosts)
    {
        List<Vector3> path = FindPath(startPos, endPos, moveCosts);

        return (path == null) ? int.MaxValue : path.Count;
    }

    /// <summary>
    /// Determines if it is possible to construct a path from one point to another
    /// </summary>
    /// <param name="startPos">The start of the proposed path</param>
    /// <param name="endPos">The end of the proposed path</param>
    /// <param name="moveCosts">Cost of moving to any given tile</param>
    /// <param name="speed">Speed of the character checking for a path, and thus the maximum lenght of the path</param>
    /// <returns>True if there is a possible path</returns>
    public static bool CheckSquare(Vector3 startPos, Vector3 endPos, float[,] moveCosts, int speed)
    {
        List<Vector3> path = FindPath(startPos, endPos, moveCosts);

        return (path == null) ? false : path.Count <= speed;
    }

    /// <summary>
    /// Gets all neighbors of the current node
    /// Also handles adding newly created nodes to the priority queue
    /// </summary>
    private List<AStarNode> GetNeighbors(float[,] moveCosts, AStarNode[,] graph, int targetX, int targetY, AStarPriorityQueue queue)
    {
        List<AStarNode> neighbors = new List<AStarNode>();

        //checks all mathematicall possible neighbors and either creates a new node for that position
        //or adds the node for that position if it has already been creaded and is not yet permanent
        if(x - 1 >= 0 && moveCosts[x - 1, y] != 0)
        {
            if(graph[x - 1, y] == null)
            {
                AStarNode newNode = new AStarNode(x - 1, y, targetX, targetY, moveCosts[x - 1, y], this);
                neighbors.Add(newNode);
                graph[x - 1, y] = newNode;
                queue.Insert(newNode);
            }
            else if(!graph[x - 1, y].permanent) { neighbors.Add(graph[x - 1, y]); }
        }
        if (x + 1 <= moveCosts.GetLength(0) && moveCosts[x + 1, y] != 0)
        {
            if (graph[x + 1, y] == null)
            {
                AStarNode newNode = new AStarNode(x + 1, y, targetX, targetY, moveCosts[x + 1, y], this);
                neighbors.Add(newNode);
                graph[x + 1, y] = newNode;
                queue.Insert(newNode);
            }
            else if (!graph[x + 1, y].permanent) { neighbors.Add(graph[x + 1, y]); }
        }
        if (y - 1 >= 0 && moveCosts[x, y - 1] != 0)
        {
            if (graph[x, y - 1] == null)
            {
                AStarNode newNode = new AStarNode(x, y - 1, targetX, targetY, moveCosts[x, y - 1], this);
                neighbors.Add(newNode);
                graph[x, y - 1] = newNode;
                queue.Insert(newNode);
            }
            else if (!graph[x, y - 1].permanent) { neighbors.Add(graph[x, y - 1]); }
        }
        if (y + 1 <= moveCosts.GetLength(1) && moveCosts[x, y + 1] != 0)
        {
            if (graph[x, y + 1] == null)
            {
                AStarNode newNode = new AStarNode(x, y + 1, targetX, targetY, moveCosts[x, y + 1], this);
                neighbors.Add(newNode);
                graph[x, y + 1] = newNode;
                queue.Insert(newNode);
            }
            else if (!graph[x, y + 1].permanent) { neighbors.Add(graph[x, y + 1]); }
        }

        return neighbors;
    }

    /// <summary>
    /// Determines the path from this node to the start node assuming that the search has already been run
    /// </summary>
    /// <returns>The path</returns>
    private List<Vector3> DeterminePath()
    {
        List<Vector3> reversedPath = new List<Vector3>();
        AStarNode current = this;

        while(current.parent != null)
        {
            reversedPath.Add(new Vector3(current.x, current.y));

            current = current.parent;
        }

        List<Vector3> path = new List<Vector3>();
        for(int i = reversedPath.Count - 1; i >= 0; i--)
        {
            path.Add(reversedPath[i]);
        }

        return path;
    }










    public static List<Vector3> FindPath(Vector3 startPos, Vector3 endPos, Dictionary<Vector3, GameObject> vision)
    {
        //calculate move costs based on provided range of vision
        float[,] moveCosts = CombatSceneController.MoveCosts;
        List<CombatChar> obstacles = CombatSceneController.GoodGuys;
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (vision.ContainsKey(obstacles[i].transform.position))
            {
                moveCosts[(int)obstacles[i].transform.position.x, (int)obstacles[i].transform.position.y] = 0;
            }
        }


        //set up search parameters
        AStarPriorityQueue openQueue = new AStarPriorityQueue();
        AStarNode[,] graph = new AStarNode[moveCosts.GetLength(0), moveCosts.GetLength(1)];
        int targetX = (int)endPos.x;
        int targetY = (int)endPos.y;
        AStarNode startNode = new AStarNode((int)startPos.x, (int)startPos.y);
        openQueue.Insert(startNode);
        graph[startNode.x, startNode.y] = startNode;

        AStarNode current;
        while (!openQueue.IsEmpty())
        {
            //get the item from the open set with the lowest priority
            current = openQueue.ExtractMin();
            current.permanent = true;
            //if the target position is found, return the path
            if (current.x == targetX && current.y == targetY) { return current.DeterminePath(); }

            //evaluate each neighbor of current
            List<AStarNode> neighbors = current.GetNeighbors(moveCosts, graph, targetX, targetY, openQueue);
            for (int i = 0; i < neighbors.Count; i++)
            {
                //if moving to the neighbor is more efficient through current than through its previous parent
                //then updaate its fields
                float distanceToNeighbor = current.g + moveCosts[neighbors[i].x, neighbors[i].y];
                if (distanceToNeighbor < neighbors[i].g)
                {
                    neighbors[i].g = distanceToNeighbor;
                    neighbors[i].f = neighbors[i].g + neighbors[i].h;
                    neighbors[i].parent = current;
                }
            }
        }
        return null;
    }

    public static int PathDistance(Vector3 startPos, Vector3 endPos, Dictionary<Vector3, GameObject> vision)
    {
        List<Vector3> path = FindPath(startPos, endPos, vision);

        return (path == null) ? int.MaxValue : path.Count;
    }
}
