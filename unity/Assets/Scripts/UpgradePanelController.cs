using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelController : MonoBehaviour
{
  [Header("Build-Mode (Upgrade) UI")]
  [SerializeField] private GameObject upgradePanel;
  [SerializeField] private Button upgradeButton;

  private MiningDrillData _currentDrill;
  private Toggle _buildToggle;

  void Awake()
  {
    // wire up the button immediately
    if (upgradeButton == null)
      Debug.LogError("[UpgradePanel] upgradeButton not assigned");
    else
      upgradeButton.onClick.AddListener(OnUpgradePressed);

    // cache & hook the build toggle so we refresh whenever it changes
    _buildToggle = PlotSelector.Instance.buildToggle;
    if (_buildToggle != null)
      _buildToggle.onValueChanged.AddListener(_ => RefreshPanel());

    // subscribe to both collect- and upgrade-requests
    var ps = PlotSelector.Instance;
    ps.onCollectPanelRequested += HandleDrillSelection;
    ps.onUpgradePanelRequested += HandleDrillSelection;
  }

  void Start()
  {
    // now that Awake has hooked everything, we can safely hide
    if (upgradePanel != null)
      upgradePanel.SetActive(false);
  }

  void OnDestroy()
  {
    // clean up all subscriptions
    var ps = PlotSelector.Instance;
    if (ps != null)
    {
      ps.onCollectPanelRequested -= HandleDrillSelection;
      ps.onUpgradePanelRequested -= HandleDrillSelection;
    }

    if (_buildToggle != null)
      _buildToggle.onValueChanged.RemoveListener(_ => RefreshPanel());
  }

  private void HandleDrillSelection(MiningDrillData drill)
  {
    _currentDrill = drill;
    RefreshPanel();
  }

  private void RefreshPanel()
  {
    bool show = _currentDrill != null
                && _buildToggle != null
                && _buildToggle.isOn;

    upgradePanel?.SetActive(show);
  }

  private void OnUpgradePressed()
  {
    if (_currentDrill == null) return;

    // bump your drill’s Level directly
    _currentDrill.Level++;

    // re-invoke ShowDrillPanels so the panel stays open and any
    // other UI (like the roman-numeral shield) refreshes
    PlotSelector.Instance.ShowDrillPanels(_currentDrill);
  }
}
