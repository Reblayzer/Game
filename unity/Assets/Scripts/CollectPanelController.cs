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

  void OnEnable()
  {
    PlotSelector.Instance.onCollectPanelRequested += HandleShowRequest;
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

  void OnDisable()
  {
    PlotSelector.Instance.onCollectPanelRequested -= HandleShowRequest;
  }

  private void HandleShowRequest(MiningDrillData drill)
  {
    // 1) unsubscribe any old listeners
    if (_currentUI != null)
    {
      _currentUI.OnIconsSpawned -= HandleIconsOrData;
      _currentDrill.OnCollectedDelta -= HandleIconsOrData;
      _currentUI = null;
      _currentDrill = null;
    }

    // 2) now install the new drill (or null)
    _currentDrill = drill;
    collectPanel.SetActive(drill != null);
    collectButton.interactable = (drill != null);

    if (drill == null)
      return;

    _currentUI = drill.GetComponentInChildren<MiningDrillUI>();
    _currentUI.OnIconsSpawned += HandleIconsOrData;
    _currentDrill.OnCollectedDelta += HandleIconsOrData;
    RefreshDisplay(_currentDrill.CollectedCounts);
  }

  private void HandleIconsOrData(int[] dummy) => HandleIconsOrData();
  private void HandleIconsOrData()
  {
    if (_currentDrill == null || !collectPanel.activeInHierarchy)
      return;
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
}
