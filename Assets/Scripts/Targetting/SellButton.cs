using UnityEngine;
using UnityEngine.UI;

public class SellButton : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn == null)
        {
            btn = GetComponentInChildren<Button>();
            if (btn != null)
            {
                Debug.LogWarning("SellButton: Button component found in children, not on root. Consider moving SellButton script to the same GameObject as the Button.");
            }
        }
        if (btn == null)
        {
            Debug.LogError("SellButton: No Button component found on prefab or its children!");
        }
        else
        {
            int count = btn.onClick.GetPersistentEventCount();
            Debug.Log($"SellButton: Button found. OnClick event count: {count}");
            if (count == 0)
            {
                Debug.LogWarning("SellButton: No OnClick event assigned! You must wire OnSellButtonClicked in the Inspector.");
            }
        }
    }
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