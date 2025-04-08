using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;
    public Image symbolImage;
    public GameObject highlightBorder;

    public enum SymbolType {Circle, Square};
    public SymbolType currentSymbol;

    public void SetSymbol(Sprite sprite, SymbolType type)
    {
        symbolImage.sprite = sprite;
        currentSymbol = type;
    }

    public void SetHighlight(bool on)
    {
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(on);
        }
    }
}
