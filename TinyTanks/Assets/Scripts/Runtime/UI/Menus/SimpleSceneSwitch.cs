using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneSwitch : MonoBehaviour
{
    [Header("SceneTransition")]
    [SerializeField] private Animator anime;
    [SerializeField] private float transitionTime = 1f;

    private int _fadeHash = Animator.StringToHash("FadeOut");


    public void LoadNewScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void FadeToNewScene(int scene)
    {
        StartCoroutine(Fade(scene));
    }

    private IEnumerator Fade(int scene)
    {
        anime.SetTrigger(_fadeHash);
        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(scene);
    }



    public void QuitGame()
    {
        Application.Quit();
    }
}
