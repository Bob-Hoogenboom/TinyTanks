using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuTankData", menuName = "Tank/MenuTankData")]
public class MenuTankData : ScriptableObject
{
    public string tankName;

    [Header("Prefabs")]
    public List<GameObject> bodyOptions;
    public List<GameObject> cupolaOptions;

    [Header("Materials")]
    public List<Material> availableMaterials;
}
