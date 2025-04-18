using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPanelToggle : MonoBehaviour
{
    [Header("Panel with Mining Drill, Warehouse, etc.")]
    public GameObject buildingButtonsPanel;

    public void TogglePanel()
    {
        // 🔄 Clear Unity’s selected object to ensure click is registered
        EventSystem.current.SetSelectedGameObject(null);

        if (buildingButtonsPanel != null)
        {
            bool isActive = buildingButtonsPanel.activeSelf;
            buildingButtonsPanel.SetActive(!isActive);
        }
    }
}
