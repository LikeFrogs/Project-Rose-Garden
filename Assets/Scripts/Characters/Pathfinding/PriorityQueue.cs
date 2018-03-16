﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    private List<List<Node>> containers;
    private int lowestPriority;
    private int nonEmptyBuckets;


    public PriorityQueue()
    {
        containers = new List<List<Node>>();
        for (int i = 0; i < containers.Count; i++) { containers[i] = new List<Node>(); }
        lowestPriority = int.MaxValue;

        nonEmptyBuckets = 0;
    }

    /// <summary>
    /// Adds a node to the priority queue using its distance as priority
    /// </summary>
    /// <param name="node">The node to insert</param>
    public void Insert(Node node)
    {
        containers[node.F].Add(node);

        //if adding a node whose priority does not match that of any other node in the queue,
        //the number of non empty buckets will increase by 1
        if (containers[node.F].Count == 1) { nonEmptyBuckets++; }

        //if the inserted node had a priority lower than the minimum, update the minimum
        lowestPriority = Mathf.Min(lowestPriority, node.F);
    }

    /// <summary>
    /// Returns the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public Node FindMin()
    {
        return containers[lowestPriority][containers[lowestPriority].Count - 1];
    }

    /// <summary>
    /// Returns and removes the node with the smallest priority 
    /// (arbitrarily selected if there are multiple nodes with the same lowest priority)
    /// </summary>
    public Node ExtractMin()
    {
        Node minNode = containers[lowestPriority][containers[lowestPriority].Count - 1];
        containers[lowestPriority].Remove(minNode);

        //if the last item of the lowest priority was removed, determine the new lowest priority
        if (containers[lowestPriority].Count == 0)
        {
            //if there are no more nodes with the previous lowest priority,
            //the number of non empty buckets has decreased by 1
            nonEmptyBuckets--;

            lowestPriority = int.MaxValue;
            for (int i = containers.Count - 1; i >= 0; i--)
            {
                if (containers[i].Count > 0) { lowestPriority = i; }
            }
        }

        return minNode;
    }

    /// <summary>
    /// Updates the priority of a node in the queue
    /// </summary>
    /// <param name="node">The node that should be updatated</param>
    public void UpdatePriority(Node node)
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
