using UnityEngine;
using Dreamteck.Splines;

public class SplineDolly: MonoBehaviour
{
    [Header("Refernces")]
    public SplineComputer spline;

    private Rigidbody _rb;
    private SplineSample _sample = new SplineSample();
    private bool _isOnSpline = false;
    private double _splinePercent = 0.0;

    [Header("Variables")]
    public float snapDistance = 3f;
    public float dollySpeed = 1f;
    public bool loopSpline = false;

    private float currentSpeed;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!_isOnSpline)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TrySnapToSpline();
            }
        }
        else
        {
            RideSpline();
        }
    }

    private void TrySnapToSpline()
    {
        if (spline == null) return;

        // Project the player onto the spline to find the closest point
        spline.Project(transform.position, ref _sample);
        float dist = Vector3.Distance(transform.position, _sample.position);

        if (dist < snapDistance)
        {
            // Snap to spline
            _isOnSpline = true;
            _rb.isKinematic = true;

            transform.position = _sample.position;
            transform.rotation = Quaternion.LookRotation(_sample.forward);
            _splinePercent = _sample.percent;
        }
    }

    private void RideSpline()
    {
        if (spline == null) return;

        _splinePercent += (dollySpeed * Time.deltaTime) / spline.CalculateLength();

        if (loopSpline)
        {
            _splinePercent %= 1.0;
        }
        else
        {
            _splinePercent = Mathf.Clamp01((float)_splinePercent);
        }

        spline.Evaluate(_splinePercent, ref _sample);
        transform.position = _sample.position;
        transform.rotation = Quaternion.LookRotation(_sample.forward);

        if ((!loopSpline && _splinePercent >= 1.0))
        {
            DetachFromSpline();
        }
    }

    private void DetachFromSpline()
    {
        _isOnSpline = false;
        _rb.isKinematic = false;
        _rb.velocity = _rb.transform.forward * dollySpeed * 0.5f; 
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    public void AddSpeed(float delta)
    {
        currentSpeed = Mathf.Max(0, currentSpeed + delta);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
