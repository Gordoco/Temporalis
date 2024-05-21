using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityUpdate : MonoBehaviour
{
    [SerializeField] private Image primary;
    [SerializeField] private Image secondary;
    [SerializeField] private Image ability1;
    [SerializeField] private Image ability2;
    [SerializeField] private Image ability3;
    [SerializeField] private Image ability4;

    [SerializeField] private GameObject player;
    private AttackManager manager;

    // Start is called before the first frame update
    void Start()
    {
        if (!player.GetComponent<AttackManager>()) Destroy(gameObject);
        manager = player.GetComponent<AttackManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager == null) return;
        primary.color = manager.GetAbilityReady(0) ? Color.white : Color.black;
        secondary.color = manager.GetAbilityReady(1) ? Color.white : Color.black;
        ability1.color = manager.GetAbilityReady(2) ? Color.white : Color.black;
        ability2.color = manager.GetAbilityReady(3) ? Color.white : Color.black;
        ability3.color = manager.GetAbilityReady(4) ? Color.white : Color.black;
        ability4.color = manager.GetAbilityReady(5) ? Color.white : Color.black;
    }
}
