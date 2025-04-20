using UnityEngine;

public class BuildingButtonToggle : MonoBehaviour
{
    [Tooltip("Drag the Panel that contains the building buttons (e.g. Mining Drill, Tall Building, Warehouse)")]
    public GameObject targetPanel;

    [Tooltip("Reference to the BuildingButtonSelector script")]
    public BuildingButtonSelector buildingButtonSelector;

    private bool isVisible = false;

    void Start()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }

    public void ToggleButtons()
    {
        if (targetPanel == null) return;

        isVisible = !isVisible;
        targetPanel.SetActive(isVisible);

        if (buildingButtonSelector != null)
        {
            buildingButtonSelector.ToggleEditMode(isVisible);

            // ✅ Also hide cuboid info when toggling into edit mode
            GridManager current = buildingButtonSelector.GetActiveGridManager();
            if (current != null)
            {
                current.HideCuboidInfo();
            }
        }
    }

    public void HidePanel()
    {
        if (targetPanel != null && isVisible)
        {
            isVisible = false;
            targetPanel.SetActive(false);
            if (buildingButtonSelector != null)
            {
                buildingButtonSelector.ToggleEditMode(false);
            }
        }
    }
}