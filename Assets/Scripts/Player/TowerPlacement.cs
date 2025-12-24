using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Targetting;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private Camera PlayerCamera;

    // Set this to the Y height where the tower base should be placed (e.g., ground level)
    [SerializeField] private float towerBaseHeight = 0f;

    // Optional: Set which layers can be placed on (e.g., ground)
    [SerializeField] private LayerMask placementLayerMask = ~0; // Default: Everything

    [SerializeField] private Color hoverColor = new Color(0.298f, 0.318f, 0.357f, 0.5f);
    [SerializeField] private Color occupiedColor = Color.red;

    private GameObject CurrentPlacingTower;
    private TurretBlueprint currentBlueprint;
    private GameObject currentHoveredPlate;
    private Color originalColor;
    private HashSet<GameObject> occupiedPlates = new HashSet<GameObject>();

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

                // Handle hover effect
                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject.CompareTag("PlacementPlate"))
                {
                    if (currentHoveredPlate != hitObject)
                    {
                        // Revert previous plate color
                        if (currentHoveredPlate != null)
                        {
                            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                        }
                        // Set new plate to hover color
                        currentHoveredPlate = hitObject;
                        Renderer rend = currentHoveredPlate.GetComponent<Renderer>();
                        originalColor = rend.material.color;
                        if (occupiedPlates.Contains(hitObject))
                        {
                            rend.material.color = occupiedColor; // Red for occupied
                        }
                        else
                        {
                            rend.material.color = hoverColor; // Normal hover
                        }
                    }
                }
                else
                {
                    // Not hovering over a plate, revert color
                    if (currentHoveredPlate != null)
                    {
                        currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                        currentHoveredPlate = null;
                    }
                }
            }
            else
            {
                // No hit, revert color
                if (currentHoveredPlate != null)
                {
                    currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                    currentHoveredPlate = null;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Check if can place and has enough money
                if (currentHoveredPlate != null && !occupiedPlates.Contains(currentHoveredPlate))
                {
                    if (currentBlueprint != null && PlayerStats.Money >= currentBlueprint.cost)
                    {
                        // Deduct money
                        PlayerStats.Money -= currentBlueprint.cost;
                        // Finalize placement
                        Turret turret = CurrentPlacingTower.GetComponent<Turret>();
                        if (turret != null)
                        {
                            turret.enabled = true; // Enable turret shooting
                            turret.PlaceTurret();
                            occupiedPlates.Add(currentHoveredPlate); // Mark as occupied
                        }
                        // Revert hover color after placement
                        if (currentHoveredPlate != null)
                        {
                            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                            currentHoveredPlate = null;
                        }
                        CurrentPlacingTower = null;
                        currentBlueprint = null;
                    }
                    else
                    {
                        Debug.Log("Not enough money to place this turret!");
                    }
                }
                // If occupied, do nothing (can't place)
            }
        }
        else
        {
            // Not placing, ensure no hover
            if (currentHoveredPlate != null)
            {
                currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                currentHoveredPlate = null;
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
        currentBlueprint = blueprint;
        // Disable turret behavior on preview
        Turret turretScript = CurrentPlacingTower.GetComponent<Turret>();
        if (turretScript != null)
        {
            turretScript.enabled = false;
        }
        // Reset hover
        if (currentHoveredPlate != null)
        {
            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
            currentHoveredPlate = null;
        }
        // Mark as not placed yet
        Turret turret = CurrentPlacingTower.GetComponent<Turret>();
        if (turret != null)
        {
            turret.isPlaced = false;
        }
    }
}
