using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class menuHandler : MonoBehaviour
{
    [Tooltip("Drag your menu panels here in order.")]
    public List<GameObject> menus = new List<GameObject>();

    [Tooltip("Drag the underline Image (or any GameObject) under each corresponding button here.")]
    public List<GameObject> underlines = new List<GameObject>();

    public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    /// <summary>
    /// Show only menus[index], hide the rest, 
    /// and toggle underlines so only the chosen one is visible.
    /// </summary>
    public void SwitchToMenu(int index)
    {

        for (int i = 0; i < menus.Count; i++)
        {
            bool active = (i == index);

            menus[i].SetActive(active);
            underlines[i]?.SetActive(active);
            cameras[i].enabled = active;
        }
    }

}