using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MiningRateCanvasController : MonoBehaviour
{
  private CanvasGroup _cg;
  private Toggle _buildToggle;
  private Toggle _editToggle;

  void Awake()
  {
    _cg = GetComponent<CanvasGroup>();
    _buildToggle = PlotSelector.Instance.buildToggle;
    _editToggle = EditToggleController.InstanceToggle;

    // Subscribe
    _buildToggle.onValueChanged.AddListener(OnModeToggled);
    _editToggle.onValueChanged.AddListener(OnModeToggled);

    // Initial
    OnModeToggled(_buildToggle.isOn);
  }

  void OnDestroy()
  {
    _buildToggle.onValueChanged.RemoveListener(OnModeToggled);
    _editToggle.onValueChanged.RemoveListener(OnModeToggled);
  }

  private void OnModeToggled(bool _)
  {
    bool inAnyMode = _buildToggle.isOn || _editToggle.isOn;
    // hide when build/edit active
    _cg.alpha = inAnyMode ? 0f : 1f;
    _cg.interactable = !inAnyMode;
    _cg.blocksRaycasts = !inAnyMode;
  }
}
