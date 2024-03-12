using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public struct ItemUnique { public BaseItem item; public bool bUnique; public ItemUnique(BaseItem i, bool b) { item = i; bUnique = b; } }

public class PlayerStatManager : StatManager
{
    public event System.EventHandler<ItemUnique> OnItemAdded;

    protected override void OnDeath()
    {
        gameObject.transform.parent.GetComponent<PlayerObjectController>().Die();
        base.OnDeath();
    }

    [Server]
    protected override void AddItemChild(BaseItem item)
    {
        base.AddItemChild(item);
        bool unique = true;
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ItemName == item.ItemName) 
            {
                count++;
                if (count > 1)
                {
                    unique = false;
                    break;
                }
            }
        }
        if (OnItemAdded != null) OnItemAdded.Invoke(this, new ItemUnique(item, unique));
    }
}
