using UnityEngine;
using UnityEngine.EventSystems;

public class PlotSelector : MonoBehaviour
{
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;
  private GridManager currentHoverPlot;

  void Update()
  {
    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      int plotMask = LayerMask.GetMask("Plot");

      if (Physics.Raycast(ray, out RaycastHit hit, 100f, plotMask))
      {
        GridManager gm = hit.collider.GetComponentInParent<GridManager>();

        if (gm != null)
        {
          if (buttonSelector.GetActiveGridManager() == gm)
            return;

          var toggle = FindFirstObjectByType<BuildingButtonToggle>();
          if (toggle != null && toggle.IsVisible())
            toggle.ToggleButtons();

          buttonSelector.SetActiveGridManager(gm);

          if (cameraController != null)
            cameraController.target.position = gm.transform.position;

          buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);
          Debug.Log($"📍 Selected Plot: {gm.name}");
        }
      }
    }
    HandlePlotHover();
  }

  void HandlePlotHover()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    int plotMask = LayerMask.GetMask("Plot");

    if (Physics.Raycast(ray, out RaycastHit hit, 100f, plotMask))
    {
      GridManager hovered = hit.collider.GetComponentInParent<GridManager>();
      GridManager selected = buttonSelector.GetActiveGridManager();

      if (hovered != null && hovered != selected)
      {
        if (hovered != currentHoverPlot)
        {
          // Restore old hover plot
          if (currentHoverPlot != null)
            currentHoverPlot.HighlightPlot(buttonSelector.normalTileColor);

          // Highlight new hover plot
          hovered.HighlightPlot(buttonSelector.hoverHighlightPlot);
          currentHoverPlot = hovered;
        }
        return;
      }
    }

    // Reset hover if no valid hit or hovering selected plot
    if (currentHoverPlot != null)
    {
      currentHoverPlot.HighlightPlot(buttonSelector.normalTileColor);
      currentHoverPlot = null;
    }
  }
}
