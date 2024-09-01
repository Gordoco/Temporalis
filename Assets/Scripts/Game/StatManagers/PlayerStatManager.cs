using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Serializable struct to keep track of whether an item being added is unique or not, used to spawn new item display images vs simply updating an existing one
/// </summary>
[System.Serializable]
public struct ItemUnique { public BaseItem item; public bool bUnique; public ItemUnique(BaseItem i, bool b) { item = i; bUnique = b; } }

public class PlayerStatManager : StatManager
{
    [SerializeField] private ItemDisplay display;
    /// <summary>
    /// Handle Player death more elegantly through spawning of spectator objects, etc.
    /// </summary>
    protected override void OnDeath()
    {
        gameObject.transform.parent.GetComponent<PlayerObjectController>().Die();
        base.OnDeath();
    }

    /// <summary>
    /// Item addition interface for players
    /// </summary>
    /// <param name="item"></param>
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
        display.OnItemAdded(new ItemUnique(item, unique));
    }
}
