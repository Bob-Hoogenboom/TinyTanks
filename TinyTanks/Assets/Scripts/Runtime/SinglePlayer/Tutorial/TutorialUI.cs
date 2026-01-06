using UnityEngine;


public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject driverPanel;
    [SerializeField] private GameObject observerPanel;


    private void OnEnable()
    {
        SinglePlayerTank.OnUpdateRole += UpdateUI;
    }

    private void OnDisable()
    {
        SinglePlayerTank.OnUpdateRole -= UpdateUI;
    }


    private void UpdateUI(TankRole role)
    {
        if(role == TankRole.TANK_DRIVER)
        {
            driverPanel.SetActive(true);
            observerPanel.SetActive(false);
        }
        else if(role == TankRole.TANK_OBSERVER)
        {
            driverPanel.SetActive(false);
            observerPanel.SetActive(true);
        }
        else
        {
            driverPanel.SetActive(false);
            observerPanel.SetActive(false);
        }
    }
}
