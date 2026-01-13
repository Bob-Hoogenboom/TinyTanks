using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public List<GameObject> healthIcons;
    public Image healthFill;

    public void MinusHealth(float fillamount)
    {
        healthFill.fillAmount = fillamount;
    }

    public void MinushealthIcons()
    {
        if(healthIcons.Count - 1 <= 0)
        {
            //Dood

        }
        else
        {
            //niet dood je hebt nog een leven
            healthFill.fillAmount = 1;
        }

        for (int i = 0; i < healthIcons.Count; i++)
        {
            if(healthIcons.Count -1 == i)
            {
                healthIcons[i].SetActive(false);
                healthIcons.RemoveAt(i);
            }
        }
    }

}
