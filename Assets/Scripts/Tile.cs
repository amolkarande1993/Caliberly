using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Image backImage;
    [SerializeField] Image frontImage;

    public int ID;
    public int Row;
    public int Column;

    public delegate void TileClicked(Tile A);

    public static event TileClicked OnTileClicked;

    private void Awake()
    {

    }

    void Show()
    {
        frontImage.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        Show();
        OnTileClicked?.Invoke(this);
    }

    public void SetData(Sprite image) {
        backImage.sprite = image;
    }

    public void Hide()
    {
        frontImage.gameObject.SetActive(true);
    }
}
