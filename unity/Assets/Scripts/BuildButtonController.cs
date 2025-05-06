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
    // 1) grab the currently selected plot & blueprint
    GridManager gm = plotSelector.buttonSelector.GetActiveGridManager();
    int idx = buildingSelector.CurrentIndex;
    if (gm == null || idx < 0 || idx >= gm.cuboidTypes.Length) return;

    // 2) hide the blueprint UI
    _toggleCtrl.HideBlueprintInfoContainer();

    // 3) enter build‐mode on both UI and grid
    buildingSelector.ToggleEditMode(true);
    gm.SetActive(true);       // <— add this!
    gm.SetEditMode(true);

    // 4) tell the grid which cuboid we want (this sets hasSelectedCuboid = true)
    gm.SetSelectedCuboid(idx);

    // 5) subscribe so we can exit build‐mode after placement
    gm.OnCuboidPlaced += OnCuboidPlaced;
  }

  private void OnCuboidPlaced()
  {
    // once they place, leave edit‐mode everywhere
    GridManager gm = plotSelector
                     .buttonSelector
                     .GetActiveGridManager();
    if (gm != null)
    {
      _buildingSelector.ToggleEditMode(false);
      gm.SetEditMode(false);
      gm.OnCuboidPlaced -= OnCuboidPlaced;
    }
  }
}
