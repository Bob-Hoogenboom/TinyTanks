using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DefaultExecutionOrder(+100)]
public class TankTrackPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("RayCast")]
    [SerializeField] float contactRadius = 0.22f;
    [SerializeField] float contactCapsuleHalfLength = 0.45f;
    [SerializeField] private float trackSpacing = 2.6f;
    [SerializeField] private float trackRayStartHeight = 0.6f;
    [SerializeField] private float trackRayLength = 1.2f;

    [Header("Tuning")]
    [SerializeField] private float extraDownForce = 0f;
    [SerializeField] private float yawDamping = 0.2f;

    [Header("Movement Variables")]
    [SerializeField] private TankData hardFloorData;
    [SerializeField] private TankData carpetData;
    [SerializeField] private TankData wetFloorData;
    [SerializeField] private TankData currData;

    [Header("Lateral Model")]
    [Tooltip("Dead-zone for side slip (m/s). Not a smoother—just avoids micro chatter.")]
    [SerializeField] private float slipDead = 0.4f;

    private float leftInput;
    private float rightInput;

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

    public void SetInputs(float left, float right)
    {
        leftInput = left;
        rightInput = right;
    }

    private void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        Vector3 leftBase = transform.TransformPoint(new Vector3(-trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 rightBase = transform.TransformPoint(new Vector3(trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 along = transform.forward;
        Vector3 castDir = -transform.up;

        bool lHit = Physics.CapsuleCast(leftBase - along * contactCapsuleHalfLength, leftBase + along * contactCapsuleHalfLength, contactRadius, castDir, out RaycastHit lInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);

        bool rHit = Physics.CapsuleCast(rightBase - along * contactCapsuleHalfLength, rightBase + along * contactCapsuleHalfLength, contactRadius, castDir, out RaycastHit rInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);

        if (lHit) CheckTrackSurface(lInfo);
        if (rHit) CheckTrackSurface(rInfo);

        float g = Physics.gravity.magnitude;
        float normalPerTrack = (rb.mass * g) * 0.5f;

        // No smoothing/holding: only apply when we actually have a hit this frame
        if (lHit) ApplyTrackForces(lInfo.point, lInfo.normal, leftInput, rightInput, normalPerTrack, true);
        if (rHit) ApplyTrackForces(rInfo.point, rInfo.normal, rightInput, leftInput, normalPerTrack, false);
    }

    private void ApplyTrackForces(
        Vector3 contactPoint, Vector3 contactNormal,
        float input, float otherInput, float normalForce, bool isLeft)
    {
        // Ground-tied basis (no axis smoothing)
        Vector3 lateral = Vector3.ProjectOnPlane(transform.right, contactNormal);
        if (lateral.sqrMagnitude < 1e-6f) lateral = Vector3.Cross(contactNormal, transform.forward);
        lateral = lateral.normalized;
        Vector3 forward = Vector3.Cross(lateral, contactNormal).normalized;

        // Velocities at contact, projected to the ground plane
        Vector3 vPt = rb.GetPointVelocity(contactPoint);
        Vector3 vGround = Vector3.ProjectOnPlane(vPt, contactNormal);

        float vFwd = Vector3.Dot(vGround, forward);
        Vector3 vSlipVec = vGround - forward * vFwd;       // lateral slip vector
        float vSlipMag = vSlipVec.magnitude;             // no smoothing

        // Traction limits
        float tractionLong = currData.muLong * normalForce;
        float tractionLat = currData.muLat * normalForce;

        // Engine/drive
        float vForPower = Mathf.Max(0.5f, Mathf.Abs(vFwd));
        float engineForceCap = Mathf.Min(currData.maxForcePerTrack, currData.EnginePowerW / vForPower);
        bool neutralSteer = (input * otherInput) < -0.2f;
        if (neutralSteer) engineForceCap *= currData.neutralSteerTorqueBoost;

        float drive = Mathf.Clamp(input * engineForceCap, -tractionLong, tractionLong);

        // Lateral force (dead-zone, no smoothing)
        Vector3 latF = Vector3.zero;
        if (vSlipMag > slipDead)
        {
            Vector3 slipDir = vSlipVec / (vSlipMag + 1e-6f);
            float s = vSlipMag - slipDead; // continuous dead-zone
            float desired = currData.lateralStiffness * s;

            // FIX: cap against muLat * N (your file used muLat / N by mistake)
            float capped = Mathf.Min(Mathf.Abs(desired), tractionLat);
            latF = -slipDir * capped;
        }

        // Rolling + speed resistance along forward
        float resistMag = currData.rollingResistance * normalForce + currData.speedDrag * vFwd;
        Vector3 resist = -forward * resistMag;

        // Front/rear lateral damping (no smoothing)
        Vector3 pFront = contactPoint + forward * currData.contactHalfLength;
        Vector3 pRear = contactPoint - forward * currData.contactHalfLength;

        Vector3 vFront = Vector3.ProjectOnPlane(rb.GetPointVelocity(pFront), contactNormal);
        Vector3 vRear = Vector3.ProjectOnPlane(rb.GetPointVelocity(pRear), contactNormal);

        float vLatFront = Vector3.Dot(vFront, lateral);
        float vLatRear = Vector3.Dot(vRear, lateral);

        float Ffront = -(currData.linearLatDrag * vLatFront + currData.quadLatDrag * vLatFront * Mathf.Abs(vLatFront));
        float Frear = -(currData.linearLatDrag * vLatRear + currData.quadLatDrag * vLatRear * Mathf.Abs(vLatRear));

        // Keep within lateral budget after main latF
        float latBudget = Mathf.Max(0f, tractionLat - latF.magnitude);
        float sumAbs = Mathf.Abs(Ffront) + Mathf.Abs(Frear);
        if (sumAbs > latBudget && sumAbs > 1e-3f)
        {
            float scale = latBudget / sumAbs;
            Ffront *= scale;
            Frear *= scale;
        }

        // Debug rays
        Debug.DrawRay(contactPoint, forward * (drive * 0.0005f), isLeft ? Color.green : Color.cyan, Time.fixedDeltaTime, false);
        Debug.DrawRay(contactPoint, resist * 0.0005f, Color.red, Time.fixedDeltaTime, false);
        Debug.DrawRay(contactPoint, latF * 0.05f, Color.magenta, Time.fixedDeltaTime, false);
        Debug.DrawRay(contactPoint, vSlipVec * 0.02f, Color.yellow, Time.fixedDeltaTime, false);

        // Apply forces (no contact scaling / smoothing)
        rb.AddForceAtPosition(forward * drive, contactPoint, ForceMode.Force);
        rb.AddForceAtPosition(resist, contactPoint, ForceMode.Force);
        rb.AddForceAtPosition(latF, contactPoint, ForceMode.Force);
        rb.AddForceAtPosition(lateral * Ffront, pFront, ForceMode.Force);
        rb.AddForceAtPosition(lateral * Frear, pRear, ForceMode.Force);
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
