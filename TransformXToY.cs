using UnityEngine;
using UnityEngine.Events;

public class TransformXToY : MonoBehaviour
{
    [Header("References")]
    public Transform ObjectX;        // Object to move
    public Transform TargetY;        // Destination

    [Header("Settings")]
    public bool MatchScale = false;
    public bool SmoothMove = true;

    [Tooltip("Position & Rotation speed")]
    public float MoveSpeed = 5f;
    public float RotateSpeed = 5f;

    public UnityEvent OnTransformComplete;

    private bool isTransforming = false;
    private bool isCompleted = false;

    // =========================
    // PUBLIC API
    // =========================
    [ContextMenu("Start Transform")]
    public void StartTransform()
    {
        if (ObjectX == null || TargetY == null)
            return;

        isTransforming = true;
        isCompleted = false;
    }

    void Update()
    {
        if (!isTransforming || ObjectX == null || TargetY == null)
            return;

        // -------------------------
        // POSITION & ROTATION
        // -------------------------
        if (SmoothMove)
        {
            ObjectX.position = Vector3.Lerp(
                ObjectX.position,
                TargetY.position,
                Time.deltaTime * MoveSpeed
            );

            ObjectX.rotation = Quaternion.Slerp(
                ObjectX.rotation,
                TargetY.rotation,
                Time.deltaTime * RotateSpeed
            );
        }
        else
        {
            ObjectX.position = TargetY.position;
            ObjectX.rotation = TargetY.rotation;
        }

        // -------------------------
        // SCALE (OPTIONAL)
        // -------------------------
        if (MatchScale)
        {
            ObjectX.localScale = Vector3.Lerp(
                ObjectX.localScale,
                TargetY.localScale,
                Time.deltaTime * MoveSpeed
            );
        }

        // -------------------------
        // COMPLETION CHECK
        // -------------------------
        bool positionDone =
            Vector3.Distance(ObjectX.position, TargetY.position) < 0.01f;

        bool rotationDone =
            Quaternion.Angle(ObjectX.rotation, TargetY.rotation) < 0.5f;

        if (positionDone && rotationDone)
        {
            CompleteTransform();
        }
    }

    void CompleteTransform()
    {
        if (isCompleted)
            return;

        isTransforming = false;
        isCompleted = true;

        Debug.Log("Transform X → Y completed");
        OnTransformComplete?.Invoke();
    }
}
