using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum TankPart
{
    TANK_BODY,
    TANK_CUPOLA
}

public class TankCustomizationObject : MonoBehaviour
{
    [System.Serializable]
    public class TankPartData
    {
        [SerializeField] public TankPart part;
        [SerializeField] public MeshFilter meshFilter;
        [SerializeField] public Mesh[] meshParts;
    }

    [SerializeField] private TankPartData[] tankPartDatas;
    public void ChangeTankPart(TankPart tankPart)
    {
        TankPartData tankPartData = GetTankPartData(tankPart);
        if (tankPartData == null || tankPartData.meshParts.Length == 0)
        {
            Debug.LogWarning($"No mesh parts found for {tankPart}");
            return;
        }

        int meshIndex = System.Array.IndexOf(tankPartData.meshParts, tankPartData.meshFilter.sharedMesh);

        // Fallback if not found
        if (meshIndex == -1)
        {
            meshIndex = 0;
        }

        int nextIndex = (meshIndex + 1) % tankPartData.meshParts.Length;
        tankPartData.meshFilter.mesh = tankPartData.meshParts[nextIndex];

        Debug.Log($"{tankPart} changed to index {nextIndex}");
    }

    private TankPartData GetTankPartData(TankPart tankPart)
    {
        foreach (TankPartData data in tankPartDatas)
        {
            if (data.part == tankPart)
                return data;
        }
        return null;
    }
}
