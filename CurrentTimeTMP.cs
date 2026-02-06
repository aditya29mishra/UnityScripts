using UnityEngine;
using TMPro;
using System;

public class CurrentTimeTMP : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI or TextMeshPro component")]
    public TMP_Text timeText;

    [Tooltip("Time format example: HH:mm, hh:mm tt")]
    public string timeFormat = "HH:mm";

    void Awake()
    {
        if (timeText == null)
            timeText = GetComponent<TMP_Text>();

        if (timeText == null)
            return;

        timeText.text = DateTime.Now.ToString(timeFormat);
    }
}
