using UnityEngine;
using UnityEngine.EventSystems;

public class EscapeUIHandler : MonoBehaviour
{
    public GridManager gridManager;
    public BuildingButtonSelector buttonSelector;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var toggle = FindFirstObjectByType<BuildingButtonToggle>();
            if (toggle != null)
                toggle.HidePanel();

            if (gridManager != null)
            {
                gridManager.ClearCuboidSelection();
                gridManager.HideCuboidInfo();
            }

            if (buttonSelector != null)
                buttonSelector.ClearSelection();

            // ✅ Forcefully clear any selected cuboid globally
            if (SelectableCuboid.currentlySelectedCuboid != null)
            {
                SelectableCuboid.currentlySelectedCuboid.HideInfo();
            }

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
