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
      Debug.LogError("[CollectPanelController] PlotSelector.Instance is null.");
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
    // unsubscribe old
    if (_currentDrill != null)
      _currentDrill.OnCollectedDelta -= HandleIconsOrData;

    _currentDrill = drill;
    collectPanel.SetActive(drill != null);
    collectButton.interactable = (drill != null);

    if (drill == null)
      return;

    _currentUI = drill.GetComponentInChildren<MiningDrillUI>();
    _currentDrill.OnCollectedDelta += HandleIconsOrData;
    RefreshDisplay(_currentDrill.CollectedCounts);
  }

  private void HandleIconsOrData(int[] _)
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

    for (int i = 0; i < slotsParent.childCount; i++)
    {
      var child = slotsParent.GetChild(i);
      if (!child.name.EndsWith("Resource")) continue;

      var label = child
        .Find("Amount/AmountLabel")
        .GetComponent<TMP_Text>();

      int value = resourceIndex < totals.Length ? totals[resourceIndex] : 0;
      label.text = value.ToString();
      resourceIndex++;
    }
  }

  private void OnCollectPressed()
  {
    if (_currentDrill == null) return;
    var justMined = _currentDrill.ConsumeAll();
    WarehouseData.Instance.AddToWarehouse(justMined);

    // reset UI slots using the drill's collected‐counts length
    RefreshDisplay(new int[_currentUI.Materials.Count]);
  }
}
