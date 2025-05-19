using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TankCustomizationObject : MonoBehaviour
{
    [System.Serializable]
    public class TankPartData
    {
        [SerializeField] public string partName;
        [SerializeField] public MeshFilter meshFilter;
        [SerializeField] public Mesh[] meshParts;
    }

    [SerializeField] private TankPartData[] tankPartDatas;

    public void ChangeBody()
    {
        int meshIndex = System.Array.IndexOf(tankPartDatas[0].meshParts, tankPartDatas[0].meshFilter.mesh);
        tankPartDatas[0].meshFilter.mesh = tankPartDatas[0].meshParts[(meshIndex + 1) % tankPartDatas.Length];
    }
}
