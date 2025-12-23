using UnityEngine;

public class shop : MonoBehaviour
{
    public TurretBlueprint standardTurret;
    public TurretBlueprint missileTurret;
    public TurretBlueprint laserTurret;

    BuildManager buildManager;
    TowerPlacement towerPlacement;

    void Start()
    {
        buildManager = BuildManager.instance;
        towerPlacement = FindObjectOfType<TowerPlacement>();

        if (buildManager == null)
        {
            Debug.LogError("BuildManager not found in scene!");
        }

        if (towerPlacement == null)
        {
            Debug.LogError("TowerPlacement not found in scene!");
        }
    }

    public void SelectStandardTurret()
    {
        if (buildManager == null || towerPlacement == null) return;

        Debug.Log("Standard Turret Selected");
        buildManager.SelectTurretToBuild(standardTurret);
        towerPlacement.SetTowerToPlace(standardTurret);
    }

    public void SelectMissileTurret()
    {
        if (buildManager == null || towerPlacement == null) return;

        Debug.Log("Missile Turret Selected");
        buildManager.SelectTurretToBuild(missileTurret);
        towerPlacement.SetTowerToPlace(missileTurret);
    }

    public void SelectLaserTurret()
    {
        Debug.Log("Laser Turret Selected");
        buildManager.SelectTurretToBuild(laserTurret);
        towerPlacement.SetTowerToPlace(laserTurret);
    }
}
