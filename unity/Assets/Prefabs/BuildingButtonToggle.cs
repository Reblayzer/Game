using UnityEngine;

public class BuildingButtonToggle : MonoBehaviour
{
    [Tooltip("Panels")]
    public GameObject normalPanel;
    public GameObject voidPanel;

    [Tooltip("Reference to the BuildingButtonSelector script")]
    public BuildingButtonSelector buildingButtonSelector;

    private bool isVisible = false;

    void Start()
    {
        if (normalPanel != null)
            normalPanel.SetActive(false);

        if (voidPanel != null)
            voidPanel.SetActive(false);
    }

    public bool IsVisible()
    {
        return isVisible;
    }

    public void ToggleButtons()
    {
        if (normalPanel == null || voidPanel == null) return;

        isVisible = !isVisible;

        GridManager current = buildingButtonSelector.GetActiveGridManager();

        // Hide both panels first
        normalPanel.SetActive(false);
        voidPanel.SetActive(false);

        if (isVisible)
        {
            if (current.plotType == PlotType.Void)
                voidPanel.SetActive(true);
            else
                normalPanel.SetActive(true);
        }

        if (buildingButtonSelector != null)
        {
            buildingButtonSelector.ToggleEditMode(isVisible);

            if (current != null)
            {
                current.HideCuboidInfo();
            }
        }
    }

    public void HidePanel()
    {
        if (!isVisible) return;

        isVisible = false;
        normalPanel.SetActive(false);
        voidPanel.SetActive(false);

        if (buildingButtonSelector != null)
        {
            buildingButtonSelector.ToggleEditMode(false);
        }
    }
}