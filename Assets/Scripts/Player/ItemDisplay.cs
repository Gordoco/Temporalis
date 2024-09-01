using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ItemDisplay : NetworkBehaviour
{
    // Editor values
    [SerializeField] private GameObject ItemListPrefab;
    [SerializeField] private GameObject ItemPopupPrefab;
    [SerializeField] private PlayerStatManager manager;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject canvas;

    [Server]
    public void OnItemAdded(ItemUnique IU)
    {
        OnItemAddedClient(IU);
    }

    private static string ITEM_PATH = "ConcreteItems";
    [ClientRpc]
    void OnItemAddedClient(ItemUnique IU)
    {
        if (!isOwned) return;

        //Add a popup and display image
        if (IU.bUnique)
        {
            UniqueItemAddition(IU);
        }
        //Update the display image count
        else
        {
            DuplicateItemAddition(IU);
        }
    }

    // TODO: Queue the item popups so they cannot overlap one-another
    /// <summary>
    /// Creates and adds a new UI element corresponding to the added item to the Player's display. Additionally adds a temporary popup to display the item information
    /// </summary>
    /// <param name="IU"></param>
    private void UniqueItemAddition(ItemUnique IU)
    {
        GameObject itemListing = Instantiate(ItemListPrefab);
        GameObject[] itemPrefabs = Resources.LoadAll<GameObject>(ITEM_PATH);
        Texture2D image = null;
        foreach (GameObject item in itemPrefabs)
        {
            if (item.GetComponent<BaseItemComponent>().ItemName == IU.item.ItemName)
            {
                image = item.GetComponent<BaseItemComponent>().GetItemImage();
                break;
            }
        }
        itemListing.GetComponent<ItemListItem>().Initialize(IU.item.ItemName, image, IU.item.stats, IU.item.values, IU.item.percent);
        itemListing.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        itemListing.transform.SetParent(panel.transform);
        itemListing.transform.localScale = Vector3.one;
        GameObject itemPopup = Instantiate(ItemPopupPrefab, canvas.transform);
        string desc = "";
        for (int i = 0; i < IU.item.stats.Length; i++)
        {
            if (IU.item.percent)
            {
                string plusminus = IU.item.values[i] > 1 ? "+" : "-";
                float val = IU.item.values[i] > 1 ? (float)IU.item.values[i] - 1 : Mathf.Abs((float)IU.item.values[i] - 1);
                desc += plusminus + val * 100 + "% " + IU.item.stats[i].ToString() + "\n";
            }
            else
            {
                string plusminus = IU.item.values[i] > 0 ? "+" : "-";
                desc += plusminus + Mathf.Abs((float)IU.item.values[i]) + " " + IU.item.stats[i].ToString() + "\n";
            }
        }
        itemPopup.GetComponent<ItemPopup>().Init(IU.item.ItemName, desc);
        StartCoroutine(DisablePopup(itemPopup));
    }

    /// <summary>
    /// Updates the counter for the corresponding display element
    /// </summary>
    /// <param name="IU"></param>
    private void DuplicateItemAddition(ItemUnique IU)
    {
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            GameObject child = panel.transform.GetChild(i).gameObject;
            ItemListItem LI = child.GetComponent<ItemListItem>();
            if (LI == null) continue;
            if (LI.GetItemName() == IU.item.ItemName && child.transform.childCount > 0)
            {
                string text = child.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
                child.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "" + (int.Parse(text) + 1);
            }
        }
    }

    /// <summary>
    /// Turns off the new item popup
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private IEnumerator DisablePopup(GameObject obj)
    {
        yield return new WaitForSeconds(2);
        Destroy(obj);
    }
}
