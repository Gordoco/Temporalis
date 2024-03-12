using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ItemDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject ItemListPrefab;
    [SerializeField] private GameObject ItemPopupPrefab;
    [SerializeField] private PlayerStatManager manager;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject canvas;

    private void Start()
    {
        if (isServer) manager.OnItemAdded += OnItemAdded;
    }

    [Server]
    private void OnItemAdded(object sender, ItemUnique IU)
    {
        OnItemAddedClient(IU);
    }

    [ClientRpc]
    void OnItemAddedClient(ItemUnique IU)
    {
        GameObject itemListing = Instantiate(ItemListPrefab);
        itemListing.GetComponent<ItemListItem>().Initialize(IU.item.ItemName, IU.item.stats, IU.item.values, IU.item.percent);
        itemListing.transform.SetParent(panel.transform);
        itemListing.transform.localScale = Vector3.one;

        if (IU.bUnique)
        {
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
    }

    private IEnumerator DisablePopup(GameObject obj)
    {
        yield return new WaitForSeconds(2);
        Destroy(obj);
    }
}
