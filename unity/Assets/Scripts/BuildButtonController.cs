using UnityEngine;
using UnityEngine.UI;
using System;

public class BuildButtonController : MonoBehaviour
{
  public Button buildButton;
  public PlotSelector plotSelector;
  public BuildingButtonSelector buildingSelector;

  BuildToggleController _toggleCtrl;
  GridManager _activeGrid;

  void Awake()
  {
    buildButton.onClick.AddListener(OnBuildClicked);
    _toggleCtrl = plotSelector.buildToggle.GetComponent<BuildToggleController>();
    Debug.Log("_toggleCtrl = " + (_toggleCtrl == null ? "NULL!" : _toggleCtrl.name));
  }

  void OnEnable() => PlotSelector.Instance.onPlotChanged += PlotChanged;
  void OnDisable() => PlotSelector.Instance.onPlotChanged -= PlotChanged;

  void PlotChanged(GridManager gm)
  {
    if (_activeGrid != null)
      _activeGrid.OnCuboidPlaced -= OnCuboidPlaced;

    _activeGrid = gm;

    if (_activeGrid != null)
      _activeGrid.OnCuboidPlaced += OnCuboidPlaced;
  }

  void OnBuildClicked()
  {
    if (_activeGrid == null) return;

    Debug.Log("🗑️  HideAllBuildUI()");
    _toggleCtrl.HideAllBuildUI();

    plotSelector.buildToggle.isOn = false;

    buildingSelector.ToggleEditMode(true);
    _activeGrid.SetEditMode(true);

    plotSelector.plotInfoPanel.SetActive(false);
    plotSelector.buyPlotInfoPanel.SetActive(false);
  }

  void OnCuboidPlaced()
  {
    plotSelector.buildToggle.isOn = false;

    buildingSelector.ToggleEditMode(false);
    _activeGrid.SetEditMode(false);

  }
}
