using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedGameObjectActivator : MonoBehaviour
{
    [System.Serializable]
    public class TimedObject
    {
        public GameObject target;
        public float startTime;
        public float endTime;
    }

    public List<TimedObject> timedObjects = new List<TimedObject>();

    [Header("Timeline Settings")]
    public bool loop = false;

    [Header("Events")]
    public UnityEvent onTimelineCompleted;

    private Coroutine timelineRoutine;
    private float totalDuration;

    private void Start()
    {
        CalculateTotalDuration();
        StartTimeline();
        OutLine();
    }

    void CalculateTotalDuration()
    {
        totalDuration = 0f;

        foreach (var obj in timedObjects)
        {
            if (obj.endTime > totalDuration)
                totalDuration = obj.endTime;
        }
    }

    void OutLine()
    {
        foreach (var obj in timedObjects)
        {
            if(obj.target != null)
            {
                var outline = obj.target.GetComponent<outlineManger>();
                if (outline != null)
                {
                    outline.outlineOn();
                }
            }
        }
    }

    public void StartTimeline()
    {
        ResetAll();
        timelineRoutine = StartCoroutine(HandleTimedObjects());
    }

    private IEnumerator HandleTimedObjects()
    {
        float elapsedTime = 0f;

        while (elapsedTime <= totalDuration)
        {
            elapsedTime += Time.deltaTime;

            foreach (var obj in timedObjects)
            {
                if (obj.target == null) continue;

                bool shouldBeActive =
                    elapsedTime >= obj.startTime &&
                    elapsedTime < obj.endTime;

                if (obj.target.activeSelf != shouldBeActive)
                    obj.target.SetActive(shouldBeActive);
            }

            yield return null;
        }

        // Timeline finished
        ResetAll();
        onTimelineCompleted?.Invoke();

        if (loop)
        {
            StartTimeline();
        }
    }

    public void ResetAll()
    {
        if (timelineRoutine != null)
            StopCoroutine(timelineRoutine);

        foreach (var obj in timedObjects)
        {
            if (obj.target != null)
                obj.target.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ResetAll();
    }
}
