using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathSegment : MonoBehaviour
{
    [SerializeField] private List<Sprite> possibleSprites;
    [SerializeField] private Image spriteRenderer;

    //set this segments image to the correct orientation
    public void SetImage(bool horizontal)
    {
        if (horizontal)
        {
            spriteRenderer.sprite = possibleSprites[1];
        }
        else
        {
            spriteRenderer.sprite = possibleSprites[0];
        }
    }
}
