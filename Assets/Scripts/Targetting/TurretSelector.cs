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
            // Check if clicking on an actual UI element (not world space canvas on turrets)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Additional check: see if we're clicking on a sell button specifically
                var pointerData = new UnityEngine.EventSystems.PointerEventData(EventSystem.current);
                pointerData.position = Input.mousePosition;

                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                // If we hit a button, it's the sell button - let it handle the click
                bool hitButton = false;
                foreach (var result in results)
                {
                    if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                    {
                        Debug.Log("Clicking on sell button, ignoring");
                        hitButton = true;
                        break;
                    }
                }

                if (hitButton) return;
            }

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Click detected, casting ray");

            // DEBUG: Try without layer mask first
            RaycastHit[] allHits = Physics.RaycastAll(ray, 100f);
            Debug.Log($"Raycast found {allHits.Length} total hits (no layer filter)");

            foreach (RaycastHit h in allHits)
            {
                Debug.Log($"Hit: {h.collider.name}, Layer: {LayerMask.LayerToName(h.collider.gameObject.layer)}");
            }

            // Use RaycastAll with layer mask
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f, turretLayer);
            Debug.Log($"Raycast found {hits.Length} hits with turret layer filter");

            Turret clickedTurret = null;

            // Find the first valid turret in the hits
            foreach (RaycastHit hit in hits)
            {
                Debug.Log($"Hit object: {hit.collider.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                Turret turret = hit.collider.GetComponent<Turret>();

                if (turret != null && turret.isPlaced)
                {
                    Debug.Log($"Found valid turret: {turret.name}, isPlaced: {turret.isPlaced}");
                    clickedTurret = turret;
                    break;
                }
            }

            if (clickedTurret != null)
            {
                SelectTurret(clickedTurret);
            }
            else
            {
                Debug.Log("No valid turret found in raycasts, deselecting");
                DeselectTurret();
            }
        }
    }

    void LateUpdate()
    {
        if (currentSellButton != null)
        {
            currentSellButton.transform.LookAt(playerCamera.transform);
            currentSellButton.transform.Rotate(0, 180, 0);
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
            Canvas canvas = currentSellButton.GetComponentInChildren<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace) { 
                canvas.worldCamera = playerCamera;
                canvas.enabled = false; // Force refresh
                canvas.enabled = true;
                Debug.Log("Assigned event camera to sell button canvas: " + playerCamera.name);
            }
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