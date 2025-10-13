using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneSwitch : MonoBehaviour
{
    public void LoadNewScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
