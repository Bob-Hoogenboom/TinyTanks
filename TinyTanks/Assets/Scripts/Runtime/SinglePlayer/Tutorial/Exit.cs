using UnityEngine;
using UnityEngine.Events;

public class Exit : MonoBehaviour
{
    public UnityEvent onExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            onExit.Invoke();
        }
    }
}
