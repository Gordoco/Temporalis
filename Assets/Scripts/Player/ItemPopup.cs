using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Component holding the data required for the display of an informational popup when aquiring a new item object
/// </summary>
public class ItemPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text ItemNameDisplay;
    [SerializeField] private TMP_Text ItemStatsDisplay;

    public void Init(string ItemName, string ItemDescription)
    {
        ItemNameDisplay.text = ItemName;
        ItemStatsDisplay.text = ItemDescription;
    }
}
