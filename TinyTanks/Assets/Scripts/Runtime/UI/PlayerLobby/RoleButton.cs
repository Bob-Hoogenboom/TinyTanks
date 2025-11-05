using UnityEngine;
using UnityEngine.Events;

public class RoleButton : MonoBehaviour
{
    public UnityEvent onClick;
    public bool interactable = true;

    private void OnMouseDown()
    {
        if (interactable) 
        { 
            onClick.Invoke();
        }
    }
}
