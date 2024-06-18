using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedItemList : MonoBehaviour
{
    [SerializeField] private GameObject[] ItemList;
    [SerializeField] private int[] Weights;

    public GameObject GetRandomItemPrefab()
    {
        if (ItemList == null || ItemList.Length == 0) return null;
        int sum = 0;
        foreach (int i in Weights) sum += i;
        int rand = Random.Range(0, sum);
        int index = -1;
        sum = 0;
        for (int i = 0; i < ItemList.Length; i++) { sum += Weights[i]; if (sum > rand) { index = i; break; } }
        return ItemList[index];
    }
}
