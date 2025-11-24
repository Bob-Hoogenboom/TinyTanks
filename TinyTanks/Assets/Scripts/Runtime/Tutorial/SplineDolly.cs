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
    [SerializeField] private GameObject splineIndicator;
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


    private float _dist;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        currentSpeed = dollySpeed;
    }

    private void Update()
    {
        spline.Project(transform.position, ref _sample);
        _dist = Vector3.Distance(transform.position, _sample.position);
        bool active = (_dist < snapDistance && _sample.percent < 0.01f);

        if (!isOnSpline)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                TrySnapToSpline();
            }
        }
        else
        {
            RideSpline();
            active = false;
        }


        splineIndicator.SetActive(active);

    }

    private void TrySnapToSpline()
    {
        if (spline == null) return;

        // Re-project position
        spline.Project(transform.position, ref _sample);

        float dist = Vector3.Distance(transform.position, _sample.position);

        // Check distance
        if (dist >= snapDistance) return;

        // Check position on spline (near start)
        if (_sample.percent > 0.01f)   // Allow first 1% of spline
        {
            Debug.Log("Too far from spline start!");
            return;
        }

        // Snap onto spline
        isOnSpline = true;
        _rb.isKinematic = true;
        _splinePercent = 0.0;  // FORCE them to start at EXACT start
        spline.Evaluate(0.0, ref _sample);

        transform.position = _sample.position;
        transform.rotation = Quaternion.LookRotation(_sample.forward);
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

        if (spline == null) return;

        Gizmos.color = Color.green;

        // we sample from 0% to 1%
        const float minPercent = 0f;
        const float maxPercent = 0.01f;

        const int steps = 10; // more = smoother
        SplineSample s = new SplineSample();

        for (int i = 0; i < steps; i++)
        {
            float t = Mathf.Lerp(minPercent, maxPercent, i / (float)(steps - 1));

            spline.Evaluate(t, ref s);

            // Draw a small cube
            Gizmos.DrawSphere(s.position, 0.1f);
        }
    }
}
