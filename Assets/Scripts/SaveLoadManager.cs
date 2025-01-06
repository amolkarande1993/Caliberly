using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/gameData.json"; 
    }

    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data); 
        File.WriteAllText(filePath, json);
        Debug.Log(filePath);
        Debug.Log("Game Saved");
    }

    public GameData LoadGame()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game Loaded");
            return data;
        }
        else
        {
            Debug.LogWarning("No saved data found");
            return null;
        }
    }
}
