using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectPanelController : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private GameObject collectPanel;
  [SerializeField] private Button collectButton;
  [SerializeField] private Transform slotsParent;

  private MiningDrillData _currentDrill;
  private MiningDrillUI _currentUI;
  private Coroutine _refreshRoutine;

  void Awake()
  {
    if (collectButton == null)
      Debug.LogError("[CollectPanelController] collectButton reference is missing!");
    else
      collectButton.onClick.AddListener(OnCollectPressed);
  }

  void Start()
  {
    if (PlotSelector.Instance == null)
    {
      Debug.LogError("[CollectPanelController] PlotSelector.Instance is null. Has your PlotSelector already Awake() and set Instance?");
      return;
    }

    PlotSelector.Instance.onCollectPanelRequested += HandleShowRequest;
  }

  private void HandleShowRequest(MiningDrillData drill)
  {
    // tear down any old listeners
    if (_currentUI != null)
    {
      _currentUI.OnIconsSpawned -= HandleIconsOrData;
      _currentDrill.OnCollectedDelta -= HandleIconsOrData;
    }

    _currentDrill = drill;

    if (drill == null)
    {
      // hide panel
      collectPanel.SetActive(false);
      _currentUI = null;
      return;
    }

    // otherwise, show it
    collectPanel.SetActive(true);

    // grab its UI helper
    _currentUI = drill.GetComponentInChildren<MiningDrillUI>();

    // listen for both floating‐icon batches **and** raw data deltas
    _currentUI.OnIconsSpawned += HandleIconsOrData;
    _currentDrill.OnCollectedDelta += HandleIconsOrData;

    // immediate refresh so you don’t see stale numbers
    RefreshDisplay(_currentDrill.CollectedCounts);
  }

  private void HandleIconsOrData(int[] dummy) => HandleIconsOrData();
  private void HandleIconsOrData()
  {
    // sync the CollectPanel **after** the icons have faded in
    float delay = _currentUI?.FadeInDuration ?? 0f;
    if (_refreshRoutine != null) StopCoroutine(_refreshRoutine);
    _refreshRoutine = StartCoroutine(DelayedRefresh(_currentDrill.CollectedCounts, delay));
  }

  private IEnumerator DelayedRefresh(int[] totals, float delay)
  {
    yield return new WaitForSeconds(delay);
    RefreshDisplay(totals);
  }

  private void RefreshDisplay(int[] totals)
  {
    int resourceIndex = 0;

    // walk every child of the panel…
    for (int i = 0; i < slotsParent.childCount; i++)
    {
      var child = slotsParent.GetChild(i);

      // only process objects whose name ends with "Resource"
      if (!child.name.EndsWith("Resource"))
        continue;

      // find the AmountLabel under that slot
      var label = child
        .Find("Amount/AmountLabel")
        .GetComponent<TMP_Text>();

      // pick the right total or zero if out of range
      int value = resourceIndex < totals.Length ? totals[resourceIndex] : 0;
      label.text = value.ToString();

      resourceIndex++;
    }
  }

  private void OnCollectPressed()
  {
    Debug.Log("Collect pressed! _currentDrill = " + _currentDrill);
    if (_currentDrill == null) return;

    var justMined = _currentDrill.ConsumeAll();
    Debug.Log("  pulled: " + string.Join(",", justMined));
    WarehouseData.Instance.AddToWarehouse(justMined);
    RefreshDisplay(new int[_currentUI.Materials.Count]);
  }

  void OnDestroy()
  {
    if (PlotSelector.Instance != null)
      PlotSelector.Instance.onCollectPanelRequested -= HandleShowRequest;
  }
}
