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

    public void Init()
    {
        Health = MaxHealth;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        if (Health <= 0 && !isDead)
        {
            isDead = true;
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
            Destroy(gameObject);
        }
    }
}
