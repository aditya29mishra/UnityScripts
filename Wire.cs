using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour
{
    public event Action OnPointsChanged;

    [Header("Rope Transforms")]
    [Tooltip("The rope will start at this point")]
    [SerializeField] private Transform startPoint;
    public Transform StartPoint => startPoint;

    [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
    [SerializeField] private Transform midPoint;
    public Transform MidPoint => midPoint;

    [Tooltip("The rope will end at this point")]
    [SerializeField] private Transform endPoint;
    public Transform EndPoint => endPoint;

    [Header("Wire Curve (Non-Gravity)")]
    private const float curveResponse = 2f;
    [SerializeField] private Vector3 curveDirection = Vector3.up;
    [SerializeField] private float maxCurveHeight = 0.03f;


    [Header("Wire Length Constraint")]
    [SerializeField] private float maxWireLength = 0.15f; // meters


    [Header("Rope Settings")]
    [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
    [Range(2, 100)] public int linePoints = 10;

    [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
    public float stiffness = 350f;

    [Tooltip("0 is no damping, 50 is a lot")]
    public float damping = 15f;

    [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
    public float ropeLength = 15;

    [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
    public float ropeWidth = 0.1f;

    [Header("Rational Bezier Weight Control")]
    [Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
    [Range(1, 15)] public float midPointWeight = 1f;
    private const float StartPointWeight = 1f; //these need to stay at 1, could be removed but makes calling the rational bezier function easier to read and understand
    private const float EndPointWeight = 1f;

    [Header("Midpoint Position")]
    [Tooltip("Position of the midpoint along the line between start and end points")]
    [Range(0.25f, 0.75f)] public float midPointPosition = 0.5f;

    private Vector3 currentValue;
    private Vector3 currentVelocity;
    private Vector3 targetValue;
    public Vector3 otherPhysicsFactors { get; set; }
    private const float valueThreshold = 0.01f;
    private const float velocityThreshold = 0.01f;

    private LineRenderer lineRenderer;
    private bool isFirstFrame = true;

    private Vector3 prevStartPointPosition;
    private Vector3 prevEndPointPosition;
    private float prevMidPointPosition;
    private float prevMidPointWeight;

    private float prevLineQuality;
    private float prevRopeWidth;
    private float prevstiffness;
    private float prevDampness;
    private float prevRopeLength;
    private Vector3 lockedPerp;
    private bool perpInitialized = false;
    private Vector3 lastDir;
    [Header("Snap State")]
    public bool isStartSnapped;
    public bool isEndSnapped;




    public bool IsPrefab => gameObject.scene.rootCount == 0;

    private void Start()
    {
        InitializeLineRenderer();
        if (AreEndPointsValid())
        {
            currentValue = GetMidPoint();
            targetValue = currentValue;
            currentVelocity = Vector3.zero;
            SetSplinePoint(); // Ensure initial spline point is set correctly
        }
    }
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            InitializeLineRenderer();
            if (AreEndPointsValid())
            {
                RecalculateRope();
                SimulatePhysics();
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }
    }

    private void InitializeLineRenderer()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
    }

    private void Update()
    {
        if (IsPrefab)
        {
            return;
        }

        if (AreEndPointsValid())
        {
            SetSplinePoint();

            if (!Application.isPlaying && (IsPointsMoved() || IsRopeSettingsChanged()))
            {
                SimulatePhysics();
                NotifyPointsChanged();
            }

            prevStartPointPosition = startPoint.position;
            prevEndPointPosition = endPoint.position;
            prevMidPointPosition = midPointPosition;
            prevMidPointWeight = midPointWeight;

            prevLineQuality = linePoints;
            prevRopeWidth = ropeWidth;
            prevstiffness = stiffness;
            prevDampness = damping;
            prevRopeLength = ropeLength;
        }
    }
    void LateUpdate()
    {
        if (AreEndPointsValid())
        {
            EnforceMaxLength();
            SetSplinePoint();
        }
    }

    private bool AreEndPointsValid()
    {
        return startPoint != null && endPoint != null;
    }

    private void SetSplinePoint()
    {
        if (lineRenderer.positionCount != linePoints + 1)
        {
            lineRenderer.positionCount = linePoints + 1;
        }

        Vector3 mid = GetMidPoint();
        currentValue = mid;
        targetValue = mid;


        if (midPoint != null)
        {
            midPoint.position = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, midPointPosition, StartPointWeight, midPointWeight, EndPointWeight);
        }

        for (int i = 0; i < linePoints; i++)
        {
            Vector3 p = GetRationalBezierPoint(startPoint.position, mid, endPoint.position, i / (float)linePoints, StartPointWeight, midPointWeight, EndPointWeight);
            lineRenderer.SetPosition(i, p);
        }

        lineRenderer.SetPosition(linePoints, endPoint.position);
    }

    private float CalculateYFactorAdjustment(float weight)
    {
        //float k = 0.360f; //after testing this seemed to be a good value for most cases, more accurate k is available.
        float k = Mathf.Lerp(0.493f, 0.323f, Mathf.InverseLerp(1, 15, weight)); //K calculation that is more accurate, interpolates between precalculated values.
        float w = 1f + k * Mathf.Log(weight);
        return w;
    }

    private Vector3 GetMidPoint()
    {
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        // Base midpoint along the line
        Vector3 midpos = Vector3.Lerp(startPos, endPos, midPointPosition);

        float X = Vector3.Distance(startPos, endPos);
        float L = maxWireLength;

        if (X <= 0.0001f || X >= L)
            return midpos; // no sag when too close or fully stretched

        Vector3 safeCurveDir = curveDirection.sqrMagnitude < 0.0001f
            ? Vector3.zero
            : curveDirection.normalized;

        // 1️⃣ Normalized distance (0 → 1)
        float t = Mathf.Clamp01(X / L);

        // 2️⃣ Bell-shaped activation (0 → 1 → 0)
        float activation = 4f * t * (1f - t);

        // 3️⃣ Geometric max sag (triangle constraint)
        float halfX = X * 0.5f;
        float halfL = L * 0.5f;

        float geoMaxSag = Mathf.Sqrt(
            Mathf.Max(0f, (halfL * halfL) - (halfX * halfX))
        );

        // 4️⃣ Final sag height (MR-safe)
        float sagHeight = activation * geoMaxSag;
        sagHeight = Mathf.Min(sagHeight, maxCurveHeight);

        midpos += safeCurveDir * sagHeight;
        return midpos;
    }


    private void EnforceMaxLength()
    {
        if (!startPoint || !endPoint)
            return;

        Vector3 a = startPoint.position;
        Vector3 b = endPoint.position;

        Vector3 delta = b - a;
        float distance = delta.magnitude;

        if (distance <= maxWireLength || distance < 0.0001f)
            return;

        Vector3 dir = delta / distance;
        float excess = distance - maxWireLength;

        // Determine movement weights
        float wA, wB;

        if (isStartSnapped && isEndSnapped)
        {
            // Fully constrained — do nothing
            return;
        }
        else if (isStartSnapped && !isEndSnapped)
        {
            wA = 0f;
            wB = 1f;
        }
        else if (!isStartSnapped && isEndSnapped)
        {
            wA = 1f;
            wB = 0f;
        }
        else
        {
            // Both free
            wA = 0.5f;
            wB = 0.5f;
        }

        // Apply constraint correction
        startPoint.position += dir * excess * wA;
        endPoint.position -= dir * excess * wB;
    }





    private Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0, float w1, float w2)
    {
        //scale each point by its weight (can probably remove w0 and w2 if the midpoint is the only adjustable weight)
        Vector3 wp0 = w0 * p0;
        Vector3 wp1 = w1 * p1;
        Vector3 wp2 = w2 * p2;

        //calculate the denominator of the rational Bézier curve
        float denominator = w0 * Mathf.Pow(1 - t, 2) + 2 * w1 * (1 - t) * t + w2 * Mathf.Pow(t, 2);
        //calculate the numerator and devide by the demoninator to get the point on the curve
        Vector3 point = (wp0 * Mathf.Pow(1 - t, 2) + wp1 * 2 * (1 - t) * t + wp2 * Mathf.Pow(t, 2)) / denominator;

        return point;
    }

    public Vector3 GetPointAt(float t)
    {
        if (!AreEndPointsValid())
        {
            Debug.LogError("StartPoint or EndPoint is not assigned.", gameObject);
            return Vector3.zero;
        }

        return GetRationalBezierPoint(startPoint.position, currentValue, endPoint.position, t, StartPointWeight, midPointWeight, EndPointWeight);
    }

    private void FixedUpdate()
    {
        if (IsPrefab)
        {
            return;
        }

        if (AreEndPointsValid())
        {
            if (!isFirstFrame)
            {
                // SimulatePhysics();
            }

            isFirstFrame = false;
        }
    }

    private void SimulatePhysics()
    {
        float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
        Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
        currentVelocity = currentVelocity * dampingFactor + acceleration + otherPhysicsFactors;
        currentValue += currentVelocity * Time.fixedDeltaTime;

        if (Vector3.Distance(currentValue, targetValue) < valueThreshold && currentVelocity.magnitude < velocityThreshold)
        {
            currentValue = targetValue;
            currentVelocity = Vector3.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (!AreEndPointsValid())
            return;

        Vector3 midPos = GetMidPoint();
        // Uncomment if you need to visualize midpoint
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(midPos, 0.2f);
    }

    // New API methods for setting start and end points
    // with instantAssign parameter to recalculate the rope immediately, without 
    // animating the rope to the new position.
    // When newStartPoint or newEndPoint is null, the rope will be recalculated immediately

    public void SetStartPoint(Transform newStartPoint, bool instantAssign = false)
    {
        startPoint = newStartPoint;
        prevStartPointPosition = startPoint == null ? Vector3.zero : startPoint.position;

        if (instantAssign || newStartPoint == null)
        {
            RecalculateRope();
        }

        NotifyPointsChanged();
    }
    public void SetMidPoint(Transform newMidPoint, bool instantAssign = false)
    {
        midPoint = newMidPoint;
        prevMidPointPosition = midPoint == null ? 0.5f : midPointPosition;

        if (instantAssign || newMidPoint == null)
        {
            RecalculateRope();
        }
        NotifyPointsChanged();
    }

    public void SetEndPoint(Transform newEndPoint, bool instantAssign = false)
    {
        endPoint = newEndPoint;
        prevEndPointPosition = endPoint == null ? Vector3.zero : endPoint.position;

        if (instantAssign || newEndPoint == null)
        {
            RecalculateRope();
        }

        NotifyPointsChanged();
    }

    public void RecalculateRope()
    {
        if (!AreEndPointsValid())
        {
            lineRenderer.positionCount = 0;
            return;
        }

        currentValue = GetMidPoint();
        targetValue = currentValue;
        currentVelocity = Vector3.zero;
        SetSplinePoint();
    }

    private void NotifyPointsChanged()
    {
        OnPointsChanged?.Invoke();
    }

    private bool IsPointsMoved()
    {
        var startPointMoved = startPoint.position != prevStartPointPosition;
        var endPointMoved = endPoint.position != prevEndPointPosition;
        return startPointMoved || endPointMoved;
    }

    private bool IsRopeSettingsChanged()
    {
        var lineQualityChanged = !Mathf.Approximately(linePoints, prevLineQuality);
        var ropeWidthChanged = !Mathf.Approximately(ropeWidth, prevRopeWidth);
        var stiffnessChanged = !Mathf.Approximately(stiffness, prevstiffness);
        var dampnessChanged = !Mathf.Approximately(damping, prevDampness);
        var ropeLengthChanged = !Mathf.Approximately(ropeLength, prevRopeLength);
        var midPointPositionChanged = !Mathf.Approximately(midPointPosition, prevMidPointPosition);
        var midPointWeightChanged = !Mathf.Approximately(midPointWeight, prevMidPointWeight);

        return lineQualityChanged
               || ropeWidthChanged
               || stiffnessChanged
               || dampnessChanged
               || ropeLengthChanged
               || midPointPositionChanged
               || midPointWeightChanged;
    }
}