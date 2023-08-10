using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    private bool isRunning;

    private bool canActive;
    private float actionDelayTime;
    private List<Vector2Int> moveList;
    private List<int> emptyList;
    private bool[] isFilled;
    private bool[] isMerged;
    private int[] numState;
    private int moveCount;


    private void Awake()
    {
        puzzleSize = new Vector2Int(4, 4);
        boardState = new Dictionary<int, GameObject>();
        moveList = new List<Vector2Int>();
        emptyList = new List<int>();

        float unitCnt = (puzzleSize.x+1)*spaceRatio + puzzleSize.x;
        RectTransform boarderRectTransform = tilesParent.GetComponent<RectTransform>();
        tileSize = new Vector2(boarderRectTransform.rect.width / unitCnt, boarderRectTransform.rect.height / unitCnt);
        Spacing = tileSize.x * spaceRatio;
        anchorPos = tilesParent.position;
        isRunning = true;

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
            emptyList.Add(i);
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
        if(!isRunning)
        {
            Application.Quit();
        }


        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if(!canActive) return;

        if(x != 0 || y != 0) 
        {
            canActive = false;
            ForceTiles(x, y);
        }
    }

    private bool IsFinish()
    {
        int[] dy = new int[] {-1, 1, 0, 0};
        int[] dx = new int[] {0, 0, -1, 1};

        for(int i=0; i<puzzleSize.y; ++i)
        {
            for(int j=0; j<puzzleSize.x; ++j)
            {
                int curIndex = Pos2Dto1D(i, j);
                for(int dir=0; dir<4; ++dir)
                {
                    int ny = i + dy[dir];
                    int nx = j + dx[dir];
                    if(OOB(ny, nx)) continue;

                    int nextIndex = Pos2Dto1D(ny, nx);
                    if(numState[curIndex] == numState[nextIndex])
                    {
                        // 현재 타일과 인접한 타일의 상태가 같음(게임 진행 가능)
                        return false;
                    }
                }
                
            }
        }

        return true;
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

    
    private bool SpawnTile()
    {
        if(emptyList.Count == 0) return false;

        int randomIndex = Random.Range(0, emptyList.Count);
        int index = emptyList[randomIndex];
        Tile tile = boardState[index].GetComponent<Tile>();
        tile.Numeric = 1 << (Random.Range(0, 2) + 1);

        numState[index] = tile.Numeric;
        emptyList.RemoveAt(randomIndex);

        return true;
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
        emptyList.Clear();
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            boardState[i].GetComponent<RectTransform>().localPosition = tilePos[i];
            boardState[i].GetComponent<Tile>().Numeric = numState[i];

            if(numState[i] == 0)
            {
                emptyList.Add(i);
            }
        }
    }

    private void HandleMoveToComplete()
    {
        moveCount--;
        if(moveCount == 0)
        {   
            UpdateBoardState();
            while(true)
            {
                SpawnTile();
                break;
            }

            if(emptyList.Count == 0)
            {
                isRunning = !IsFinish();
            }

            canActive = true;
        }
    }

    private void ForceTiles(float dx, float dy)
    {
        isFilled = new bool[puzzleSize.x*puzzleSize.y];
        isMerged = new bool[puzzleSize.x*puzzleSize.y];
        
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
        }
        else  // 수직 움직임
        {
            bool moveDown = (dy < 0.0 ? true : false);
            
            for(int j=0; j<puzzleSize.x; ++j)
            {
                for(int i=0; i<puzzleSize.y; ++i)
                {
                    int cur = (moveDown ? puzzleSize.y-1-i : i);
                    int curIndex = Pos2Dto1D(cur, j);
                    int moveIndex = curIndex;

                    int curNum = numState[curIndex];
                    if(curNum == 0) continue;

                    int ny = cur;
                    while(true)
                    {
                        ny += (moveDown ? 1 : -1);
                        if(OOB(ny, j)) break;

                        int nextIndex = Pos2Dto1D(ny, j);
                        int nextNum = numState[nextIndex];
                        if(!isFilled[nextIndex])
                        {
                            moveIndex = nextIndex;
                        }
                        else if(!isMerged[nextIndex] && (curNum == nextNum))
                        {
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
        }

        foreach(Vector2Int v in moveList)
        {
            boardState[v.x].GetComponent<Tile>().OnMoveTo(tilePos[v.y]);
        }
        moveList.Clear();
    }

    
}
