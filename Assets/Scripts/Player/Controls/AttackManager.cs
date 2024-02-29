using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("PrimaryAttack"))
        {
            Debug.Log("Shot Gat");
        }

        if (Input.GetButtonDown("SecondaryAttack"))
        {
            Debug.Log("Shot Gat Extra Good");
        }
    }
}
