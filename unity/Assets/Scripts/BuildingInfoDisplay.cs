using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BuildingInfoDisplay : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI _nameText;
  [SerializeField] private TextMeshProUGUI _levelText;

  private CanvasGroup _cg;
  private Toggle _buildToggle, _editToggle;
  private SelectableCuboid _owner;
  private UnityAction<bool> _onToggleChanged;

  // ←– re-add this so GridManager can assign the SelectableCuboid
  public void SetOwner(SelectableCuboid owner)
  {
    _owner = owner;
  }

  void Awake()
  {
    // always ensure you have a CanvasGroup
    _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

    // start hidden
    _cg.alpha = 0;
    _cg.interactable = _cg.blocksRaycasts = false;

    _buildToggle = PlotSelector.Instance.buildToggle;
    _editToggle = EditToggleController.InstanceToggle;
    _onToggleChanged = _ => Hide();
  }

  void OnEnable()
  {
    SelectableCuboid.OnCuboidSelected += HandleSelection;
    _buildToggle.onValueChanged.AddListener(_onToggleChanged);
    _editToggle.onValueChanged.AddListener(_onToggleChanged);
  }

  void OnDisable()
  {
    SelectableCuboid.OnCuboidSelected -= HandleSelection;
    _buildToggle.onValueChanged.RemoveListener(_onToggleChanged);
    _editToggle.onValueChanged.RemoveListener(_onToggleChanged);
  }

  private void HandleSelection(SelectableCuboid selected)
  {
    bool anyMode = _buildToggle.isOn || _editToggle.isOn;
    if (selected == _owner && anyMode)
    {
      var data = _owner.GetComponent<MiningDrillData>();
      if (data != null)
      {
        _nameText.text = data.DrillName;
        _levelText.text = $"Level {data.Level}";

        var s = data.InfoCanvasStyle;
        _nameText.font = s.Font;
        _nameText.fontSize = s.FontSize;
        _nameText.color = s.Color;

        _levelText.font = s.Font;
        _levelText.fontSize = s.FontSize;
        _levelText.color = s.Color;
      }

      _cg.alpha = 1;
      _cg.interactable = true;
      _cg.blocksRaycasts = true;
    }
    else
    {
      Hide();
    }
  }

  public void Hide()
  {
    _cg.alpha = 0;
    _cg.interactable = false;
    _cg.blocksRaycasts = false;
  }
}
