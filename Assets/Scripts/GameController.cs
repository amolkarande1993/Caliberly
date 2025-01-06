using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class GameController : MonoBehaviour
{
    const int EasyModeGridSizeWidth = 2;
    const int EasyModeGridSizeHeight = 2;
    const int MediumModeGridSizeWidth = 2; 
    const int MediumModeGridSizeHeight = 3;
    const int HardModeGridSizeWidth = 5; 
    const int HardModeGridSizeHeight = 6;

    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gamePlayScreen;

    [SerializeField] Tile tilePrefab;
    [SerializeField] Transform tiletransform;
    [SerializeField] Data data;
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] ContentSizeFitter sizeFitter;
    [SerializeField] GridLayoutGroup layoutGroup;
    [SerializeField] EventSystem eventSystem;

    Tile tileA;
    Tile tileB;

    List<Tile> tiles = new List<Tile>();

    void Awake()
    {
        Tile.OnSimpleEvent += onTileClicked;
        mainMenu.SetActive(true);
        gamePlayScreen.SetActive(false);
    }

    public void OnPlayButtonClicked() 
    {
        int boardSizeWidth  = 0;
        int boardSizeHeight = 0;
        switch (GetSelectedToggleName())
        {
            case "Easy":
                boardSizeWidth = EasyModeGridSizeWidth;
                boardSizeHeight = EasyModeGridSizeWidth;
                break;
            case "Medium":
                boardSizeWidth = MediumModeGridSizeWidth;
                boardSizeHeight = MediumModeGridSizeHeight;
                break;
            case "Hard":
                boardSizeWidth = HardModeGridSizeWidth;
                boardSizeHeight = HardModeGridSizeHeight;
                break;
            default:
                break;
        }

        CreateLevel(boardSizeWidth, boardSizeHeight);
        mainMenu.SetActive(false);
        gamePlayScreen.SetActive(true);
    }

    public void onTileClicked(Tile tile) {

        sizeFitter.enabled = false;
        layoutGroup.enabled = false;

        if (tileA == null)
            tileA = tile;
        else
        {
            tileB = tile;
            eventSystem.enabled = false;
            if (tileA.id != tileB.id)
            {
                StartCoroutine(DelayHide());         
            }
            else {
                StartCoroutine(DelayDestroy());
                
            }
        }
    }

    public void CreateLevel(int width, int height)
    {
        var randomNumber = GenerateRandomArray(width, height);
        var counter = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var tile = Instantiate(tilePrefab, tiletransform);
                tile.id = randomNumber[counter];
                tile.SetData(data.GetAsset(tile.id));
                counter++;
                tiles.Add(tile);
            }
        }
    }

    public void OnCloseClicked()
    {
        DeleteTiles();
        Restart();
    }

    void Restart()
    {
        sizeFitter.enabled = true;
        layoutGroup.enabled = true;
        mainMenu.SetActive(true);
        gamePlayScreen.SetActive(false);
        tiles.Clear();
    }

    void DeleteTiles()
    {
        for (int i = 0; i < tiletransform.childCount; i++)
        {
            Destroy(tiletransform.GetChild(i).gameObject);
        }
    }
    IEnumerator DelayHide()
    {
        yield return new WaitForSeconds(0.5f);
        tileA.Hide();
        tileB.Hide();
        tileA = null;
        tileB = null;
        eventSystem.enabled = true;
    }

    IEnumerator DelayDestroy()
    {

        yield return new WaitForSeconds(0.5f);
        Destroy(tileA.gameObject);
        Destroy(tileB.gameObject);
        eventSystem.enabled = true;
    }

    string GetSelectedToggleName()
    {
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>())
            if (toggle.isOn)
                return toggle.name;

        return null;
    }

    int[] GenerateRandomArray(int width, int height)
    {
        int[] numbers = new int[width * height];
        int count = 1;
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = count++;
            if (count == 3)
                count = 1;
        }

        System.Random random = new System.Random();
        for (int i = numbers.Length - 1; i > 0; i--)
        {
            int randomIndex = random.Next(0, i + 1);

            int temp = numbers[i];
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }

        return numbers;
    }

    void Update()
    {

        if (tiletransform.childCount == 0)
            Restart();
    }

}
