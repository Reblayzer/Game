using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlotSelector : MonoBehaviour
{
  public static PlotSelector Instance { get; private set; }

  public event Action<MiningDrillData> onCollectPanelRequested;

  [Header("Wiring")]
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;

  [Header("UI")]
  public Toggle mapToggle;
  public Toggle buildToggle;
  public Button buildingsButton;

  [Header("Plot Details Panels")]
  public GameObject plotInfoPanel;
  public GameObject buyPlotInfoPanel;

  [Header("Building Details Panel")]
  public GameObject buildInfoPanel;

  [Header("Collect Panel")]
  public GameObject collectPanel;

  [Header("VFX Prefabs")]
  public GameObject fogVFXPrefab;
  public GameObject dustVFXPrefab;

  [Header("VFX Settings")]
  public float spawnHeightOffset = 0.5f;

  private GameObject currentFog;
  private GameObject currentDust;

  private MiningDrillData _currentDrill;
  private MiningDrillUI _currentUI;

  void Awake()
  {
    if (Instance == null) Instance = this;
    mapToggle?.onValueChanged.AddListener(_ => UpdateBuildingsButton());
    UpdateBuildingsButton();
  }

  void Start()
  {
    buildingsButton?.onClick.AddListener(OnBuildingsClicked);
    UpdateBuildingsButton();
    StartCoroutine(SelectInitialPlot());
  }

  private IEnumerator SelectInitialPlot()
  {
    yield return new WaitUntil(() => buttonSelector.GetActiveGridManager() != null);
    SelectPlot(buttonSelector.GetActiveGridManager());
  }

  public void SelectPlot(GridManager gm)
  {
    // 0. Tear down any existing VFX
    if (currentFog != null)
    {
      Destroy(currentFog);
      currentFog = null;
    }
    if (currentDust != null)
    {
      Destroy(currentDust);
      currentDust = null;
    }

    // 1. Your existing UI/selection logic
    ShowPlotInfoPanels();

    if (buttonSelector.GetActiveGridManager() == gm)
    {
      UpdateBuildingsButton();
      return;
    }

    if (buildToggle != null && buildToggle.isOn && gm.ownership != Ownership.Yours)
      buildToggle.isOn = false;

    buildInfoPanel?.SetActive(false);
    FindFirstObjectByType<BuildingButtonToggle>()?.ToggleButtons();

    buttonSelector.SetActiveGridManager(gm);
    cameraController?.SetTargetPosition(gm.transform.position);
    buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);

    switch (gm.ownership)
    {
      case Ownership.Yours:
      case Ownership.Opponent:
        plotInfoPanel?.SetActive(true);
        buyPlotInfoPanel?.SetActive(false);
        break;
      case Ownership.Unclaimed:
        plotInfoPanel?.SetActive(false);
        buyPlotInfoPanel?.SetActive(true);
        break;
      default:
        plotInfoPanel?.SetActive(false);
        buyPlotInfoPanel?.SetActive(false);
        break;
    }

    UpdateBuildingsButton();

    // 2. Spawn new fog & dust VFX at the selected plot
    Vector3 spawnPos = gm.transform.position + Vector3.up * spawnHeightOffset;

    if (fogVFXPrefab != null)
    {
      currentFog = Instantiate(fogVFXPrefab, spawnPos, Quaternion.identity);
      currentFog.transform.SetParent(gm.transform, worldPositionStays: true);
    }

    if (dustVFXPrefab != null)
    {
      Vector3 randomOffset = new Vector3(
          UnityEngine.Random.Range(-0.5f, 0.5f),
          0,
          UnityEngine.Random.Range(-0.5f, 0.5f)
      );
      currentDust = Instantiate(dustVFXPrefab, spawnPos + randomOffset, Quaternion.identity);
      currentDust.transform.SetParent(gm.transform, worldPositionStays: true);
    }
  }

  public void ShowPlotInfoPanels()
  {
    collectPanel?.SetActive(false);

    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;

    // Mountain plots get no standard UI…
    if (gm.plotType == PlotType.Mountain)
    {
      plotInfoPanel?.SetActive(false);
      buyPlotInfoPanel?.SetActive(false);
      buildInfoPanel?.SetActive(false);
      return;
    }

    switch (gm.ownership)
    {
      case Ownership.Yours:
      case Ownership.Opponent:
        plotInfoPanel?.SetActive(true);
        buyPlotInfoPanel?.SetActive(false);
        break;
      case Ownership.Unclaimed:
        plotInfoPanel?.SetActive(false);
        buyPlotInfoPanel?.SetActive(true);
        break;
      default:
        plotInfoPanel?.SetActive(false);
        buyPlotInfoPanel?.SetActive(false);
        break;
    }
  }

  public void UpdateBuildingsButton()
  {
    bool mapOpen = mapToggle != null && mapToggle.isOn;
    var gm = buttonSelector.GetActiveGridManager();
    bool canBuild = gm != null && gm.ownership == Ownership.Yours;

    buildingsButton?.gameObject.SetActive(canBuild && !mapOpen);
    if (buildToggle != null)
    {
      buildToggle.interactable = canBuild;
      if (!canBuild && buildToggle.isOn)
        buildToggle.isOn = false;
    }
  }

  void OnBuildingsClicked()
  {
    HideCollectPanel();
    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;
    buttonSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);
    gm.StartPlacementPhase();
  }

  public void ShowCollectPanel(MiningDrillData drill)
  {
    // hide other panels (your existing code)
    plotInfoPanel?.SetActive(false);
    buyPlotInfoPanel?.SetActive(false);
    buildInfoPanel?.SetActive(false);

    collectPanel?.SetActive(true);
    onCollectPanelRequested?.Invoke(drill);

    // 1) unhook *both* old events
    if (_currentUI != null)
      _currentUI.OnIconsSpawned -= HandleIconsSpawned;
    if (_currentDrill != null)
      _currentDrill.OnCollectedDelta -= HandleDataDelta;

    // 2) hook up new drill & UI
    _currentUI = drill.GetComponentInChildren<MiningDrillUI>();
    _currentDrill = drill;

    _currentUI.OnIconsSpawned += HandleIconsSpawned;
    _currentDrill.OnCollectedDelta += HandleDataDelta;

    // 3) initial refresh
    RefreshDisplay(_currentDrill.CollectedCounts);
  }

  public void HideCollectPanel()
  {
    collectPanel?.SetActive(false);

    if (_currentUI != null)
      _currentUI.OnIconsSpawned -= HandleIconsSpawned;
    if (_currentDrill != null)
      _currentDrill.OnCollectedDelta -= HandleDataDelta;

    _currentUI = null;
    _currentDrill = null;
    onCollectPanelRequested?.Invoke(null);
  }

  private void HandleIconsSpawned()
  {
    // wait for fade-in, then update
    StartCoroutine(DelayedRefresh(_currentDrill.CollectedCounts, _currentUI.FadeInDuration));
  }

  private IEnumerator DelayedRefresh(int[] totals, float delay)
  {
    yield return new WaitForSeconds(delay);
    RefreshDisplay(totals);
  }

  private void RefreshDisplay(int[] totals)
  {
    var panel = collectPanel.transform;
    for (int i = 0; i < panel.childCount; i++)
    {
      var slot = panel.GetChild(i);
      if (slot == null) continue;

      var labelTf = slot.Find("Amount/AmountLabel");
      if (labelTf == null) continue;

      var label = labelTf.GetComponent<TMP_Text>();
      if (label == null) continue;

      int value = i < totals.Length ? totals[i] : 0;
      label.text = value.ToString();
    }
  }

  private void HandleDataDelta(int[] totals)
  {
    RefreshDisplay(totals);
    StartCoroutine(DelayedRefresh(totals, _currentUI.FadeInDuration));
  }
}
