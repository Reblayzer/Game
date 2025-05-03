using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class PlotSelector : MonoBehaviour
{
  public static PlotSelector Instance { get; private set; }

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

  void Awake()
  {
    if (Instance == null) Instance = this;

    if (mapToggle != null)
      mapToggle.onValueChanged.AddListener(_ => UpdateBuildingsButton());

    UpdateBuildingsButton();
  }

  void Start()
  {
    if (buildingsButton != null)
      buildingsButton.onClick.AddListener(OnBuildingsClicked);

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
    if (buttonSelector.GetActiveGridManager() == gm)
    {
      UpdateBuildingsButton();
      return;
    }

    if (buildToggle != null && buildToggle.isOn && gm.ownership != Ownership.Yours)
      buildToggle.isOn = false;

    buildInfoPanel?.SetActive(false);

    var panelToggle = Object.FindFirstObjectByType<BuildingButtonToggle>();
    if (panelToggle != null && panelToggle.IsVisible())
      panelToggle.ToggleButtons();

    buttonSelector.SetActiveGridManager(gm);

    cameraController?.SetTargetPosition(gm.transform.position);

    buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);

    Debug.Log($"📍 Selected Plot: {gm.plotRow},{gm.plotCol}");

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

    Ownership owner = Ownership.Unclaimed;
    if (gm != null)
    {
      var ptc = gm.GetComponentInChildren<PlotTriggerController>();
      if (ptc != null) owner = ptc.ownership;
    }

    bool canBuild = (gm != null && owner == Ownership.Yours);

    buildingsButton?.gameObject.SetActive(canBuild && !mapOpen);

    if (buildToggle != null)
    {
      buildToggle.interactable = canBuild;

      if (!canBuild && buildToggle.isOn)
        buildToggle.isOn = false;
    }
  }


  private void OnBuildingsClicked()
  {
    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;
    buttonSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);
  }

  void Update()
  {
    HandlePlotHover();
  }

  private void HandlePlotHover()
  {
    if (EventSystem.current.IsPointerOverGameObject()) return;

    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    int mask = LayerMask.GetMask("Plot");
    if (Physics.Raycast(ray, out var hit, 100f, mask))
    {
      var hovered = hit.collider.GetComponentInParent<GridManager>();
      var selected = buttonSelector.GetActiveGridManager();

      if (hovered != null && hovered != selected)
      {
        foreach (var other in Object.FindObjectsByType<GridManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
          if (other != selected)
            other.HighlightPlot(buttonSelector.normalTileColor);
        }

        hovered.HighlightPlot(buttonSelector.hoverHighlightPlot);
      }
      return;
    }

    var active = buttonSelector.GetActiveGridManager();
    foreach (var other in Object.FindObjectsByType<GridManager>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None))
    {
      if (other != active)
        other.HighlightPlot(buttonSelector.normalTileColor);
    }
  }
}
