using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUpdater : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private StatManager manager;
    [SerializeField] private TMP_Text text;
    [SerializeField] private bool bEnemy = false;

    // Update is called once per frame
    void Update()
    {
        if (!manager.CheckReady()) return;
        if (slider) slider.value = (float)(manager.GetHealth() / manager.GetStat(NumericalStats.Health));
        if (text) text.text = (int)manager.GetHealth() + "/" + (int)manager.GetStat(NumericalStats.Health);
        //Debug.Log("HBU VALUE: " + manager.CheckIfShouldShowHP());
        if (bEnemy && manager.CheckIfShouldShowHP())
        {
            for (var i = 0; i < transform.childCount; ++i)
            { transform.GetChild(i).gameObject.SetActive(true); }
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
            //Debug.DrawLine(Camera.main.transform.position, gameObject.transform.position);
            Quaternion rot = Camera.main ? Quaternion.LookRotation(Camera.main.transform.position - gameObject.transform.position, Vector3.up) : Quaternion.identity;
            rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + 180, rot.eulerAngles.z);
            gameObject.GetComponent<RectTransform>().rotation = rot;
        }
        else if (bEnemy)
        {
            for (var i = 0; i < transform.childCount; ++i)
            { transform.GetChild(i).gameObject.SetActive(false); }
        }
    }
}
