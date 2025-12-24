using UnityEngine;
using UnityEngine.UI;

public class SellButton : MonoBehaviour
{
    private Turret targetTurret;
    private TurretSelector selector;

    public void Initialize(Turret turret, TurretSelector turretSelector)
    {
        targetTurret = turret;
        selector = turretSelector;
    }

    public void OnSellButtonClicked()
    {
        Debug.Log("Sell button clicked!");

        if (targetTurret != null)
        {
            Debug.Log($"Selling turret: {targetTurret.name}");

            // Sell the turret
            targetTurret.SellTurret();

            // Deselect and destroy button
            if (selector != null)
            {
                selector.DeselectTurret();
            }
        }
        else
        {
            Debug.LogError("Target turret is null!");
        }
    }

    void Update()
    {
        // If turret is destroyed somehow, destroy this button
        if (targetTurret == null)
        {
            Destroy(gameObject);
        }
    }
}