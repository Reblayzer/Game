using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BuildingInfoDisplay : MonoBehaviour
{
  [Header("UI Elements")]
  [Tooltip("Child TextMeshProUGUI that shows the Roman numeral")]
  [SerializeField] private TextMeshProUGUI _levelText;

  [Tooltip("The Shield background that should appear/vanish")]
  [SerializeField] private GameObject _shieldGraphic;

  private CanvasGroup _cg;
  private Toggle _buildToggle, _editToggle;
  private SelectableCuboid _owner;
  private UnityAction<bool> _onToggleChanged;

  public void SetOwner(SelectableCuboid owner)
  {
    _owner = owner;
  }

  void Awake()
  {
    // ensure there’s a CanvasGroup
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
        // set level text to Roman
        _levelText.text = ToRoman(data.Level);

        // apply text style from InfoCanvasStyle
        var s = data.InfoCanvasStyle;
        _levelText.font = s.Font;
        _levelText.fontSize = s.FontSize;
        _levelText.color = s.Color;
      }

      // show shield + text
      _shieldGraphic.SetActive(true);
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
    // hide shield + text
    _shieldGraphic.SetActive(false);
    _cg.alpha = 0;
    _cg.interactable = false;
    _cg.blocksRaycasts = false;
  }

  private static string ToRoman(int number)
  {
    var map = new[]
    {
            new { Value=1000, Symbol="M" }, new { Value=900, Symbol="CM" },
            new { Value=500,  Symbol="D" }, new { Value=400, Symbol="CD" },
            new { Value=100,  Symbol="C" }, new { Value=90,  Symbol="XC" },
            new { Value=50,   Symbol="L" }, new { Value=40,  Symbol="XL" },
            new { Value=10,   Symbol="X" }, new { Value=9,   Symbol="IX" },
            new { Value=5,    Symbol="V" }, new { Value=4,   Symbol="IV" },
            new { Value=1,    Symbol="I" },
        };
    var result = "";
    foreach (var item in map)
      while (number >= item.Value)
      {
        result += item.Symbol;
        number -= item.Value;
      }
    return result;
  }
}
