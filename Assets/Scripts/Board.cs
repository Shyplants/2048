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
    private Dictionary<int, GameObject> boardState;
    
    private Vector2Int puzzleSize;
    private Vector3 anchorPos;
    private Vector3[] tilePos;
    private Vector2 tileSize;
    private float Spacing;

    private bool canActive;
    private float actionDelayTime;
    private List<Vector2Int> moveList;
    private bool[] isFilled;
    private bool[] isMerged;
    private int[] numState;
    private int moveCount;

    private void Awake()
    {
        puzzleSize = new Vector2Int(4, 4);
        boardState = new Dictionary<int, GameObject>();
        moveList = new List<Vector2Int>();

        float unitCnt = (puzzleSize.x+1)*spaceRatio + puzzleSize.x;
        RectTransform boarderRectTransform = tilesParent.GetComponent<RectTransform>();
        tileSize = new Vector2(boarderRectTransform.rect.width / unitCnt, boarderRectTransform.rect.height / unitCnt);
        Spacing = tileSize.x * spaceRatio;
        anchorPos = tilesParent.position;

        actionDelayTime = 0.0f;
    }

    private void OnApplicationQuit() 
    {
        Debug.Log("Called OnApplicationQuit");
    }

    private void InitBoard()
    {
        tilePos = new Vector3[puzzleSize.x*puzzleSize.y];
        numState = new int[puzzleSize.x*puzzleSize.y];

        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            GameObject clone = GameObject.Instantiate(tilePrafab, tilesParent);

            tilePos[i] = GetTilePos(i);
            RectTransform rect = clone.GetComponent<RectTransform>();
            
            rect.localPosition = tilePos[i];
            rect.sizeDelta = tileSize;

            Tile tile = clone.GetComponent<Tile>();
            tile.Setup(0);
            tile.OnMoveToComplete += HandleMoveToComplete;

            boardState[i] = clone;
        }
    }

    private void Start()
    {
        InitBoard();
        // test code
        /*
        DebugSpawn(0, 0, 2); DebugSpawn(0, 1, 2); DebugSpawn(0, 2, 2); DebugSpawn(0, 3, 2);
        DebugSpawn(1, 2, 1); DebugSpawn(1, 3, 2); 
        DebugSpawn(2, 1, 1); DebugSpawn(2, 2, 2); DebugSpawn(2, 3, 4);
        DebugSpawn(3, 1, 1); DebugSpawn(3, 2, 2); DebugSpawn(3, 3, 4);
        */
        
        for(int i=0; i<2; ++i)
        {
            SpawnTile();
        }
        
        canActive = true;
    }

    private void Update() 
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if(!canActive) return;

        if(x != 0 || y != 0) 
        {
            canActive = false;
            ForceTiles(x, y);
        }
    }
    private Vector2 GetTilePos(int index)
    {
        int y = index / puzzleSize.x;
        int x = index % puzzleSize.x;

        return new Vector3(x*tileSize.x + (x+1)*Spacing, -(y*tileSize.y + (y+1)*Spacing), 0);
    }

    private void DebugSpawn(int y, int x, int num)
    {
        int index = y*puzzleSize.x + x;

        Tile tile = boardState[index].GetComponent<Tile>();
        tile.Setup(num);

        numState[index] = num;
    }

    
    private void SpawnTile()
    {
        int index;
        while(true)
        {   
            index = Random.Range(0, puzzleSize.x*puzzleSize.y);
            if(!boardState[index].GetComponent<Tile>().IsActive) break;
        }

        Tile tile = boardState[index].GetComponent<Tile>();
        tile.Numeric = 1 << (Random.Range(0, 2) + 1);

        numState[index] = tile.Numeric;
    }
    private int Pos2Dto1D(int y, int x)
    {
        return y*puzzleSize.x + x;
    }

    private bool OOB(int y, int x)
    {
        return y < 0 || y >= puzzleSize.y || x < 0 || x >= puzzleSize.x;
    }

    private void UpdateBoardState()
    {
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            boardState[i].GetComponent<Tile>().Numeric = numState[i];
        }
    }

    private void ClearBoardState()
    {
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            boardState[i].GetComponent<RectTransform>().localPosition = tilePos[i];
            boardState[i].GetComponent<Tile>().Numeric = 0;
        }
    }

    private void HandleMoveToComplete()
    {
        moveCount--;
        if(moveCount == 0)
        {   
            ClearBoardState();
            UpdateBoardState();
        
            canActive = true;     
            while(true)
            {
                SpawnTile();
                break;
            }
        }
    }

    private void ForceTiles(float dx, float dy)
    {
        isFilled = new bool[puzzleSize.x*puzzleSize.y];
        isMerged = new bool[puzzleSize.x*puzzleSize.y];
        moveCount = 0;
        
        if(dx != 0.0f) // 수평 움직임
        {
            bool moveRight = (dx > 0.0f ? true : false);
            
            for(int i=0; i<puzzleSize.y; ++i)
            {
                for(int j=0; j<puzzleSize.x; ++j)
                {
                    int cur = (moveRight ? puzzleSize.x-1-j : j);
                    int curIndex = Pos2Dto1D(i, cur);
                    int moveIndex = curIndex;

                    int curNum = numState[curIndex];
                    if(curNum == 0) continue;

                    int nx = cur;
                    while(true)
                    {
                        nx += (moveRight ? 1 : -1);
                        if(OOB(i, nx)) break;

                        int nextIndex = Pos2Dto1D(i, nx);
                        int nextNum = numState[nextIndex];
                        if(!isFilled[nextIndex]) 
                        {
                            moveIndex = nextIndex;
                        }
                        else if(!isMerged[nextIndex] && (curNum == nextNum))
                        {
                            // nextTile 레벨 올려야됨
                            moveIndex = nextIndex;
                            isMerged[moveIndex] = true;
                            break;
                        }
                        else break;
                    }
                    
                    if(moveIndex != curIndex)
                    {
                        numState[moveIndex] += numState[curIndex];
                        numState[curIndex] = 0;
                        moveList.Add(new Vector2Int(curIndex, moveIndex));
                    }
                    isFilled[moveIndex] = true;
                }                    
            }
            moveCount = moveList.Count;
            if(moveCount == 0)
            {
                canActive = true;
                return;
            }

            foreach(Vector2Int v in moveList)
            {
                boardState[v.x].GetComponent<Tile>().OnMoveTo(tilePos[v.y]);
            }
            moveList.Clear();
        }
    }
}
