using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankCustomizationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankCustomizationObject tankOBJ;
    [Space]
    [SerializeField] private Button tankBodyButton;

    private void Awake()
    {
        tankBodyButton.onClick.AddListener(() =>
        {
            Debug.Log("TankBodyChanged");
            tankOBJ.ChangeBody();
        });
    }
}
