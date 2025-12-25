using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MaxHealth;
    [HideInInspector] public float Health;
    public float Speed;
    public int MoneyReward = 50;
    public int ID;
    public int NodeIndex;
    [HideInInspector] public bool isDead = false;
    public AudioClip deathSound;

    [HideInInspector]
    public float startSpeed; // Store original speed for slow effect

    public void Init()
    {
        Health = MaxHealth;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;
        isDead = false;

        // Ensure Speed is set to a reasonable value at spawn
        if (Speed <= 0.01f)
        {
            Debug.LogWarning($"{gameObject.name} spawned with Speed = {Speed}. Setting default speed of 2.");
            Speed = 2f;
        }
        startSpeed = Speed;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;

        // Sync health with parent if this is a child taking damage
        Enemy parentEnemy = transform.parent?.GetComponent<Enemy>();
        if (parentEnemy != null && parentEnemy != this)
        {
            parentEnemy.Health = Health;
        }

        if (Health <= 0 && !isDead)
        {
            isDead = true;
            Health = 0; // Prevent negative health
            Debug.Log("Enemy dying");
            // Play death sound
            if (deathSound != null)
            {
                Debug.Log("Playing death sound");
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }
            else
            {
                Debug.Log("Death sound is null - please assign a death sound in the inspector");
            }
            Die();
        }

        // Store the original speed when enemy spawns
        if (Speed <= 0)
        {
            Debug.LogWarning($"{gameObject.name} has Speed = 0! Setting default speed of 2");
            Speed = 2f;
        }
        startSpeed = Speed;

        // If this is a parent with child enemies, sync stats to children
        Enemy[] childEnemies = GetComponentsInChildren<Enemy>();
        foreach (Enemy child in childEnemies)
        {
            if (child != this) // Don't sync to self
            {
                child.Speed = Speed;
                child.startSpeed = startSpeed;
                child.MaxHealth = MaxHealth;
                child.Health = Health;
                child.ID = ID;
                child.NodeIndex = NodeIndex;
                Debug.Log($"Synced stats from {gameObject.name} to child {child.gameObject.name}");
            }
        }

        Debug.Log($"{gameObject.name} initialized with Speed: {Speed}, startSpeed: {startSpeed}, MaxHealth: {MaxHealth}");
    }

    void LateUpdate()
    {
        // Reset speed at the END of the frame, after GameLoopManager has moved the enemy
        // This ensures the slowed speed is used for movement this frame
        Speed = startSpeed;

        // Also reset parent speed if this is a child
        if (transform.parent != null)
        {
            Enemy parentEnemy = transform.parent.GetComponent<Enemy>();
            if (parentEnemy != null && parentEnemy != this)
            {
                parentEnemy.Speed = startSpeed;
            }
        }
    }

    // ...existing code...

    public void Slow(float slowPercent)
    {
        // Reduce speed by percentage (0.5 = 50% slow)
        float oldSpeed = Speed;
        Speed = startSpeed * (1f - slowPercent);

        Debug.Log($"{gameObject.name} SLOWED: oldSpeed={oldSpeed}, newSpeed={Speed}, startSpeed={startSpeed}, slowPercent={slowPercent}");

        // Sync speed with parent if this is a child being slowed
        if (transform.parent != null)
        {
            Debug.Log($"Parent found: {transform.parent.name}");
            Enemy parentEnemy = transform.parent.GetComponent<Enemy>();
            if (parentEnemy != null && parentEnemy != this)
            {
                Debug.Log($"Syncing slow to parent {parentEnemy.gameObject.name}: parent speed was {parentEnemy.Speed}");
                parentEnemy.Speed = Speed;
                Debug.Log($"Parent speed now: {parentEnemy.Speed}");
            }
            else
            {
                Debug.LogWarning($"Parent {transform.parent.name} has no Enemy component or is self!");
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no parent!");
        }
    }

    void Die()
    {
        // If this is a child, remove the parent instead
        Enemy parentEnemy = transform.parent?.GetComponent<Enemy>();
        if (parentEnemy != null && parentEnemy != this)
        {
            GameLoopManager.EnqueueEnemyToRemove(parentEnemy);
        }
        else
        {
            // This is the parent, remove normally
            GameLoopManager.EnqueueEnemyToRemove(this);
        }
    }
}