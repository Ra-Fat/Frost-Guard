using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public float speed = 70f;
    public float damage = 10f;
    public float explosionRadius = 0f;
    public GameObject impactEffect;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        GameObject effectIns = (GameObject)Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 5f);

        if (explosionRadius > 0f)
        {
            Explode();
        }
        else
        {
            Damage(target);
        }

        Destroy(gameObject);
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Damage(collider.transform);
            }
        }
    }

    void Damage(Transform enemyTransform)
    {
        Enemy enemy = enemyTransform.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyTransform.GetComponentInParent<Enemy>();
        }
        if (enemy != null && !enemy.isDead)
        {
            Debug.Log("Bullet dealing damage: " + damage + " to enemy");
            enemy.Health -= damage;
            if (enemy.Health <= 0)
            {
                enemy.isDead = true;
                PlayerStats.Money += enemy.MoneyReward;
                GameLoopManager.EnqueueEnemyToRemove(enemy);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}