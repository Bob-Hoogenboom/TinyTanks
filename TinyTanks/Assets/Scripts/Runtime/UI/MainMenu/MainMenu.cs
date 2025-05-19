using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Failsafe for cursor locking
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnGUI()
    {
        string versionText = Application.version.ToString();

        float labelWidth = 150;
        float labelHeight = 60;
        int fontSize = 32;

        GUI.skin.label.fontSize = fontSize;
        GUI.Label(new Rect(Screen.width - labelWidth, Screen.height - labelHeight, labelWidth, labelHeight), "'" + versionText + "'");
    }

    public void StartGame(int goToScene)
    {
        SceneManager.LoadScene(goToScene);
    }


    public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
}
