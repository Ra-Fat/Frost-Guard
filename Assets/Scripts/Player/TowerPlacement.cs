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
                // Finalize placement by clearing current tower reference
                CurrentPlacingTower = null;
            }
        }
    }

    public void SetTowerToPlace(GameObject tower)
    {
        if (CurrentPlacingTower != null)
        {
            Destroy(CurrentPlacingTower); // Optional: destroy existing tower if placing new one
        }
        // Instantiate the tower at some initial position near camera or zero
        CurrentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
    }
}
