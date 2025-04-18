using UnityEngine;

public class BuildingButtonToggle : MonoBehaviour
{
    [Tooltip("Drag the Panel that contains the building buttons (e.g. Mining Drill, Tall Building, Warehouse)")]
    public GameObject targetPanel;

    private bool isVisible = false;

    void Start()
    {
        // Ensure the panel is hidden when the game starts
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }

    public void ToggleButtons()
    {
        if (targetPanel == null)
        {
            Debug.LogWarning("BuildingButtonToggle: targetPanel is not assigned!");
            return;
        }

        isVisible = !isVisible;
        targetPanel.SetActive(isVisible);
    }
}
