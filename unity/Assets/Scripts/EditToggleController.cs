using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class EditToggleController : MonoBehaviour
{
  // expose this toggle for any runtime grabbers
  public static Toggle InstanceToggle { get; private set; }

  [Tooltip("The panel to show/hide when this toggle changes state")]
  public GameObject editPanel;

  [Tooltip("The default info-panel shown for a selected plot")]
  public GameObject plotInfoPanel;

  private Toggle _toggle;
  private GridManager _currentGrid;

  void Awake()
  {
    _toggle = GetComponent<Toggle>();
    _toggle.onValueChanged.AddListener(OnToggleChanged);

    // set the static reference here
    InstanceToggle = _toggle;
  }

  void OnDestroy()
  {
    _toggle.onValueChanged.RemoveListener(OnToggleChanged);

    // clear static if this instance goes away
    if (InstanceToggle == _toggle)
      InstanceToggle = null;
  }

  void Update()
  {
    // 1) Which plot is selected right now?
    var ps = PlotSelector.Instance;
    if (ps == null) return;
    var gm = ps.buttonSelector.GetActiveGridManager();

    // 2) If we just switched plots, force edit-mode off
    if (gm != _currentGrid)
    {
      _currentGrid = gm;
      _toggle.isOn = false;
    }

    // 3) Compute whether the button should be interactable:
    //    only if it's your plot AND you have at least one cuboid placed
    bool canEdit =
       _currentGrid != null &&
       _currentGrid.ownership == Ownership.Yours &&
       _currentGrid.CuboidCount > 0;

    _toggle.interactable = canEdit;
  }

  private void OnToggleChanged(bool isOn)
  {
    // show/hide panels
    editPanel?.SetActive(isOn);
    plotInfoPanel?.SetActive(!isOn);
  }
}
