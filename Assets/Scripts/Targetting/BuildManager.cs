using UnityEngine;

public class BuildManager : MonoBehaviour
{
	public static BuildManager instance;

	void Awake()
	{
		if (instance != null)
		{
			Debug.LogError("More than one BuildManager in scene!");
			return;
		}
		instance = this;
	}

	private TurretBlueprint turretToBuild;

	public bool CanBuild { get { return turretToBuild != null; } }
	public bool HasMoney { get { return PlayerStats.Money >= turretToBuild.cost; } }

	public void SelectTurretToBuild(TurretBlueprint turret)
	{
		turretToBuild = turret;
	}

	public TurretBlueprint GetTurretToBuild()
	{
		return turretToBuild;
	}
}
