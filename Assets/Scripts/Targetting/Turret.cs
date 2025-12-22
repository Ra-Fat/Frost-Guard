using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    private Transform target;

    [Header("Attributes")]
    public float range = 15f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Economy")]
    public int cost = 100; // Cost to build
    public float sellPercentage = 0.75f; // Get 75% back when selling

    [Header("Unity Setup Field")]
    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float turnSpeed = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public AudioClip shootSound;

    public bool isPlaced = false;

    void Start()
    {
        if (isPlaced)
        {
            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }
    }

    public void PlaceTurret()
    {
        isPlaced = true;
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    public void SellTurret()
    {
        Debug.Log($"SellTurret called on {gameObject.name}");

        // Calculate refund amount
        int refundAmount = Mathf.RoundToInt(cost * sellPercentage);

        // Add money back (you'll need to implement your money system)
        // Example: PlayerStats.Money += refundAmount;
        Debug.Log($"Turret sold! Refunded: {refundAmount}");

        // Remove from occupied plates
        TowerPlacement placement = FindObjectOfType<TowerPlacement>();
        if (placement != null)
        {
            Debug.Log("Found TowerPlacement, removing turret from occupied list");
            placement.RemoveTurret(gameObject);
        }
        else
        {
            Debug.LogError("TowerPlacement not found!");
        }

        // Destroy the turret
        Debug.Log($"Destroying turret: {gameObject.name}");
        Destroy(gameObject);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    void Update()
    {
        if (!isPlaced) return;

        if (target == null)
            return;

        // Target lock on
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Seek(target);

        // Play gun shot sound
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, firePoint.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}