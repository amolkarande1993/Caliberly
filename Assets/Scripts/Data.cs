using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "Custom/Image Collection", order = 1)]
public class Data : ScriptableObject
{
    [System.Serializable]
    public class ImageEntry
    {
        public int id;       
        public Sprite image;
    }

    public ImageEntry[] imageEntries; 
    public Sprite GetAsset(int id) {
        for (int i = 0; i < imageEntries.Length; i++)
        {
            if (imageEntries[i].id == id)
                return imageEntries[i].image;
        }

        return null;
    }
}