using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlotSelector : MonoBehaviour
{
  [Header("Wiring")]
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;
  public MapUIController mapUI;

  [Header("UI")]
  public Toggle mapToggle;
  public Button buildingsButton;

  private GridManager _hoveredPlot;

  void Awake()
  {
    if (mapToggle != null)
      mapToggle.onValueChanged.AddListener(_ => UpdateBuildingsButton());
  }

  void Start()
  {
    if (buildingsButton != null)
      buildingsButton.onClick.AddListener(OnBuildingsClicked);

    UpdateBuildingsButton();
  }

  void Update()
  {
    UpdateBuildingsButton();

    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      int plotMask = LayerMask.GetMask("Plot");
      if (Physics.Raycast(ray, out var hit, 100f, plotMask))
      {
        var gm = hit.collider.GetComponentInParent<GridManager>();
        if (gm != null && buttonSelector.GetActiveGridManager() != gm)
        {
          var toggle = FindFirstObjectByType<BuildingButtonToggle>();
          if (toggle != null && toggle.IsVisible())
            toggle.ToggleButtons();

          buttonSelector.SetActiveGridManager(gm);
          if (cameraController != null)
            cameraController.target.position = gm.transform.position;

          buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);

          Debug.Log($"📍 Selected Plot: {gm.name}");
          UpdateBuildingsButton();
        }
      }
    }

    HandlePlotHover();
  }

  public void UpdateBuildingsButton()
  {
    bool mapOpen = mapToggle != null && mapToggle.isOn;
    var gm = buttonSelector.GetActiveGridManager();

    Ownership owner = Ownership.Unclaimed;
    if (gm != null)
    {
      var ptc = gm.GetComponentInChildren<PlotTriggerController>();
      if (ptc != null)
        owner = ptc.ownership;
    }

    bool shouldShow = !mapOpen && gm != null && owner == Ownership.Yours;
    if (buildingsButton != null)
      buildingsButton.gameObject.SetActive(shouldShow);
  }

  private void OnBuildingsClicked()
  {
    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;

    buttonSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);
  }

  private void HandlePlotHover()
  {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    int plotMask = LayerMask.GetMask("Plot");

    if (Physics.Raycast(ray, out var hit, 100f, plotMask))
    {
      var hovered = hit.collider.GetComponentInParent<GridManager>();
      var selected = buttonSelector.GetActiveGridManager();

      if (hovered != null && hovered != selected)
      {
        if (hovered != _hoveredPlot)
        {
          if (_hoveredPlot != null)
            _hoveredPlot.HighlightPlot(buttonSelector.normalTileColor);

          hovered.HighlightPlot(buttonSelector.hoverHighlightPlot);
          _hoveredPlot = hovered;
        }
        return;
      }
    }

    if (_hoveredPlot != null)
    {
      _hoveredPlot.HighlightPlot(buttonSelector.normalTileColor);
      _hoveredPlot = null;
    }
  }
}
