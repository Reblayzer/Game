using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlotSelector : MonoBehaviour
{
  public static PlotSelector Instance { get; private set; }

  [Header("Wiring")]
  public BuildingButtonSelector buttonSelector;
  public CameraController cameraController;

  [Header("UI")]
  public Toggle mapToggle;
  public Button buildingsButton;

  void Awake()
  {
    if (Instance == null) Instance = this;

    if (mapToggle != null)
      mapToggle.onValueChanged.AddListener(_ => UpdateBuildingsButton());
  }

  void Start()
  {
    if (buildingsButton != null)
      buildingsButton.onClick.AddListener(OnBuildingsClicked);

    UpdateBuildingsButton();
  }

  public void SelectPlot(GridManager gm)
  {
    if (buttonSelector.GetActiveGridManager() == gm)
      return;

    var toggle = Object.FindFirstObjectByType<BuildingButtonToggle>();
    if (toggle != null && toggle.IsVisible())
      toggle.ToggleButtons();

    buttonSelector.SetActiveGridManager(gm);

    if (cameraController != null)
      cameraController.target.position = gm.transform.position;

    buttonSelector.SelectByIndex(buttonSelector.CurrentIndex);

    Debug.Log($"📍 Selected Plot: {gm.plotRow},{gm.plotCol}");

    UpdateBuildingsButton();
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

    bool shouldShow = !mapOpen && gm != null && owner == Ownership.Yours;
    buildingsButton?.gameObject.SetActive(shouldShow);
  }

  private void OnBuildingsClicked()
  {
    var gm = buttonSelector.GetActiveGridManager();
    if (gm == null) return;

    buttonSelector.ToggleEditMode(true);
    gm.SetActive(true);
    gm.SetEditMode(true);
  }
}
