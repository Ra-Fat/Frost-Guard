using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image healthBarImage; 

    private Enemy enemy;

    void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    void Update()
    {
        if (enemy != null && healthBarImage != null)
        {
            float fill = enemy.Health / enemy.MaxHealth;
            healthBarImage.fillAmount = fill;
        }
    }
}