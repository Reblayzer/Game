using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlotSelector : MonoBehaviour
{
  public static PlotSelector Instance { get; private set; }

  public event Action OnPlotChanged;
  public event Action<MiningDrillData> onCollectPanelRequested;
  public event Action<MiningDrillData> onUpgradePanelRequested;

  [Header("Wiring")]
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;

  [Header("UI")]
  public Toggle mapToggle;
  public Toggle buildToggle;
  public Toggle editToggle;
  public Button buildingsButton;

  [Header("Plot Details Panels")]
  public GameObject plotInfoPanel;
  public GameObject buyPlotInfoPanel;

  [Header("Building Details Panel")]
  public GameObject buildInfoPanel;

  [Header("Building-Selected Panel")]
  public GameObject collectPanel;
  public GameObject upgradePanel;
  public GameObject editPanel;

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

    // whenever I flip Build or Edit, I want the panels
    // to re-evaluate the currently selected drill.
    buildToggle.onValueChanged.AddListener(_ => ShowDrillPanels(_currentDrill));
    editToggle.onValueChanged.AddListener(_ => ShowDrillPanels(_currentDrill));

    // keep your existing exclusivity logic if you like
    buildToggle.onValueChanged.AddListener(on =>
    {
      if (on && editToggle.isOn) editToggle.isOn = false;
      UpdateBuildingsButton();
    });
    editToggle.onValueChanged.AddListener(on =>
    {
      if (on && buildToggle.isOn) buildToggle.isOn = false;
      // EditToggleController will show/hide editPanel for you
    });

    mapToggle.onValueChanged.AddListener(_ => UpdateBuildingsButton());
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
    // teardown old VFX
    if (currentFog != null) { Destroy(currentFog); currentFog = null; }
    if (currentDust != null) { Destroy(currentDust); currentDust = null; }

    ShowPlotInfoPanels();

    if (buttonSelector.GetActiveGridManager() == gm)
    {
      UpdateBuildingsButton();
      return;
    }

    if (buildToggle.isOn && gm.ownership != Ownership.Yours)
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

    // spawn new VFX
    Vector3 spawnPos = gm.transform.position + Vector3.up * spawnHeightOffset;
    if (fogVFXPrefab != null)
    {
      currentFog = Instantiate(fogVFXPrefab, spawnPos, Quaternion.identity);
      currentFog.transform.SetParent(gm.transform, true);
    }
    if (dustVFXPrefab != null)
    {
      Vector3 rand = new Vector3(UnityEngine.Random.Range(-.5f, .5f), 0, UnityEngine.Random.Range(-.5f, .5f));
      currentDust = Instantiate(dustVFXPrefab, spawnPos + rand, Quaternion.identity);
      currentDust.transform.SetParent(gm.transform, true);
    }

    OnPlotChanged?.Invoke();
  }

  public void ShowPlotInfoPanels()
  {
    collectPanel?.SetActive(false);
    upgradePanel?.SetActive(false);
    editPanel?.SetActive(false);

    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;

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

    // ensure Build‐mode toggle is on
    if (buildToggle != null)
      buildToggle.isOn = true;

    // now enter placement
    buttonSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);
    gm.StartPlacementPhase();
  }

  public void ShowDrillPanels(MiningDrillData drill)
  {
    Debug.Log($"[PlotSelector] ShowDrillPanels: " +
              $"drill={(drill == null ? "null" : drill.name)}, " +
              $"build={buildToggle.isOn}, edit={editToggle.isOn}");

    // hide everything
    plotInfoPanel?.SetActive(false);
    buyPlotInfoPanel?.SetActive(false);
    buildInfoPanel?.SetActive(false);
    collectPanel?.SetActive(false);
    upgradePanel?.SetActive(false);
    editPanel?.SetActive(false);

    // unhook old
    if (_currentDrill != null)
      _currentDrill.OnCollectedDelta -= HandleDataDelta;

    _currentDrill = drill;
    _currentUI = drill?.GetComponentInChildren<MiningDrillUI>();

    if (drill == null)
    {
      // clear both controllers
      onCollectPanelRequested?.Invoke(null);
      onUpgradePanelRequested?.Invoke(null);
      return;
    }

    // 4A) BUILD mode → UPGRADE
    if (buildToggle.isOn)
    {
      Debug.Log("[PlotSelector] → build mode, showing upgradePanel");
      onUpgradePanelRequested?.Invoke(drill);
      upgradePanel.SetActive(true);
    }
    // 4B) EDIT mode → EDIT
    else if (editToggle.isOn)
    {
      Debug.Log("[PlotSelector] → edit mode, showing editPanel");
      // EditToggleController.OnToggleChanged will show it
      editPanel.SetActive(true);
    }
    // 4C) neither → COLLECT
    else
    {
      Debug.Log("[PlotSelector] → neither build nor edit, showing collectPanel");
      onCollectPanelRequested?.Invoke(drill);
      collectPanel.SetActive(true);
      drill.OnCollectedDelta += HandleDataDelta;
      RefreshDisplay(drill.CollectedCounts);
    }
  }

  public void HideCollectPanel()
  {
    collectPanel?.SetActive(false);
    if (_currentDrill != null)
      _currentDrill.OnCollectedDelta -= HandleDataDelta;
    _currentUI = null;
    _currentDrill = null;
    onCollectPanelRequested?.Invoke(null);
  }

  private void HandleDataDelta(int[] totals)
  {
    RefreshDisplay(totals);
    float delay = _currentUI?.FadeInDuration ?? 0f;
    StartCoroutine(DelayedRefresh(totals, delay));
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
      int value = i < totals.Length ? totals[i] : 0;
      label.text = value.ToString();
    }
  }

  public void RaisePlotChanged()
  {
    OnPlotChanged?.Invoke();
  }
}
