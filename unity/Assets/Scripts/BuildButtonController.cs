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

  void Awake()
  {
    _toggleCtrl = plotSelector.buildToggle.GetComponent<BuildToggleController>();
    buildButton.onClick.AddListener(OnBuildClicked);
  }

  void OnBuildClicked()
  {
    // 1) grab the currently selected plot
    GridManager gm = plotSelector.buttonSelector.GetActiveGridManager();
    if (gm == null) return;

    Debug.Log("🗑️  HideAllBuildUI()");

    // 2) hide the Blueprint UI
    _toggleCtrl.HideAllBuildUI();
    plotSelector.buildToggle.isOn = false;

    // 3) enter edit mode
    buildingSelector.ToggleEditMode(true);
    gm.SetEditMode(true);

    // 4) subscribe to the placement event
    gm.OnCuboidPlaced += OnCuboidPlaced;
  }

  void OnCuboidPlaced()
  {
    // once they place a cuboid, leave edit mode
    GridManager gm = plotSelector.buttonSelector.GetActiveGridManager();
    if (gm != null)
    {
      buildingSelector.ToggleEditMode(false);
      gm.SetEditMode(false);
      gm.OnCuboidPlaced -= OnCuboidPlaced;   // unsubscribe
    }
  }
}
