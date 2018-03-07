using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraNode
{
    #region Fields and Properties
    private DijkstraNode parent;
    private int distance;
    private bool permanent;

    int x;
    int y;
    #endregion

    public DijkstraNode(int x, int y)
    {
        this.x = x;
        this.y = y;

        parent = null;
        distance = int.MaxValue;
        permanent = false;
    }
    public DijkstraNode(int x, int y, DijkstraNode parent, int cost)
    {
        this.x = x;
        this.y = y;

        this.parent = parent;
        distance = parent.distance + cost;
        permanent = false;
    }


    //public static List<Vector3> MoveRange(Vector3 start, int speed)
    //{
    //    //get matrix with cost to move to each node

    //    //make a node for start and add to priority queue

    //    //DijkstraNode current = queue.GetMin()

    //    //while(true)
    //        //get all neighbors to current
    //        //if they're within speed and there is a cost > 0 of moving to it, calculate cost and add to queue with parent as current

    //        //set current to permanent

    //        //current = queue.GetMin()

    //    //return all the nodes whose distance is <= to speed
    //}

}
