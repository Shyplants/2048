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
    private Dictionary<int, GameObject> nxtBoardState;
    
    private Vector2Int puzzleSize;
    private Vector3 anchorPos;
    private Vector2 tileSize;
    private float Spacing;

    private bool canActive;
    private float actionDelayTime;
    private List<Vector2Int> moveList;
    private bool[] isBlocked;
    private int moveCount;

    private void Awake()
    {
        puzzleSize = new Vector2Int(4, 4);
        boardState = new Dictionary<int, GameObject>();
        nxtBoardState = new Dictionary<int, GameObject>();
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
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            GameObject clone = GameObject.Instantiate(tilePrafab, tilesParent);

            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.position = GetTargetPos(i);
            rect.sizeDelta = tileSize;

            Tile tile = clone.GetComponent<Tile>();
            tile.Index = i;
            tile.Setup();
            tile.OnMoveToComplete += HandleMoveToComplete;

            boardState[i] = clone;
        }
    }

    private void Start()
    {
        InitBoard();
        // test code
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

        if(!canActive)
        {
            actionDelayTime += Time.deltaTime;
            if(actionDelayTime > 0.5f)
            {
                canActive = true;
                actionDelayTime = 0.0f;
            }
            else
                return;
        }

        if(x != 0 || y != 0) 
        {
            if(ForceTiles(x, y))
            {
                SpawnTile();
            }
            canActive = false;
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

    private Vector2 GetNextPos(int index)
    {
        int y = index / puzzleSize.x;
        int x = index % puzzleSize.x;

        return new Vector3(x*tileSize.x + (x+1)*Spacing, -(y*tileSize.y + (y+1)*Spacing), 0);
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
        tile.IsActive = true;
        Debug.Log("Spawned: " + index/puzzleSize.x + " " + index%puzzleSize.x);
        tile.Level = Random.Range(0, 2) + 1;
    }
    private int Pos2Dto1D(int y, int x)
    {
        return y*puzzleSize.x + x;
    }

    private bool OOB(int y, int x)
    {
        return y < 0 || y >= puzzleSize.y || x < 0 || x >= puzzleSize.x;
    }

    private void SwapObject(int src, int dest)
    {
        GameObject temp = nxtBoardState[src];
        nxtBoardState[src] = nxtBoardState[dest];
        nxtBoardState[dest] = temp;
    }

    private void HandleMoveToComplete()
    {
        moveCount--;
        Debug.Log("이동이 끝났습니다. ");

        if(moveCount == 0)
        {
            for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
            {
                boardState[i] = nxtBoardState[i];
            }
            canActive = true;
        }
    }

    private bool ForceTiles(float dx, float dy)
    {
        bool IsChanged = false;
        isBlocked = new bool[puzzleSize.x*puzzleSize.y];
        for(int i=0; i<puzzleSize.x*puzzleSize.y; ++i)
        {
            nxtBoardState[i] = boardState[i];
        }
        moveCount = 0;

        if(dx != 0.0f) // 수평 움직임
        {
            Debug.Log("수평 움직임");
            bool moveRight = (dx > 0.0f ? true : false);
            
            for(int i=0; i<puzzleSize.y; ++i)
            {
                for(int j=0; j<puzzleSize.x; ++j)
                {
                    int cur = (moveRight ? puzzleSize.x-1-j : j);
                    int curIndex = Pos2Dto1D(i, cur);
                    int targetIndex = -1;

                    Tile curTile = boardState[curIndex].GetComponent<Tile>();
                    if(!curTile.IsActive) continue;

                    int nx = cur;
                    while(true)
                    {
                        nx += (moveRight ? 1 : -1);
                        if(OOB(i, nx)) break;

                        int nextIndex = Pos2Dto1D(i, nx);
                        Tile nextTile = boardState[nextIndex].GetComponent<Tile>();
                        if(!nextTile.IsActive) 
                        {
                            targetIndex = nextIndex;
                        }
                        else if(curTile.Level == nextTile.Level)
                        {
                            // nextTile 레벨 올려야됨
                            targetIndex = nextIndex;
                            isBlocked[nextIndex] = true;
                        }
                    }

                    if(targetIndex != -1)
                    {
                        moveCount++;
                        moveList.Add(new Vector2Int(curIndex, targetIndex));
                        SwapObject(curIndex, targetIndex);
                    }
                }                    
            }
            if(moveCount == 0) return false;

            canActive = false;
            foreach(Vector2Int v in moveList)
            {
                boardState[v.x].GetComponent<Tile>().OnMoveTo(GetNextPos(v.y));
            }

            moveList.Clear();
        }

        return IsChanged;
    }
    
}
