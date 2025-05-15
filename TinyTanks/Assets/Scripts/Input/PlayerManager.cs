using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public int index;


    public void OnPlayerJoined()
    {
        index++;
    }
}
