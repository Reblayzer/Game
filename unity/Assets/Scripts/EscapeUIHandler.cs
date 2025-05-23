using UnityEngine;
using UnityEngine.EventSystems;

public class EscapeUIHandler : MonoBehaviour
{
    [Tooltip("Hook up your PlotSelector")]
    public PlotSelector plotSelector;

    [Tooltip("Hook up your BuildingButtonSelector")]
    public BuildingButtonSelector buttonSelector;

    [Tooltip("Optional: clear grid highlights")]
    public GridManager gridManager;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 1) turn off Build & Edit modes
        if (plotSelector != null)
        {
            plotSelector.buildToggle.isOn = false;
            EditToggleController.InstanceToggle.isOn = false;
        }

        // 2) clear any building-info canvas via the selection event
        SelectableCuboid.ClearSelection();

        // 3) hide collect panel
        PlotSelector.Instance.HideCollectPanel();

        // 4) clear blueprint list
        buttonSelector?.ClearSelection();

        // 5) clear any grid selection
        gridManager?.ClearCuboidSelection();
        gridManager?.HideCuboidInfo();

        // 6) unfocus UI
        EventSystem.current?.SetSelectedGameObject(null);
    }
}
