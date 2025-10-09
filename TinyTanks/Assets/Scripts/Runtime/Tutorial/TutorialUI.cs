using UnityEngine;


public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject driverPanel;
    [SerializeField] private GameObject observerPanel;


    private void OnEnable()
    {
        TutorialTank.OnUpdateRole += UpdateUI;
    }

    private void OnDisable()
    {
        TutorialTank.OnUpdateRole -= UpdateUI;
    }


    private void UpdateUI(TankRole role)
    {
        if(role == TankRole.TANK_DRIVER)
        {
            driverPanel.SetActive(true);
            observerPanel.SetActive(false);
        }
        else
        {
            driverPanel.SetActive(false);
            observerPanel.SetActive(true);
        }
    }
}
