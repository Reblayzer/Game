using UnityEngine;
using System;

public class WarehouseData : MonoBehaviour
{
  public static WarehouseData Instance { get; private set; }

  public event Action<int[]> OnInventoryChanged;

  private int[] _storedCounts;

  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public void Initialize(int resourceTypesCount)
  {
    _storedCounts = new int[resourceTypesCount];
  }

  public void AddToWarehouse(int[] batch)
  {
    if (_storedCounts == null || _storedCounts.Length < batch.Length)
      _storedCounts = new int[batch.Length];

    for (int i = 0; i < batch.Length; i++)
      _storedCounts[i] += batch[i];

    OnInventoryChanged?.Invoke(_storedCounts);
  }

  public int[] GetAllStored() => (int[])_storedCounts.Clone();
}
