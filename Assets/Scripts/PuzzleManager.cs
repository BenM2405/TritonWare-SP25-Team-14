using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public int gridWidth = 3;
    public int gridHeight = 3;

    public Tile[,] tiles;
    public Tile[,] targetTiles;
    public Vector2Int selectedPos = new Vector2Int(0,0);
    private Vector2Int? secondselectedPos = null;

    public Sprite circleSprite;
    public Sprite squareSprite;

    void Start()
    {
        ConfigureGrid(gridWidth, gridHeight);
    }

    public void ConfigureGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;

        tiles = new Tile[gridWidth, gridHeight];
        targetTiles = new Tile[gridWidth, gridHeight];

        EnableRelevantTiles();
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

    void EnableRelevantTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                string playerName = $"Tile_{x}_{y}";
                string targetName = $"TargetTile_{x}_{y}";

                GameObject playerTile = GameObject.Find(playerName);
                GameObject targetTile = GameObject.Find(targetName);

                bool shouldBeActive = (x < gridWidth && y < gridHeight);

                if (playerTile != null) playerTile.SetActive(shouldBeActive);
                if (targetTile != null) targetTile.SetActive(shouldBeActive);
            }
        }
    }


    void AssignPlayerTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                string objName = $"Tile_{x}_{y}";
                GameObject tileObj = GameObject.Find(objName);

                if (tileObj == null)
                {
                    Debug.LogError($"Could not find GameObject: {objName}");
                    continue;
                }

                tileObj.SetActive(true);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);
                tiles[x, y] = tile;
            }
        }
    }


    void AssignTargetTiles()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
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
        int total = gridWidth * gridHeight;
        List<Tile.SymbolType> symbols = new List<Tile.SymbolType>();

        for (int i = 0; i < total; i++)
        {
            symbols.Add(i % 2 == 0 ? Tile.SymbolType.Circle : Tile.SymbolType.Square);
        }

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
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var type = playerSymbols[index];
                Sprite sprite = (type == Tile.SymbolType.Circle) ? circleSprite : squareSprite;
                tiles[x, y].SetSymbol(sprite, type);
                index++;
            }
        }

        index = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var type = targetSymbols[index];
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
            StartCoroutine(RegeneratePuzzleAfterDelay(1f));
        }
    }

    void HighlightSelected(){
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++) 
            {
                tiles[x, y].SetHighlight((selectedPos == new Vector2Int(x,y)));
            }
        }
    }

    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y)) == 1;
    }

    bool CheckIfSolved()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
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
