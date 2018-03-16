using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Priority Queue for Dijkstra's algorithm
/// </summary>
public class DijkstraPriorityQueue
{
    private Dictionary<float, List<DijkstraNode>> containers;
    private float lowestPriority;
    private int nonEmptyBuckets;


    public DijkstraPriorityQueue(int maxPriority)
    {
        containers = new Dictionary<float, List<DijkstraNode>>();
        lowestPriority = float.PositiveInfinity;
        nonEmptyBuckets = 0;
    }

    /// <summary>
    /// Adds a node to the priority queue using its distance as priority
    /// </summary>
    /// <param name="node">The node to insert</param>
    public void Insert(DijkstraNode node)
    {
        //if adding a node whose priority does not match that of any other node in the queue,
        //the number of non empty buckets will increase by 1
        if (!containers.ContainsKey(node.Distance))
        {
            nonEmptyBuckets++;
            containers[node.Distance] = new List<DijkstraNode>();
        }else if(containers[node.Distance].Count == 0)
        {
            nonEmptyBuckets++;
        }
        containers[node.Distance].Add(node);
        //if the inserted node had a priority lower than the minimum, update the minimum
        lowestPriority = Mathf.Min(lowestPriority, node.Distance);
    }

    /// <summary>
    /// Returns the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public DijkstraNode FindMin()
    {
        return containers[lowestPriority][containers[lowestPriority].Count - 1];
    }

    /// <summary>
    /// Returns and removes the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public DijkstraNode ExtractMin()
    {
        DijkstraNode minNode = containers[lowestPriority][containers[lowestPriority].Count - 1];
        containers[lowestPriority].Remove(minNode);

        //if the last item of the lowest priority was removed, determine the new lowest priority
        if (containers[lowestPriority].Count == 0)
        {
            //if there are no more nodes with the previous lowest priority,
            //the number of non empty buckets has decreased by 1
            nonEmptyBuckets--;

            lowestPriority = float.PositiveInfinity;
            foreach(KeyValuePair<float, List<DijkstraNode>> pair in containers)
            {
                if(pair.Value.Count > 0 && pair.Key < lowestPriority) { lowestPriority = pair.Key; }
            }
        }
        return minNode;
    }

    /// <summary>
    /// Updates the priority of a node in the queue
    /// </summary>
    /// <param name="node">The node that should be updatated</param>
    public void UpdatePriority(DijkstraNode node)
    {
        //removes the node from the queue and reinserts it based on its new priority
        containers[node.Distance].Remove(node);
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
