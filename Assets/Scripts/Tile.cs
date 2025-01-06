using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Image backImage;
    [SerializeField] Image frontImage;

    public int id;

    public delegate void SimpleEvent(Tile A);

    public static event SimpleEvent OnSimpleEvent;

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
        OnSimpleEvent?.Invoke(this);
    }

    public void SetData(Sprite image) {
        backImage.sprite = image;
    }

    public void Hide()
    {
        frontImage.gameObject.SetActive(true);
    }
}
