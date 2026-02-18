using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BNG;

public class SnapObject : MonoBehaviour
{
    bool isSnapping = false;

    [HideInInspector]
    public Transform targetObject;

    public float snapSpeed = 5f;

    [Header("Allowed Tags (Leave Empty = Allow All)")]
    [SerializeField] private List<string> allowedTags = new List<string>();

    Vector3 targetPosition;
    Quaternion targetRotation;

    DefaultPosObj defaultPos;
    Grabber g;
    Grabbable grabbable;

    [SerializeField]
    public UnityEvent onSnapComplete;

    private void OnTriggerEnter(Collider other)
    {
        if (isSnapping) return;

        Grabbable grab = other.GetComponent<Grabbable>();
        if (grab == null) return;

        // ✅ Tag Validation
        if (!IsTagAllowed(other)) return;

        targetObject = other.transform;
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        grabbable = grab;
        g = grabbable.GetPrimaryGrabber();

        DefaultPosObj dp = other.GetComponent<DefaultPosObj>();
        if (dp != null)
        {
            defaultPos = dp;
            defaultPos.IsTime = false;
        }

        if (g != null)
        {
            g.TryRelease();
        }

        isSnapping = true;
    }

    private bool IsTagAllowed(Collider other)
    {
        // 🔹 If no tags provided → allow all
        if (allowedTags == null || allowedTags.Count == 0)
            return true;

        // 🔹 Otherwise check if object's tag exists in list
        return allowedTags.Contains(other.tag);
    }

    private void Update()
    {
        if (!isSnapping || targetObject == null) return;

        targetObject.position = Vector3.Lerp(
            targetObject.position,
            targetPosition,
            Time.deltaTime * snapSpeed
        );

        targetObject.rotation = Quaternion.Slerp(
            targetObject.rotation,
            targetRotation,
            Time.deltaTime * snapSpeed
        );

        if (Vector3.Distance(targetObject.position, targetPosition) < 0.01f &&
            Quaternion.Angle(targetObject.rotation, targetRotation) < 1f)
        {
            targetObject.position = targetPosition;
            targetObject.rotation = targetRotation;

            onSnapComplete?.Invoke();

            g = null;
            grabbable = null;
            targetObject = null;
            isSnapping = false;
        }
    }
}
