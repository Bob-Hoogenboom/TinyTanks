using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Tank Variables")]
public class TankData : ScriptableObject
{
    [Header("Health & Damage")]
    public int damage;
    public int maxHealth;

    [Header("Shooting")]
    public float baseReloadTime;
    public float noBatteryReloadTime;
    public float shellSpeed;

    [Header("Battery")]
    public float maxBtry = 100f;
    public float batteryDrainMove = 0.5f;
    public float batteryDrainTurning = 0.3f;
    public float batteryDrainNeutralSteer = 0.2f;
    public float batteryDrainShot = 6f;
}
