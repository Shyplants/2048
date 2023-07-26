using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField]
    private GameObject tilePrafab;
    private MemoryPool memoryPool;
    [SerializeField]
    private Transform tilesParent;
    [SerializeField]
    private float spaceRatio;
    
    private Vector2Int puzzleSize;
    private int[] boardState;
    private int tileCount;
    private Vector3 anchorPos;
    private Vector2 tileSize;
    private float Spacing;

    private void Awake()
    {
        puzzleSize = new Vector2Int(4, 4);
        memoryPool = new MemoryPool(tilePrafab, tilesParent, puzzleSize.x);
    
        boardState = new int[puzzleSize.x*puzzleSize.y];
        tileCount = 0;

        float unitCnt = (puzzleSize.x+1)*spaceRatio + puzzleSize.x;
        RectTransform boarderRectTransform = tilesParent.GetComponent<RectTransform>();
        tileSize = new Vector2(boarderRectTransform.rect.width / unitCnt, boarderRectTransform.rect.height / unitCnt);
        Spacing = tileSize.x * spaceRatio;
        anchorPos = tilesParent.position;
    }

    private void OnApplicationQuit() 
    {
        Debug.Log("Called OnApplicationQuit");
        memoryPool.DestoryObjects();
    }

    private void Start()
    {
        // test code
        for(int i=0; i<2; ++i)
        {
            SpawnTile();
        }
    }

    private void Update() 
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if(x != 0 || y != 0) 
        {
            
        }
    }

    private Vector3 getTargetPos(int index)
    {
        Vector3 pos = anchorPos;

        int y = index / puzzleSize.x;
        int x = index % puzzleSize.x;

        pos += new Vector3(x*tileSize.x + (x+1)*Spacing, -(y*tileSize.y + (y+1)*Spacing), 0);
        return pos;
    }
    private void SpawnTile(int level = 1)
    {
        int index;
        while(true)
        {
            index = Random.Range(0, puzzleSize.x*puzzleSize.y);
            if(boardState[index] == 0) break;
        }
        level += Random.Range(0, 2);
        boardState[index] = level;
        tileCount++;
        
        GameObject clone = memoryPool.ActivePoolItem();

        RectTransform rect = clone.GetComponent<RectTransform>();
        rect.position = getTargetPos(index);
        rect.sizeDelta = tileSize;
        
        Tile tile = clone.GetComponent<Tile>();
        tile.Setup(level);
    }


}
