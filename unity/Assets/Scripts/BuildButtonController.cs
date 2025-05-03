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

    _toggleCtrl.HideAllBuildUI();

    plotSelector.buildInfoPanel.SetActive(false);

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
