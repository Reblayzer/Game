using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonSelector : MonoBehaviour
{
    [System.Serializable]
    public class BuildingButton
    {
        public GameObject rootObject;       // The full button GameObject
        public Image backgroundImage;       // For visual highlighting
    }

    public List<BuildingButton> buildingButtons;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private int currentIndex = -1;

    public int CurrentIndex => currentIndex;

    public void SelectByIndex(int index)
    {
        // Clicked same again? → Deselect
        if (index == currentIndex)
        {
            ClearSelection();
            return;
        }

        // Apply colors
        for (int i = 0; i < buildingButtons.Count; i++)
        {
            var button = buildingButtons[i];
            if (button != null && button.backgroundImage != null)
            {
                button.backgroundImage.color = (i == index) ? selectedColor : normalColor;
            }
        }

        currentIndex = index;

        // Notify GridManager
        var gm = FindFirstObjectByType<GridManager>();
        if (gm != null)
            gm.SetSelectedCuboid(index);
    }

    public void ClearSelection()
    {
        foreach (var button in buildingButtons)
        {
            if (button != null && button.backgroundImage != null)
                button.backgroundImage.color = normalColor;
        }

        currentIndex = -1;

        var gm = FindFirstObjectByType<GridManager>();
        if (gm != null)
            gm.ClearCuboidSelection();
    }
}
