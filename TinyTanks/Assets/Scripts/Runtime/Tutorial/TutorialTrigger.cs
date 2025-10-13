using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))] // Use Collider if 3D
public class TutorialTrigger : MonoBehaviour
{
    [TextArea(1,1)]
    [SerializeField] private string title;
    [TextArea(3,10)] 
    [SerializeField] private string message;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.ShowMessage(message, title);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.HideMessage();
        }
    }
}