using UnityEngine;

[System.Serializable]
public class TurretBlueprint
{
    public GameObject prefab;
    public int cost;

    public TurretBlueprint(GameObject prefab, int cost)
    {
        this.prefab = prefab;
        this.cost = cost;
    }
}
