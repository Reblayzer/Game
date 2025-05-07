using UnityEngine;
using UnityEngine.UI;
using System;

public class BuildButtonController : MonoBehaviour
{
  [Header("Wiring")]
  public Button buildButton;
  public PlotSelector plotSelector;
  public BuildingButtonSelector buildingSelector;

  BuildToggleController _toggleCtrl;
  private BuildingButtonSelector _buildingSelector;

  void Awake()
  {
    // 1) grab references
    _toggleCtrl = plotSelector.buildToggle
                  .GetComponent<BuildToggleController>();
    _buildingSelector = plotSelector.buttonSelector;

    buildButton.onClick.AddListener(OnBuildClicked);
  }

  void OnDestroy()
  {
    buildButton.onClick.RemoveListener(OnBuildClicked);
  }

  private void OnBuildClicked()
  {
    _toggleCtrl.buildToggle.isOn = true;
    GridManager gm = plotSelector.buttonSelector.GetActiveGridManager();
    int idx = buildingSelector.CurrentIndex;
    if (gm == null || idx < 0 || idx >= gm.cuboidTypes.Length) return;

    _toggleCtrl.HideBlueprintInfoContainer();

    buildingSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);

    gm.SetSelectedCuboid(idx);

    gm.StartPlacementPhase();

    gm.OnCuboidPlaced += OnCuboidPlaced;
  }

  private void OnCuboidPlaced()
  {
    var gm = plotSelector.buttonSelector.GetActiveGridManager();
    if (gm == null) return;

    gm.EndPlacementPhase();
    buildingSelector.ClearSelection();
    gm.ClearCuboidSelection();
    _toggleCtrl.ClearBlueprintToggles();

    gm.OnCuboidPlaced -= OnCuboidPlaced;
  }
}
