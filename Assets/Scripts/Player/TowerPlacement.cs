using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private Camera PlayerCamera;

    // Set this to the Y height where the tower base should be placed (e.g., ground level)
    [SerializeField] private float towerBaseHeight = 0f;

    // Optional: Set which layers can be placed on (e.g., ground)
    [SerializeField] private LayerMask placementLayerMask = ~0; // Default: Everything

    private GameObject CurrentPlacingTower;

    void Update()
    {
        if (CurrentPlacingTower != null)
        {
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(camray, out RaycastHit hitInfo, 100f, placementLayerMask))
            {
                Vector3 newPos = hitInfo.point;
                newPos.y = towerBaseHeight; // Fix the tower's vertical position to avoid weird movement
                CurrentPlacingTower.transform.position = newPos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Check if BuildManager exists
                if (BuildManager.instance == null)
                {
                    Debug.LogError("BuildManager instance not found!");
                    Destroy(CurrentPlacingTower);
                    CurrentPlacingTower = null;
                    return;
                }

                // Check if player has enough money
                if (BuildManager.instance.CanBuild && BuildManager.instance.HasMoney)
                {
                    TurretBlueprint blueprint = BuildManager.instance.GetTurretToBuild();
                    
                    // Deduct money
                    PlayerStats.Money -= blueprint.cost;
                    
                    Debug.Log("Tower placed! Remaining money: " + PlayerStats.Money);
                    
                    // Get the position before destroying preview
                    Vector3 placePosition = CurrentPlacingTower.transform.position;
                    Quaternion placeRotation = CurrentPlacingTower.transform.rotation;
                    
                    Debug.Log($"Placing turret at position: {placePosition}");
                    
                    // Destroy preview tower
                    Destroy(CurrentPlacingTower);
                    
                    // Spawn the actual functional turret
                    GameObject placedTurret = Instantiate(blueprint.prefab, placePosition, placeRotation);
                    Debug.Log($"Turret placed successfully: {placedTurret.name}");
                    
                    // Clear reference
                    CurrentPlacingTower = null;
                }
                else
                {
                    // Not enough money - show error and destroy the preview tower
                    Debug.Log("Not enough money to build this tower!");
                    Destroy(CurrentPlacingTower);
                    CurrentPlacingTower = null;
                }
            }
        }
    }

    public void SetTowerToPlace(TurretBlueprint blueprint)
    {
        if (CurrentPlacingTower != null)
        {
            Destroy(CurrentPlacingTower);
        }
        // Get the prefab from the blueprint and instantiate it
        CurrentPlacingTower = Instantiate(blueprint.prefab, Vector3.zero, Quaternion.identity);
        
        // Disable turret behavior on preview
        Turret turretScript = CurrentPlacingTower.GetComponent<Turret>();
        if (turretScript != null)
        {
            turretScript.enabled = false;
        }
    }
}
