using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Very Simple SceneLloader with an animation attached to it
/// </summary>
public class LandingMenu : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private float transitionTime;
    private int _garageDoorHash = Animator.StringToHash("CloseDoor");


    [Tooltip("Fill in the scene you want to transition to")]
    public void LoadScene(int scene)
    {
        StartCoroutine(TransitionToScene(scene));
    }

    IEnumerator TransitionToScene(int scene)
    {
        anim.SetTrigger(_garageDoorHash);
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scene);
    }
}