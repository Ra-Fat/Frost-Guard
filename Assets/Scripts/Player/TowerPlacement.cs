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
    private GameObject currentHoveredPlate;
    private Color originalColor;
    private HashSet<GameObject> occupiedPlates = new HashSet<GameObject>();
    private Dictionary<GameObject, GameObject> plateTurretMap = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        if (CurrentPlacingTower != null)
        {
            Ray camray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(camray, out RaycastHit hitInfo, 100f, placementLayerMask))
            {
                Vector3 newPos = hitInfo.point;
                newPos.y = towerBaseHeight;
                CurrentPlacingTower.transform.position = newPos;

                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject.CompareTag("PlacementPlate"))
                {
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
                        else
                        {
                            rend.material.color = hoverColor;
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
                    Turret turret = CurrentPlacingTower.GetComponent<Turret>();
                    if (turret != null)
                    {
                        turret.PlaceTurret();
                        occupiedPlates.Add(currentHoveredPlate);
                        plateTurretMap[currentHoveredPlate] = CurrentPlacingTower;
                    }

                    if (currentHoveredPlate != null)
                    {
                        currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
                        currentHoveredPlate = null;
                    }
                    CurrentPlacingTower = null;
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

    public void SetTowerToPlace(GameObject tower)
    {
        if (CurrentPlacingTower != null)
        {
            Destroy(CurrentPlacingTower);
        }

        if (currentHoveredPlate != null)
        {
            currentHoveredPlate.GetComponent<Renderer>().material.color = originalColor;
            currentHoveredPlate = null;
        }

        CurrentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
        Turret turret = CurrentPlacingTower.GetComponent<Turret>();
        if (turret != null)
        {
            turret.isPlaced = false;
        }
    }

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

    public GameObject GetCurrentPlacingTower()
    {
        return CurrentPlacingTower;
    }
}