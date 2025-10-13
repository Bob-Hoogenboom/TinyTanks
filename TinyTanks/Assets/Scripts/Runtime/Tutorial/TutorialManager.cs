using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialTitle;
    [SerializeField] private TMP_Text tutorialText;

    private void Awake()
    {
        Instance = this;
        tutorialPanel.SetActive(false);
    }

    public void ShowMessage(string message, string title)
    {
        tutorialTitle.text = title;
        tutorialText.text = message;
        tutorialPanel.SetActive(true);
    }

    public void HideMessage()
    {
        tutorialPanel.SetActive(false);
    }
}
