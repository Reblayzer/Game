using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonSelector : MonoBehaviour
{
    [System.Serializable]
    public class BuildingButton
    {
        public GameObject rootObject;
        public Image backgroundImage;
    }

    public List<BuildingButton> buildingButtons;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private int currentIndex = -1;
    private GridManager activeGridManager;

    public int CurrentIndex => currentIndex;

    public void SetActiveGridManager(GridManager gm)
    {
        if (activeGridManager != null)
            activeGridManager.SetActive(false);

        activeGridManager = gm;
        activeGridManager.SetActive(true);

        // Reapply current selection if valid
        if (currentIndex >= 0 && currentIndex < buildingButtons.Count)
        {
            activeGridManager.SetSelectedCuboid(currentIndex);
        }
    }


    public GridManager GetActiveGridManager() => activeGridManager;

    public void SelectByIndex(int index)
    {
        // Always update the UI
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            if (buildingButtons[i] != null && buildingButtons[i].backgroundImage != null)
            {
                buildingButtons[i].backgroundImage.color = (i == index) ? selectedColor : normalColor;
            }
        }

        currentIndex = index;

        if (activeGridManager != null)
        {
            activeGridManager.SetSelectedCuboid(index); // 🔁 Always set the selected cuboid
        }
        else
        {
            Debug.LogWarning("No active GridManager assigned.");
        }
    }

    public void ClearSelection()
    {
        foreach (var button in buildingButtons)
        {
            if (button != null && button.backgroundImage != null)
                button.backgroundImage.color = normalColor;
        }

        currentIndex = -1;

        if (activeGridManager != null)
        {
            activeGridManager.ClearCuboidSelection();
        }
    }
}
