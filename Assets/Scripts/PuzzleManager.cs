using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public int gridWidth;
    public int gridHeight;

    public Tile[,] tiles;
    public Tile[,] targetTiles;
    public Vector2Int selectedPos = new Vector2Int(0, 0);
    private Vector2Int? secondselectedPos = null;

    public Sprite circleSprite;
    public Sprite squareSprite;
    [System.Serializable]
    public class SymbolSpritePair
    {
        public Tile.SymbolType symbol;
        public Sprite sprite;
    }
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float tutorialDisplayTime = 10f;


    public List<SymbolSpritePair> symbolSpritePairs;
    private Dictionary<Tile.SymbolType, Sprite> symbolToSprite;

    public GameObject tilePrefab;
    public Transform puzzleGridTransform;
    public GameObject targetTilePrefab;
    public Transform targetGridTransform;


    public TMPro.TextMeshProUGUI parText;
    private int levelPar = 0;
    public TMPro.TextMeshProUGUI moveText;
    public int playerMoves = 0;
    private EventInstance endlessPuzzleMusicEventInstance;
    private string endlessPuzzleMusicPath = "event:/music/endless_music";
    private string levelCompleteSFXPath = "event:/sfx/puzzle/level_complete";
    private string noteSwapSFXPath = "event:/sfx/puzzle/note_swap";
    private string noteSelectSFXPath = "event:/sfx/puzzle/note_select";

    void Start()
    {
        InitializeSymbolToSpriteMap();
        if (GameConfig.isStoryMode)
        {
            ResetAllTilePositions();
            LoadLevelFromFile(LevelLoader.Instance.levelToLoad);

            //Tutorial logic
            if (LevelLoader.Instance.levelToLoad == "0")
            {
                ShowTutorialMessage("Use the arrow keys to move.\nPress SPACE once to select the first tile,\nand again to select the second.\nTiles will swap if they are adjacent!");
            }
        }
        else
        {
            ResetAllTilePositions();
            ConfigureGrid(GameConfig.GridWidth, GameConfig.GridHeight);
            startEndlessPuzzleMusic();
        }
    }


    public void ConfigureGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;

        tiles = new Tile[gridWidth, gridHeight];
        targetTiles = new Tile[gridWidth, gridHeight];

        CreateTiles();
        CreateTargetTiles();
        DisableUnusedTileGraphics();
        InitializeBalancedGrids();
        HighlightSelected();
    }

    void Update()
    {
        HandleInput();
        HandleSwap();
    }

    void CreateTiles()
    {
        foreach (Transform child in puzzleGridTransform)
        {
            Destroy(child.gameObject);
        }

        tiles = new Tile[gridWidth, gridHeight];

        GridLayoutGroup layout = puzzleGridTransform.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridWidth;

        RectTransform gridRect = puzzleGridTransform.GetComponent<RectTransform>();
        float width = gridRect.rect.width;
        float height = gridRect.rect.height;
        float cellSize = Mathf.Min(width / gridWidth, height / gridHeight);
        layout.cellSize = new Vector2(cellSize, cellSize);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tileObj = Instantiate(tilePrefab, puzzleGridTransform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);
                tiles[x, y] = tile;
            }
        }
    }
    void CreateTargetTiles()
    {
        foreach (Transform child in targetGridTransform)
        {
            Destroy(child.gameObject);
        }

        targetTiles = new Tile[gridWidth, gridHeight];

        GridLayoutGroup layout = targetGridTransform.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridWidth;

        RectTransform gridRect = targetGridTransform.GetComponent<RectTransform>();
        float width = gridRect.rect.width;
        float height = gridRect.rect.height;
        float cellSize = Mathf.Min(width / gridWidth, height / gridHeight);
        layout.cellSize = new Vector2(cellSize, cellSize);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tileObj = Instantiate(targetTilePrefab, targetGridTransform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.gridPos = new Vector2Int(x, y);
                targetTiles[x, y] = tile;
            }
        }
    }



    private void startEndlessPuzzleMusic()
    {
        endlessPuzzleMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        endlessPuzzleMusicEventInstance = FMODUnity.RuntimeManager.CreateInstance(endlessPuzzleMusicPath);
        endlessPuzzleMusicEventInstance.start();
    }

    public void stopEndlessPuzzleMusic()
    {
        endlessPuzzleMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void playPuzzleCompleteSFX()
    {
        RuntimeManager.PlayOneShot(levelCompleteSFXPath);
    }

    private void playNoteSwapSFX()
    {
        RuntimeManager.PlayOneShot(noteSwapSFXPath);
    }

    private void playNoteSelectSFX()
    {
        RuntimeManager.PlayOneShot(noteSelectSFXPath);
    }

    System.Collections.IEnumerator RegeneratePuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        selectedPos = new Vector2Int(0, 0);
        secondselectedPos = null;
        playerMoves = 0;
        moveText.text = $"Moves: {0}";

        InitializeBalancedGrids();
        HighlightSelected();
        startEndlessPuzzleMusic();
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
                tile.gridPos = new Vector2Int(x, y);
                targetTiles[x, y] = tile;
            }
        }
    }

    void InitializeBalancedGrids()
    {
        int total = gridWidth * gridHeight;
        List<Tile.SymbolType> availableSymbols = new List<Tile.SymbolType>(symbolToSprite.Keys);
        if (availableSymbols.Count == 0)
        {
            Debug.LogError("No available symbols with mapped sprites.");
            return;
        }

        List<Tile.SymbolType> symbols = new List<Tile.SymbolType>();
        for (int i = 0; i < total; i++)
        {
            symbols.Add(availableSymbols[i % availableSymbols.Count]);
        }

        ShuffleList(symbols);
        List<Tile.SymbolType> playerSymbols = new List<Tile.SymbolType>(symbols);
        List<Tile.SymbolType> targetSymbols = new List<Tile.SymbolType>(symbols);

        if (total <= 9)
        {
            int maxAllowedOverlap = total / 4;
            int overlapCount = total;
            int tries = 0;
            int maxTries = 1000;

            while (overlapCount > maxAllowedOverlap && tries < maxTries)
            {
                int a = UnityEngine.Random.Range(0, total);
                int b = UnityEngine.Random.Range(0, total);
                (targetSymbols[a], targetSymbols[b]) = (targetSymbols[b], targetSymbols[a]);

                overlapCount = 0;
                for (int i = 0; i < total; i++)
                {
                    if (playerSymbols[i] == targetSymbols[i])
                        overlapCount++;
                }

                tries++;
            }

            if (tries >= maxTries)
            {
                Debug.LogWarning("Max retries hit while trying to reduce overlaps. Proceeding anyway.");
            }
        }
        else
        {
            ShuffleList(targetSymbols);
        }

        int index = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Tile.SymbolType pType = playerSymbols[index];
                if (!symbolToSprite.TryGetValue(pType, out Sprite pSprite))
                {
                    Debug.LogError($"No sprite mapped for symbol: {pType}");
                }
                tiles[x, y].SetSymbol(pSprite, pType);
                index++;
            }
        }

        index = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Tile.SymbolType tType = targetSymbols[index];
                if (!symbolToSprite.TryGetValue(tType, out Sprite tSprite))
                {
                    Debug.LogError($"No sprite mapped for symbol: {tType}");
                }
                targetTiles[x, y].SetSymbol(tSprite, tType);
                index++;
            }
        }

        int totals = gridWidth * gridHeight;
        int par = 0;

        if (GameConfig.isStoryMode)
        {
            par = levelPar;
            Debug.Log($"Story mode level par loaded from JSON: {par}");
        }
        else if (totals <= 9)
        {
            ParCalculator solver = FindObjectOfType<ParCalculator>();
            if (solver == null)
            {
                Debug.LogError("ParCalculator not found in scene.");
            }
            else
            {
                par = solver.CalculatePar(tiles, targetTiles) + 1;
            }
        }
        else
        {
            Debug.Log("Skipping par calculation for large grid in endless mode.");
        }

        if (parText != null)
        {
            parText.text = $"Par: {par}";
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

        if (Input.GetKeyDown(KeyCode.UpArrow))    move = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.DownArrow))  move = new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.LeftArrow))  move = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.RightArrow)) move = new Vector2Int(1, 0);

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
            if (secondselectedPos == null || selectedPos == secondselectedPos.Value)
            {
                playNoteSelectSFX();
                secondselectedPos = selectedPos;
            }
            else
            {
                Vector2Int first = secondselectedPos.Value;
                Vector2Int second = selectedPos;

                if (AreAdjacent(first, second))
                {
                    playNoteSwapSFX();
                    SwapTiles(first, second);
                }
                else
                {
                    playNoteSelectSFX();
                }
                secondselectedPos = null;
                HighlightSelected();
            }
        }
    }

    void SwapTiles(Vector2Int a, Vector2Int b)
    {
        Tile tileA = tiles[a.x, a.y];
        Tile tileB = tiles[b.x, b.y];

        var tempType = tileA.currentSymbol;
        var tempSprite = tileA.symbolImage.sprite;

        tileA.SetSymbol(tileB.symbolImage.sprite, tileB.currentSymbol);
        tileB.SetSymbol(tempSprite, tempType);

        playerMoves++;

        moveText.text = $"Moves: {playerMoves}";

        if (CheckIfSolved())
        {
            Debug.Log("Puzzle Solved!");

            if (!GameConfig.isStoryMode)
            {
                stopEndlessPuzzleMusic();
                StartCoroutine(RegeneratePuzzleAfterDelay(0.2f));
            }
            else
            {
                Debug.Log("You beat a story level!");
                GameConfig.resumePostPuzzle = true;
                playPuzzleCompleteSFX();
                StartCoroutine(WaitBeforeTransition(3f));
            }
        }

    }

    IEnumerator WaitBeforeTransition(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("DialogueScene");
    }

    void HighlightSelected()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tiles[x, y].SetHighlight((selectedPos == new Vector2Int(x, y)));
            }
        }
    }

    void LoadLevelFromFile(string levelName)
    {
        TextAsset json = Resources.Load<TextAsset>($"Levels/{levelName}");
        LevelData data = JsonUtility.FromJson<LevelData>(json.text);
        levelPar = data.par;

        ConfigureGrid(data.width, data.height);

        // parse player + target symbols
        int index = 0;
        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                Tile.SymbolType pType = (Tile.SymbolType)System.Enum.Parse(typeof(Tile.SymbolType), data.playerSymbols[index]);
                Tile.SymbolType tType = (Tile.SymbolType)System.Enum.Parse(typeof(Tile.SymbolType), data.targetSymbols[index]);

                if (!symbolToSprite.TryGetValue(pType, out Sprite pSprite))
                {
                    Debug.LogError($"Missing sprite for symbol: {pType}");
                }
                tiles[x, y].SetSymbol(pSprite, pType);

                if (!symbolToSprite.TryGetValue(tType, out Sprite tSprite))
                {
                    Debug.LogError($"Missing sprite for symbol: {tType}");
                }
                targetTiles[x, y].SetSymbol(tSprite, tType);

                index++;
            }
        }
    }


    void DisableUnusedTileGraphics()
    {
        Transform puzzleGrid = GameObject.Find("PuzzleGrid").transform;
        Transform targetGrid = GameObject.Find("TargetGrid").transform;

        foreach (Transform tile in puzzleGrid)
        {
            Tile tileScript = tile.GetComponent<Tile>();
            bool isUsed = tileScript != null && IsValidPosition(tileScript.gridPos);

            Image img = tile.GetComponentInChildren<Image>(true);
            if (img != null)
                img.enabled = isUsed;
        }

        foreach (Transform tile in targetGrid)
        {
            Tile tileScript = tile.GetComponent<Tile>();
            bool isUsed = tileScript != null && IsValidPosition(tileScript.gridPos);

            Image img = tile.GetComponentInChildren<Image>(true);
            if (img != null)
                img.enabled = isUsed;
        }
    }

    void ResetAllTilePositions()
    {
        foreach (Transform tile in GameObject.Find("PuzzleGrid").transform)
        {
            Tile script = tile.GetComponent<Tile>();
            if (script != null) script.gridPos = new Vector2Int(-1, -1);
        }

        foreach (Transform tile in GameObject.Find("TargetGrid").transform)
        {
            Tile script = tile.GetComponent<Tile>();
            if (script != null) script.gridPos = new Vector2Int(-1, -1);
        }
    }



    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
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
        LevelLoader.Instance.StopMusic();
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
    void InitializeSymbolToSpriteMap()
    {
        symbolToSprite = new Dictionary<Tile.SymbolType, Sprite>();
        foreach (var pair in symbolSpritePairs)
        {
            symbolToSprite[pair.symbol] = pair.sprite;
        }
    }


    void OnDisable()
    {
        stopEndlessPuzzleMusic();
    }

    private void ShowTutorialMessage(string message)
    {
        if (tutorialPopup != null && tutorialText != null)
        {
            tutorialText.text = message;
            tutorialPopup.SetActive(true);
        }
    }


    [System.Serializable]
    public class LevelData
    {
        public int width;
        public int height;
        public List<string> playerSymbols;
        public List<string> targetSymbols;
        public int par;
    }

}