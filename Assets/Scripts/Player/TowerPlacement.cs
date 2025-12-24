using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private float towerBaseHeight = 0f;
    [SerializeField] private LayerMask placementLayerMask = ~0;
    [SerializeField] private Color hoverColor = new Color(0.298f, 0.318f, 0.357f, 0.5f);
    [SerializeField] private Color occupiedColor = Color.red;

    private GameObject CurrentPlacingTower;
    private TurretBlueprint currentBlueprint;
    private GameObject currentHoveredPlate;
    private Color originalColor;
    private HashSet<GameObject> occupiedPlates = new HashSet<GameObject>();

    // NEW: Track which turret is on which plate for selling
    private Dictionary<GameObject, GameObject> plateTurretMap = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        if (CurrentPlacingTower != null)
        {
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(camray, out RaycastHit hitInfo, 100f, placementLayerMask))
            {
                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject.CompareTag("PlacementPlate"))
                {
                    // Snap turret to center of plate
                    Vector3 plateCenter = hitObject.transform.position;
                    plateCenter.y = towerBaseHeight;
                    CurrentPlacingTower.transform.position = plateCenter;

                    if (currentHoveredPlate != hitObject)
                    {
                        if (currentHoveredPlate != null)
                        {
                            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                        }
                        currentHoveredPlate = hitObject;
                        Renderer rend = currentHoveredPlate.GetComponent<Renderer>();
                        originalColor = rend.material.color;
                        if (occupiedPlates.Contains(hitObject))
                        {
                            rend.material.color = occupiedColor;
                        }
                        else if (currentBlueprint != null && PlayerStats.Money < currentBlueprint.cost)
                        {
                            rend.material.color = Color.red;
                        }
                        else
                        {
                            rend.material.color = hoverColor;
                        }
                    }
                }
                else
                {
                    // Snap turret to mouse hit point if not on plate
                    Vector3 newPos = hitInfo.point;
                    newPos.y = towerBaseHeight;
                    CurrentPlacingTower.transform.position = newPos;

                    if (currentHoveredPlate != null)
                    {
                        currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                        currentHoveredPlate = null;
                    }
                }
            }
            else
            {
                if (currentHoveredPlate != null)
                {
                    currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                    currentHoveredPlate = null;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (currentHoveredPlate != null && !occupiedPlates.Contains(currentHoveredPlate))
                {
                    if (currentBlueprint != null && PlayerStats.Money >= currentBlueprint.cost)
                    {
                        PlayerStats.Money -= currentBlueprint.cost;
                        Turret turret = CurrentPlacingTower.GetComponent<Turret>();
                        if (turret != null)
                        {
                            turret.enabled = true;
                            turret.PlaceTurret();
                            turret.blueprint = currentBlueprint; // NEW: Store blueprint reference
                            occupiedPlates.Add(currentHoveredPlate);
                            plateTurretMap[currentHoveredPlate] = CurrentPlacingTower; // NEW: Track turret-plate relationship
                        }
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
            }
        }
        else
        {
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
        CurrentPlacingTower = Instantiate(blueprint.prefab, Vector3.zero, Quaternion.identity);
        currentBlueprint = blueprint;

        Turret turretScript = CurrentPlacingTower.GetComponent<Turret>();
        if (turretScript != null)
        {
            turretScript.enabled = false;
            turretScript.isPlaced = false;
        }

        // NEW: Disable collider during placement
        Collider col = CurrentPlacingTower.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (currentHoveredPlate != null)
        {
            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
            currentHoveredPlate = null;
        }
    }

    // NEW: Method to remove turret from occupied plates when selling
    public void RemoveTurret(GameObject turret)
    {
        GameObject plateToRemove = null;
        foreach (var kvp in plateTurretMap)
        {
            if (kvp.Value == turret)
            {
                plateToRemove = kvp.Key;
                break;
            }
        }

        if (plateToRemove != null)
        {
            occupiedPlates.Remove(plateToRemove);
            plateTurretMap.Remove(plateToRemove);
        }
    }

    // NEW: Public getter for TurretSelector
    public GameObject GetCurrentPlacingTower()
    {
        return CurrentPlacingTower;
    }
}