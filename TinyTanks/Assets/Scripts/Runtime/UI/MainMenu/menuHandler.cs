using System.Collections.Generic;
using UnityEngine;

public class menuHandler : MonoBehaviour
{
    [Tooltip("Drag your menu panels here in order.")]
    public List<GameObject> menus = new List<GameObject>();

    [Tooltip("Drag the underline Image (or any GameObject) under each corresponding button here.")]
    public List<GameObject> underlines = new List<GameObject>();

    /// <summary>
    /// Show only menus[index], hide the rest, 
    /// and toggle underlines so only the chosen one is visible.
    /// </summary>
    public void SwitchToMenu(int index)
    {
        //LMAO nee, get out-commented *coconut mall plays*
/*        if (menus.Count != underlines.Count)
        {
            Debug.LogError("Menus and Underlines lists must be the same length!");
            return;
        }*/

        for (int i = 0; i < menus.Count; i++)
        {
            bool active = (i == index);
            menus[i].SetActive(active);
            underlines[i]?.SetActive(active);
        }
    }

}