using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField]
    private GameObject tilePrafab;
    [SerializeField]
    private Transform tilesParent;
    [SerializeField]
    private float spaceRatio;
    
    private Vector2Int puzzleSize;
    private Vector3 anchorPos;
    private Vector2 tileSize;
    private float Spacing;



    private void Start()
    {
        puzzleSize = new Vector2Int(4, 4);
        float unitCnt = (puzzleSize.x+1)*spaceRatio + puzzleSize.x;
        RectTransform boarderRectTransform = tilesParent.GetComponent<RectTransform>();
        tileSize = new Vector2(boarderRectTransform.rect.width / unitCnt, boarderRectTransform.rect.height / unitCnt);
        Spacing = tileSize.x * spaceRatio;
        anchorPos = tilesParent.position;
        
        for(int i=0; i<5; ++i)
        {
            SpawnTile(Random.Range(0, puzzleSize.x*puzzleSize.y));
        }
    }

    private void SpawnTile(int index, int level = 0)
    {
        int y = index / puzzleSize.x;
        int x = index % puzzleSize.x;

        GameObject clone = Instantiate(tilePrafab, tilesParent);

        RectTransform rect = clone.GetComponent<RectTransform>();
        rect.position = anchorPos + new Vector3(x*tileSize.x + (x+1)*Spacing, -(y*tileSize.y + (y+1)*Spacing), 0);
        rect.sizeDelta = tileSize;
        
        Tile tile = clone.GetComponent<Tile>();
        tile.Setup(level + Random.Range(0, 5));
    }


}
