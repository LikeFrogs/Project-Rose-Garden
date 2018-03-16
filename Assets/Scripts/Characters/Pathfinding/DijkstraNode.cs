using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Used for running Dijkstra's algorithm
/// </summary>
public class DijkstraNode
{
    #region Fields and Properties
    private float distance;
    private bool permanent;
    
    private int x;
    private int y;

    /// <summary>
    /// Gets the distance from the starting node to this node
    /// </summary>
    public float Distance { get { return distance; } }
    #endregion

    public DijkstraNode(int x, int y, bool isStart)
    {
        this.x = x;
        this.y = y;

        if (!isStart) { distance = float.PositiveInfinity; }
        else { distance = 0; }
    }

    /// <summary>
    /// Calculates the possible range of movement for a playable character
    /// </summary>
    /// <param name="start">The starting position of the character</param>
    /// <param name="speed">The speed fo the character</param>
    /// <param name="moveCosts">An array representing the cost of moving to any tile in the map</param>
    /// <returns></returns>
    public static List<Vector3> MoveRange(Vector3 start, int speed, float[,] moveCosts)
    {        
        //creates the searh queue and adds the start to it
        DijkstraPriorityQueue queue = new DijkstraPriorityQueue();
        queue.Insert(new DijkstraNode((int)start.x, (int)start.y, true));

        //create a graph of all nodes (positions) to check
        DijkstraNode[,] graph = new DijkstraNode[(int)start.x + speed + 1, (int) start.y + speed + 1];
        for (int x = (int)start.x - speed; x <= (int)start.x + speed; x++)
        {
            for (int y = (int)start.y - (speed - Math.Abs((int)start.x - x)); Math.Abs((int)start.x - x) + Math.Abs((int)start.y - y) <= speed; y++)
            {
                //add all possible nodes except the start node
                //also do not add nodes outside of the scope of the map
                //if the cost to move to a tile is listed as 0, it is blocked
                if (!(x == start.x && y == start.y) && x >= 0 && y >= 0 && x < moveCosts.GetLength(0) && y < moveCosts.GetLength(1) && moveCosts[x, y] > 0)
                {
                    graph[x, y] = new DijkstraNode(x, y, false);
                }
            }
        }

        List<Vector3> moveRange = new List<Vector3>();
        DijkstraNode current;

        while (!queue.IsEmpty())
        {
            //get the item from the queue with the lowest priority
            current = queue.ExtractMin();

            //get and handle all neighbors of the current node
            List<DijkstraNode> neighbors = current.GetNeighbors(graph);
            for(int i = 0; i < neighbors.Count; i++)
            {
                //distance to the neighbor, from the current node is the current node's distance + the cost to move to the neighbor
                float distanceToNeighbor = current.distance + moveCosts[neighbors[i].x, neighbors[i].y];

                //if distance is greater than speed or greater than previously calculated distance, don't add the node to the queue
                if(distanceToNeighbor < neighbors[i].distance && distanceToNeighbor <= speed)
                {
                    //if the distance is not the default value, the node is not already in the queue
                    bool inQueue = neighbors[i].distance != float.PositiveInfinity;

                    //update the stats on the node and then add it to the queue or update its position in the queu
                    neighbors[i].distance = distanceToNeighbor;
                    if (inQueue)
                    {
                        queue.UpdatePriority(neighbors[i]);
                    }
                    else
                    {
                        queue.Insert(neighbors[i]);
                    }
                }
            }
            //after finishing with the current node, it should never be handled again
            current.permanent = true;
            moveRange.Add(new Vector3(current.x, current.y));
        }

        return moveRange;
    }

    /// <summary>
    /// Gets all neighbors of the current node
    /// </summary>
    /// <param name="graph">The graph to check for neighbors</param>
    /// <returns></returns>
    private List<DijkstraNode> GetNeighbors(DijkstraNode[,] graph)
    {
        List<DijkstraNode> neighbors = new List<DijkstraNode>(4);
        //adds all mathematically possible neighbors that are present in the graph and not yet permanent
        if (x - 1 >= 0 && graph[x - 1, y] != null && !graph[x - 1, y].permanent) { neighbors.Add(graph[x - 1, y]); }
        if (x + 1 < graph.GetLength(0) && graph[x + 1, y] != null && !graph[x + 1, y].permanent) { neighbors.Add(graph[x + 1, y]); }
        if (y - 1 >= 0 && graph[x, y - 1] != null && !graph[x, y - 1].permanent) { neighbors.Add(graph[x, y - 1]); }
        if (y + 1 < graph.GetLength(1) && graph[x, y + 1] != null && !graph[x, y + 1].permanent) { neighbors.Add(graph[x, y + 1]); }

        return neighbors;
    }
}
