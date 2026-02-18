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

    Vector3 targetPosition;
    Quaternion targetRotation;

    DefaultPosObj defaultPos;
    Grabber g;
    Grabbable grabbable;

    [SerializeField]
    public UnityEvent onSnapComplete;


    private void OnTriggerEnter(Collider other)
    {
        if (!isSnapping && other.GetComponent<Grabbable>())
        {
            targetObject = other.transform;
            targetPosition = transform.position;
            targetRotation = transform.rotation;

            grabbable = other.GetComponent<Grabbable>();
            g = grabbable.GetPrimaryGrabber();

            if (other.GetComponent<DefaultPosObj>() != null)
            {
                defaultPos = other.GetComponent<DefaultPosObj>();
                defaultPos.IsTime = false;
            }

            if (g != null)
            {
                g.TryRelease();
            }

            isSnapping = true;
        }
    }

    private void Update()
    {
        if (isSnapping && targetObject != null)
        {
            // Smooth snapping
            targetObject.position = Vector3.Lerp(targetObject.position, targetPosition, Time.deltaTime * snapSpeed);
            targetObject.rotation = Quaternion.Slerp(targetObject.rotation, targetRotation, Time.deltaTime * snapSpeed);

            if (Vector3.Distance(targetObject.position, targetPosition) < 0.01f &&
                Quaternion.Angle(targetObject.rotation, targetRotation) < 1f)
            {
                targetObject.position = targetPosition;
                targetObject.rotation = targetRotation;
                onSnapComplete.Invoke();
                // Reset all
                g = null;
                grabbable = null;
                targetObject = null;
                isSnapping = false;
            }
        }
    }
}
