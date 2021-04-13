using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBackground : MonoBehaviour
{

    //Drag Variables
    private Vector2 firstTouchPosition, finalTouchPosition;
    public float angle = 0;


    //Sprite related Variables
    private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
    private static TileBackground previousSelected = null;
    private SpriteRenderer rend;

    private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };


    private bool isSelected = false;
    private bool matchFound = false;
    private bool swapBack = false;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }

 
    private void Select()
    {
        isSelected = true;
        rend.color = selectedColor;
        previousSelected = gameObject.GetComponent<TileBackground>();
    }

    private void Deselect()
    {
        isSelected = false;
        rend.color = Color.white;
        previousSelected = null;
    }

    void OnMouseDown()
    {
        if (rend.sprite == null || BoardManager.instance.IsShifting) return;
        
        if (isSelected) Deselect();
        else 
        {
            if (previousSelected == null) Select();
            else 
            {
                if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
                {
                    SwapSprite(previousSelected.gameObject);
                    previousSelected.ClearAllMatches();
                    ClearAllMatches();
                    if (swapBack) {
                        SwapSprite(previousSelected.gameObject);
                        swapBack = false;
                    }
                    previousSelected.Deselect();
                } 
                else 
                {
                    previousSelected.GetComponent<TileBackground>().Deselect();
                    Select();
                }
            }
        }
    }

    public void SwapSprite(GameObject previousSelected)
    {
        SpriteRenderer render = previousSelected.GetComponent<SpriteRenderer>();
        if (rend.sprite == render.sprite) return;
        Sprite tempSprite = render.sprite;
        render.sprite = rend.sprite;
        rend.sprite = tempSprite;
    }

    private GameObject GetAdjacent(Vector2 castDir) 
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        if (hit.collider != null) 
        {
            return hit.collider.gameObject;
        }
        return null;
    }


    private List<GameObject> GetAllAdjacentTiles() 
    {
        List<GameObject> adjacentTiles = new List<GameObject>();
        for (int i = 0; i < adjacentDirections.Length; i++) 
        {
            adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
        }
        return adjacentTiles;
    }

    private List <GameObject> FindMatch(Vector2 castDir)
    {
        List<GameObject> matchingTiles = new List<GameObject>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        while(hit.collider != null && hit.collider.GetComponent<SpriteRenderer>
        ().sprite == rend.sprite)
        {
            matchingTiles.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
        }
        return matchingTiles;
    }

    private void ClearMatch (Vector2[] paths)
    {
        List<GameObject> matchingTiles = new List<GameObject>();
        for (int i=0; i<paths.Length; i++)
        {
            matchingTiles.AddRange(FindMatch(paths[i]));
        }
        if (matchingTiles.Count >= 2)
        {
            for (int i=0; i<matchingTiles.Count; i++)
            {
                matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
            }
            matchFound = true;
        }
    }

    public void ClearAllMatches() {
        if (rend.sprite == null) return;

        ClearMatch(new Vector2[2] {Vector2.left, Vector2.right});
        ClearMatch(new Vector2[2] {Vector2.up, Vector2.down});
        if (matchFound) {
            rend.sprite = null;
            matchFound = false;
            StopCoroutine(BoardManager.instance.FindNullTiles());
            StartCoroutine(BoardManager.instance.FindNullTiles());
        }
        else
        {
            swapBack = true;
        }

    }  
}
