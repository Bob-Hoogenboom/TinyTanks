using System;
using UnityEngine;

[Serializable]
public class TankMaterial 
{
    public string name = "Default";
    public Material material;
    public Color color = new Color(0, 0, 255, 100); // blue is standard
}
