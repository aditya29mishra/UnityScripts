using UnityEngine;
using UnityEngine.Events;

public class TriggerSensor : MonoBehaviour
{
    [Header("Sensor Conditions")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private int requiredHits = 1;

    [Header("Event")]
    public UnityEvent onSensorTriggered;

    private int currentHits = 0;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (!other.CompareTag(requiredTag)) return;

        currentHits++;

        if (currentHits >= requiredHits)
        {
            Trigger();
        }
    }

    private void Trigger()
    {
        triggered = true;
        onSensorTriggered?.Invoke();
    }
}
