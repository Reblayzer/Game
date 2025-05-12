using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MiningDrillData : MonoBehaviour
{
  public event Action<int[]> OnCollectedDelta;

  private MiningDrillUI _ui;
  List<int> _minedIndices;
  private int[] _collectedCounts;
  private float[] _intervals;

  void Awake()
  {
    _ui = GetComponentInChildren<MiningDrillUI>();
    if (_ui == null)
      Debug.LogError($"[DrillData] missing MiningDrillUI on {name}");

    var all = _ui.Materials;

    // only include the ones that are actually mined
    _minedIndices = new List<int>();
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
    for (int slot = 0; slot < _intervals.Length; slot++)
      StartCoroutine(AccumulateLoop(slot, _intervals[slot]));
  }

  IEnumerator AccumulateLoop(int slot, float interval)
  {
    // initial wait
    yield return new WaitForSeconds(interval);

    while (true)
    {
      // increment the compact counter
      _collectedCounts[slot]++;

      // notify any listeners
      OnCollectedDelta?.Invoke(_collectedCounts);

      // wait for next tick
      yield return new WaitForSeconds(interval);
    }
  }

  public int[] CollectedCounts => _collectedCounts;
}
