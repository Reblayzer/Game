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

    // pull the data component off the root of your prefab
    var data = miningDrillParentPrefab.GetComponent<MiningDrillData>();
    if (data == null)
    {
      Debug.LogError("WarehouseBootstrap: no MiningDrillData found on your prefab.");
      return;
    }

    // use the number of rates you've defined in the data as the slot count
    int slotCount = data.MiningRatePerLevel.Count;
    WarehouseData.Instance.Initialize(slotCount);
  }
}
