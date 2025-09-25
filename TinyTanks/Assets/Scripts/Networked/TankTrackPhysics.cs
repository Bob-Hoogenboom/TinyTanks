using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DefaultExecutionOrder(+100)]
public class TankTrackPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    [Tooltip("This is a multiplier. it does the default animator speed and multiplies that by animSpeed. \n(animator.speed * animSpeed)")]
    [SerializeField] private float animSpeed = 1f;
    private int _leftTrackAnim = Animator.StringToHash("LeftTrack");
    private int _rightTrackAnim = Animator.StringToHash("RightTrack");

    [Header("RayCast")]
    [SerializeField] float contactRadius = 0.22f;
    [SerializeField] float contactCapsuleHalfLength = 0.45f;
    [SerializeField] private float trackSpacing = 2.6f;
    [SerializeField] private float trackRayStartHeight = 0.6f;
    [SerializeField] private float trackRayLength = 1.2f;

    [Header("Tuning")]
    [SerializeField] private float extraDownForce = 0f;
    [SerializeField] private float yawDamping = 0.2f;
    [SerializeField] private float stabilizationForce = 500f;
    [SerializeField] private float groundingForce = 1000f;

    [Header("Movement Variables")]
    [SerializeField] private TankData hardFloorData;
    [SerializeField] private TankData carpetData;
    [SerializeField] private TankData wetFloorData;
    [SerializeField] private TankData currData;

    [Header("Lateral Model")]
    [Tooltip("Dead-zone for side slip (m/s).")]
    [SerializeField] private float slipDead = 0.4f;
    [Tooltip("Minimum velocity magnitude to prevent division by zero")]
    [SerializeField] private float minVelocityMagnitude = 0.01f;
    [Tooltip("Smoothing factor for track contact (0-1, higher = more responsive)")]
    [SerializeField] private float contactSmoothing = 0.8f;

    private float leftInput;
    private float rightInput;

    // Track contact state
    private float leftContactFactor = 0f;
    private float rightContactFactor = 0f;
    private Vector3 lastLeftNormal = Vector3.up;
    private Vector3 lastRightNormal = Vector3.up;

    // Stability helpers
    private Vector3 smoothedForward = Vector3.forward;
    private Vector3 smoothedRight = Vector3.right;
    private float forwardSmoothing = 0.9f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Stability guards
        rb.maxDepenetrationVelocity = 2f;
        rb.maxAngularVelocity = Mathf.Max(rb.maxAngularVelocity, 4f);
        rb.solverIterations = Mathf.Max(rb.solverIterations, 12);
        rb.solverVelocityIterations = Mathf.Max(rb.solverVelocityIterations, 12);
        if (rb.angularDrag < 0.15f) rb.angularDrag = 0.15f;

        currData = hardFloorData;
    }

    private void Start()
    {
        smoothedForward = transform.forward;
        smoothedRight = transform.right;
    }

    public void SetInputs(float left, float right)
    {
        leftInput = left;
        rightInput = right;
        
        anim.SetFloat(_leftTrackAnim, leftInput);
        anim.SetFloat(_rightTrackAnim, rightInput);
    }

    private void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        smoothedForward = Vector3.Slerp(smoothedForward, transform.forward, forwardSmoothing).normalized;
        smoothedRight = Vector3.Slerp(smoothedRight, transform.right, forwardSmoothing).normalized;

        // --- compute track anchor bases (midpoints) ---
        Vector3 leftMid = transform.TransformPoint(new Vector3(-trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 rightMid = transform.TransformPoint(new Vector3(trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 fwd = transform.forward;
        Vector3 down = -transform.up;

        // --- define front/rear cast starts for each track ---
        float anchorHalfLen = contactCapsuleHalfLength; // half the contact length along the track
        Vector3 leftFrontStart = leftMid + fwd * anchorHalfLen;
        Vector3 leftRearStart = leftMid - fwd * anchorHalfLen;
        Vector3 rightFrontStart = rightMid + fwd * anchorHalfLen;
        Vector3 rightRearStart = rightMid - fwd * anchorHalfLen;

        // --- sphere casts straight down at each anchor ---
        bool lfHit = Physics.SphereCast(leftFrontStart, contactRadius, down, out RaycastHit lfInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);
        bool lrHit = Physics.SphereCast(leftRearStart, contactRadius, down, out RaycastHit lrInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);
        bool rfHit = Physics.SphereCast(rightFrontStart, contactRadius, down, out RaycastHit rfInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);
        bool rrHit = Physics.SphereCast(rightRearStart, contactRadius, down, out RaycastHit rrInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);

        // update surface type + smoothed normals
        if (lfHit) { CheckTrackSurface(lfInfo); lastLeftNormal = Vector3.Slerp(lastLeftNormal, lfInfo.normal, 0.5f).normalized; }
        if (lrHit) { CheckTrackSurface(lrInfo); lastLeftNormal = Vector3.Slerp(lastLeftNormal, lrInfo.normal, 0.5f).normalized; }
        if (rfHit) { CheckTrackSurface(rfInfo); lastRightNormal = Vector3.Slerp(lastRightNormal, rfInfo.normal, 0.5f).normalized; }
        if (rrHit) { CheckTrackSurface(rrInfo); lastRightNormal = Vector3.Slerp(lastRightNormal, rrInfo.normal, 0.5f).normalized; }

        // per-track normal load split across valid hit points
        float g = Physics.gravity.magnitude;
        float normalPerTrack = (rb.mass * g) * 0.5f;

        int leftHits = (lfHit ? 1 : 0) + (lrHit ? 1 : 0);
        int rightHits = (rfHit ? 1 : 0) + (rrHit ? 1 : 0);

        float leftShare = leftHits > 0 ? normalPerTrack / leftHits : 0f;
        float rightShare = rightHits > 0 ? normalPerTrack / rightHits : 0f;

        // fallbacks if a spherecast misses (keeps behavior graceful when airborne)
        Vector3 lfPt = lfHit ? lfInfo.point : leftFrontStart - down * (trackRayLength * 0.5f);
        Vector3 lrPt = lrHit ? lrInfo.point : leftRearStart - down * (trackRayLength * 0.5f);
        Vector3 rfPt = rfHit ? rfInfo.point : rightFrontStart - down * (trackRayLength * 0.5f);
        Vector3 rrPt = rrHit ? rrInfo.point : rightRearStart - down * (trackRayLength * 0.5f);

        // LEFT track forces (front + rear if present)
        if (lfHit) ApplyTrackForces(lfPt, lfHit ? lfInfo.normal : lastLeftNormal, leftInput, rightInput, leftShare, true);
        if (lrHit) ApplyTrackForces(lrPt, lrHit ? lrInfo.normal : lastLeftNormal, leftInput, rightInput, leftShare, true);

        // RIGHT track forces (front + rear if present)
        if (rfHit) ApplyTrackForces(rfPt, rfHit ? rfInfo.normal : lastRightNormal, rightInput, leftInput, rightShare, false);
        if (rrHit) ApplyTrackForces(rrPt, rrHit ? rrInfo.normal : lastRightNormal, rightInput, leftInput, rightShare, false);

        var velMag = rb.velocity.magnitude;
        anim.speed = velMag * 2;
        Debug.Log(velMag * 2);
    }

    private void ApplyTrackForces( Vector3 contactPoint, Vector3 contactNormal, float input, float otherInput, float normalForce, bool isLeft)
    {
        Vector3 lateral = Vector3.ProjectOnPlane(smoothedRight, contactNormal);
        float latMag = lateral.magnitude;

        // Prevent zero-magnitude lateral vector
        if (latMag < minVelocityMagnitude)
        {
            lateral = Vector3.Cross(contactNormal, smoothedForward);
            latMag = lateral.magnitude;
            if (latMag < minVelocityMagnitude)
            {
                // Fallback to world axes if still degenerate
                lateral = Vector3.ProjectOnPlane(Vector3.right, contactNormal);
            }
        }
        lateral = lateral.normalized;

        Vector3 forward = Vector3.Cross(lateral, contactNormal).normalized;

        // Velocities at contact, projected to the ground plane
        Vector3 vPt = rb.GetPointVelocity(contactPoint);
        Vector3 vGround = Vector3.ProjectOnPlane(vPt, contactNormal);

        float vFwd = Vector3.Dot(vGround, forward);
        Vector3 vSlipVec = vGround - forward * vFwd;       // lateral slip vector
        float vSlipMag = vSlipVec.magnitude;             // no smoothing

        // Traction limits
        float tractionLong = Mathf.Max(0.1f, currData.muLong * normalForce);
        float tractionLat = Mathf.Max(0.1f, currData.muLat * normalForce);

        // Engine/drive
        float vForPower = Mathf.Max(minVelocityMagnitude, Mathf.Abs(vFwd) + 0.5f);
        float engineForceCap = Mathf.Min(currData.maxForcePerTrack, currData.EnginePowerW / vForPower);

        bool neutralSteer = (input * otherInput) < -0.2f;
        if (neutralSteer)
        {
            engineForceCap *= currData.neutralSteerTorqueBoost;
        }

        float smoothedInput = input;
        if (Mathf.Abs(input) < 0.05f) smoothedInput = 0f; // Dead zone

        float drive = Mathf.Clamp(smoothedInput * engineForceCap, -tractionLong, tractionLong);

        Vector3 latF = Vector3.zero;
        if (vSlipMag > slipDead)
        {
            Vector3 slipDir = vSlipVec / (vSlipMag + minVelocityMagnitude);
            float s = vSlipMag - slipDead;
            float desired = currData.lateralStiffness * s;
            float capped = Mathf.Min(Mathf.Abs(desired), tractionLat);
            latF = -slipDir * capped;
        }
        else if (vSlipMag > minVelocityMagnitude)
        {
            // Apply small centering force in dead zone to prevent drift
            latF = -vSlipVec * (currData.lateralStiffness * 0.1f);
        }

        // Rolling resistance with stability
        float resistMag = currData.rollingResistance * normalForce;
        if (Mathf.Abs(vFwd) > minVelocityMagnitude)
        {
            resistMag += currData.speedDrag * vFwd;
        }
        Vector3 resist = -forward * resistMag;

        // Front/rear lateral damping (no smoothing)
        Vector3 pFront = contactPoint + forward * currData.contactHalfLength;
        Vector3 pRear = contactPoint - forward * currData.contactHalfLength;

        Vector3 vFront = Vector3.ProjectOnPlane(rb.GetPointVelocity(pFront), contactNormal);
        Vector3 vRear = Vector3.ProjectOnPlane(rb.GetPointVelocity(pRear), contactNormal);

        float vLatFront = Vector3.Dot(vFront, lateral);
        float vLatRear = Vector3.Dot(vRear, lateral);

        // Apply damping with stability checks
        float Ffront = 0f;
        float Frear = 0f;

        if (Mathf.Abs(vLatFront) > minVelocityMagnitude)
        {
            Ffront = -(currData.linearLatDrag * vLatFront +
                      currData.quadLatDrag * vLatFront * Mathf.Abs(vLatFront));
        }

        if (Mathf.Abs(vLatRear) > minVelocityMagnitude)
        {
            Frear = -(currData.linearLatDrag * vLatRear +
                     currData.quadLatDrag * vLatRear * Mathf.Abs(vLatRear));
        }

        // Budget lateral forces
        float latBudget = Mathf.Max(0f, tractionLat - latF.magnitude);
        float sumAbs = Mathf.Abs(Ffront) + Mathf.Abs(Frear);
        if (sumAbs > latBudget && sumAbs > minVelocityMagnitude)
        {
            float scale = latBudget / sumAbs;
            Ffront *= scale;
            Frear *= scale;
        }

        // Debug rays
        Debug.DrawRay(contactPoint, forward * (drive * 0.0005f), isLeft ? Color.green : Color.cyan, Time.fixedDeltaTime, false);
        Debug.DrawRay(contactPoint, resist * 0.0005f, Color.red, Time.fixedDeltaTime, false);
        Debug.DrawRay(contactPoint, latF * 0.05f, Color.magenta, Time.fixedDeltaTime, false);

        // Apply forces with clamping for stability
        Vector3 totalForce = forward * drive + resist + latF;
        float forceMag = totalForce.magnitude;
        float maxForce = tractionLong + tractionLat;

        if (forceMag > maxForce && forceMag > minVelocityMagnitude)
        {
            totalForce = totalForce * (maxForce / forceMag);
        }

        rb.AddForceAtPosition(totalForce, contactPoint, ForceMode.Force);

        // Apply lateral damping forces
        if (Mathf.Abs(Ffront) > minVelocityMagnitude)
        {
            rb.AddForceAtPosition(lateral * Ffront, pFront, ForceMode.Force);
        }
        if (Mathf.Abs(Frear) > minVelocityMagnitude)
        {
            rb.AddForceAtPosition(lateral * Frear, pRear, ForceMode.Force);
        }

        // Apply grounding force to keep tank stable
        if (normalForce > 0.1f)
        {
            rb.AddForceAtPosition(contactNormal * (-extraDownForce - groundingForce * Time.fixedDeltaTime), contactPoint, ForceMode.Force);
        }
    }

    private void ApplyStabilization(bool hasGroundContact)
    {
        if (!hasGroundContact) return;

        // Anti-roll stabilization
        Vector3 up = transform.up;
        Vector3 targetUp = Vector3.up;
        Vector3 torque = Vector3.Cross(up, targetUp) * stabilizationForce;

        // Only apply roll/pitch stabilization, not yaw
        torque = Vector3.ProjectOnPlane(torque, Vector3.up);

        rb.AddTorque(torque, ForceMode.Force);
    }

    private void ApplyYawDamping()
    {
        // Extra yaw damping to prevent spinning
        Vector3 angVel = rb.angularVelocity;
        float yawRate = Vector3.Dot(angVel, transform.up);

        // Apply stronger damping at low speeds to prevent spin-up
        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / 5f);
        float dampingStrength = Mathf.Lerp(yawDamping * 2f, yawDamping, speedFactor);

        if (Mathf.Abs(yawRate) > minVelocityMagnitude)
        {
            Vector3 yawDampTorque = -transform.up * yawRate * dampingStrength * rb.mass;
            rb.AddTorque(yawDampTorque, ForceMode.Force);
        }
    }

    private void CheckTrackSurface(RaycastHit hitInfo)
    {
        if (hitInfo.collider == null) return;
        int layer = hitInfo.collider.gameObject.layer;

        switch (layer)
        {
            case 10: currData = hardFloorData; break;
            case 11: currData = carpetData; break;
            case 12: currData = wetFloorData; break;
            default: currData = hardFloorData; break;
        }
    }
}
