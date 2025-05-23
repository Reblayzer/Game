using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoModeWatcher : MonoBehaviour
{
  private Toggle _buildToggle, _editToggle;
  private BuildingInfoDisplay _info;

  void Awake()
  {
    _buildToggle = PlotSelector.Instance.buildToggle;
    _editToggle = EditToggleController.InstanceToggle;
    _info = GetComponent<BuildingInfoDisplay>();
  }

  void Update()
  {
    // only call Hide after our display has actually initialized
    if (_info == null) return;

    // if neither mode is on, hide
    if (!_buildToggle.isOn && !_editToggle.isOn)
      _info.Hide();
  }
}
