using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public Player driver1, driver2, gunner1, gunner2;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void AssingDriver1(Player player)
    {
        driver1 = player;
    }
    public void AssingDriver2(Player player)
    {
        driver2 = player;
    }
    public void AssingGunner1(Player player)
    {
        gunner1 = player;
    }
    public void AssingGunner2(Player player)
    {
        gunner2 = player;
    }
}
