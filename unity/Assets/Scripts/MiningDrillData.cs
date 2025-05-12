using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MiningDrillData : MonoBehaviour
{
  public event Action<int[]> OnCollectedDelta;

  private MiningDrillUI _ui;
  private List<int> _minedIndices;
  private int[] _collectedCounts;
  private float[] _intervals;

  void Awake()
  {
    _ui = GetComponentInChildren<MiningDrillUI>();

    // build “which materials we actually mine”
    _minedIndices = new List<int>();
    var all = _ui.Materials;
    for (int i = 0; i < all.Count; i++)
      if (all[i].isMined)
        _minedIndices.Add(i);

    int n = _minedIndices.Count;
    _collectedCounts = new int[n];
    _intervals = new float[n];
    for (int slot = 0; slot < n; slot++)
      _intervals[slot] = all[_minedIndices[slot]].spawnInterval;
  }

  void OnEnable()
  {
    // start one loop per mined‐slot
    for (int slot = 0; slot < _intervals.Length; slot++)
      StartCoroutine(AccumulateLoop(slot, _intervals[slot]));
  }

  IEnumerator AccumulateLoop(int slot, float interval)
  {
    yield return new WaitForSeconds(interval);
    while (true)
    {
      _collectedCounts[slot]++;
      OnCollectedDelta?.Invoke(_collectedCounts);
      yield return new WaitForSeconds(interval);
    }
  }

  public int[] ConsumeAll()
  {
    int fullCount = _ui.Materials.Count;
    var result = new int[fullCount];

    // copy out and zero internal
    for (int slot = 0; slot < _collectedCounts.Length; slot++)
    {
      int originalIndex = _minedIndices[slot];
      result[originalIndex] = _collectedCounts[slot];
      _collectedCounts[slot] = 0;
    }

    // update the CollectPanel (so it jumps back to zero)
    OnCollectedDelta?.Invoke(_collectedCounts);

    return result;
  }

  public int[] CollectedCounts => _collectedCounts;
}
