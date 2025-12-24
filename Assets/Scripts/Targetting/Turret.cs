using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    private Transform target;
    private Enemy targetEnemy;

    [Header("General")]
    public float range = 15f;

    [Header("Use Bullets (default)")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Use Laser")]
    public bool useLaser = false;
    public int damageOverTime = 30;
    public float slowAmount = .5f;
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public Light impactLight;

    [Header("Unity Setup Fields")]
    public string enemyTag = "Enemy";
    public Transform partToRotate;
    public float turnSpeed = 10f;
    public Transform firePoint;
    public AudioClip shootSound;

    public bool isPlaced = false;

    // NEW: Store blueprint reference for selling
    [HideInInspector]
    public TurretBlueprint blueprint;

    // NEW: Sell refund percentage
    [Header("Economy")]
    public float sellPercentage = 0.75f; // Get 75% back when selling

    void Start()
    {   
        gameObject.layer = LayerMask.NameToLayer("Turret");
        if (isPlaced)
        {
            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }
    }

    public void PlaceTurret()
    {
        isPlaced = true;
        InvokeRepeating("UpdateTarget", 0f, 0.5f);

        // NEW: Re-enable collider after placement
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    // NEW: Sell turret method
    public void SellTurret()
    {
        // Calculate refund amount based on blueprint cost
        if (blueprint != null)
        {
            int refundAmount = Mathf.RoundToInt(blueprint.cost * sellPercentage);
            PlayerStats.Money += refundAmount;
            Debug.Log($"Turret sold! Refunded: {refundAmount}");
        }

        // Remove from occupied plates
        TowerPlacement placement = FindObjectOfType<TowerPlacement>();
        if (placement != null)
        {
            placement.RemoveTurret(gameObject);
        }

        // Destroy the turret
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
            targetEnemy = nearestEnemy.GetComponent<Enemy>();

            if (targetEnemy == null)
            {
                targetEnemy = nearestEnemy.GetComponentInParent<Enemy>();
            }

            if (targetEnemy == null)
            {
                Debug.LogError($"Target {nearestEnemy.name} (parent: {nearestEnemy.transform.parent?.name}) has Enemy tag but no Enemy component found in object or parent!");
                target = null;
            }
        }
        else
        {
            target = null;
            targetEnemy = null;
        }
    }

    void Update()
    {
        if (!isPlaced) return;

        if (target == null)
        {
            if (useLaser)
            {
                if (lineRenderer != null && lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                    if (impactEffect != null) impactEffect.Stop();
                    if (impactLight != null) impactLight.enabled = false;
                }
            }
            return;
        }

        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            target = null;
            targetEnemy = null;
            if (useLaser && lineRenderer != null && lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                if (impactEffect != null) impactEffect.Stop();
                if (impactLight != null) impactLight.enabled = false;
            }
            return;
        }

        LockOnTarget();

        if (useLaser)
        {
            Laser();
        }
        else
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;
        }
    }

    void LockOnTarget()
    {
        if (partToRotate == null || target == null) return;

        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Laser()
    {
        if (targetEnemy == null || target == null)
        {
            Debug.LogWarning("Laser called but targetEnemy or target is null!");
            return;
        }

        targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
        targetEnemy.Slow(slowAmount);

        if (lineRenderer != null && !lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            if (impactEffect != null) impactEffect.Play();
            if (impactLight != null) impactLight.enabled = true;
        }

        if (lineRenderer != null && firePoint != null && target != null)
        {
            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, target.position);
        }

        if (impactEffect != null && firePoint != null && target != null)
        {
            Vector3 dir = firePoint.position - target.position;
            impactEffect.transform.position = target.position + dir.normalized;
            impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Seek(target);

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