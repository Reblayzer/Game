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
        if (targetPanel == null || buildingButtonSelector == null)
        {
            Debug.LogWarning("Missing reference on BuildingButtonToggle.");
            return;
        }

        // If panel was already visible, we're about to hide it
        if (isVisible)
        {
            buildingButtonSelector.ClearSelection(); // 👈 Clear ghost & selection
        }

        isVisible = !isVisible;
        targetPanel.SetActive(isVisible);
    }
}