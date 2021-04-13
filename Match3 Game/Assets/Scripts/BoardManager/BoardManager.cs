using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public List<Sprite> characters = new List<Sprite>();
    public GameObject tile;
    public int width, height;
    private GameObject[,] tiles;
    public bool IsShifting {get; set;}
    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<BoardManager>();
        Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        SetUp(offset.x, offset.y);

    }

    private void SetUp(float xOffset, float yOffset)
    {   
        tiles = new GameObject[width, height];
        float startX = transform.position.x;
        float startY = transform.position.y;

        Sprite [] previousLeft = new Sprite [height];
        Sprite previousBelow = null;

        for (int x=0; x<width; x++){    
            for (int y=0; y<height; y++)
            {
                float tempX = startX + (xOffset*x);
                float tempY = startY + (yOffset*y);
                Vector3 tempV = new Vector3 (tempX, tempY, 0);
                GameObject newTile = Instantiate(tile, tempV, tile.transform.rotation);

                tiles[x,y] = newTile;
                newTile.transform.parent = transform;

                //prevents matching before player input
                //creates a list of possible Characters
                //adding all characters, then removing ones already used by adjacent
                List<Sprite> possibleCharacters =  new List<Sprite>();
                possibleCharacters.AddRange(characters);

                possibleCharacters.Remove(previousLeft[y]);
                possibleCharacters.Remove(previousBelow);
                // end of listing

                Sprite newSprite = possibleCharacters[Random.Range(0, 
                possibleCharacters.Count)];
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite;

                //sets adjacent sprites
                previousLeft[y] = newSprite;
                previousBelow = newSprite;
            }
        }
    }

    public IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
    {
        IsShifting = true;
        List<SpriteRenderer> renders = new List<SpriteRenderer>();
        int nullCount = 0;

        for (int y=yStart; y<height; y++)
        {
            SpriteRenderer rend = tiles[x,y].GetComponent<SpriteRenderer>();
            if (rend.sprite == null) nullCount++;
            renders.Add(rend);
        }

        for (int i=0; i<nullCount; i++)
        {
            yield return new WaitForSeconds(shiftDelay);
            for (int k=0; k< renders.Count-1; k++)
            {
                renders[k].sprite = renders[k+1].sprite;
                renders[k+1].sprite = GetNewSprite(x, height-1);
            }
        }
        IsShifting = false;
    }

    public IEnumerator FindNullTiles()
    {
        for(int x=0; x<width; x++){
            for (int y=0; y<height; y++)
            {
                if (tiles[x,y].GetComponent<SpriteRenderer>().sprite == null)
                {
                    yield return StartCoroutine(ShiftTilesDown(x,y));
                    break;
                }
            }
        }
        for (int x = 0; x<width; x++) {
            for (int y = 0; y<height; y++)
            {
                tiles[x, y].GetComponent<TileBackground>().ClearAllMatches();
            }
        }

    }

    private Sprite GetNewSprite(int x, int y)
    {
        List<Sprite> possibleCharacters = new List<Sprite>();
        possibleCharacters.AddRange(characters);

        if (x>0)
        {
            possibleCharacters.Remove(tiles[x-1, 
            y].GetComponent<SpriteRenderer>().sprite);
        }
        if (x <width-1)
        {
            possibleCharacters.Remove(tiles[x+1, 
            y].GetComponent<SpriteRenderer>().sprite);
        }
        if (y>0)
        {
            possibleCharacters.Remove(tiles[x,
            y-1].GetComponent<SpriteRenderer>().sprite);
        }
        return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
    }
}
