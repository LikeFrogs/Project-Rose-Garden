using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPriorityQueue
{
    private Dictionary<float, List<AStarNode>> containers;
    private float lowestPriority;
    private int nonEmptyBuckets;


    public AStarPriorityQueue()
    {
        containers = new Dictionary<float, List<AStarNode>>();
        lowestPriority = float.PositiveInfinity;
        nonEmptyBuckets = 0;
    }

    /// <summary>
    /// Adds a node to the priority queue using its distance as priority
    /// </summary>
    /// <param name="node">The node to insert</param>
    public void Insert(AStarNode node)
    {
        //if adding a node whose priority does not match that of any other node in the queue,
        //the number of non empty buckets will increase by 1
        if (!containers.ContainsKey(node.F))
        {
            nonEmptyBuckets++;
            containers[node.F] = new List<AStarNode>();
        }
        else if (containers[node.F].Count == 0)
        {
            nonEmptyBuckets++;
        }
        containers[node.F].Add(node);
        //if the inserted node had a priority lower than the minimum, update the minimum
        lowestPriority = Mathf.Min(lowestPriority, node.F);
    }

    /// <summary>
    /// Returns the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public AStarNode FindMin()
    {
        return containers[lowestPriority][containers[lowestPriority].Count - 1];
    }

    /// <summary>
    /// Returns and removes the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public AStarNode ExtractMin()
    {
        AStarNode minNode = containers[lowestPriority][containers[lowestPriority].Count - 1];
        containers[lowestPriority].Remove(minNode);

        //if the last item of the lowest priority was removed, determine the new lowest priority
        if (containers[lowestPriority].Count == 0)
        {
            //if there are no more nodes with the previous lowest priority,
            //the number of non empty buckets has decreased by 1
            nonEmptyBuckets--;

            lowestPriority = float.PositiveInfinity;
            foreach (KeyValuePair<float, List<AStarNode>> pair in containers)
            {
                if (pair.Value.Count > 0 && pair.Key < lowestPriority) { lowestPriority = pair.Key; }
            }
        }
        return minNode;
    }

    /// <summary>
    /// Updates the priority of a node in the queue
    /// </summary>
    /// <param name="node">The node that should be updatated</param>
    public void UpdatePriority(AStarNode node)
    {
        //removes the node from the queue and reinserts it based on its new priority
        containers[node.F].Remove(node);
        Insert(node);
    }

    /// <summary>
    /// Returns true if the queue is empty
    /// </summary>
    public bool IsEmpty()
    {
        return nonEmptyBuckets == 0;
    }
}
