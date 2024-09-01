using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Depricated.
/// </summary>
public class ItemImageList : MonoBehaviour
{
    [System.Serializable]
    private class ImageAndName
    {
        public Texture2D itemImage;
        public string itemName;
    }

    [SerializeField] private ImageAndName[] ItemArray;

    public Texture2D GetItemImage(string ItemName)
    {
        for (int i = 0; i < ItemArray.Length; i++)
        {
            if (ItemArray[i].itemName == ItemName) return ItemArray[i].itemImage;
        }
        return ItemArray[0].itemImage; //Default
    }
}
