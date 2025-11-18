using UnityEngine;
using Dreamteck.Splines;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// put this component on an object thats able to ride a spline
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SplineDolly : MonoBehaviour
{
    [Header("Refernces")]
    public SplineComputer spline;

    private Rigidbody _rb;
    private SplineSample _sample = new SplineSample();

    [Header("Variables")]
    public bool loopSpline = false;
    public bool isNPC = false;
    public float snapDistance = 1f;

    public float dollySpeed = 1f;
    [HideInInspector] public float currentSpeed;

    public bool isOnSpline { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    private double _splinePercent = 0.0;

    [Header("Effects and Actions")]
    public UnityEvent onTrackEnd;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        currentSpeed = dollySpeed;
    }

    private void Update()
    {
        if (!isOnSpline)
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

        spline.Project(transform.position, ref _sample);
        float dist = Vector3.Distance(transform.position, _sample.position);
        if (dist < snapDistance)
        {
            isOnSpline = true;
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

        //This checks triggers without the use of a splineFollower/splineUser
        if (!isNPC)
        {
            spline.CheckTriggers(oldPercent, _splinePercent);
        }

        //Evaluate and apply position/rotation
        spline.Evaluate(_splinePercent, ref _sample);
        transform.position = _sample.position;
        transform.rotation = Quaternion.LookRotation(_sample.forward);

        if (!loopSpline && _splinePercent >= 1.0)
        {
            DetachFromSpline();
        }
    }

    private void DetachFromSpline()
    {
        isOnSpline = false;
        _rb.isKinematic = false;
        _rb.velocity = transform.forward * currentSpeed * 0.5f;

        onTrackEnd.Invoke();
    }

    // Pause immediately for a fixed duration, then resume
    public void PauseForSeconds(float duration)
    {
        if (!isPaused)
            StartCoroutine(PauseCoroutine(duration));
    }

    private IEnumerator PauseCoroutine(float duration)
    {
        isPaused = true;
        float pausedSpeed = currentSpeed;
        currentSpeed = 0f;

        yield return new WaitForSeconds(duration);

        currentSpeed = pausedSpeed;
        isPaused = false;
    }

    // Pause until player presses Space to continue
    public void PauseUntilInput(KeyCode key = KeyCode.Space)
    {
        if (!isPaused)
            StartCoroutine(PauseUntilInputCoroutine(key));
    }

    private IEnumerator PauseUntilInputCoroutine(KeyCode key)
    {
        isPaused = true;
        float pausedSpeed = currentSpeed;
        currentSpeed = 0f;

        // Wait for input
        while (!Input.GetKeyDown(key))
        {
            yield return null;
        }

        currentSpeed = pausedSpeed;
        isPaused = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
