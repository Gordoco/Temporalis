using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component holding data associated with the display of an item object on the UI
/// </summary>
public class ItemListItem : MonoBehaviour
{
    [SerializeField] private Image image;

    private GameObject ItemImageList;

    private string ItemName;
    private NumericalStats[] statsAffected;
    private double[] statsChanged;
    private bool percent;

    public string GetItemName() { return ItemName; }

    public void Initialize(string name, Texture2D texture, NumericalStats[] inStats, double[] inVals, bool bPercent)
    {
        //ItemImageList = GameObject.FindGameObjectWithTag("ItemImageList");
        ItemName = name;
        statsAffected = inStats;
        statsChanged = inVals;
        percent = bPercent;
        Texture2D tex = texture;
        image.sprite = Sprite.Create(tex, new Rect(Vector2.zero, new Vector2(tex.width, tex.height)), Vector2.zero);
    }
}
