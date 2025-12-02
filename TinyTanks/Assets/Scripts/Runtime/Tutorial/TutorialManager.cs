using UnityEngine;
using TMPro;
using System.Collections;
using Mirror.SimpleWeb;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public static TutorialManager Instance;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialTitle;
    [SerializeField] private TMP_Text tutorialText;
    [Space]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] textClips;
    [SerializeField] private AudioClip endOfLine;
    [SerializeField] private AudioClip soldierGreeting;

    [Header("Settings")]
    [SerializeField] private float textSpeed;
    private bool _skip = false;
    
    private void Awake()
    {
        Instance = this;
        tutorialPanel.SetActive(false);
    }

    private void Update()
    {
        _skip = Input.GetKey(KeyCode.Space);
    }

    public void ShowMessage(string message, string title)
    {
        tutorialTitle.text = title;
        tutorialPanel.SetActive(true);

        StartCoroutine(TextAnimation(message));
    }

    public void HideMessage()
    {
        tutorialPanel.SetActive(false);
        StopAllCoroutines();
    }

    IEnumerator TextAnimation(string message)
    {
        tutorialText.text = "";

        if (soldierGreeting != null)
        {
            audioSource.PlayOneShot(soldierGreeting);
        }

        int letterCount = 0;
        foreach (char letter in message)
        {
            if(_skip)
            {
                tutorialText.text = message;
                yield break;
            }
            tutorialText.text += letter;

            if (letter == '.' || letter == '!' || letter == '?')
            {
                audioSource.PlayOneShot(endOfLine);
                yield return new WaitForSeconds(textSpeed * 5);
            }

            //letters grouped to play one audio per X amount of letters displayed (ideal is textSpeed = 0.1f and letterCount % 2)
            if (letterCount % 4 == 0)
            {
                PlayTypingSound();
            }
            yield return new WaitForSeconds(textSpeed);
            letterCount++;
        }
    }

    private void PlayTypingSound()
    {
        if (textClips.Length == 0 || audioSource == null) return;

        AudioClip clip = textClips[Random.Range(0, textClips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
