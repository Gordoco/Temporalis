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

    // Update is called once per frame
    void Update()
    {
        if (!manager.CheckReady()) return;
        slider.value = (float)(manager.GetHealth() / manager.GetStat(NumericalStats.Health));
        text.text = (int)manager.GetHealth() + "/" + (int)manager.GetStat(NumericalStats.Health);
    }
}
