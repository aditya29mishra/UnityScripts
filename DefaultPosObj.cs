using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using System;
using UnityEngine.Events;

public class DefaultPosObj : MonoBehaviour
{
    public GrabbableUnityEvents grabbableUnityEvents;
    Vector3 DefaultPos;
    public GameObject Obj;
    Quaternion DefaultRotation;
    public bool IsOn;

    internal bool IsTime = true;
    [SerializeField]
    public UnityEvent onDefaultReached;
    void Start()
    {
        grabbableUnityEvents = GetComponent<GrabbableUnityEvents>();
        grabbableUnityEvents.onGrab.AddListener(grabEnvent);
        grabbableUnityEvents.onRelease.AddListener(releaseEnvent);
        DefaultPos = Obj.transform.position;
        DefaultRotation = Obj.transform.rotation;
    }

    private void releaseEnvent()
    {
        Obj.GetComponent<Rigidbody>().isKinematic = true;
        IsOn = true;
    }

    private void grabEnvent(Grabber arg0)
    {
        IsOn = false;
    }

    void Update()
    {
        if(IsOn && IsTime)
        {
            Obj.transform.position = Vector3.Lerp(Obj.transform.position, DefaultPos, 10 * Time.deltaTime);
            Obj.transform.rotation = DefaultRotation;


            // Check if object reached default position and rotation
            if (Vector3.Distance(Obj.transform.position, DefaultPos) < 0.01f &&
                Quaternion.Angle(Obj.transform.rotation, DefaultRotation) < 1f)
            {
                onDefaultReached?.Invoke();
                IsOn = false;// Event call jab position match ho
            }
        }

       
    }

    public void SetDefaultPosition()
    {
        Obj.transform.position = DefaultPos;
        Obj.transform.rotation = DefaultRotation;
     
    }

    public void Lerp()
    {
        IsOn = true;
    }
    public void LerpFalse()
    {
        IsOn = false;
    }
}
