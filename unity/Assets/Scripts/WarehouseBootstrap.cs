using UnityEngine;

[RequireComponent(typeof(WarehouseData))]
public class WarehouseBootstrap : MonoBehaviour
{
  [Tooltip("Drag your MiningDrillParent01 prefab here")]
  [SerializeField] private GameObject miningDrillParentPrefab;

  void Awake()
  {
    if (miningDrillParentPrefab == null)
    {
      Debug.LogError("Assign MiningDrillParent01 prefab to WarehouseBootstrap!");
      return;
    }

    // find the MiningDrillUI on one of its children:
    var ui = miningDrillParentPrefab.GetComponentInChildren<MiningDrillUI>();
    if (ui == null)
    {
      Debug.LogError("WarehouseBootstrap: no MiningDrillUI found on the child of your parent prefab.");
      return;
    }

    // now initialize with the right slot count:
    WarehouseData.Instance.Initialize(ui.Materials.Count);
  }
}
