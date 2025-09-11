using UnityEngine;
using Mirror;

public class TankTrackPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("RayCast")]
    [SerializeField] private float trackSpacing = 2.6f;
    [SerializeField] private float trackRayStartHeight = 0.6f;
    [SerializeField] private float trackRayLength = 1.2f;

    //[Header("Engine")]
    //[SerializeField] private float enginePowerHP = 600f;
    //[SerializeField] private float maxForcePerTrack = 80000f;

    //[Header("Grip & Resistance")]
    //[SerializeField] private float muLong = 0.85f;
    //[SerializeField] private float muLat = 1.6f;
    //[SerializeField] private float rollingResistance = 0.02f;
    //[SerializeField] private float speedDrag = 120f;

    //[SerializeField] private float contactHalfLength = 2.5f;   // half the track-ground footprint length (m)
    //[SerializeField] private float linearLatDrag = 400; // N·s/m
    //[SerializeField] private float quadLatDrag = 50;  // N·s²/m²

    [Header("Tuning")]
    [SerializeField] private float extraDownForce = 0f;
    [SerializeField] private float yawDamping = 0.2f;
    //[SerializeField] private float lateralStiffness = 20000f;

    //[Header("Braking & Steer Assist")]
    //[SerializeField] private float trackBrakeForce = 180000f;
    //[SerializeField] private float brakeDeadzone = 0.15f;
    //[SerializeField] private float steerBrakeMultiplier = 1.3f;
    //[SerializeField] private float neutralSteerTorqueBoost = 1.5f;
    [Header("Movemen Variables")]
    [SerializeField] private TankData hardFloorData;
    [SerializeField] private TankData carpetData;
    [SerializeField] private TankData wetFloorData;
    [SerializeField] private TankData currData;

    private float leftInput;
    private float rightInput;

    //private float EnginePowerW => enginePowerHP * 735.5f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        currData = hardFloorData;
    }

    public void SetInputs(float left, float right)
    {
        leftInput = left;
        rightInput = right;
    }

    private void FixedUpdate() // TO DO use SO to change variables to represent different surfaces -> use raycast to see what groundType it is.
    {       
        if (!NetworkServer.active) return;
        Vector3 lOrigin = transform.TransformPoint(new Vector3(-trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 rOrigin = transform.TransformPoint(new Vector3(trackSpacing * 0.5f, trackRayStartHeight, 0f));

        bool lHit = Physics.Raycast(lOrigin, Vector3.down, out RaycastHit lInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);
        bool rHit = Physics.Raycast(rOrigin, Vector3.down, out RaycastHit rInfo, trackRayLength, groundMask, QueryTriggerInteraction.Ignore);

        float g = Physics.gravity.magnitude;
        float normalPerTrack = (rb.mass * g) * 0.5f;

        if (extraDownForce > 0) rb.AddForce(-Physics.gravity.normalized * extraDownForce, ForceMode.Force);

        CheckTrackSurface(lInfo);
        if (lHit) ApplyTrackForces(lInfo, leftInput, rightInput, normalPerTrack);
        CheckTrackSurface(rInfo);
        if (rHit) ApplyTrackForces(rInfo, rightInput, leftInput, normalPerTrack);

        // yaw damping
        var av = rb.angularVelocity;
        av.y *= (1f - Mathf.Clamp01(yawDamping) * Time.fixedDeltaTime);
        rb.angularVelocity = av;
    }

    private void ApplyTrackForces(in RaycastHit hit, float input, float otherInput, float normalForce)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        Vector3 lateral = Vector3.Cross(hit.normal, forward).normalized;

        Vector3 vPt = rb.GetPointVelocity(hit.point);
        float vFwd = Vector3.Dot(vPt, forward);
        float vLat = Vector3.Dot(vPt, lateral);

        // Traction limits
        float tractionLong = currData.muLong * normalForce;
        float tractionLat = currData.muLat * normalForce;

        // Drive force
        float vForPower = Mathf.Max(0.5f, Mathf.Abs(vFwd));
        float engineForceCap = Mathf.Min(currData.maxForcePerTrack, currData.EnginePowerW / vForPower);

        bool neutralSteer = (input * otherInput) < -0.2f;
        if (neutralSteer) engineForceCap *= currData.neutralSteerTorqueBoost;

        float drive = Mathf.Clamp(input * engineForceCap, -tractionLong, tractionLong);

        // Lateral grip
        float latMag = Mathf.Clamp(-vLat * currData.lateralStiffness, -tractionLat, tractionLat);
        Vector3 latF = lateral * latMag;

        // Drag along forward
        float resistMag = currData.rollingResistance * normalForce + currData.speedDrag * vFwd;
        Vector3 resist = -forward * resistMag;

        //Measure front and rear point for turning
        Vector3 pFront = hit.point + forward * currData.contactHalfLength;
        Vector3 pRear = hit.point - forward * currData.contactHalfLength;

        Vector3 vFront = rb.GetPointVelocity(pFront);
        Vector3 vRear = rb.GetPointVelocity(pRear);

        float vLatFront = Vector3.Dot(vFront, lateral);
        float vLatRear = Vector3.Dot(vRear, lateral);

        float Ffront = -(currData.linearLatDrag * vLatFront + currData.quadLatDrag * vLatFront * Mathf.Abs(vLatFront));
        float Frear = -(currData.linearLatDrag * vLatRear + currData.quadLatDrag * vLatRear * Mathf.Abs(vLatRear));

        // Clutch brake
        bool released = Mathf.Abs(input) < currData.brakeDeadzone;
        bool commandOpposesMotion = (Mathf.Abs(input) > 0.2f) && (Mathf.Sign(input) != Mathf.Sign(vFwd));
        float brakeCap = Mathf.Min(currData.trackBrakeForce, tractionLong);

        if (released || commandOpposesMotion)
        {
            // More brake if the other track is being driven -> pivot
            float steerMul = (Mathf.Abs(otherInput) > 0.2f) ? currData.steerBrakeMultiplier : 1f;

            if (Mathf.Abs(vFwd) > 0.01f)
                rb.AddForceAtPosition(-Mathf.Sign(vFwd) * forward * (brakeCap * steerMul), hit.point, ForceMode.Force);
        }

        rb.AddForceAtPosition(forward * drive, hit.point, ForceMode.Force);
        rb.AddForceAtPosition(resist, hit.point, ForceMode.Force);
        rb.AddForceAtPosition(latF, hit.point, ForceMode.Force);
        rb.AddForceAtPosition(lateral * Ffront, pFront, ForceMode.Force);
        rb.AddForceAtPosition(lateral * Frear, pRear, ForceMode.Force);
    }

    private void CheckTrackSurface(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject == null) return;
        int layer = hitInfo.collider.gameObject.layer;

        switch (layer)
        {
            case 10:
                currData = hardFloorData;
                break;
            case 11:
                currData = carpetData;
                break;
            case 12:
                currData = wetFloorData;
                break;
            default:
                currData = hardFloorData;
                break;
        }
    }
}
