using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public struct TextStyle
{
  public TMP_FontAsset Font;
  public float FontSize;
  public Color Color;
}

public class MiningDrillData : MonoBehaviour
{
  [Header("Identity")]
  [Tooltip("Displayed in the BuildingInfoCanvas")]
  public string DrillName;
  [Tooltip("Initial level of the drill (1-based)")]
  public int Level = 1;

  [Header("Mining Rate by Level")]
  [Tooltip("units per second; index = level - 1")]
  public List<float> MiningRatePerLevel = new List<float>();

  [Header("Popup Settings")]
  [Tooltip("The prefix symbol shown in the floating popups")]
  public string PopupSymbol = "+";

  [Header("Building Info Canvas Style")]
  public TextStyle InfoCanvasStyle;

  [Header("Mining Rate Canvas Style")]
  public TextStyle RateCanvasStyle;

  public event Action<int[]> OnCollectedDelta;

  private MiningDrillUI _ui;
  private List<int> _minedIndices;
  private int[] _collectedCounts;
  private float[] _intervals;

  void Awake()
  {
    if (gameObject.layer == LayerMask.NameToLayer("Ghost"))
    {
      enabled = false;
      return;
    }

    _ui = GetComponentInChildren<MiningDrillUI>();
    if (_ui == null)
    {
      Debug.LogError($"[{name}] MiningDrillData needs a MiningDrillUI child");
      enabled = false;
      return;
    }

    var all = _ui.Materials;
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

    for (int slot = 0; slot < _collectedCounts.Length; slot++)
    {
      int orig = _minedIndices[slot];
      result[orig] = _collectedCounts[slot];
      _collectedCounts[slot] = 0;
    }

    OnCollectedDelta?.Invoke(_collectedCounts);
    return result;
  }

  public int[] CollectedCounts => _collectedCounts;

  // show the rate from the list directly, rounded to int
  public int PopupAmount
  {
    get
    {
      int idx = Mathf.Clamp(Level - 1, 0, MiningRatePerLevel.Count - 1);
      return Mathf.RoundToInt(MiningRatePerLevel[idx]);
    }
  }
}
