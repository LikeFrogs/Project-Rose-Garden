using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an area of the map. To have effectively searched an area at least one square from each List<Vector3> must have been visited and searched
/// </summary>
public class SearchZone : MonoBehaviour
{
    [System.Serializable]
    private class ListWrapper
    {
        public List<Vector3> positionOptions;
    }
    [SerializeField] List<ListWrapper> wrappedList = new List<ListWrapper>();
    private List<List<Vector3>> keyPositionLists;

    //sets up the List<List<Vector3>> from the wrapped list
    private void Start()
    {
        keyPositionLists = new List<List<Vector3>>();


        foreach(ListWrapper list in wrappedList)
        {
            keyPositionLists.Add(list.positionOptions);
        }
    }

    /// <summary>
    /// Returns a list of lists of positions. One position from each list must be searched to have effectively searched an area
    /// </summary>
    public List<List<Vector3>> KeyPositionLists
    {
        get { return keyPositionLists; }
    }
}
