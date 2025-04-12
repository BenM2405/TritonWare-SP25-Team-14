using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public Tile[,] tiles = new Tile[2,2];
    public Tile[,] targetTiles = new Tile[2,2];
    public Vector2Int selectedPos = new Vector2Int(0,0);
    private Vector2Int? secondselectedPos = null;

    public Sprite circleSprite;
    public Sprite squareSprite;

    void Start()
    {
        AssignPlayerTiles();
        AssignTargetTiles();
        InitializeBalancedGrids();
        HighlightSelected();
    }

    void Update()
    {
        HandleInput();
        HandleSwap();
    }

    System.Collections.IEnumerator RegeneratePuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        selectedPos = new Vector2Int(0,0);
        secondselectedPos = null;

        InitializeBalancedGrids();
        HighlightSelected();
    }


    void AssignPlayerTiles()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                string objName = $"Tile_{x}_{y}";
                GameObject tileObj = GameObject.Find(objName);

                if (tileObj == null)
                {
                    Debug.LogError($"Could not find GameObject: {objName}");
                    continue;
                }

                Tile tile = tileObj.GetComponent<Tile>();

                if (tile == null)
                {
                    Debug.LogError($"Tile.cs is missing on: {objName}");
                    continue;
                }

                tile.gridPos = new Vector2Int(x, y);
                tiles[x, y] = tile;
            }
        }
    }


    void AssignTargetTiles()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                string objName = $"TargetTile_{x}_{y}";
                GameObject tileObj = GameObject.Find(objName);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x,y);
                targetTiles[x, y] = tile;
            }
        }
    }

    void InitializeBalancedGrids()
    {
        List<Tile.SymbolType> symbols = new List<Tile.SymbolType>
        {
            Tile.SymbolType.Circle,
            Tile.SymbolType.Circle,
            Tile.SymbolType.Square,
            Tile.SymbolType.Square
        };

        List<Tile.SymbolType> playerSymbols;
        List<Tile.SymbolType> targetSymbols;

        do
        {
            playerSymbols = new List<Tile.SymbolType>(symbols);
            targetSymbols = new List<Tile.SymbolType>(symbols);

            ShuffleList(playerSymbols);
            ShuffleList(targetSymbols);

        } while (AreLayoutsEqual(playerSymbols, targetSymbols));

        int index = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Tile.SymbolType type = playerSymbols[index];
                Sprite sprite = (type == Tile.SymbolType.Circle) ? circleSprite : squareSprite;
                tiles[x, y].SetSymbol(sprite, type);
                index++;
            }
        }

        index = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Tile.SymbolType type = targetSymbols[index];
                Sprite sprite = (type == Tile.SymbolType.Circle) ? circleSprite : squareSprite;
                targetTiles[x, y].SetSymbol(sprite, type);
                index++;
            }
        }
    }
    
    void ShuffleList(List<Tile.SymbolType> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }



    void HandleInput()
    {
        Vector2Int move = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) move = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) move = new Vector2Int(1, 0);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) move = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) move = new Vector2Int(0, 1);

        if (move != Vector2Int.zero)
        {
            Vector2Int newPos = selectedPos + move;

            if (IsValidPosition(newPos))
            {
                tiles[selectedPos.x, selectedPos.y].SetHighlight(false);
                selectedPos = newPos;
                HighlightSelected();
            }
        }
    }

    void HandleSwap()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (secondselectedPos == null)
            {
                secondselectedPos = selectedPos;
            }
            else
            {
                Vector2Int first = secondselectedPos.Value;
                Vector2Int second = selectedPos;

                if (AreAdjacent(first, second))
                {
                    SwapTiles(first, second);
                }

                secondselectedPos = null;
                HighlightSelected();
            }
        }
    }

    void SwapTiles(Vector2Int a, Vector2Int b){
        Tile tileA = tiles[a.x, a.y];
        Tile tileB = tiles[b.x, b.y];

        var tempType = tileA.currentSymbol;
        var tempSprite = tileA.symbolImage.sprite;

        tileA.SetSymbol(tileB.symbolImage.sprite, tileB.currentSymbol);
        tileB.SetSymbol(tempSprite, tempType);
        if (CheckIfSolved())
        {
            Debug.Log("Puzzle Solved!");
            StartCoroutine(RegeneratePuzzleAfterDelay(2f));
        }
    }

    void HighlightSelected(){
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++) 
            {
                tiles[x, y].SetHighlight((selectedPos == new Vector2Int(x,y)));
            }
        }
    }

    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 2 && pos.y >= 0 && pos.y < 2;
    }

    bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y)) == 1;
    }

    bool CheckIfSolved()
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (tiles[x, y].currentSymbol != targetTiles[x, y].currentSymbol)
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool AreLayoutsEqual(List<Tile.SymbolType> a, List<Tile.SymbolType> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }
}
