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
    private Dictionary<int, GameObject> boardState;
    
    private Vector2Int puzzleSize;
    private Vector3 anchorPos;
    private Vector2 tileSize;
    private float Spacing;

    private void Awake()
    {
        puzzleSize = new Vector2Int(4, 4);
        memoryPool = new MemoryPool(tilePrafab, tilesParent, puzzleSize.x);
        boardState = new Dictionary<int, GameObject>();

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

    private void InitBoard()
    {
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            GameObject clone = memoryPool.ActivePoolItem();

            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.position = GetTargetPos(i);
            rect.sizeDelta = tileSize;

            Tile tile = clone.GetComponent<Tile>();
            tile.Setup();

            boardState[i] = clone;
        }

        memoryPool.DeactivatePoolItems();
    }

    private void Start()
    {
        InitBoard();

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
            MoveTiles(x, y);
        }
    }

    private Vector3 GetTargetPos(int index)
    {
        Vector3 pos = anchorPos;

        int y = index / puzzleSize.x;
        int x = index % puzzleSize.x;

        pos += new Vector3(x*tileSize.x + (x+1)*Spacing, -(y*tileSize.y + (y+1)*Spacing), 0);
        return pos;
    }
    private void SpawnTile()
    {
        int index;
        while(true)
        {
            index = Random.Range(0, puzzleSize.x*puzzleSize.y);
            if(!memoryPool.IsActivePoolItem(boardState[index])) break;
        }

        GameObject clone = memoryPool.ActivePoolItem(boardState[index]);
        Tile tile = clone.GetComponent<Tile>();
        tile.Level = Random.Range(0, 2) + 1;
    }
    private void MergeTile(int src, int dest)
    {
    
    }

    private int Pos2Dto1D(int y, int x)
    {
        return y*puzzleSize.x + x;
    }

    private void MergeTiles(float dx, float dy)
    {
        
    }
    private void MoveTiles(float dx, float dy)
    {
        if(dx != 0.0f) // 수평 움직임
        {
            bool moveRight = (dx > 0.0f ? true : false);
            bool IsMerge = false;
            for(int i=0; i<puzzleSize.y; ++i)
            {
                for(int j=0; j<puzzleSize.x-1; ++j)
                {
                    int cur = (moveRight ? puzzleSize.x-2-j : j+1);
                    int nxt = (moveRight ? puzzleSize.x-1-j : j);
                    int curIndex = Pos2Dto1D(i, cur);
                    int nxtIndex = Pos2Dto1D(i, nxt);

                    /*
                    if(boardState[curIndex] > 0)
                    {
                        if(boardState[nxtIndex] == 0) {
                            // move tile
                        }
                        else if(boardState[curIndex] == boardState[nxtIndex])
                        {
                            // mergre
                            memoryPool.DeactivatePoolItem(indexToTile[cur]);
                            indexToTile.Remove(cur);
                            boardState[cur] = 0;
                            
                            IsMerge = true;
                        }
                    }

                    if((boardState[cur] > 0) && (boardState[cur] == boardState[nxt]))
                    {
                        
                        break;
                    }
                    */
                }
            }
        }
        else 
        {

        }
    }


}
