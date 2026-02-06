using UnityEngine;
using UnityEngine.Events;

public class TransformToTarget : MonoBehaviour
{
    [Header("Target")]
    public Transform TargetTransform;

    [Header("Settings")]
    public bool MatchScale = false;
    public bool SmoothMove = true;

    [Tooltip("Position & Rotation speed")]
    public float MoveSpeed = 5f;
    public float RotateSpeed = 5f;
    private bool isMoved = false;
    public UnityEvent OnTransformComplete;
    bool isTransforming = false;

    // =========================
    // PUBLIC FUNCTIONS
    // =========================
    [ContextMenu("Start Transform")]
    public void StartTransform()
    {
        if (TargetTransform == null)
            return;

        isMoved = false;
        isTransforming = true;
    }
    void Update()
    {
        if (!isTransforming || TargetTransform == null)
            return;

        if (SmoothMove)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                TargetTransform.position,
                Time.deltaTime * MoveSpeed
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                TargetTransform.rotation,
                Time.deltaTime * RotateSpeed
            );
        }
        else
        {
            transform.position = TargetTransform.position;
            transform.rotation = TargetTransform.rotation;
        }

        if (MatchScale)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                TargetTransform.localScale,
                Time.deltaTime * MoveSpeed
            );
        }

        // Stop when close enough
        if (Vector3.Distance(transform.position, TargetTransform.position) < 0.01f &&
            Quaternion.Angle(transform.rotation, TargetTransform.rotation) < 0.5f)
        {
            isTransforming = false;
            isMoved = true;
        }
        if (isMoved)
        {
            Debug.Log("Transform completed.");
            OnTransformComplete.Invoke();
            isMoved = false; // Prevent multiple invocations
        }
    }
}
