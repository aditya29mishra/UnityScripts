using UnityEngine;
using TMPro;
using System;

public class TodayDateTMP : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI or TextMeshPro component")]
    public TMP_Text dateText;

    [Tooltip("Date format example: dd/MM/yyyy, MMM dd, yyyy")]
    public string dateFormat = "dd/MM/yyyy";

    void Awake()
    {
        if (dateText == null)
            dateText = GetComponent<TMP_Text>();

        if (dateText == null)
            return;

        dateText.text = DateTime.Now.ToString(dateFormat);
    }
}
