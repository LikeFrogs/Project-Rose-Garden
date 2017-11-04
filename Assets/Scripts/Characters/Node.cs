using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Used for pathfinding
/// </summary>
public class Node
{
    #region Fields and Properties
    private Node parentNode;
    private Vector3 pos;
    private int g;
    private int h;
    private int f;

    /// <summary>
    /// Gets the parent Node of this Node
    /// </summary>
    Node Parent
    {
        get { return parentNode; }
    }
    /// <summary>
    /// Gets the Vector3 position of this Node
    /// </summary>
    Vector3 Position
    {
        get { return pos; }
    }
    /// <summary>
    /// Returns the number of squares a character must move from the starting Node
    /// to get to this Node
    /// </summary>
    int G
    {
        get { return g; }
    }
    /// <summary>
    /// Gets the manhattan distance from this Node to the goal Node
    /// </summary>
    int H
    {
        get { return h; }
    }
    /// <summary>
    /// Gets the total A* pathfinding cost of this Node (G + H values)
    /// </summary>
    int F
    {
        get { return f; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a default Node with ints at 0 and a parentNode that is null. Used to create a node for the origin square of the pathfinder
    /// </summary>
    /// <param name="pos">The position of the node  (should be the origin square of the pathfinder)</param>
    public Node(Vector3 pos)
    {
        this.pos = pos;
        g = 0;
        h = 0;
        f = 0;
        this.parentNode = null;
    }
    /// <summary>
    /// Creates a node with information needed in pathfinding
    /// </summary>
    /// <param name="startPos">The starting position of the pathfinding algorithm. Used to find h</param>
    /// <param name="pos">The position of the Node being created</param>
    /// <param name="parentNode">The new Node's parent Node</param>
    public Node(Vector3 endPos, Vector3 pos, Node parentNode)
    {
        this.pos = pos;
        g = parentNode.G + 1;
        h = System.Math.Abs((int)(endPos.x - pos.x)) + System.Math.Abs((int)(endPos.y - pos.y));
        f = g + h;
        this.parentNode = parentNode;
    }
    #endregion

    /// <summary>
    /// Determines if a character with a certain speed can reach endPos when starting from startPos
    /// </summary>
    /// <param name="startPos">The position the character would start from</param>
    /// <param name="endPos">The square that the characteris attempting to reach</param>
    /// <param name="speed">The speed of the character</param>
    /// <returns>True if the character can reach endPos</returns>
    public static bool CheckSquare(Vector3 startPos, Vector3 endPos, int speed) //additional boolean optional parameters can be added for special types of movement like flight
    {
        //will be set to true if a path is found to endPos
        bool found = false;

        //determines the bounds of the play area
        CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        Vector3 bottomLeft = controller.BottomLeftCorner;
        Vector3 topRight = controller.TopRightCorner;


        //creates a list of Vector3's that cannot be moved into
        //add a new line for each new tag
        List<Vector3> blockedList = (from gameObject in GameObject.FindGameObjectsWithTag("Blocking") select gameObject.transform.position).ToList();
        blockedList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Enemy") select gameObject.transform.position).ToList());
        blockedList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Interactable") select gameObject.transform.position).ToList());

        //no need to run the algorithm if the destination is not a reachable square
        if (blockedList.Contains(endPos))
        {
            return false;
        }

        //start the open list with the starting Node
        Node startNode = new Node(startPos);
        List<Node> openList = new List<Node> { startNode };
        blockedList.Add(startNode.Position);
        //start the closed list empty
        List<Node> closedList = new List<Node>();

        //the pathfinding algorithm continues as long as there are squares that can be tested in the open list
        while(openList.Count != 0)
        {
            //searches for the Node in the open list with the lowest F value and adds it to the closed list
            Node nextNode = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].F < nextNode.F)
                {
                    nextNode = openList[i];
                }
            }
            closedList.Add(nextNode);
            openList.Remove(nextNode);
            //if the node that was added was the ending position found is changed to true and we leave the while loop
            if (nextNode.Position == endPos)
            {
                found = true;
                break;
            }

            //adds Nodes to the open list for all positions that are unblocked and have not already been added to the list
            //any Nodes that are added to the open list have their positions added to blockedList so they can't be readded in the future
            Vector3 currentPos = closedList[closedList.Count - 1].Position;
            if (!blockedList.Contains(new Vector3(currentPos.x - 1, currentPos.y)) && (new Vector3(currentPos.x - 1, currentPos.y)).x >= bottomLeft.x)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x - 1, currentPos.y), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x - 1, currentPos.y));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x + 1, currentPos.y)) && (new Vector3(currentPos.x + 1, currentPos.y)).x <= topRight.x)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x + 1, currentPos.y), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x + 1, currentPos.y));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x, currentPos.y - 1)) && (new Vector3(currentPos.x, currentPos.y - 1)).y >= bottomLeft.y)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x, currentPos.y - 1), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x, currentPos.y - 1));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x, currentPos.y + 1)) && (new Vector3(currentPos.x, currentPos.y + 1)).y <= topRight.y)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x, currentPos.y + 1), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x, currentPos.y + 1));
            }
        }

        //if there is no path, the square is unreachable
        if (!found)
        {
            return false;
        }

        //creates a list that is only the Nodes in the shortest path from endPos to startPos
        List<Node> path = new List<Node> { closedList[closedList.Count - 1] };
        while(path[path.Count - 1].Parent != null)
        {
            path.Add(path[path.Count - 1].Parent);
        }

        //if the length of the path is less than or equal to the speed being checked then the square can be reached
        //path.Count - 1 is used because the origin square is included in path, but should not be counted as move cost
        if(path.Count - 1 <= speed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Finds and a path. Note that this method is only intended for use by the Enemy class. Also note that it does not take into account the positions
    /// of players and will return a path as if there are no obstructions. In this method endPos is assumed to be reachable from startPos in the scene's base state
    /// </summary>
    /// <param name="startPos">The position the character would start from</param>
    /// <param name="endPos">The square that the characteris attempting to reach</param>
    /// <param name="speed">The speed of the character</param>
    /// <returns>The found path</returns>
    public static List<Vector3> FindPath(Vector3 startPos, Vector3 endPos) //additional boolean optional parameters can be added for special types of movement like flight
    {
        //determines the bounds of the play area
        CombatSceneController controller = GameObject.FindGameObjectWithTag("SceneController").GetComponent<CombatSceneController>();
        Vector3 bottomLeft = controller.BottomLeftCorner;
        Vector3 topRight = controller.TopRightCorner;

        //creates a list of Vector3's that cannot be moved into
        //add a new line for each new tag
        List<Vector3> blockedList = (from gameObject in GameObject.FindGameObjectsWithTag("Blocking") select gameObject.transform.position).ToList();
        blockedList.AddRange((from gameObject in GameObject.FindGameObjectsWithTag("Interactable") select gameObject.transform.position).ToList());

        //start the open list with the starting Node
        Node startNode = new Node(startPos);
        List<Node> openList = new List<Node> { startNode };
        blockedList.Add(startNode.Position);
        //start the closed list empty
        List<Node> closedList = new List<Node>();

        //the pathfinding algorithm continues as long as there are squares that can be tested in the open list
        while (openList.Count != 0)
        {
            //searches for the Node in the open list with the lowest F value and adds it to the closed list
            Node nextNode = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].F < nextNode.F)
                {
                    nextNode = openList[i];
                }
            }
            closedList.Add(nextNode);
            openList.Remove(nextNode);
            //if the node that was added was the ending position found is changed to true and we leave the while loop
            if (nextNode.Position == endPos)
            {
                break;
            }

            //adds Nodes to the open list for all positions that are unblocked and have not already been added to the list
            //any Nodes that are added to the open list have their positions added to blockedList so they can't be readded in the future
            Vector3 currentPos = closedList[closedList.Count - 1].Position;
            if (!blockedList.Contains(new Vector3(currentPos.x - 1, currentPos.y)) && (new Vector3(currentPos.x - 1, currentPos.y)).x >= bottomLeft.x)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x - 1, currentPos.y), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x - 1, currentPos.y));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x + 1, currentPos.y)) && (new Vector3(currentPos.x + 1, currentPos.y)).x <= topRight.x)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x + 1, currentPos.y), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x + 1, currentPos.y));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x, currentPos.y - 1)) && (new Vector3(currentPos.x, currentPos.y - 1)).y >= bottomLeft.y)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x, currentPos.y - 1), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x, currentPos.y - 1));
            }
            if (!blockedList.Contains(new Vector3(currentPos.x, currentPos.y + 1)) && (new Vector3(currentPos.x, currentPos.y + 1)).y <= topRight.y)
            {
                openList.Add(new Node(endPos, new Vector3(currentPos.x, currentPos.y + 1), closedList[closedList.Count - 1]));
                blockedList.Add(new Vector3(currentPos.x, currentPos.y + 1));
            }
        }

        //creates a list that is only the Nodes in the shortest path from endPos to startPos
        List<Node> path = new List<Node> { closedList[closedList.Count - 1] };
        while (path[path.Count - 1].Parent != null)
        {
            path.Add(path[path.Count - 1].Parent);
        }

        //creates a list of vector 3's going from startPos to endPos along the shortest path and returns it
        List<Vector3> finalPath = new List<Vector3>();
        for(int  i = path.Count - 1; i >= 0; i--)
        {
            finalPath.Add(path[i].pos);
        }
        return finalPath;
    }

}
