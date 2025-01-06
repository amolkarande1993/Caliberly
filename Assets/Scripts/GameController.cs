using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#region Data Models
[System.Serializable]
public class TileData
{
    public int Row;
    public int Column;
    public int ID;
}

[System.Serializable]
public class GameData
{
    public string PlayMode;
    public int PlayerScore;
    public List<TileData> Tiles;

    public GameData()
    {
        PlayerScore = 0;
        Tiles = new List<TileData>();
    }
}
#endregion

public class GameController : MonoBehaviour
{
    #region Constants
    private const int EasyModeWidth = 2;
    private const int EasyModeHeight = 2;
    private const int MediumModeWidth = 2;
    private const int MediumModeHeight = 3;
    private const int HardModeWidth = 5;
    private const int HardModeHeight = 6;
    private const float TimerDuration = 120f;  // 2min 
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameplayScreen;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject loadSavedGameButton;

    [Header("Game Components")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform tileParent;
    [SerializeField] private Tile blankTile;
    [SerializeField] private Data scriptableData;
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("Audio")]
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip flipSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Save/Load")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    [Header("Grid Settings")]
    [SerializeField] private ContentSizeFitter sizeFitter;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private EventSystem eventSystem;
    #endregion

    #region Private Fields
    private Tile firstSelectedTile;
    private Tile secondSelectedTile;
    private List<Tile> tiles = new List<Tile>();
    private List<TileData> tileDataList = new List<TileData>();
    private GameData currentGameData;
    private string selectedGameMode;
    private int gridWidth;
    private int gridHeight;
    private float remainingTime;
    private bool isGameOver;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Tile.OnTileClicked += HandleTileClicked;
        mainMenu.SetActive(true);
        gameplayScreen.SetActive(false);
    }

    private void Start()
    {
        var savedGameData = saveLoadManager.LoadGame();
        loadSavedGameButton.SetActive(savedGameData != null && savedGameData.Tiles.Count > 0);
    }

    private void Update()
    {
        if (isGameOver || mainMenu.activeSelf) return;

        UpdateTimer();
        CheckForGameOver();
    }
    #endregion

    #region Game Management
    public void StartNewGame()
    {
        selectedGameMode = GetSelectedToggleName();
        SetBoardSize(selectedGameMode);

        CreateGameBoard();
        mainMenu.SetActive(false);
        gameplayScreen.SetActive(true);
    }

    public void LoadSavedGame()
    {
        var savedData = saveLoadManager.LoadGame(); 
        SetBoardSize(savedData.PlayMode);
        LoadGameBoard(savedData);

        mainMenu.SetActive(false);
        gameplayScreen.SetActive(true);
    }

    public void CloseGame()
    {
        ClearTiles();
        ResetGame();
    }

    private void ResetGame()
    {
        sizeFitter.enabled = true;
        layoutGroup.enabled = true;

        mainMenu.SetActive(true);
        gameplayScreen.SetActive(false);

        tiles.Clear();
        isGameOver = false;

        saveLoadManager.SaveGame(currentGameData);
        loadSavedGameButton.SetActive(currentGameData != null && currentGameData.Tiles.Count > 0);

    }
    #endregion

    #region Tile Management
    private void HandleTileClicked(Tile clickedTile)
    {
        sizeFitter.enabled = false;
        layoutGroup.enabled = false;

        PlaySound(flipSound);

        if (firstSelectedTile == null)
        {
            firstSelectedTile = clickedTile;
        }
        else
        {
            secondSelectedTile = clickedTile;
            eventSystem.enabled = false;

            if (firstSelectedTile.ID != secondSelectedTile.ID)
            {
                StartCoroutine(HandleMismatch());
            }
            else
            {
                StartCoroutine(HandleMatch());
            }
        }
    }

    private IEnumerator HandleMismatch()
    {
        yield return new WaitForSeconds(0.5f);
        firstSelectedTile.Hide();
        secondSelectedTile.Hide();
        ResetTileSelection();
        PlaySound(mismatchSound);
    }

    private IEnumerator HandleMatch()
    {
        yield return new WaitForSeconds(0.5f);

        RemoveTileData(firstSelectedTile);
        RemoveTileData(secondSelectedTile);

        tiles.Remove(firstSelectedTile);
        tiles.Remove(secondSelectedTile);

        Destroy(firstSelectedTile.gameObject);
        Destroy(secondSelectedTile.gameObject);

        currentGameData.Tiles = tileDataList;

        ResetTileSelection();
        PlaySound(matchSound);
    }

    private void ResetTileSelection()
    {
        firstSelectedTile = null;
        secondSelectedTile = null;
        eventSystem.enabled = true;
    }

    private void RemoveTileData(Tile tile)
    {
        var data = tileDataList.Find(t => t.Row == tile.Row && t.Column == tile.Column);
        if (data != null) tileDataList.Remove(data);
    }
    #endregion

    #region Game Board Management
    private void CreateGameBoard()
    {
        int[] randomNumbers = GenerateRandomArray(gridWidth, gridHeight);
        int counter = 0;

        currentGameData = new GameData { PlayMode = selectedGameMode };
        tileDataList.Clear();

        for (int row = 0; row < gridWidth; row++)
        {
            for (int col = 0; col < gridHeight; col++)
            {
                var tile = Instantiate(tilePrefab, tileParent);
                tile.ID = randomNumbers[counter];
                tile.Row = row;
                tile.Column = col;
                tile.SetData(scriptableData.GetAsset(tile.ID));

                tileDataList.Add(new TileData { Row = row, Column = col, ID = tile.ID });
                tiles.Add(tile);
                counter++;
            }
        }

        remainingTime = TimerDuration;
        isGameOver = false;
        currentGameData.Tiles = tileDataList;
        saveLoadManager.SaveGame(currentGameData);
    }

    private void LoadGameBoard(GameData savedData)
    {
        ClearTiles();


        currentGameData = new GameData { PlayMode = savedData.PlayMode };
        SetBoardSize(currentGameData.PlayMode);
        tileDataList = new List<TileData>(savedData.Tiles);

        for (int row = 0; row < gridWidth; row++)
        {
            for (int col = 0; col < gridHeight; col++)
            {
                var tileSavedData = savedData.Tiles.Find(t => t.Row == row && t.Column == col);
                if (tileSavedData != null)
                {
                    var tile = Instantiate(tilePrefab, tileParent);
                    tile.Row = tileSavedData.Row;
                    tile.Column = tileSavedData.Column;
                    tile.ID = tileSavedData.ID;
                    tile.SetData(scriptableData.GetAsset(tile.ID));
                    tiles.Add(tile);
                }
                else
                    Instantiate(blankTile, tileParent);
            }
        }

        remainingTime = TimerDuration;
        isGameOver = false;
        currentGameData.Tiles = tileDataList;
        saveLoadManager.SaveGame(currentGameData);
    }

    private int[] GenerateRandomArray(int width, int height)
    {
        int[] numbers = new int[width * height];
        int counter = 1;

        for (int i = 0; i < numbers.Length; i += 2)
        {
            numbers[i] = numbers[i + 1] = counter++;
            if (counter == 9) counter = 1;
        }

        System.Random rng = new System.Random();
        numbers = numbers.OrderBy(_ => rng.Next()).ToArray();

        return numbers;
    }

    private void ClearTiles()
    {
        foreach (Transform child in tileParent)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Helpers

    private void SetBoardSize(string mode)
    {
        switch (mode)
        {
            case "Easy":
                gridWidth = EasyModeWidth;
                gridHeight = EasyModeHeight;
                break;
            case "Medium":
                gridWidth = MediumModeWidth;
                gridHeight = MediumModeHeight;
                break;
            case "Hard":
                gridWidth = HardModeWidth;
                gridHeight = HardModeHeight;
                break;
        }
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        timerText.text = FormatTime(remainingTime);


    }



    private string GetSelectedToggleName()
    {
        return toggleGroup.GetComponentsInChildren<Toggle>().FirstOrDefault(t => t.isOn)?.name;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void CheckForGameOver()
    {
        if (tiles.Count == 0)
        {
            PlaySound(gameOverSound);
            ClearTiles();
            ResetGame();
            isGameOver = true;
            return;
        }

        if (remainingTime <= 0)
        {
            PlaySound(gameOverSound);
            ClearTiles();
            ResetGame();
            isGameOver = true;
        }
    }

    #endregion
}
