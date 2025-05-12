using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SelectableCuboid : MonoBehaviour
{
    public static SelectableCuboid currentlySelectedCuboid = null;

    public string cuboidName;
    public GameObject infoPanel;
    public TMP_Text infoDisplay;
    public Button upgradeButton;
    public GridManager GridManager { get; private set; }

    private int level = 1;

    public void Init(GridManager manager)
    {
        GridManager = manager;
    }

    void OnMouseDown()
    {
        // ignore clicks over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        ShowInfo();
    }

    public void ShowInfo()
    {
        if (infoPanel == null || infoDisplay == null || upgradeButton == null)
            return;

        // 🔁 Toggle if clicking same building again
        if (currentlySelectedCuboid == this && infoPanel.activeSelf)
        {
            GridManager.HideCuboidInfo(); // Hide via GridManager logic
            currentlySelectedCuboid = null;
            return;
        }

        // hide the old one
        if (currentlySelectedCuboid != null && currentlySelectedCuboid != this)
            currentlySelectedCuboid.HideInfo();

        // show this one's info
        infoPanel.SetActive(true);
        upgradeButton.gameObject.SetActive(true);
        infoDisplay.text = $"{cuboidName} (Level {level})";

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() =>
        {
            level++;
            infoDisplay.text = $"{cuboidName} (Level {level})";
        });

        currentlySelectedCuboid = this;

        // *** only this overload ***
        var drillData = GetComponent<MiningDrillData>();
        if (drillData != null)
        {
            // this single line tells PlotSelector which drill's counts to show
            PlotSelector.Instance.ShowCollectPanel(drillData);
        }
        else
        {
            // non-drills (other buildings) should just hide the collect panel
            PlotSelector.Instance.HideCollectPanel();
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);
    }
}
