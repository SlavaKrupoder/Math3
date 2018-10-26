using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;
	private SpriteRenderer render;
	private bool isSelected = false;
	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private bool matchFound = false;

    void Awake()
    {
		render = GetComponent<SpriteRenderer>();
    }

	private void Select()
    {
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect()
    {
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

    void OnMouseDown()
    {
        if (render.sprite == null || BoardManager.instance.IsShifting)
        {
            return;
        }
        if (isSelected)
        {  
            Deselect();
        }
        else
        {
            if (previousSelected == null)
            {  
                Select();
            }
            else
            {
                if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
                {  
                    SwapSprite(previousSelected.render);  
                    previousSelected.ClearAllMatches();
                    ClearAllMatches();
                    previousSelected.Deselect();
                }
                else
                {  
                    previousSelected.GetComponent<Tile>().Deselect();
                    Select();
                }
            }
        }
    }

    public void SwapSprite(SpriteRenderer render2)
    {
        var renderSprite = render.sprite;
        var render2Sprite = render2.sprite;
        if (renderSprite == render2Sprite)
        {
            return;
        }
        Sprite tempSprite = render2.sprite;  
        render2.sprite = render.sprite;  
        render.sprite = tempSprite;  
        SFXManager.instance.PlaySFX(Clip.Swap); 
        GUIManager.instance.MovesLefCounter--;
    }

    private GameObject GetAdjacent(Vector2 castDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        var hitCollider = hit.collider;
        if (hitCollider != null)
        {
            return hitCollider.gameObject;
        }
        return null;
    }

    private List<GameObject> GetAllAdjacentTiles()
    {
        List<GameObject> adjacentTiles = new List<GameObject>();
        var adjDirectionsLength = adjacentDirections.Length;
        for (var i = 0; i < adjDirectionsLength; i++)
        {
            adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
        }
        return adjacentTiles;
    }

    private List<GameObject> FindMatch(Vector2 castDir)
    {
        List<GameObject> matchingTiles = new List<GameObject>();  
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);  
        while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite)
        {  
            matchingTiles.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
        }
        return matchingTiles;    
    }

    private void ClearMatch(Vector2[] paths)  
    {
        List<GameObject> matchingTiles = new List<GameObject>();  
        var pathsLength = paths.Length;
        for (var i = 0; i < pathsLength; i++)  
        {
            matchingTiles.AddRange(FindMatch(paths[i]));
        }
        var matchingTilesCount = matchingTiles.Count;
        if (matchingTilesCount >= 2)  
        {
            for (int i = 0; i < matchingTilesCount; i++)  
            {
                matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
            }
            matchFound = true;  
        }
    }

    public void ClearAllMatches()
    {
        var renderSprite = render.sprite;
        if (renderSprite == null)
        {
            return;
        }
        ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
        ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
        if (matchFound)
        {
            renderSprite = null;
            matchFound = false;
            StartCoroutine(BoardManager.instance.FindNullTiles());
            StopCoroutine(BoardManager.instance.FindNullTiles());
            SFXManager.instance.PlaySFX(Clip.Clear);
            GUIManager.instance.MoveCounter--;
        }
    }
}