using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    [Header("Health & lives")]
    [SerializeField] public int maxHitPoints = 3;
    [SerializeField] private int lives = 3;

    [Header("Respawn settings")]
    [SerializeField] private float minSpawnDistance = 10f;
    [SerializeField] private float respawnDelay = 3f;

    [Header("UI")]
    [SerializeField] private GameObject driverRespawnTimer;
    [SerializeField] private GameObject gunnerRespawnTimer;
    [SerializeField] private TMP_Text driverRespawnTimerText;
    [SerializeField] private TMP_Text gunnerRespawnTimerText;

    private int currentHitpoints;

    private List<GameObject> spawnPoints = new();

    private PlayerDriver driver;
    private PlayerGunner gunner;

    private void Start()
    {
        currentHitpoints = maxHitPoints;

        driver = GetComponent<PlayerDriver>();
        gunner = GetComponentInChildren<PlayerGunner>();
        spawnPoints.AddRange(GameObject.FindGameObjectsWithTag("SpawnPoint"));
    }

    public void TakeDamage(int amount)
    {
        currentHitpoints -= amount;
        if (currentHitpoints <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        lives--;
        if (lives > 0)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            // GameOver screen or something
            HandleGameOver();
        }
    }

    private void HandleGameOver()
    {
        //Disable controls
        driver.enabled = false;
        gunner.enabled = false;

        // sent message to GameHandler that game is over
    }

    private IEnumerator RespawnCoroutine()
    {
        TimedWait waitTimer = new TimedWait(respawnDelay);

        driver.enabled = false;
        gunner.enabled = false;
        driverRespawnTimer.SetActive(true);
        gunnerRespawnTimer.SetActive(true);

        Debug.Log($"Player:{driver.gameObject.name} is respawning");

        while(waitTimer.keepWaiting)
        {
            // desiplay wait UI
            gunnerRespawnTimerText.text = $"{(int)waitTimer.RemainingTime}";
            driverRespawnTimerText.text = $"{(int)waitTimer.RemainingTime}";
            yield return null;
        }

        driverRespawnTimer.SetActive(false);
        gunnerRespawnTimer.SetActive(false);

        Respawn();
    }

    private void Respawn()
    {
        currentHitpoints = maxHitPoints;

        Transform spawnTransform = GetSpawnLocation();
        if (spawnTransform != null)
        {
            TeleportToSpawn(spawnTransform);

            driver.enabled = true;
            gunner.enabled = true;
        }
        else
        {
            Debug.LogError("Failed to find valid spawn point for respawn");
        }
    }

    private Transform GetSpawnLocation()
    {
        List<Transform> spawnLocations = spawnPoints.Select(sp => sp.transform).ToList();
        List<Transform> enemyTransforms = GetEnemyTranform();

        List<Transform> safeSpawnpoints = spawnLocations
            .Where(spawn => !enemyTransforms
            .Any(enemy => Vector3.Distance(spawn.position, enemy.position) < minSpawnDistance)).ToList();

        List<Transform> chosenList = safeSpawnpoints.Count > 0 ? safeSpawnpoints : spawnLocations;
        return chosenList[Random.Range(0, chosenList.Count)];
    }

    private List<Transform> GetEnemyTranform()
    {
        return FindObjectsOfType<PlayerDriver>()
            .Where(player => player.gameObject != this.gameObject == true)
            .Select(player => player.transform)
            .ToList();
    }

    private void TeleportToSpawn(Transform spawnLocation)
    {
        transform.position = spawnLocation.position;
        transform.rotation = spawnLocation.rotation;

        // Remove any velociy player has when spawned
        Rigidbody _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

}
