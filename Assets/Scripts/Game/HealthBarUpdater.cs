using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUpdater : MonoBehaviour
{
    // Editor exposed values
    [SerializeField] private Slider slider;
    [SerializeField] private StatManager manager;
    [SerializeField] private TMP_Text text;
    [SerializeField] private bool bEnemy = false;
    [SerializeField] private Image fill;

    [SerializeField] private Color hpColor;
    [SerializeField] private Color shieldColor;

    // Update is called once per frame
    void Update()
    {
        if (manager.GetShield() > 0) fill.color = shieldColor;
        else fill.color = hpColor;

        if (!manager.CheckReady()) return;

        SetSlider();
        SetDisplayText();

        if (bEnemy && manager.CheckIfShouldShowHP())
        {
            ToggleVisibility(true);
            RotateToFaceLocalPlayer();
        }
        else if (bEnemy)
        {
            ToggleVisibility(false);
        }
    }

    /// <summary>
    /// Sets slider value parameter between 0 - 1 based on percent health remaining
    /// </summary>
    private void SetSlider()
    {
        if (slider) slider.value = (float)(manager.GetHealth() / manager.GetStat(NumericalStats.Health));
    }

    /// <summary>
    /// Displays a numerical health value
    /// </summary>
    private void SetDisplayText()
    {
        if (text) text.text = (int)manager.GetHealth() + (int)manager.GetShield() + "/" + (int)manager.GetStat(NumericalStats.Health);
    }

    /// <summary>
    /// Enables/Disables health bar visuals above Unit's head
    /// </summary>
    /// <param name="b"></param>
    private void ToggleVisibility(bool b)
    {
        for (var i = 0; i < transform.childCount; ++i)
        { transform.GetChild(i).gameObject.SetActive(b); }
    }

    /// <summary>
    /// Identifies the Local player over network and rotates the healthbar locally to face them
    /// </summary>
    private void RotateToFaceLocalPlayer()
    {
        GameObject player = null;
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in objs)
        {
            if (obj.transform.parent.name == "LocalGamePlayer")
            {
                player = obj;
                break;
            }
        }

        Quaternion rot = Camera.main ? Quaternion.LookRotation(Camera.main.transform.position - gameObject.transform.position, Vector3.up) : Quaternion.identity;
        rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + 180, rot.eulerAngles.z);
        gameObject.GetComponent<RectTransform>().rotation = rot;
    }
}
