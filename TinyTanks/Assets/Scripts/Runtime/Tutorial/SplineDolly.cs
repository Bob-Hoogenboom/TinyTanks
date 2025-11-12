using UnityEngine;
using Dreamteck.Splines;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SplineDolly : MonoBehaviour
{
    public SplineComputer spline;
    public SplineTracer tracer;
    public float snapDistance = 3f;

    public float dollySpeed = 1f;
    [HideInInspector] public float currentSpeed;

    public bool loopSpline = false;

    private Rigidbody _rb;
    private SplineSample _sample = new SplineSample();
    private bool _isOnSpline = false;
    private double _splinePercent = 0.0;

    private bool _isPaused = false;         // Flag for pause

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        tracer.spline = spline;

        currentSpeed = dollySpeed;
    }

    private void Update()
    {
        if (!_isOnSpline)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                TrySnapToSpline();
        }
        else
        {
            RideSpline();
        }
    }

    private void TrySnapToSpline()
    {
        if (spline == null) return;

        spline.Project(transform.position, ref _sample);
        float dist = Vector3.Distance(transform.position, _sample.position);
        if (dist < snapDistance)
        {
            _isOnSpline = true;
            _rb.isKinematic = true;
            _splinePercent = _sample.percent;
            transform.position = _sample.position;
            transform.rotation = Quaternion.LookRotation(_sample.forward);
        }
    }

    private void RideSpline()
    {
        if (spline == null) return;

        double oldPercent = _splinePercent;

        _splinePercent += (currentSpeed * Time.deltaTime) / spline.CalculateLength();

        if (loopSpline)
        {
            _splinePercent %= 1.0;
        }
        else
        {
            _splinePercent = Mathf.Clamp01((float)_splinePercent);
        }

        // Tell the spline computer to check triggers between old and new percent
        spline.CheckTriggers(oldPercent, _splinePercent, tracer);

        // Evaluate and apply position/rotation
        spline.Evaluate(_splinePercent, ref _sample);
        transform.position = _sample.position;
        transform.rotation = Quaternion.LookRotation(_sample.forward);

        if (!loopSpline && _splinePercent >= 1.0)
            DetachFromSpline();
    }

    private void DetachFromSpline()
    {
        _isOnSpline = false;
        _rb.isKinematic = false;
        _rb.velocity = transform.forward * currentSpeed * 0.5f;
    }

    // ----------- Pause / Resume Methods -----------

    // Pause immediately for a fixed duration, then resume
    public void PauseForSeconds(float duration)
    {
        if (!_isPaused)
            StartCoroutine(PauseCoroutine(duration));
    }

    private IEnumerator PauseCoroutine(float duration)
    {
        _isPaused = true;
        float pausedSpeed = currentSpeed;
        currentSpeed = 0f;

        yield return new WaitForSeconds(duration);

        currentSpeed = pausedSpeed;
        _isPaused = false;
    }

    // Pause until player presses Space to continue
    public void PauseUntilInput(KeyCode key = KeyCode.Space)
    {
        if (!_isPaused)
            StartCoroutine(PauseUntilInputCoroutine(key));
    }

    private IEnumerator PauseUntilInputCoroutine(KeyCode key)
    {
        _isPaused = true;
        float pausedSpeed = currentSpeed;
        currentSpeed = 0f;

        // Wait for input
        while (!Input.GetKeyDown(key))
        {
            yield return null;
        }

        currentSpeed = pausedSpeed;
        _isPaused = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
