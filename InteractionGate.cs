using UnityEngine;
using UnityEngine.Events;

public class InteractionGate : MonoBehaviour
{
    [Header("Gate Settings")]
    [Tooltip("Target count to open the gate")]
    public int requiredCount = 1;

    [Tooltip("Reset counter after gate opens")]
    public bool resetAfterInvoke = true;

    [Header("Events")]
    // public UnityEvent OnIncrement;
    // public UnityEvent OnDecrement;
    public UnityEvent OnGateOpened;

    private int currentCount = 0;
    private bool gateOpen = false;

    // ---------------------------------------------------
    // CALL FROM ANY EVENT (ENTER / PRESS / ATTACH / GRAB)
    // ---------------------------------------------------
    public void Increment()
    {
        if (gateOpen)
            return;

        currentCount++;

        Debug.Log($"[InteractionGate] Increment → {currentCount}/{requiredCount}");

        // OnIncrement?.Invoke();

        EvaluateGate();
    }

    // ---------------------------------------------------
    // CALL FROM ANY EVENT (EXIT / RELEASE / DETACH)
    // ---------------------------------------------------
    public void Decrement()
    {
        if (gateOpen)
            return;

        currentCount--;

        if (currentCount < 0)
            currentCount = 0;

        Debug.Log($"[InteractionGate] Decrement → {currentCount}/{requiredCount}");

        //  OnDecrement?.Invoke();
    }

    // ---------------------------------------------------
    private void EvaluateGate()
    {
        if (currentCount >= requiredCount)
        {
            gateOpen = true;

            Debug.Log("[InteractionGate] Gate OPENED");

            OnGateOpened?.Invoke();

            if (resetAfterInvoke)
                ResetGate();
        }
    }

    // ---------------------------------------------------
    public void ResetGate()
    {
        currentCount = 0;
        gateOpen = false;
    }

    // ---------------------------------------------------
    public int GetCurrentCount()
    {
        return currentCount;
    }
}
