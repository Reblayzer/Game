using UnityEngine;
using UnityEngine.EventSystems;

public class EscapeUIHandler : MonoBehaviour
{
    public GameObject buildingButtonsPanel;
    public GridManager gridManager;
    public BuildingButtonSelector buttonSelector;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (buildingButtonsPanel != null)
                buildingButtonsPanel.SetActive(false);

            if (gridManager != null)
                gridManager.ClearCuboidSelection();

            if (buttonSelector != null)
                buttonSelector.ClearSelection();

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
