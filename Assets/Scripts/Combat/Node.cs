using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    Node parentNode;
    Vector3 pos;
    int g;
    int h;
    int f;

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
        parentNode = null;
    }
    //public Node(Vector3 startPos, Vector3 pos,)

    public static bool CheckSquare(Vector3 startPos, Vector3 endPos, int speed)
    {
        //run the pathfinding algorithm from startPos to endPos and count the movement it takes to get there
        //if greater than speed return false

        //FindGameObjectsWithTag will be used to get obstacles and enemies that can't be moved through
        return false;
    }
}
