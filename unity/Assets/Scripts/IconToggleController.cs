using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class IconToggleController : MonoBehaviour
{
  private Toggle _toggle;

  void Awake()
  {
    _toggle = GetComponent<Toggle>();
    _toggle.onValueChanged.AddListener(OnIconToggled);
  }

  void OnDestroy()
  {
    _toggle.onValueChanged.RemoveListener(OnIconToggled);
  }

  private void OnIconToggled(bool isOn)
  {
    // only react when this icon is turned on
    if (!isOn) return;

    var ps = PlotSelector.Instance;
    if (ps == null) return;

    // if the collect panel is up, switch back to plot info
    if (ps.collectPanel != null && ps.collectPanel.activeSelf)
    {
      ps.HideCollectPanel();
      ps.ShowPlotInfoPanels();
    }
  }
}
