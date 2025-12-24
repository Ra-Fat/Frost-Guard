using UnityEngine;
using UnityEngine.EventSystems;

public class TurretSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject sellButtonPrefab; // Assign the UI sell button prefab

    [Header("Settings")]
    [SerializeField] private LayerMask turretLayer; // Set to only detect turrets
    [SerializeField] private float buttonOffsetY = 2f; // How high above turret to show button

    private Turret selectedTurret;
    private GameObject currentSellButton;

    void Update()
    {
        // Only detect clicks when not placing a tower
        TowerPlacement placement = GetComponent<TowerPlacement>();
        if (placement != null && placement.GetCurrentPlacingTower() != null)
        {
            Debug.Log("Currently placing tower, ignoring clicks");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Don't process if clicking on UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicking on UI, ignoring");
                return;
            }

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Click detected, casting ray");

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, turretLayer))
            {
                Debug.Log($"Hit object: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                Turret clickedTurret = hit.collider.GetComponent<Turret>();

                if (clickedTurret != null && clickedTurret.isPlaced)
                {
                    Debug.Log($"Found turret: {clickedTurret.name}, isPlaced: {clickedTurret.isPlaced}");
                    SelectTurret(clickedTurret);
                }
                else
                {
                    Debug.Log($"No valid turret found. Turret component: {clickedTurret != null}, isPlaced: {clickedTurret?.isPlaced}");
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing, deselecting");
                DeselectTurret();
            }
        }
    }

    void SelectTurret(Turret turret)
    {
        Debug.Log($"SelectTurret called for: {turret.name}");

        // If clicking the same turret, deselect it
        if (selectedTurret == turret)
        {
            Debug.Log("Same turret clicked, deselecting");
            DeselectTurret();
            return;
        }

        // Deselect previous turret
        DeselectTurret();

        // Select new turret
        selectedTurret = turret;
        Debug.Log($"Selected turret: {selectedTurret.name}");

        // Show sell button above the turret
        if (sellButtonPrefab != null)
        {
            Vector3 buttonPosition = turret.transform.position + Vector3.up * buttonOffsetY;
            Debug.Log($"Creating sell button at position: {buttonPosition}");
            currentSellButton = Instantiate(sellButtonPrefab, buttonPosition, Quaternion.identity);

            // Make button face camera
            currentSellButton.transform.LookAt(playerCamera.transform);
            currentSellButton.transform.Rotate(0, 180, 0); // Flip to face correctly

            // Set up the button's reference to this turret
            SellButton sellBtn = currentSellButton.GetComponent<SellButton>();
            if (sellBtn != null)
            {
                sellBtn.Initialize(turret, this);
                Debug.Log("SellButton initialized successfully");
            }
            else
            {
                Debug.LogError("SellButton component not found on prefab!");
            }
        }
        else
        {
            Debug.LogError("Sell Button Prefab is not assigned!");
        }
    }

    public void DeselectTurret()
    {
        selectedTurret = null;

        // Destroy sell button
        if (currentSellButton != null)
        {
            Destroy(currentSellButton);
            currentSellButton = null;
        }
    }

    public Turret GetSelectedTurret()
    {
        return selectedTurret;
    }
}