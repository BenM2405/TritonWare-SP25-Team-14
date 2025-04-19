using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;
    public Image symbolImage;
    public GameObject highlightObject;

    public enum SymbolType {Star, Moon, Comet, Saturn, BHole};
    public SymbolType currentSymbol;

    public void SetSymbol(Sprite sprite, SymbolType type)
    {
        symbolImage.sprite = sprite;
        currentSymbol = type;
    }

    public void SetHighlight(bool on)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(on);
        }
    }
}
